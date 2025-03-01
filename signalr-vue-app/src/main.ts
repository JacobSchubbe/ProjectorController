import { createApp } from 'vue';
import App from './ProjectorApp.vue';
import axios from 'axios';
import './Logger'; // Ensure Logger.ts is imported

const app = createApp(App);
app.config.globalProperties.$axios = axios;
app.mount('#app');