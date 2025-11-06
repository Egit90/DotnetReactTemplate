import { Link, useSearchParams } from "react-router-dom";
import { useEffect, useState } from "react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { CheckCircle, AlertCircle, Loader2, XCircle } from 'lucide-react';
import { useAuth } from "@/providers/AuthProvider";
import { Button } from "@/components/ui/button";

export const ConfirmEmail = () => {
    const [params] = useSearchParams();
    const code = params.get("code");
    const userId = params.get("userId");
    const valid = code && userId;

    const [loading, setLoading] = useState(false);
    const [success, setSuccess] = useState<boolean | null>(null);
    const { authClient } = useAuth();

    useEffect(() => {
        const confirmEmail = async () => {
            if (valid) {
                setLoading(true);
                try {
                    await authClient.confirmEmail(code, userId);
                    setSuccess(true);
                } catch {
                    setSuccess(false);
                } finally {
                    setLoading(false);
                }
            }
        };

        confirmEmail();
    }, [valid, code, userId, authClient]);

    return (
        <div className="min-h-screen flex items-center justify-center bg-background p-4">
            <Card className="w-full max-w-md">
                <CardHeader className="text-center">
                    {!valid && (
                        <>
                            <div className="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-destructive/10">
                                <XCircle className="h-6 w-6 text-destructive" />
                            </div>
                            <CardTitle className="text-2xl">Invalid Request</CardTitle>
                            <CardDescription>
                                The confirmation link is invalid or malformed.
                            </CardDescription>
                        </>
                    )}

                    {valid && loading && (
                        <>
                            <div className="mx-auto mb-4 flex h-12 w-12 items-center justify-center">
                                <Loader2 className="h-8 w-8 animate-spin text-primary" />
                            </div>
                            <CardTitle className="text-2xl">Confirming Email</CardTitle>
                            <CardDescription>
                                Please wait while we confirm your email address...
                            </CardDescription>
                        </>
                    )}

                    {valid && !loading && success === false && (
                        <>
                            <div className="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-destructive/10">
                                <AlertCircle className="h-6 w-6 text-destructive" />
                            </div>
                            <CardTitle className="text-2xl">Confirmation Failed</CardTitle>
                            <CardDescription>
                                Something went wrong while confirming your email.
                            </CardDescription>
                        </>
                    )}

                    {valid && !loading && success === true && (
                        <>
                            <div className="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-primary/10">
                                <CheckCircle className="h-6 w-6 text-primary" />
                            </div>
                            <CardTitle className="text-2xl">Email Confirmed!</CardTitle>
                            <CardDescription>
                                Your email has been successfully verified.
                            </CardDescription>
                        </>
                    )}
                </CardHeader>

                <CardContent className="space-y-4">
                    {!valid && (
                        <p className="text-sm text-muted-foreground text-center">
                            Please check your email for the correct confirmation link, or request a new confirmation email.
                        </p>
                    )}

                    {valid && !loading && success === false && (
                        <p className="text-sm text-muted-foreground text-center">
                            The confirmation link may have expired or already been used. Please try requesting a new confirmation email.
                        </p>
                    )}

                    {valid && !loading && success === true && (
                        <div className="space-y-4">
                            <p className="text-sm text-muted-foreground text-center">
                                You can now sign in to your account.
                            </p>
                            <Button asChild className="w-full" size="lg">
                                <Link to="/signin">Sign In</Link>
                            </Button>
                        </div>
                    )}
                </CardContent>
            </Card>
        </div>
    );
};
