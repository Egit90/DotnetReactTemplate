export type User = {
    id: string;
    userName: string;
    email: string;
    emailConfirmed: boolean;
    aboutMe: string;
    mySiteUrl: string;
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
