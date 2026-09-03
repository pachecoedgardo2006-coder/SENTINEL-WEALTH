# SentinelWealth

- `backend/` — API .NET (`http://localhost:5088`)
- `frontend/` — Vite/React (`http://localhost:5000`)
- `.env` — `GEMINI_API_KEY` y `VITE_API_URL` (no se sube a git)

```bash
# terminal 1
cd backend && dotnet run --launch-profile http

# terminal 2
cd frontend && npm install && npm run dev
```

Abre http://localhost:5000 y el escáner en http://localhost:5000/scanner
