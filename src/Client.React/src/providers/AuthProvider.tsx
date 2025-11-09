import React, { createContext, useContext, useMemo } from 'react';
import { CrystalClient } from 'crystal-client/src/crystal-client.ts';
import { createAxiosInstance } from 'crystal-client/src/axios-utils.ts';
import { AuthUser } from 'crystal-client/src/account/types';

export const axios = createAxiosInstance(import.meta.env.VITE_API_URL);
export const authClient = new CrystalClient({
    apiBaseUrl: import.meta.env.VITE_API_URL,
    axiosInstance: axios,
    adminApiPrefix: import.meta.env.VITE_ADMIN_API_BASE_PATH
});

const AuthContext = createContext<AuthContextProps>({ user: null } as AuthContextProps);

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
    const [user, setUser] = React.useState<AuthUser | null>(authClient.account.getUser());

    authClient.initAxiosInterceptors(() => {
        setUser(null);
    });

    authClient.account.onEvents({
        onSignIn: (user) => {
            setUser(user);
        },
        onSignOut: () => {
            setUser(null);
        },
    });

    const value = useMemo<AuthContextProps>(
        () => ({
            user,
            authClient,
        }),
        [user],
    );

    return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = () => {
    return useContext(AuthContext);
};

export interface AuthContextProps {
    user: AuthUser | null;
    authClient: CrystalClient;
}

