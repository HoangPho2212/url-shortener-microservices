import { describe, it, expect, vi, beforeEach } from 'vitest';
import { mount } from '@vue/test-utils';
import Register from '../Register.vue';
import { authService } from '../../services/AuthService';

vi.mock('../../services/AuthService', () => ({
  authService: {
    register: vi.fn()
  }
}));

describe('Register.vue', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders registration form correctly', () => {
    const wrapper = mount(Register, {
      global: {
        stubs: ['router-link']
      }
    });
    expect(wrapper.find('h2').text()).toBe('Register');
    expect(wrapper.find('input[placeholder="Enter username"]').exists()).toBe(true);
    expect(wrapper.find('input[type="email"]').exists()).toBe(true);
    expect(wrapper.find('input[type="password"]').exists()).toBe(true);
    expect(wrapper.find('button[type="submit"]').text()).toBe('Register');
  });

  it('shows error message on failed registration', async () => {
    const errorMsg = 'Email already exists';
    (authService.register as any).mockRejectedValue({
      response: { data: errorMsg }
    });

    const wrapper = mount(Register, {
      global: {
        stubs: ['router-link']
      }
    });

    await wrapper.find('form').trigger('submit');

    expect(authService.register).toHaveBeenCalled();
    await wrapper.vm.$nextTick();
    expect(wrapper.find('.error').text()).toBe(errorMsg);
  });

  it('successfully registers and shows success message', async () => {
    (authService.register as any).mockResolvedValue({});

    const wrapper = mount(Register, {
      global: {
        stubs: ['router-link']
      }
    });

    await wrapper.find('input[placeholder="Enter username"]').setValue('newuser');
    await wrapper.find('input[type="email"]').setValue('newuser@example.com');
    await wrapper.find('input[type="password"]').setValue('password123');
    await wrapper.find('form').trigger('submit');

    expect(authService.register).toHaveBeenCalledWith({
      username: 'newuser',
      email: 'newuser@example.com',
      password: 'password123'
    });

    await wrapper.vm.$nextTick();
    expect(wrapper.find('.success').text()).toContain('Registration successful');
    expect(wrapper.find('form').exists()).toBe(false);
  });

  it('disables register button when loading', async () => {
    (authService.register as any).mockReturnValue(new Promise(() => {}));

    const wrapper = mount(Register, {
      global: {
        stubs: ['router-link']
      }
    });

    await wrapper.find('form').trigger('submit');

    expect(wrapper.find('button').element.hasAttribute('disabled')).toBe(true);
  });
});
