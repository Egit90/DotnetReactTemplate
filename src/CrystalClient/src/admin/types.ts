export type User = {
    id: string;
    userName: string;
    email: string;
    emailConfirmed: boolean;
    lastLoginDate?: string;
    createdOn: string;
    lockoutEnd?: string;       // When lockout ends (null = not locked)
    isLockedOut: boolean;      // Computed: is user currently locked?
    roles: string[];
};
export type PaginatedResponse<T> = {
    data: T[];
    page: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
};

export type RolesResponse = {
    id: string;
    name: string;
};

export type StatsResponse = {
    totalUsers: number;
    confirmedUsers: number;
    unconfirmedUsers: number;
    lockedUsers: number;
    activeUsers: number;
};
