import { useEffect, useState } from 'react';
import { ChangePasswordForm } from '../components/ChangePasswordForm.tsx';
import { useAuth } from '@/providers/AuthProvider.tsx';
import { AccountInfoResponse } from 'crystal-client/src/types.ts';
import { useSearchParams } from 'react-router-dom';
import { ExternalProviders } from "../../auth/components/ExternalProviders.tsx";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Mail, Key, Link2, Shield } from "lucide-react";
import { toast } from "sonner";

export const MyAccount = () => {
    const { authClient } = useAuth();
    const [user, setUser] = useState<AccountInfoResponse>();

    useEffect(() => {
        authClient.accountInfo().then((res) => {
            setUser(res);
        });
    }, []);

    const [params] = useSearchParams();
    const link = params.get("link");
    const failed = params.get("failed");

    useEffect(() => {
        if (!link) return;

        if (failed) {
            toast.error("Failed to link account");
        } else {
            authClient.linkLogin().then((res) => {
                setUser(res);
                toast.success("Account linked successfully");
            }).catch(() => {
                toast.error("Failed to link account");
            });
        }
    }, [link, failed]);

    return (
        <div className="max-w-7xl mx-auto p-6">
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                {/* Left Column */}
                <div className="space-y-6">
                    {/* Profile Information Card */}
                    <Card>
                        <CardHeader>
                            <CardTitle className="flex items-center gap-2">
                                <Mail className="h-5 w-5 text-primary" />
                                Profile Information
                            </CardTitle>
                            <CardDescription>
                                Manage your account details and settings
                            </CardDescription>
                        </CardHeader>
                        <CardContent className="space-y-6">
                            {/* Email */}
                            <div className="flex flex-col gap-2">
                                <dt className="text-sm font-medium text-foreground">Email address</dt>
                                <dd className="text-sm text-muted-foreground">{user?.email}</dd>
                            </div>

                            {/* Logins */}
                            <div className="flex flex-col gap-2">
                                <dt className="text-sm font-medium text-foreground">Linked logins</dt>
                                <dd className="flex flex-wrap gap-2">
                                    {user?.logins.map((login) => (
                                        <Badge key={login} variant="secondary" className="capitalize">
                                            {login}
                                        </Badge>
                                    ))}
                                </dd>
                            </div>

                            {/* Roles */}
                            <div className="flex flex-col gap-2">
                                <dt className="text-sm font-medium text-foreground">Roles</dt>
                                <dd className="flex flex-wrap gap-2">
                                    {user?.roles.map((role) => (
                                        <Badge key={role} variant="outline" className="capitalize">
                                            <Shield className="h-3 w-3 mr-1" />
                                            {role}
                                        </Badge>
                                    ))}
                                </dd>
                            </div>
                        </CardContent>
                    </Card>

                    {/* Link Additional Logins Card */}
                    <Card>
                        <CardHeader>
                            <CardTitle className="flex items-center gap-2">
                                <Link2 className="h-5 w-5 text-primary" />
                                Link Additional Logins
                            </CardTitle>
                            <CardDescription>
                                Connect other accounts to sign in with multiple providers
                            </CardDescription>
                        </CardHeader>
                        <CardContent>
                            <ExternalProviders mode="LinkLogin" hide={user?.logins} />
                        </CardContent>
                    </Card>
                </div>

                {/* Right Column */}
                <div>
                    {/* Change Password Card */}
                    {user?.hasPassword && (
                        <Card>
                            <CardHeader>
                                <CardTitle className="flex items-center gap-2">
                                    <Key className="h-5 w-5 text-primary" />
                                    Change Password
                                </CardTitle>
                                <CardDescription>
                                    Update your password to keep your account secure
                                </CardDescription>
                            </CardHeader>
                            <CardContent>
                                <ChangePasswordForm />
                            </CardContent>
                        </Card>
                    )}
                </div>
            </div>
        </div>
    );
};
