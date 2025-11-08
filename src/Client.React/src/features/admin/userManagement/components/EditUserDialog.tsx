import { Button } from "@/components/ui/button"
import {
    Dialog,
    DialogClose,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
    DialogTrigger,
} from "@/components/ui/dialog"
import { useAuth } from "@/providers/AuthProvider"
import { Checkbox } from "@/components/ui/checkbox"
import { Label } from "@/components/ui/label"
import { RolesResponse, User } from "crystal-client/src/admin/types"
import { extractApiErrors } from "crystal-client/src/axios-utils"
import { Edit, Loader2 } from "lucide-react"
import { useEffect, useState } from "react"

type EditUserDialogProps = {
    user: User
    onUserUpdated?: () => void
}

export function EditUserDialog({ user, onUserUpdated }: EditUserDialogProps) {
    const { authClient } = useAuth();
    const [roles, setRoles] = useState<RolesResponse[]>([]);
    const [selectedRoles, setSelectedRoles] = useState<string[]>(user.roles || []);
    const [loading, setLoading] = useState(false);
    const [fetchingRoles, setFetchingRoles] = useState(true);
    const [errors, setErrors] = useState<string[]>([]);
    const [open, setOpen] = useState(false);

    useEffect(() => {
        const fetchRoles = async () => {
            try {
                setFetchingRoles(true);
                const rolesResponse = await authClient.admin.getAllRoles();
                setRoles(rolesResponse);
            } catch (error) {
                console.error(error);
                setErrors(extractApiErrors(error) || ['Failed to load roles']);
            } finally {
                setFetchingRoles(false);
            }
        }
        fetchRoles();
    }, [authClient])

    const handleRoleToggle = (roleName: string, checked: boolean) => {
        if (checked) {
            setSelectedRoles([...selectedRoles, roleName]);
        } else {
            setSelectedRoles(selectedRoles.filter(r => r !== roleName));
        }
    }

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setErrors([]);

        try {
            await authClient.admin.updateUserRoles(user.id, selectedRoles);
            setOpen(false);
            onUserUpdated?.();
        } catch (error) {
            console.error(error);
            setErrors(extractApiErrors(error) || ['Failed to update user roles']);
        } finally {
            setLoading(false);
        }
    }

    return (
        <Dialog open={open} onOpenChange={setOpen} >
            <DialogTrigger asChild>
                <Button variant="ghost" size="sm" title="Edit user roles">
                    <Edit className="h-4 w-4" />
                </Button>
            </DialogTrigger>
            <DialogContent className="sm:max-w-[425px]">
                <form onSubmit={handleSubmit}>
                    <DialogHeader>
                        <DialogTitle>Edit User Roles</DialogTitle>
                        <DialogDescription>
                            Manage roles for {user.email}
                        </DialogDescription>
                    </DialogHeader>

                    {errors.length > 0 && (
                        <div className="my-4 p-3 bg-destructive/10 text-destructive text-sm rounded-lg">
                            {errors.map((error, idx) => (
                                <p key={idx}>{error}</p>
                            ))}
                        </div>
                    )}

                    <div className="grid gap-4 py-4">
                        {fetchingRoles ? (
                            <div className="flex items-center justify-center py-4">
                                <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
                            </div>
                        ) : (
                            <div className="space-y-3">
                                <Label>Roles</Label>
                                {roles.map((role) => (
                                    <div key={role.id} className="flex items-center space-x-2">
                                        <Checkbox
                                            id={role.id}
                                            checked={selectedRoles.includes(role.name)}
                                            onCheckedChange={(checked) => handleRoleToggle(role.name, checked as boolean)}
                                        />
                                        <label
                                            htmlFor={role.id}
                                            className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70 cursor-pointer"
                                        >
                                            {role.name}
                                        </label>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>

                    <DialogFooter>
                        <DialogClose asChild>
                            <Button type="button" variant="outline" disabled={loading}>
                                Cancel
                            </Button>
                        </DialogClose>
                        <Button type="submit" disabled={loading || fetchingRoles}>
                            {loading && <Loader2 className="h-4 w-4 mr-2 animate-spin" />}
                            Save changes
                        </Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    )
}
