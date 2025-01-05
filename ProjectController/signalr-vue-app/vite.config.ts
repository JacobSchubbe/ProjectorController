import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';

export default defineConfig({
    plugins: [vue()],
    // Enable TypeScript support
    resolve: {
        alias: {
            '@': '/src', // Adjust as needed for your project structure
        },
    },
});
