<template>
  <div class="auth-container">
    <h2>Register</h2>
    <form v-if="!state.success" @submit.prevent="handleRegister">
      <div class="form-group">
        <label>Username</label>
        <input v-model="user.username" type="text" required placeholder="Enter username" />
      </div>
      <div class="form-group">
        <label>Email</label>
        <input v-model="user.email" type="email" required placeholder="Enter email" />
      </div>
      <div class="form-group">
        <label>Password</label>
        <input v-model="user.password" type="password" required placeholder="Enter password" />
      </div>
      <button type="submit" :disabled="state.loading">Register</button>
      <p v-if="state.error" class="error">{{ state.error }}</p>
      <p>
        Already have an account? 
        <router-link to="/login">Login here</router-link>
      </p>
    </form>
    <div v-else>
        <p class="success">Registration successful! You can now login.</p>
        <router-link to="/login">Go to Login</router-link>
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive } from 'vue';
import { authService } from '../services/AuthService';
import type { UserRegisterContract } from '../contracts/AuthContract';
import { createInitialAuthState } from '../view-states/AuthViewState';

const user = reactive<UserRegisterContract>({
  username: '',
  email: '',
  password: ''
});

const state = reactive(createInitialAuthState());

const handleRegister = async () => {
  state.loading = true;
  state.error = null;
  try {
    await authService.register(user);
    state.success = true;
  } catch (err: any) {
    state.error = err.response?.data || 'Registration failed. Please try again.';
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
.success {
  color: green;
  margin-bottom: 10px;
}
</style>
