import { AuthUser } from './account/types';

const storagePrefix = 'crystal_';

export const crystalDefaultStorage: CrystalStorage = {
    getUser: () => {
        return JSON.parse(window.localStorage.getItem(`${storagePrefix}user`) as string);
    },
    setUser: (user: AuthUser) => {
        window.localStorage.setItem(`${storagePrefix}user`, JSON.stringify(user));
    },
    clearUser: () => {
        window.localStorage.removeItem(`${storagePrefix}user`);
    },
    getToken: () => {
        return JSON.parse(window.localStorage.getItem(`${storagePrefix}token`) as string);
    },
    setToken: (token: string) => {
        window.localStorage.setItem(`${storagePrefix}token`, JSON.stringify(token));
    },
    clearToken: () => {
        window.localStorage.removeItem(`${storagePrefix}token`);
    },
};

export interface CrystalStorage {
    getUser: () => AuthUser | null;
    setUser: (user: AuthUser) => void;
    clearUser: () => void;
    getToken: () => string | null;
    setToken: (token: string) => void;
    clearToken: () => void;
}
