import { StatsCards } from "../components/StatsCards";

export default function AdminHome() {
    return (
        <div className="space-y-6">
            <div>
                <h1 className="text-3xl font-bold tracking-tight">Dashboard</h1>
                <p className="text-muted-foreground">
                    Welcome to the admin dashboard. Here's an overview of your users.
                </p>
            </div>
            <StatsCards />
        </div>
    )
}
