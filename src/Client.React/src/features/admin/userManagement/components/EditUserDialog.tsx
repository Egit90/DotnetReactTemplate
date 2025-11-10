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
import { User } from "crystal-client/src/admin/types"
import { extractApiErrors } from "crystal-client/src/axios-utils"
import { Loader2 } from "lucide-react"
import { useState } from "react"
import { useMutation, useQuery } from "@tanstack/react-query"
import { toast } from "sonner"

type EditUserDialogProps = {
    user: User
    onUserUpdated?: () => void
}

export function EditUserDialog({ user, onUserUpdated }: EditUserDialogProps) {
    const { authClient } = useAuth();

    const { data, isLoading, error } = useQuery({
        queryKey: ['roles'],
        queryFn: () => authClient.admin.getAllRoles(),
    })
    const [selectedRoles, setSelectedRoles] = useState<string[]>(user.roles || []);
    const [open, setOpen] = useState(false);

    const handleRoleToggle = (roleName: string, checked: boolean) => {
        if (checked) {
            setSelectedRoles([...selectedRoles, roleName]);
        } else {
            setSelectedRoles(selectedRoles.filter(r => r !== roleName));
        }
    }

    const updateRolesMutation = useMutation({
        mutationFn: ({ id, roleNames }: { id: string, roleNames: string[] }) => authClient.admin.updateUserRoles(id, roleNames),
        onSuccess: () => {
            setOpen(false);
            onUserUpdated?.();
            toast.success('User roles updated successfully');
        }, onError: (error) => {
            const errors = extractApiErrors(error) || ['Failed to update user roles'];
            errors.forEach(err => toast.error(err));
        }
    })

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        updateRolesMutation.mutate({ id: user.id, roleNames: selectedRoles })
    }

    function handleOpenChanged(newOpen: boolean): void {
        setOpen(newOpen)
        if (newOpen) setSelectedRoles(user.roles || [])
    }

    return (
        <Dialog open={open} onOpenChange={handleOpenChanged} >
            <DialogTrigger asChild>
                <Button variant="ghost" size="sm" title="Edit user roles">
                    Edit User Roles
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
                    {error && (
                        <div className="mb-4 p-4 bg-destructive/10 text-destructive rounded-lg">
                            <p>{extractApiErrors(error)?.[0] || 'Failed to load users'}</p>
                        </div>
                    )}
                    <div className="grid gap-4 py-4">
                        {isLoading ? (
                            <div className="flex items-center justify-center py-4">
                                <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
                            </div>
                        ) : (
                            <div className="space-y-3">
                                <Label>Roles</Label>
                                {data?.map((role) => (
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
                            <Button type="button" variant="outline" disabled={updateRolesMutation.isPending}>
                                Cancel
                            </Button>
                        </DialogClose>
                        <Button type="submit" disabled={updateRolesMutation.isPending || isLoading}>
                            {updateRolesMutation.isPending && <Loader2 className="h-4 w-4 mr-2 animate-spin" />}
                            Save changes
                        </Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    )
}
