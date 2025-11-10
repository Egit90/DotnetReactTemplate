import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuLabel, DropdownMenuSeparator, DropdownMenuTrigger } from "@/components/ui/dropdown-menu";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { ColumnDef } from "@tanstack/react-table";
import { User } from "crystal-client/src/admin/types";
import { ArrowUpDown, Unlock, Lock, MoreHorizontal, Mail, KeyRound, Trash2 } from "lucide-react";
import { EditUserDialog } from "./EditUserDialog";
import { authClient } from "@/providers/AuthProvider";
import { toast } from "sonner";
import { extractApiErrors } from "crystal-client/src/axios-utils";
import { ConfirmDialog } from "@/components/ConfirmDialog";

type createColumnsParams = {
  queryClient: ReturnType<typeof useQueryClient>
}

export const createColumns = ({ queryClient }: createColumnsParams): ColumnDef<User>[] => {

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
    mutationFn: (email: string) => authClient.account.forgotPassword(email),
    onSuccess: () => {
      toast.success('Confirmation email sent');
    },
    onError: (error) => {
      const errors = extractApiErrors(error) || ['failed to resend confirmation'];
      errors.forEach(err => toast.error(err))
    }
  })

  const lockMutation = useMutation({
    mutationFn: (userId: string) => authClient.admin.lockUser(userId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      toast.success('User locked successfully');
    },
    onError: (error) => {
      const errors = extractApiErrors(error) || ['Failed to lock user'];
      errors.forEach(err => toast.error(err))
    }
  })

  const unlockMutation = useMutation({
    mutationFn: (userId: string) => authClient.admin.unlockUser(userId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      toast.success('User unlocked successfully');
    },
    onError: (error) => {
      const errors = extractApiErrors(error) || ['Failed to unlock user'];
      errors.forEach(err => toast.error(err))
    }
  })
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


  return (
    [
      {
        id: "select",
        header: ({ table }) => (
          <Checkbox
            checked={
              table.getIsAllPageRowsSelected() ||
              (table.getIsSomePageRowsSelected() && "indeterminate")
            }
            onCheckedChange={(value) => table.toggleAllPageRowsSelected(!!value)}
            aria-label="Select all"
          />
        ),
        cell: ({ row }) => (
          <Checkbox
            checked={row.getIsSelected()}
            onCheckedChange={(value) => row.toggleSelected(!!value)}
            aria-label="Select row"
          />
        ),
        enableSorting: false,
        enableHiding: false,
      },
      {
        accessorKey: "userName",
        header: "Username",
        cell: ({ row }) => (
          <div className="font-medium">{row.getValue("userName")}</div>
        ),
      },
      {
        accessorKey: "email",
        header: ({ column }) => {
          return (
            <Button
              variant="ghost"
              onClick={() => column.toggleSorting(column.getIsSorted() === "asc")}
            >
              Email
              <ArrowUpDown />
            </Button>
          );
        },
        cell: ({ row }) => <div className="lowercase">{row.getValue("email")}</div>,
      },
      {
        accessorKey: "roles",
        header: ({ column }) => {
          return (
            <Button
              variant="ghost"
              onClick={() => column.toggleSorting(column.getIsSorted() === "asc")}
            >
              Roles
              <ArrowUpDown />
            </Button>
          );
        },
        cell: ({ row }) => {
          const roles = row.getValue("roles") as string[];
          return (
            <div className="flex gap-1 flex-wrap">
              {roles && roles.length > 0 ? (
                roles.map((role) => (
                  <Badge key={role} variant="outline">
                    {role}
                  </Badge>
                ))
              ) : (
                <span className="text-muted-foreground text-sm">No roles</span>
              )}
            </div>
          );
        },
      },
      {
        accessorKey: "emailConfirmed",
        header: ({ column }) => {
          return (
            <Button
              variant="ghost"
              onClick={() => column.toggleSorting(column.getIsSorted() === "asc")}
            >
              Status
              <ArrowUpDown />
            </Button>
          );
        },
        cell: ({ row }) => {
          const emailConfirmed = row.getValue("emailConfirmed") as boolean;
          return (
            <Badge variant={emailConfirmed ? "default" : "secondary"}>
              {emailConfirmed ? "Verified" : "Unverified"}
            </Badge>
          );
        },
      },

      {
        accessorKey: "createdOn",
        header: "Created On",
        cell: ({ row }) => {
          const createdOn = row.getValue("createdOn") as string;
          const createOnDate = new Date(createdOn).toLocaleString();
          return <p>{createOnDate}</p>;
        },
      },

      {
        accessorKey: "lastLoginDate",
        header: "Last Login Date",
        cell: ({ row }) => {
          const lastLoginDate = row.getValue("lastLoginDate") as string;
          if (!lastLoginDate)
            return <span className="text-muted-foreground text-sm">Never</span>;
          const lastLoginDateDate = new Date(lastLoginDate).toLocaleString();
          return <p>{lastLoginDateDate}</p>;
        },
      },

      {
        accessorKey: "isLockedOut",
        header: ({ column }) => {
          return (
            <Button
              variant="ghost"
              onClick={() => column.toggleSorting(column.getIsSorted() === "asc")}
            >
              Locked
              <ArrowUpDown />
            </Button>
          );
        },
        cell: ({ row }) => {
          const isLockedOut = row.getValue("isLockedOut") as boolean;
          if (!isLockedOut) return <Unlock className="h-4 w-4 text-green-600" />;
          return <Lock className="h-4 w-4 text-orange-600" />;
        },
      },
      {
        id: "actions",
        enableHiding: false,
        cell: ({ row }) => {
          const user = row.original;
          return (
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="ghost" className="h-8 w-8 p-0">
                  <span className="sr-only">Open menu</span>
                  <MoreHorizontal />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                <DropdownMenuLabel>Actions</DropdownMenuLabel>
                <DropdownMenuSeparator />
                <DropdownMenuItem onSelect={(e) => e.preventDefault()}>
                  <EditUserDialog
                    user={user}
                    onUserUpdated={() =>
                      queryClient.invalidateQueries({ queryKey: ["users"] })
                    }
                  />
                </DropdownMenuItem>

                <DropdownMenuSeparator />

                {!user.emailConfirmed && (
                  <DropdownMenuItem onSelect={(e) => e.preventDefault()}>
                    <ConfirmDialog
                      msg="Are you sure you want to resend the confirmation email?"
                      onConfirm={() => resendEmailMutation.mutate(user.id)}
                      title="Resend Confirmation Email"
                    >
                      <div className="flex items-center">
                        <Mail className="mr-2 h-4 w-4" />
                        Resend Confirmation Email
                      </div>
                    </ConfirmDialog>
                  </DropdownMenuItem>
                )}

                <DropdownMenuItem onSelect={(e) => e.preventDefault()}>
                  <ConfirmDialog
                    msg="This will send a password reset email to the user. Continue?"
                    onConfirm={() => changePasswordMutation.mutate(user.email)}
                    title="Send Password Reset"
                  >
                    <div className="flex items-center">
                      <KeyRound className="mr-2 h-4 w-4" />
                      Send Password Reset
                    </div>
                  </ConfirmDialog>
                </DropdownMenuItem>

                <DropdownMenuSeparator />

                {user.isLockedOut ? (
                  <DropdownMenuItem onSelect={(e) => e.preventDefault()}>
                    <ConfirmDialog
                      msg="Are you sure you want to unlock this user? They will be able to sign in again."
                      onConfirm={() => unlockMutation.mutate(user.id)}
                      title="Unlock User"
                    >
                      <div className="flex items-center">
                        <Unlock className="mr-2 h-4 w-4" />
                        Unlock User
                      </div>
                    </ConfirmDialog>
                  </DropdownMenuItem>
                ) : (
                  <DropdownMenuItem onSelect={(e) => e.preventDefault()}>
                    <ConfirmDialog
                      msg="Are you sure you want to lock this user? They will not be able to sign in."
                      onConfirm={() => lockMutation.mutate(user.id)}
                      title="Lock User"
                    >
                      <div className="flex items-center">
                        <Lock className="mr-2 h-4 w-4" />
                        Lock User
                      </div>
                    </ConfirmDialog>
                  </DropdownMenuItem>
                )}

                <DropdownMenuSeparator />

                <DropdownMenuItem onSelect={(e) => e.preventDefault()}>
                  <ConfirmDialog
                    msg="Are you sure you want to delete this user? This action cannot be undone."
                    onConfirm={() => deleteMutation.mutate(user.id)}
                    title="Delete User"
                  >
                    <div className="flex items-center text-destructive">
                      <Trash2 className="mr-2 h-4 w-4" />
                      Delete User
                    </div>
                  </ConfirmDialog>
                </DropdownMenuItem>

              </DropdownMenuContent>
            </DropdownMenu>
          );
        },
      },
    ]
  )
}