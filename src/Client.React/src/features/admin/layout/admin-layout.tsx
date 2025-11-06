import { AppSidebar } from "@/features/admin/layout/components/app-sidebar"
import { SiteHeader } from "@/features/admin/layout/components/site-header"
import {
    SidebarInset,
    SidebarProvider,
} from "@/components/ui/sidebar"
import { Outlet } from "react-router-dom"

export const AdminLayout = () => {
    return (
        <SidebarProvider>
            <AppSidebar variant="inset" />
            <SidebarInset>
                <SiteHeader />
                <div className="px-4 py-4 sm:px-6 md:py-6 lg:px-8 xl:px-12">
                    <Outlet />
                </div>
            </SidebarInset>
        </SidebarProvider >
    )
}