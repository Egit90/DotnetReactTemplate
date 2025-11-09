import { useAuth } from "@/providers/AuthProvider"
import { extractApiErrors } from "crystal-client/src/axios-utils";
import { useState } from "react";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table"
import { Badge } from "@/components/ui/badge"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Loader2, Trash2, ExternalLink, Mail, KeyRound } from "lucide-react"
import { EditUserDialog } from "../components/EditUserDialog";
import { ConfirmDialog } from "@/components/ConfirmDialog";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { Pagination } from "@/components/Pagination";

export const UserManagement = () => {
    const { authClient } = useAuth();
    const queryClient = useQueryClient();
    const [page, setPage] = useState(1)
    const [pageSize] = useState(10)

    const { data, isLoading, error } = useQuery({
        queryKey: ['users', page, pageSize],
        queryFn: () => authClient.admin.getUsers(page, pageSize),
    })

    const users = data?.data ?? [];
    const totalCount = data?.totalCount ?? 0;
    const totalPages = data?.totalPages ?? 0;
    const hasNextPage = data?.hasNextPage ?? false;
    const hasPreviousPage = data?.hasPreviousPage ?? false;

    const deleteMutation = useMutation({
        mutationFn: (userId: string) => authClient.admin.deleteUser(userId),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['users'] });
            toast.success('User deleted successfully');
        },
        onError: (error) => {
            const errors = extractApiErrors(error) || ['Error while deleting the user'];
            errors.forEach(err => toast.error(err))
        }
    })

    const resendEmailMutation = useMutation({
        mutationFn: (id: string) => authClient.admin.resendEmailConfirmation(id),
        onSuccess: () => {
            toast.success('Confirmation email sent');
        },
        onError: (error) => {
            const errors = extractApiErrors(error) || ['failed to resend confirmation'];
            errors.forEach(err => toast.error(err))
        }
    })

    const changePasswordMutation = useMutation({
        mutationFn: (email: string) => authClient.forgotPassword(email),
        onSuccess: () => {
            toast.success('Confirmation email sent');
        },
        onError: (error) => {
            const errors = extractApiErrors(error) || ['failed to resend confirmation'];
            errors.forEach(err => toast.error(err))
        }
    })

    return (
        <div className="space-y-4">
            <Card>
                <CardHeader>
                    <CardTitle>User Management</CardTitle>
                    <CardDescription>
                        Manage all users in the system
                    </CardDescription>
                </CardHeader>
                <CardContent>
                    {error && (
                        <div className="mb-4 p-4 bg-destructive/10 text-destructive rounded-lg">
                            <p>{extractApiErrors(error)?.[0] || 'Failed to load users'}</p>
                        </div>
                    )}

                    {isLoading ? (
                        <div className="flex items-center justify-center py-8">
                            <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
                        </div>
                    ) : users.length === 0 ? (
                        <div className="text-center py-8 text-muted-foreground">
                            No users found
                        </div>
                    ) : (
                        <div className="rounded-md border">
                            <Table>
                                <TableHeader>
                                    <TableRow>
                                        <TableHead>Username</TableHead>
                                        <TableHead>Email</TableHead>
                                        <TableHead>Roles</TableHead>
                                        <TableHead>Status</TableHead>
                                        <TableHead>About Me</TableHead>
                                        <TableHead>Website</TableHead>
                                        <TableHead className="text-right">Actions</TableHead>
                                    </TableRow>
                                </TableHeader>
                                <TableBody>
                                    {users.map((user) => (
                                        <TableRow key={user.id}>
                                            <TableCell className="font-medium">
                                                {user.userName}
                                            </TableCell>
                                            <TableCell>{user.email}</TableCell>
                                            <TableCell>
                                                <div className="flex gap-1 flex-wrap">
                                                    {user.roles && user.roles.length > 0 ? (
                                                        user.roles.map((role) => (
                                                            <Badge key={role} variant="outline">
                                                                {role}
                                                            </Badge>
                                                        ))
                                                    ) : (
                                                        <span className="text-muted-foreground text-sm">No roles</span>
                                                    )}
                                                </div>
                                            </TableCell>
                                            <TableCell>
                                                <Badge variant={user.emailConfirmed ? "default" : "secondary"}>
                                                    {user.emailConfirmed ? "Verified" : "Unverified"}
                                                </Badge>
                                            </TableCell>
                                            <TableCell className="max-w-xs truncate">
                                                {user.aboutMe || '-'}
                                            </TableCell>
                                            <TableCell>
                                                {user.mySiteUrl ? (
                                                    <a
                                                        href={user.mySiteUrl}
                                                        target="_blank"
                                                        rel="noopener noreferrer"
                                                        className="flex items-center gap-1 text-primary hover:underline"
                                                    >
                                                        Visit <ExternalLink className="h-3 w-3" />
                                                    </a>
                                                ) : '-'}
                                            </TableCell>
                                            <TableCell className="text-right">
                                                <div className="flex justify-end gap-2">
                                                    <EditUserDialog user={user} onUserUpdated={() => queryClient.invalidateQueries({ queryKey: ['users'] })} />
                                                    <ConfirmDialog
                                                        msg="Are you sure you want to delete this user?"
                                                        onConfirm={() => deleteMutation.mutate(user.id)}
                                                        title="Delete user"
                                                    >
                                                        <Trash2 className="h-4 w-4 text-destructive" />
                                                    </ConfirmDialog>
                                                    <ConfirmDialog
                                                        msg="Are you sure you want to resend confirmation?"
                                                        onConfirm={() => resendEmailMutation.mutate(user.id)}
                                                        title="Resend confirmation email"
                                                    >
                                                        <Mail className="h-4 w-4" />
                                                    </ConfirmDialog>
                                                    <ConfirmDialog
                                                        msg="Are you sure you want to send change password email?"
                                                        onConfirm={() => changePasswordMutation.mutate(user.email)}
                                                        title="Send password reset email"
                                                    >
                                                        <KeyRound className="h-4 w-4" />
                                                    </ConfirmDialog>
                                                </div>
                                            </TableCell>
                                        </TableRow>
                                    ))}
                                </TableBody>
                            </Table>
                        </div>
                    )}

                    {!isLoading && users.length > 0 && (
                        <Pagination hasNextPage={hasNextPage}
                            hasPreviousPage={hasPreviousPage}
                            page={page} pageSize={pageSize}
                            setPage={setPage}
                            totalCount={totalCount}
                            totalPages={totalPages} />
                    )}
                </CardContent>
            </Card>
        </div >
    )
}