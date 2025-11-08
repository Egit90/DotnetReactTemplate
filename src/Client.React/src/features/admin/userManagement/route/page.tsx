import { useAuth } from "@/providers/AuthProvider"
import { User } from "crystal-client/src/admin/types";
import { extractApiErrors } from "crystal-client/src/axios-utils";
import { useEffect, useState } from "react";
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
import { Button } from "@/components/ui/button"
import { Loader2, Trash2, ExternalLink, ChevronLeft, ChevronRight } from "lucide-react"
import { EditUserDialog } from "../components/EditUserDialog";

export const UserManagement = () => {
    const { authClient } = useAuth();
    const [users, setUsers] = useState<User[]>([])
    const [errors, setErrors] = useState<string[]>([])
    const [loading, setLoading] = useState(true)
    const [page, setPage] = useState(1)
    const [pageSize] = useState(10)
    const [totalCount, setTotalCount] = useState(0)
    const [totalPages, setTotalPages] = useState(0)
    const [hasNextPage, setHasNextPage] = useState(false)
    const [hasPreviousPage, setHasPreviousPage] = useState(false)

    useEffect(() => {
        const fetchUsers = async () => {
            try {
                setLoading(true)
                const response = await authClient.admin.getUsers(page, pageSize);
                setUsers(response.data);
                setTotalCount(response.totalCount);
                setTotalPages(response.totalPages);
                setHasNextPage(response.hasNextPage);
                setHasPreviousPage(response.hasPreviousPage);
                setErrors([])
            } catch (error) {
                console.error(error)
                setErrors(extractApiErrors(error) || ['Failed to load users']);
            } finally {
                setLoading(false)
            }
        }
        fetchUsers();
    }, [authClient, page, pageSize])

    const refetchUsers = async () => {
        try {
            const response = await authClient.admin.getUsers(page, pageSize);
            setUsers(response.data);
            setTotalCount(response.totalCount);
            setTotalPages(response.totalPages);
            setHasNextPage(response.hasNextPage);
            setHasPreviousPage(response.hasPreviousPage);
        } catch (error) {
            console.error(error);
            setErrors(extractApiErrors(error) || ['Failed to refresh user list']);
        }
    }

    const handleDelete = async (userId: string) => {
        if (!confirm('Are you sure you want to delete this user?')) return;

        try {
            await authClient.admin.deleteUser(userId);
            await refetchUsers();
        } catch (error) {
            console.error(error)
            setErrors(extractApiErrors(error) || ['Failed to delete user']);
        }
    }

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
                    {errors.length > 0 && (
                        <div className="mb-4 p-4 bg-destructive/10 text-destructive rounded-lg">
                            {errors.map((error, idx) => (
                                <p key={idx}>{error}</p>
                            ))}
                        </div>
                    )}

                    {loading ? (
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
                                                    <EditUserDialog user={user} onUserUpdated={refetchUsers} />
                                                    <Button
                                                        variant="ghost"
                                                        size="sm"
                                                        onClick={() => handleDelete(user.id)}
                                                    >
                                                        <Trash2 className="h-4 w-4 text-destructive" />
                                                    </Button>
                                                </div>
                                            </TableCell>
                                        </TableRow>
                                    ))}
                                </TableBody>
                            </Table>
                        </div>
                    )}

                    {/* Pagination Controls */}
                    {!loading && users.length > 0 && (
                        <div className="flex items-center justify-between px-2 py-4">
                            <div className="text-sm text-muted-foreground">
                                Showing <span className="font-medium">{(page - 1) * pageSize + 1}</span> to{" "}
                                <span className="font-medium">{Math.min(page * pageSize, totalCount)}</span> of{" "}
                                <span className="font-medium">{totalCount}</span> users
                            </div>
                            <div className="flex items-center gap-2">
                                <Button
                                    variant="outline"
                                    size="sm"
                                    onClick={() => setPage(page - 1)}
                                    disabled={!hasPreviousPage}
                                >
                                    <ChevronLeft className="h-4 w-4 mr-1" />
                                    Previous
                                </Button>
                                <div className="flex items-center gap-1">
                                    <span className="text-sm">
                                        Page {page} of {totalPages}
                                    </span>
                                </div>
                                <Button
                                    variant="outline"
                                    size="sm"
                                    onClick={() => setPage(page + 1)}
                                    disabled={!hasNextPage}
                                >
                                    Next
                                    <ChevronRight className="h-4 w-4 ml-1" />
                                </Button>
                            </div>
                        </div>
                    )}
                </CardContent>
            </Card>
        </div >
    )
}