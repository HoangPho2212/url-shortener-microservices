import { describe, it, expect, vi } from 'vitest';
import { authService } from '../AuthService';
import api from '../api';

vi.mock('../api', () => ({
  default: {
    post: vi.fn()
  }
}));

describe('AuthService', () => {
  it('login should return token on success', async () => {
    const mockResponse = { 
      data: { token: 'test-token', username: 'user', email: 'user@test.com' },
      status: 200,
      statusText: 'OK',
      headers: {},
      config: {} as any
    };
    vi.mocked(api.post).mockResolvedValue(mockResponse);

    const result = await authService.login({ email: 'user@test.com', password: 'password' });

    expect(result.token).toBe('test-token');
    expect(api.post).toHaveBeenCalledWith('/auth/login', { email: 'user@test.com', password: 'password' });
  });

  it('register should call api post', async () => {
    vi.mocked(api.post).mockResolvedValue({ 
      data: {},
      status: 201,
      statusText: 'Created',
      headers: {},
      config: {} as any
    });

    await authService.register({ username: 'user', email: 'user@test.com', password: 'password' });

    expect(api.post).toHaveBeenCalledWith('/auth/register', { username: 'user', email: 'user@test.com', password: 'password' });
  });
});
