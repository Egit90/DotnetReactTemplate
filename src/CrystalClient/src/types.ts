import { type CrystalStorage } from './crystal-storage.js';
import { type AxiosInstance } from 'axios';

export interface CrystalClientOptions {
    apiBaseUrl: string;
    axiosInstance: AxiosInstance;
    authApiPrefix?: string;
    adminApiPrefix?: string;
    accountApiPrefix?: string;
    storage?: CrystalStorage;
}
