import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    port: 5000,
    open: true, // Auto abre el navegador al hacer npm run dev
    strictPort: true, // Falla si el puerto 5000 está ocupado
  }
})
