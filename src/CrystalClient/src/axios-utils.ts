import Axios, { AxiosError, type CreateAxiosDefaults } from 'axios';

export const createAxiosInstance = (apiBaseUrl: string, config?: CreateAxiosDefaults) => {
    return Axios.create({
        baseURL: apiBaseUrl,
        withCredentials: true,

        ...config,
    });
};

export const extractApiErrors = (error: any): string[] | null => {
    if (!Axios.isAxiosError(error)) return null;

    const e = error as AxiosError;
    if (!e.response?.data) return null;

    const data = e.response.data as any;

    if (data.detail) {
        return [data.detail];
    }

    if (data.errors && typeof data.errors === 'object') {
        return Object.keys(data.errors).flatMap((key) => data.errors[key]);
    }

    if (data.message) {
        return [data.message];
    }

    if (data.error) {
        return [data.error];
    }

    if (Array.isArray(data.Errors)) {
        return data.Errors.map((err: any) => err.description || err.message);
    }

    if (e.response.statusText) {
        return [e.response.statusText];
    }

    return null;
};
