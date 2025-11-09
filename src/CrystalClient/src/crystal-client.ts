import { crystalDefaultStorage, type CrystalStorage } from './crystal-storage.js';
import { createAxiosInstance } from './axios-utils.js';
import Axios, { type AxiosInstance, type InternalAxiosRequestConfig } from 'axios';
import type { CrystalClientOptions } from './types.js';
import { AdminApi } from './admin/api.js';
import { AccountApi } from './account/api.js';

export class CrystalClient {
    private readonly apiBaseUrl: string;
    private readonly storage: CrystalStorage;
    private readonly axios: AxiosInstance;

    // API instances
    public readonly admin: AdminApi;
    public readonly account: AccountApi;

    constructor(options: CrystalClientOptions) {
        this.apiBaseUrl = options.apiBaseUrl;
        this.storage = options.storage || crystalDefaultStorage;

        // Create or use provided axios instance
        this.axios = options.axiosInstance || createAxiosInstance(this.apiBaseUrl);

        // Initialize API instances
        this.admin = new AdminApi(this.axios, options.adminApiPrefix);
        this.account = new AccountApi(this.axios, options);
    }

    initAxiosInterceptors(onSignOut?: () => void) {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const tryRefreshToken = async (originalConfig: any) => {
            try {
                await this.account.refreshToken();
                return this.axios(originalConfig);
            } catch (_error) {
                this.storage.clearToken();
                this.storage.clearUser();
                onSignOut && onSignOut();
                return Promise.reject(_error);
            }
        };

        this.axios.interceptors.request.use(authRequestInterceptorFactory(this.storage));
        this.axios.interceptors.response.use(
            (value) => value,
            async (error) => {
                if (!Axios.isAxiosError(error)) return Promise.reject(error);
                // eslint-disable-next-line @typescript-eslint/no-explicit-any
                const originalConfig = error.config as any;
                const expired = error.response?.headers
                    ? error.response.headers['x-token-expired']
                    : false;

                if (error.response?.status === 401 && expired && !originalConfig._retry) {
                    originalConfig._retry = true;
                    return tryRefreshToken(originalConfig);
                } else if (error.response?.status === 401 && !expired) {
                    this.storage.clearToken();
                    this.storage.clearUser();
                    onSignOut && onSignOut();
                }

                return Promise.reject(error);
            },
        );
    }
}

export const authRequestInterceptorFactory =
    (storage: CrystalStorage) => (config: InternalAxiosRequestConfig) => {
        const token = storage.getToken();
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        config.headers.Accept = 'application/json';
        return config;
    };
