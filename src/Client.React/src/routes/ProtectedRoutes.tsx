import { useAuth } from "@/providers/AuthProvider";
import { Navigate, Outlet } from "react-router-dom";

interface RoleProtectedRouteProps {
    allowedRoles: string[],
    children?: React.ReactNode;
}
export const RoleProtectedRoute = ({ allowedRoles, children }: RoleProtectedRouteProps) => {
    const { user } = useAuth();

    if (!user) return <Navigate to="/signin" replace />

    const hasRequiredRoles = user.roles?.some(x => allowedRoles.includes(x));

    if (!hasRequiredRoles) return <Navigate to="/unauthorized" replace />

    return children ? children : <Outlet />
}