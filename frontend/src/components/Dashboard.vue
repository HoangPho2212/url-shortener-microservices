<template>
  <div class="dashboard">
    <header class="dashboard-header">
      <h1>URL Shortener Dashboard</h1>
      <button @click="handleLogout" class="btn-logout">Logout</button>
    </header>

    <div class="shorten-section card">
      <h2>Shorten a New URL</h2>
      <form @submit.prevent="handleShorten" class="shorten-form">
        <input 
          v-model="state.newUrl" 
          type="url" 
          placeholder="Paste your long URL here..." 
          required 
          :disabled="state.isShortening"
        />
        <button type="submit" :disabled="state.isShortening">
          {{ state.isShortening ? 'Shortening...' : 'Shorten' }}
        </button>
      </form>
      <p v-if="state.error" class="error-message">{{ state.error }}</p>
    </div>

    <div class="urls-section card">
      <h2>Your Shortened URLs</h2>
      <div v-if="state.isLoading" class="loading">Loading your URLs...</div>
      <div v-else-if="state.urls.length === 0" class="empty-state">
        No URLs shortened yet. Start by pasting a link above!
      </div>
      <div v-else class="table-container">
        <table>
          <thead>
            <tr>
              <th>Original URL</th>
              <th>Short URL</th>
              <th>Clicks</th>
              <th>Created At</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="url in state.urls" :key="url.id">
              <td class="url-cell" :title="url.originalUrl">
                <a :href="url.originalUrl" target="_blank">{{ truncate(url.originalUrl, 50) }}</a>
              </td>
              <td class="short-url-cell">
                <a :href="url.fullShortUrl" target="_blank" class="short-link">{{ url.shortUrl }}</a>
                <button @click="copyToClipboard(url.fullShortUrl)" class="btn-copy">Copy</button>
              </td>
              <td class="clicks-cell">{{ url.clicks }}</td>
              <td class="date-cell">{{ formatDate(url.createdAt) }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import UrlService from '../services/UrlService';
import { initialDashboardViewState } from '../view-states/DashboardViewState';
import { logout } from '../services/api';

const router = useRouter();
const state = reactive({ ...initialDashboardViewState });

const fetchUrls = async () => {
  state.isLoading = true;
  state.error = null;
  try {
    state.urls = await UrlService.getMyUrls();
  } catch (err: any) {
    state.error = 'Failed to load URLs. Please try again.';
    console.error(err);
  } finally {
    state.isLoading = false;
  }
};

const handleShorten = async () => {
  if (!state.newUrl) return;
  
  state.isShortening = true;
  state.error = null;
  try {
    await UrlService.shortenUrl(state.newUrl);
    state.newUrl = '';
    await fetchUrls(); // Refresh the list
  } catch (err: any) {
    state.error = 'Failed to shorten URL. Make sure it is a valid link.';
    console.error(err);
  } finally {
    state.isShortening = false;
  }
};

const handleLogout = () => {
  logout();
  router.push('/login');
};

const copyToClipboard = (text: string) => {
  navigator.clipboard.writeText(text);
  alert('Copied to clipboard!');
};

const truncate = (text: string, length: number) => {
  return text.length > length ? text.substring(0, length) + '...' : text;
};

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString(undefined, {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  });
};

onMounted(fetchUrls);
</script>

<style scoped>
.dashboard {
  max-width: 1000px;
  margin: 2rem auto;
  padding: 0 1rem;
}

.dashboard-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

h1 {
  color: #2c3e50;
  margin: 0;
}

.card {
  background: white;
  border-radius: 8px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  padding: 1.5rem;
  margin-bottom: 2rem;
}

h2 {
  margin-top: 0;
  margin-bottom: 1.5rem;
  font-size: 1.25rem;
  color: #34495e;
}

.shorten-form {
  display: flex;
  gap: 1rem;
}

input[type="url"] {
  flex: 1;
  padding: 0.75rem 1rem;
  border: 1px solid #dcdfe6;
  border-radius: 4px;
  font-size: 1rem;
}

button {
  padding: 0.75rem 1.5rem;
  background-color: #42b983;
  color: white;
  border: none;
  border-radius: 4px;
  font-weight: bold;
  cursor: pointer;
  transition: background-color 0.3s;
}

button:hover:not(:disabled) {
  background-color: #3aa876;
}

button:disabled {
  background-color: #a8d8c0;
  cursor: not-allowed;
}

.btn-logout {
  background-color: #e74c3c;
}

.btn-logout:hover {
  background-color: #c0392b;
}

.btn-copy {
  padding: 0.25rem 0.5rem;
  font-size: 0.8rem;
  margin-left: 0.5rem;
  background-color: #3498db;
}

.error-message {
  color: #e74c3c;
  margin-top: 1rem;
  font-size: 0.9rem;
}

.table-container {
  overflow-x: auto;
}

table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
}

th {
  background-color: #f8f9fa;
  padding: 1rem;
  border-bottom: 2px solid #dee2e6;
  color: #495057;
  font-weight: 600;
}

td {
  padding: 1rem;
  border-bottom: 1px solid #dee2e6;
  vertical-align: middle;
}

.url-cell a {
  color: #3498db;
  text-decoration: none;
}

.short-link {
  font-weight: bold;
  color: #2c3e50;
  text-decoration: none;
}

.clicks-cell {
  text-align: center;
  font-weight: bold;
}

.date-cell {
  color: #6c757d;
  font-size: 0.85rem;
}

.empty-state, .loading {
  text-align: center;
  padding: 3rem;
  color: #95a5a6;
  font-style: italic;
}
</style>
