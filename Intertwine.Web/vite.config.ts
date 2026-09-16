import vue from '@vitejs/plugin-vue'
import { defineConfig, loadEnv } from 'vite'

// https://vite.dev/config/
export default defineConfig(({ mode }) => ({
  plugins: [vue()],
  server: {
    host: '127.0.0.1',
    proxy: {
      '/api': {
        target: loadEnv(mode, process.cwd(), '').API_PROXY_TARGET || 'https://localhost:7279',
        changeOrigin: true,
        // The ASP.NET Core development certificate is trusted by the browser,
        // but Node.js uses its own certificate trust store for proxy requests.
        secure: false,
      },
    },
  },
}))
