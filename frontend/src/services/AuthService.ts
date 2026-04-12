import api from './api';
import type { UserLoginContract, UserRegisterContract, AuthResponseContract } from '../contracts/AuthContract';

export const authService = {
    async login(credentials: UserLoginContract): Promise<AuthResponseContract> {
        const response = await api.post<any>('/auth/login', credentials);
        const data = response.data;

        return {
            token: (data.token ?? data.Token ?? '').trim(),
            username: (data.username ?? data.Username ?? '').trim(),
            email: (data.email ?? data.Email ?? '').trim()
        };
    },

    async register(user: UserRegisterContract): Promise<void> {
        await api.post('/auth/register', user);
    }
};
