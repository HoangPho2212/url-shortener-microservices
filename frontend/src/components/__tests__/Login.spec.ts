import { describe, it, expect, vi, beforeEach } from 'vitest';
import { mount } from '@vue/test-utils';
import Login from '../Login.vue';
import { authService } from '../../services/AuthService';
import { useRouter } from 'vue-router';

vi.mock('../../services/AuthService', () => ({
  authService: {
    login: vi.fn()
  }
}));

vi.mock('vue-router', () => ({
  useRouter: vi.fn()
}));

describe('Login.vue', () => {
  let mockPush: any;

  beforeEach(() => {
    vi.clearAllMocks();
    mockPush = vi.fn();
    (useRouter as any).mockReturnValue({
      push: mockPush
    });
    
    // Mock localStorage
    Object.defineProperty(window, 'localStorage', {
      value: {
        getItem: vi.fn(),
        setItem: vi.fn(),
        clear: vi.fn()
      },
      writable: true
    });
  });

  it('renders login form correctly', () => {
    const wrapper = mount(Login, {
      global: {
        stubs: ['router-link']
      }
    });
    expect(wrapper.find('h2').text()).toBe('Login');
    expect(wrapper.find('input[type="email"]').exists()).toBe(true);
    expect(wrapper.find('input[type="password"]').exists()).toBe(true);
    expect(wrapper.find('button[type="submit"]').text()).toBe('Login');
  });

  it('updates credentials reactive object on input', async () => {
    const wrapper = mount(Login, {
      global: {
        stubs: ['router-link']
      }
    });
    const emailInput = wrapper.find('input[type="email"]');
    const passwordInput = wrapper.find('input[type="password"]');

    await emailInput.setValue('test@example.com');
    await passwordInput.setValue('password123');

    // Credentials are internal reactive state, so we check if inputs are correctly bound
    expect((emailInput.element as HTMLInputElement).value).toBe('test@example.com');
    expect((passwordInput.element as HTMLInputElement).value).toBe('password123');
  });

  it('shows error message on failed login', async () => {
    const errorMsg = 'Invalid credentials';
    (authService.login as any).mockRejectedValue({
      response: { data: errorMsg }
    });

    const wrapper = mount(Login, {
      global: {
        stubs: ['router-link']
      }
    });

    await wrapper.find('form').trigger('submit');

    expect(authService.login).toHaveBeenCalled();
    // Wait for the next tick for the error to be rendered
    await wrapper.vm.$nextTick();
    expect(wrapper.find('.error').text()).toBe(errorMsg);
  });

  it('successfully logs in and redirects', async () => {
    const mockResponse = {
      token: 'fake-jwt',
      username: 'testuser',
      email: 'test@example.com'
    };
    (authService.login as any).mockResolvedValue(mockResponse);

    const wrapper = mount(Login, {
      global: {
        stubs: ['router-link']
      }
    });

    await wrapper.find('input[type="email"]').setValue('test@example.com');
    await wrapper.find('input[type="password"]').setValue('password123');
    await wrapper.find('form').trigger('submit');

    expect(authService.login).toHaveBeenCalledWith({
      email: 'test@example.com',
      password: 'password123'
    });

    expect(localStorage.setItem).toHaveBeenCalledWith('token', 'fake-jwt');
    expect(localStorage.setItem).toHaveBeenCalledWith('user', JSON.stringify({
      username: 'testuser',
      email: 'test@example.com'
    }));

    expect(mockPush).toHaveBeenCalledWith('/');
  });

  it('disables login button when loading', async () => {
    (authService.login as any).mockReturnValue(new Promise(() => {})); // Never resolves to keep loading state

    const wrapper = mount(Login, {
      global: {
        stubs: ['router-link']
      }
    });

    await wrapper.find('form').trigger('submit');

    expect(wrapper.find('button').element.hasAttribute('disabled')).toBe(true);
  });
});
