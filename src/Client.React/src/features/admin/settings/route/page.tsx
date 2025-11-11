import { Checkbox } from "@/components/ui/checkbox"
import { Label } from "@/components/ui/label"
import { useAuth } from "@/providers/AuthProvider";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { toast } from "sonner"

export function SettingsPage() {

    const { authClient } = useAuth();
    const queryClient = useQueryClient();

    const { data: response, isLoading, error } = useQuery({
        queryKey: ['maintenance'],
        queryFn: () => authClient.admin.getMaintenanceStatus(),
    })

    const toggleMaintenance = useMutation({
        mutationFn: async (enabled: boolean) => {
            return await authClient.admin.toggleMaintenanceStatus(enabled);
        },
        onSuccess: (data) => {
            queryClient.invalidateQueries({ queryKey: ['maintenance'] });
            toast.success(
                data.enabled ? "Maintenance mode enabled" : "Maintenance mode disabled",
                { description: data.message }
            );
        },
        onError: (error) => {
            toast.error("Failed to toggle maintenance mode", {
                description: error instanceof Error ? error.message : "An unexpected error occurred"
            });
        }
    });
    const isEnabled = response?.enabled ?? false;
    return (
        <div className="flex flex-col gap-6">
            <Label className="hover:bg-accent/50 flex items-start gap-3 rounded-lg border p-3 has-[[aria-checked=true]]:border-blue-600 has-[[aria-checked=true]]:bg-blue-50 dark:has-[[aria-checked=true]]:border-blue-900 dark:has-[[aria-checked=true]]:bg-blue-950">
                <Checkbox
                    id="toggle-2"
                    checked={isEnabled}
                    disabled={isLoading || toggleMaintenance.isPending}
                    onCheckedChange={() => toggleMaintenance.mutate(!isEnabled)}
                    className="data-[state=checked]:border-blue-600 data-[state=checked]:bg-blue-600 data-[state=checked]:text-white dark:data-[state=checked]:border-blue-700 dark:data-[state=checked]:bg-blue-700"
                />
                <div className="grid gap-1.5 font-normal">
                    <p className="text-sm leading-none font-medium">
                        Enable Maintenance Mode
                    </p>
                    <p className="text-muted-foreground text-sm">
                        Temporarily disable public access to the system while performing updates or maintenance tasks.
                    </p>
                </div>
            </Label>
        </div>
    )
}
