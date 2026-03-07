<template>
  <div id="app">
    <nav v-if="!isAuthPage">
      <router-link to="/">Home</router-link> |
      <template v-if="!isLoggedIn">
        <router-link to="/login">Login</router-link> |
        <router-link to="/register">Register</router-link>
      </template>
      <template v-else>
        <span>Welcome, {{ user?.username }}</span> |
        <a href="#" @click.prevent="onLogout">Logout</a>
      </template>
    </nav>

    <div v-if="isHomePage">
      <h1>User List (Protected)</h1>
      <button @click="fetchUsers">Refresh Users</button>
      <ul v-if="users.length">
        <li v-for="u in users" :key="u.id">
          {{ u.username }} ({{ u.email }})
        </li>
      </ul>
      <p v-else-if="loading">Loading...</p>
      <p v-else-if="error" class="error">{{ error }}</p>
    </div>

    <router-view></router-view>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import api, { logout } from './services/api';

const route = useRoute();
const router = useRouter();
const users = ref([]);
const loading = ref(false);
const error = ref('');

const isLoggedIn = computed(() => !!localStorage.getItem('token'));
const user = computed(() => {
  const userData = localStorage.getItem('user');
  return userData ? JSON.parse(userData) : null;
});

const isAuthPage = computed(() => ['Login', 'Register'].includes(route.name));
const isHomePage = computed(() => route.name === 'Home');

const onLogout = () => {
  logout();
  router.push('/login');
};

const fetchUsers = async () => {
  if (!isLoggedIn.value) return;
  loading.value = true;
  error.value = '';
  try {
    const response = await api.get('/users');
    users.value = response.data;
  } catch (err) {
    error.value = 'Failed to fetch users. ' + (err.response?.status === 401 ? 'Unauthorized.' : '');
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  if (isHomePage.value) {
    fetchUsers();
  }
});

// Re-fetch when navigating back to home
watch(() => route.name, (newName) => {
  if (newName === 'Home') {
    fetchUsers();
  }
});
</script>

<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  margin-top: 60px;
}
nav {
  padding: 30px;
}
nav a {
  font-weight: bold;
  color: #2c3e50;
  margin: 0 10px;
  text-decoration: none;
}
nav a.router-link-exact-active {
  color: #42b983;
}
.error {
  color: red;
}
ul {
  list-style-type: none;
  padding: 0;
}
li {
  padding: 10px;
  border-bottom: 1px solid #eee;
}
</style>
