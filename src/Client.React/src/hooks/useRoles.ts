import { useAuth } from "@/providers/AuthProvider";

export const useRole = () => {
  const { user } = useAuth();

  const hasRole = (role: string): boolean => {
    return user?.roles?.includes(role) ?? false;
  };

  const hasAnyRole = (roles: string[]): boolean => {
    return roles.some((role) => hasRole(role));
  };

  const roles = user?.roles || [];

  const isAdmin = () => hasRole("Admin");

  return { hasRole, hasAnyRole, isAdmin, roles };
};
