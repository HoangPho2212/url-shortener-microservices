<template>
  <div class="auth-container">
    <h2>Login</h2>
    <form @submit.prevent="handleLogin">
      <div class="form-group">
        <label>Email</label>
        <input v-model="credentials.email" type="email" required placeholder="Enter email" />
      </div>
      <div class="form-group">
        <label>Password</label>
        <input v-model="credentials.password" type="password" required placeholder="Enter password" />
      </div>
      <button type="submit" :disabled="state.loading">Login</button>
      <p v-if="state.error" class="error">{{ state.error }}</p>
      <p>
        Don't have an account? 
        <router-link to="/register">Register here</router-link>
      </p>
    </form>
  </div>
</template>

<script setup lang="ts">
import { reactive } from 'vue';
import { useRouter } from 'vue-router';
import { authService } from '../services/AuthService';
import type { UserLoginContract } from '../contracts/AuthContract';
import { createInitialAuthState } from '../view-states/AuthViewState';

const router = useRouter();
const credentials = reactive<UserLoginContract>({
  email: '',
  password: ''
});

const state = reactive(createInitialAuthState());

const handleLogin = async () => {
  state.loading = true;
  state.error = null;
  try {
    const data = await authService.login(credentials);
    
    localStorage.setItem('token', data.token);
    localStorage.setItem('user', JSON.stringify({
      username: data.username,
      email: data.email
    }));
    
    router.push('/');
  } catch (err: any) {
    state.error = err.response?.data || 'Login failed. Please check your credentials.';
  } finally {
    state.loading = false;
  }
};
</script>

<style scoped>
.auth-container {
  max-width: 400px;
  margin: 50px auto;
  padding: 20px;
  border: 1px solid #ddd;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}
.form-group {
  margin-bottom: 15px;
}
label {
  display: block;
  margin-bottom: 5px;
}
input {
  width: 100%;
  padding: 8px;
  box-sizing: border-box;
  border: 1px solid #ccc;
  border-radius: 4px;
}
button {
  width: 100%;
  padding: 10px;
  background-color: #42b983;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}
button:disabled {
  background-color: #a5d6a7;
}
.error {
  color: red;
  margin-top: 10px;
}
</style>
