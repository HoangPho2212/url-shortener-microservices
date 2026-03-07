import api from './api';
import type { UserLoginContract, UserRegisterContract, AuthResponseContract } from '../contracts/AuthContract';

export const authService = {
  async login(credentials: UserLoginContract): Promise<AuthResponseContract> {
    const response = await api.post<AuthResponseContract>('/auth/login', credentials);
    return response.data;
  },

  async register(user: UserRegisterContract): Promise<void> {
    await api.post('/auth/register', user);
  }
};
