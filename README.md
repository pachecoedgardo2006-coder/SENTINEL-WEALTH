# SentinelWealth.Api

API ASP.NET Core (.NET 10) para el dashboard HNW y el escáner ciudadano anti-estafas. CORS está abierto para que el frontend (Vite/Next/React) pueda consumirla desde cualquier origen.

**Base URL local:** `http://localhost:5088`

OpenAPI: `http://localhost:5088/openapi/v1.json`

## Arranque (backend)

```bash
cp appsettings.Local.example.json appsettings.Local.json
# edita appsettings.Local.json y pega GEMINI_API_KEY
dotnet run
```

Alternativa:

```bash
export GEMINI_API_KEY="tu_key"
dotnet run
```

Modelo: **gemini-3.5-flash-lite**. Sin key, el escáner responde con el motor heurístico (la demo no se cae).

`appsettings.Local.json` **no se sube a GitHub**.

## Contrato para el frontend

JSON en camelCase. No hay autenticación en esta versión (hackathon).

### `GET /api/health`

```json
{ "status": "ok", "service": "SentinelWealth AI", "runtime": "NET 10", "utc": "..." }
```

### `GET /api/portfolio/summary`

Cartera mock protegida (~USD 12.5M).

| Campo | Tipo |
|---|---|
| totalNetWorth | number |
| currency | string (`USD`) |
| riskScore | string |
| riskNumeric | number |
| clientName | string |
| tier | string |
| domicile | string |
| shieldStatus | string |
| assets | `Asset[]` |
| recentAlerts | `Alert[]` |

**Asset:** `id`, `name`, `class`, `value`, `allocationPercent`, `currency`, `status`, `ticker?`, `dayChangePercent?`, `custodian?`

**Alert:** `id`, `type`, `severity`, `title`, `description`, `status`, `detectedAt`, `analyzedBy?`, `amountUsd?`, `sourceIp?`, `recommendedAction?`

### `POST /api/scanner/analyze`

```json
{ "inputType": "text", "content": "mensaje o URL o cláusula" }
```

`inputType`: `text` | `url` | `contract`

**200**

```json
{
  "id": "SCN-5042",
  "threatScore": 86,
  "verdict": "Alto Riesgo — ...",
  "grokAnalysisSummary": "2-4 frases",
  "flags": ["señal 1", "señal 2"],
  "recommendedAction": "acción concreta",
  "analysisEngine": "gemini",
  "inputType": "text"
}
```

`analysisEngine`: `gemini` | `grok` | `heuristic-fallback`

**400** si `content` está vacío o `inputType` no es válido.

## Ejemplo fetch

```ts
const API = "http://localhost:5088";

export async function getPortfolio() {
  const res = await fetch(`${API}/api/portfolio/summary`);
  if (!res.ok) throw new Error("portfolio failed");
  return res.json();
}

export async function analyzeScan(inputType: "text" | "url" | "contract", content: string) {
  const res = await fetch(`${API}/api/scanner/analyze`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ inputType, content }),
  });
  if (!res.ok) throw new Error("scan failed");
  return res.json();
}
```

## Threat score (UI)

- `0–29` riesgo bajo
- `30–59` medio
- `60–79` alto
- `80–100` crítico
