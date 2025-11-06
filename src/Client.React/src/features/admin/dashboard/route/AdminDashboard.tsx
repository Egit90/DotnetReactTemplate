import { AppSidebar } from "@/features/admin/dashboard/components/app-sidebar"
import { SiteHeader } from "@/features/admin/dashboard/components/site-header"
import {
    SidebarInset,
    SidebarProvider,
} from "@/components/ui/sidebar"

export default function AdminDashboard() {
    return (
        <SidebarProvider>
            <AppSidebar variant="inset" />
            <SidebarInset>
                <SiteHeader />
            </SidebarInset>
        </SidebarProvider >
    )
}
