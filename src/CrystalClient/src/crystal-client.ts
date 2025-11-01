import { crystalDefaultStorage, type CrystalStorage } from './crystal-storage.js';
import { createAxiosInstance } from './axios-utils.js';
import Axios, {
    type AxiosInstance,
    type AxiosResponse,
    type InternalAxiosRequestConfig,
} from 'axios';
import type {
    AccountInfoResponse,
    CrystalClientOptions,
    AuthUser,
    ChangePasswordRequest,
    ExternalChallengeRequest,
    ResetPasswordRequest,
    SignInRequest,
    SignUpExternalRequest,
    SignUpRequest,
    SignUpResponse,
    TokenResponse,
    WhoAmIResponse,
} from './types.js';

export class CrystalClient {
    private readonly apiBaseUrl: string;
    private readonly authApiPrefix: string;
    private readonly accountApiPrefix: string;
    private readonly storage: CrystalStorage;
    private readonly axios: AxiosInstance;
    private onSignOut: () => void = () => {
        return;
    };
    private onSignIn: (user: AuthUser) => void = () => {
        return;
    };

    constructor(options: CrystalClientOptions) {
        this.apiBaseUrl = options.apiBaseUrl;
        this.authApiPrefix = options.authApiPrefix || '/auth';
        this.accountApiPrefix = options.accountApiPrefix || '/account';
        this.storage = options.storage || crystalDefaultStorage;
        this.axios = options.axiosInstance;
    }

    getUser(): AuthUser | null {
        return this.storage.getUser();
    }

    onEvents(ev: { onSignIn: (user: AuthUser) => void; onSignOut: () => void }): void {
        this.onSignIn = ev.onSignIn;
        this.onSignOut = ev.onSignOut;
    }

    async signUp(data: SignUpRequest): Promise<SignUpResponse> {
        const response = await this.axios.post<SignUpResponse>(this.authApiPrefix + '/signup', data);
        return response.data;
    }

    async signIn(data: SignInRequest): Promise<AuthUser> {
        const tokenRes = await this.axios.post<TokenResponse>(this.authApiPrefix + '/signin', data);

        const user = { email: data.email } as AuthUser;
        this.storage.setUser(user);
        this.storage.setToken(tokenRes.data.access_token);

        this.onSignIn && this.onSignIn(user);
        return user;
    }

    externalChallenge(data: ExternalChallengeRequest) {
        const callbackUrl =
            data.mode == 'SignIn'
                ? window.location.origin + '/external-challenge-callback/' + data.provider
                : window.location.origin + '/profile?link=' + data.provider;

        const api =
            this.apiBaseUrl +
            this.authApiPrefix +
            '/external/challenge/' +
            data.provider +
            '?CallbackUrl=' +
            encodeURIComponent(callbackUrl);
        window.location.replace(api);
    }

    async linkLogin(): Promise<AccountInfoResponse> {
        const path = '/link/external';
        const response = await this.axios.post<AccountInfoResponse>(this.accountApiPrefix + path);
        return response.data;
    }

    async signUpExternal(data: SignUpExternalRequest): Promise<AuthUser | AxiosResponse> {
        const path = '/signup/external';

        const tokenRes = await this.axios.post<TokenResponse>(this.authApiPrefix + path, data);
        this.storage.setToken(tokenRes.data.access_token);

        const res = await this.axios.get<WhoAmIResponse>(this.authApiPrefix + '/whoami');
        const user = { email: res.data.email } as AuthUser;
        this.storage.setUser(user);

        this.onSignIn && this.onSignIn(user);
        return user;
    }

    async signInExternal(): Promise<AuthUser | AxiosResponse> {
        const path = '/signin/external';

        const tokenRes = await this.axios.post<TokenResponse>(this.authApiPrefix + path);
        this.storage.setToken(tokenRes.data.access_token);

        const res = await this.axios.get<WhoAmIResponse>(this.authApiPrefix + '/whoami');
        const user = { email: res.data.email } as AuthUser;
        this.storage.setUser(user);

        this.onSignIn && this.onSignIn(user);
        return user;
    }

    async whoAmI(): Promise<WhoAmIResponse> {
        const response = await this.axios.get<WhoAmIResponse>(this.authApiPrefix + '/whoami');
        return response.data;
    }

    async signOut(): Promise<void> {
        try {
            await this.axios.post(this.authApiPrefix + '/signout');
        } finally {
            this.storage.clearToken();
            this.storage.clearUser();

            this.onSignOut && this.onSignOut();
        }
    }

    async refreshToken(): Promise<TokenResponse> {
        const ax = createAxiosInstance(this.apiBaseUrl);
        const tokenRes = await ax.post<TokenResponse>(this.authApiPrefix + '/signin/refresh');
        this.storage.setToken(tokenRes.data.access_token);
        return tokenRes.data;
    }

    async forgotPassword(email: string) {
        return this.axios.post(this.accountApiPrefix + '/password/forgot', { email });
    }

    async resetPassword(data: ResetPasswordRequest): Promise<AxiosResponse> {
        return this.axios.post(this.accountApiPrefix + '/password/reset', data);
    }

    async changePassword(data: ChangePasswordRequest): Promise<AxiosResponse> {
        return this.axios.post(this.accountApiPrefix + '/password/change', data);
    }

    async resendEmailConfirmation(email: string): Promise<void> {
        return this.axios.post(this.accountApiPrefix + '/email/confirm/resend', { email });
    }

    async confirmEmail(code: string, userId: string): Promise<AxiosResponse> {
        return this.axios.get(this.accountApiPrefix + '/email/confirm', {
            params: {
                code,
                userId,
            },
        });
    }

    async accountInfo(): Promise<AccountInfoResponse> {
        const response = await this.axios.get<AccountInfoResponse>(this.accountApiPrefix + '/info');
        return response.data;
    }

    initAxiosInterceptors(onSignOut?: () => void) {
        this.axios.interceptors.request.use(authRequestInterceptorFactory(this.storage));
        this.axios.interceptors.response.use(
            (value) => value,
            async (error) => {
                if (!Axios.isAxiosError(error)) return Promise.reject(error);

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

        const tryRefreshToken = async (originalRequestConfig: any) => {
            try {
                await this.refreshToken();
                return this.axios.request(originalRequestConfig);
            } catch (error) {
                this.storage.clearToken();
                this.storage.clearUser();
                onSignOut && onSignOut();

                return Promise.reject(error);
            }
        };
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
