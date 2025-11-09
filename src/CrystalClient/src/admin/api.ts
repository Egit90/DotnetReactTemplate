import { AxiosInstance } from 'axios';
import { PaginatedResponse, RolesResponse, User } from './types';

/**
 * Admin API - handles all admin-related endpoints
 * Reuses the axios instance from CrystalClient (already has auth interceptors configured)
 */
export class AdminApi {
    private readonly adminApiPrefix: string;
    private readonly axios: AxiosInstance;

    constructor(axiosInstance: AxiosInstance, adminApiPrefix = '/admin') {
        this.axios = axiosInstance;
        this.adminApiPrefix = adminApiPrefix;
    }

    async getUsers(page = 1, pageSize = 10, searchTerm = ''): Promise<PaginatedResponse<User>> {
        const response = await this.axios.get<PaginatedResponse<User>>(
            this.adminApiPrefix +
                `/users?page=${page}&pageSize=${pageSize}&searchTerm=${searchTerm}`,
        );
        return response.data;
    }

    async getUserById(userId: string): Promise<User> {
        const response = await this.axios.get<User>(`${this.adminApiPrefix}/users/${userId}`);
        return response.data;
    }

    async deleteUser(userId: string): Promise<void> {
        await this.axios.delete(`${this.adminApiPrefix}/users/${userId}`);
    }

    async updateUser(userId: string, data: Partial<User>): Promise<User> {
        const response = await this.axios.put<User>(`${this.adminApiPrefix}/users/${userId}`, data);
        return response.data;
    }
    async getAllRoles(): Promise<RolesResponse[]> {
        const response = await this.axios.get<RolesResponse[]>(`${this.adminApiPrefix}/roles`);
        return response.data;
    }

    async updateUserRoles(userId: string, roleNames: string[]): Promise<void> {
        await this.axios.put(`${this.adminApiPrefix}/users/${userId}/roles`, { roleNames });
    }

    async resendEmailConfirmation(userId: string): Promise<void> {
        await this.axios.post(`${this.adminApiPrefix}/users/${userId}/resend-confirmation`);
    }

    async lockUser(userId: string): Promise<void> {
        await this.axios.post(`${this.adminApiPrefix}/user/${userId}/lock`);
    }

    async unlockUser(userId: string): Promise<void> {
        await this.axios.post(`${this.adminApiPrefix}/user/${userId}/unLock`);
    }
}
