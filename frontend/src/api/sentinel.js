import { authHeaders } from './session';

const API_BASE = (import.meta.env.VITE_API_URL || 'http://localhost:5088').replace(/\/$/, '');

async function request(path, options = {}) {
  const response = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers: {
      ...(options.headers ?? {}),
    },
  });
  if (!response.ok) {
    throw new Error(`API ${response.status}`);
  }
  return response.json();
}

export async function login(username, password) {
  return request('/api/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username, password }),
  });
}

export async function getPortfolioSummary() {
  const json = await request('/api/portfolio/summary', {
    headers: authHeaders(),
  });
  const assets = (json.assets ?? []).map((asset, index) => ({
    id: asset.id ?? index,
    name: asset.name,
    value: Number(asset.value ?? 0),
    status: String(asset.status ?? '').toLowerCase().includes('volatil')
      || String(asset.status ?? '').toLowerCase().includes('overlay')
      || String(asset.status ?? '').toLowerCase().includes('observ')
      ? 'warning'
      : 'secure',
    icon: iconForAsset(asset),
  }));

  const alerts = (json.recentAlerts ?? json.alerts ?? []).map((alert, index) => ({
    id: alert.id ?? index,
    type: String(alert.severity ?? alert.type ?? '').toLowerCase().includes('crít')
      || String(alert.severity ?? '').toLowerCase().includes('crit')
      ? 'critical'
      : 'warning',
    message: alert.title
      ? `${alert.title}${alert.description ? ` — ${alert.description}` : ''}`
      : (alert.message ?? alert.description ?? ''),
    time: formatAlertTime(alert.detectedAt) || alert.time || '',
  }));

  return {
    totalProtected: Number(json.totalNetWorth ?? json.totalProtected ?? 0),
    riskScore: Number(json.riskNumeric ?? json.riskScore ?? 0),
    clientName: json.clientName,
    domicile: json.domicile,
    tier: json.tier,
    assets,
    alerts,
  };
}

export async function analyzeScan(text) {
  const json = await request('/api/scanner/analyze', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ inputType: 'text', content: text, text }),
  });

  return {
    score: Number(json.threatScore ?? json.score ?? 0),
    verdict: json.verdict,
    summary: json.grokAnalysisSummary ?? json.summary ?? '',
    flags: json.flags ?? [],
    recommendation: json.recommendedAction ?? json.recommendation ?? '',
    engine: json.analysisEngine,
  };
}

function iconForAsset(asset) {
  const haystack = `${asset.class ?? ''} ${asset.name ?? ''}`.toLowerCase();
  if (haystack.includes('equity') || haystack.includes('variable') || haystack.includes('tech')) {
    return 'TrendingUp';
  }
  if (haystack.includes('fx') || haystack.includes('divisa') || haystack.includes('liquidez')) {
    return 'Activity';
  }
  return 'DollarSign';
}

function formatAlertTime(iso) {
  if (!iso) {
    return '';
  }
  const then = new Date(iso);
  if (Number.isNaN(then.getTime())) {
    return '';
  }
  const minutes = Math.max(1, Math.round((Date.now() - then.getTime()) / 60000));
  if (minutes < 60) {
    return `Hace ${minutes} min`;
  }
  return then.toLocaleString('es-CO');
}
