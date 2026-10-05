import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';

// /api isteklerini backend'e yönlendirir; böylece API'de CORS ayarı gerekmez.
// Farklı port için: VITE_API_TARGET=http://localhost:5000 npm run dev
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');
  return {
    plugins: [react()],
    server: { proxy: { '/api': { target: env.VITE_API_TARGET || 'http://localhost:8080', changeOrigin: true } } },
  };
});
