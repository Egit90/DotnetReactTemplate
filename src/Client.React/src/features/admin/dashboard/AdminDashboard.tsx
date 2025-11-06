import { AppSidebar } from "@/features/admin/dashboard/app-sidebar"
import { SiteHeader } from "@/components/site-header"
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
