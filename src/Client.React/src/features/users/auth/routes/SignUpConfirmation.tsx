import { Link, useLocation } from "react-router-dom";
import { SignUpResponse } from 'crystal-client/src/types.ts';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { House, Mail, CheckCircle2 } from "lucide-react";

export const SignUpConfirmation = () => {
    const location = useLocation();
    const { requiresEmailConfirmation } = location.state as SignUpResponse;

    return (
        <>
            <div className="flex min-h-screen min-w-full w-full items-center justify-center p-6 md:p-10 relative">
                <Link
                    to="/"
                    className="absolute top-4 left-4 flex items-center gap-2 text-sm font-medium text-muted-foreground hover:text-foreground transition-colors"
                >
                    <House className="h-4 w-4" />
                    Home
                </Link>
                <div className="w-full max-w-md">
                    <Card>
                        <CardHeader className="text-center">
                            <div className="flex justify-center mb-4">
                                <div className="rounded-full bg-primary/10 p-3">
                                    <CheckCircle2 className="h-12 w-12 text-primary" />
                                </div>
                            </div>
                            <CardTitle className="text-2xl">Account Created!</CardTitle>
                            <CardDescription>
                                Congratulations! You have successfully signed up.
                            </CardDescription>
                        </CardHeader>
                        <CardContent className="space-y-4">
                            {requiresEmailConfirmation && (
                                <div className="rounded-lg border border-border bg-muted/50 p-4">
                                    <div className="flex gap-3">
                                        <Mail className="h-5 w-5 text-primary mt-0.5" />
                                        <div className="space-y-1">
                                            <p className="text-sm font-medium text-foreground">
                                                Verify your email
                                            </p>
                                            <p className="text-sm text-muted-foreground">
                                                Please check your email for a confirmation link before signing in.
                                            </p>
                                        </div>
                                    </div>
                                </div>
                            )}

                            <div className="space-y-3">
                                <Button asChild className="w-full" size="lg">
                                    <Link to="/signin">Continue to Sign In</Link>
                                </Button>

                                {requiresEmailConfirmation && (
                                    <p className="text-center text-sm text-muted-foreground">
                                        Didn't receive the email?{" "}
                                        <Link
                                            to="/resend-confirm-email"
                                            className="text-primary hover:underline font-medium"
                                        >
                                            Resend confirmation
                                        </Link>
                                    </p>
                                )}
                            </div>
                        </CardContent>
                    </Card>
                </div>
            </div>
        </>
    );
};