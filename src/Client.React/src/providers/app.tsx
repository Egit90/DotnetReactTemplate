import * as React from 'react';
import { ErrorBoundary } from 'react-error-boundary';
import { HelmetProvider } from 'react-helmet-async';
import { BrowserRouter as Router } from 'react-router-dom';
import { AuthProvider } from "./AuthProvider.tsx";
import { ThemeProvider } from "./ThemeProvider.tsx";
import { Toaster } from "sonner";
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';

const queryClient = new QueryClient({
    defaultOptions: {
        queries: {
            retry: 1,
            refetchOnWindowFocus: false,
            staleTime: 5 * 60 * 1000
        }
    }
})


const ErrorFallback = () => {
    return (
        <div
            className="text-red-500 w-screen h-screen flex flex-col justify-center items-center"
            role="alert"
        >
            <h2 className="text-lg font-semibold">Ooops, something went wrong :( </h2>
            <button className="btn" onClick={() => window.location.assign(window.location.origin)}>
                Refresh
            </button>
        </div>
    );
};

type AppProviderProps = {
    children: React.ReactNode;
};

export const AppProvider = ({ children }: AppProviderProps) => {
    return (
        <QueryClientProvider client={queryClient}>
            <React.Suspense
                fallback={
                    <div className="flex items-center justify-center w-screen h-screen">
                        ...loading
                    </div>
                }
            >
                <ErrorBoundary FallbackComponent={ErrorFallback}>
                    <HelmetProvider>
                        <ThemeProvider defaultTheme="system" storageKey="crystal-ui-theme">
                            <AuthProvider>
                                <Router>
                                    {children}
                                    <Toaster closeButton richColors position="top-right" />
                                </Router>
                            </AuthProvider>
                        </ThemeProvider>
                    </HelmetProvider>
                </ErrorBoundary>
            </React.Suspense>
            <ReactQueryDevtools />
        </QueryClientProvider>
    );
};
