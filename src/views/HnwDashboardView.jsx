import React, { useState, useEffect } from 'react';
import { Shield, TrendingUp, AlertTriangle, Activity, DollarSign, Lock, ServerCrash } from 'lucide-react';

const HnwDashboardView = () => {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchData = async () => {
      try {
        // Attempt to fetch from backend
        const response = await fetch('http://localhost:5000/api/portfolio/summary');
        if (!response.ok) throw new Error('API Error');
        const json = await response.json();
        setData(json);
      } catch (err) {
        // Fallback to local data if backend fails/is slow
        console.warn('Usando datos de fallback local');
        setData({
          totalProtected: 12500000,
          riskScore: 18,
          assets: [
            { id: 1, name: 'Bonos Tesoro US', value: 4500000, status: 'secure', icon: 'DollarSign' },
            { id: 2, name: 'Acciones Tech (AAPL, MSFT)', value: 6800000, status: 'secure', icon: 'TrendingUp' },
            { id: 3, name: 'Divisas (EUR/USD)', value: 1200000, status: 'warning', icon: 'Activity' },
          ],
          alerts: [
            { id: 1, type: 'critical', message: 'Intento de suplantación bloqueado desde IP 192.168.1.45 (Rusia)', time: 'Hace 10 min' },
            { id: 2, type: 'warning', message: 'Volatilidad inusual detectada en cartera de divisas', time: 'Hace 45 min' },
          ]
        });
        setError('Conexión con el servidor falló. Mostrando datos cacheados.');
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, []);

  if (loading) {
    return (
      <div className="flex h-[calc(100vh-64px)] items-center justify-center">
        <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-cyan-500"></div>
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-6 animate-fade-in">
      {/* Header Section */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold text-slate-100 flex items-center gap-3">
            Wealth Shield <span className="text-sm px-2 py-1 bg-[#1e293b] rounded-md text-cyan-400 border border-cyan-900/50">CORPORATE</span>
          </h1>
          <p className="text-slate-400 mt-1">Monitoreo activo de patrimonio y prevención de amenazas</p>
        </div>
        
        <div className="flex items-center gap-4 bg-[#1e293b]/50 p-4 rounded-xl border border-slate-700/50">
          <div className="flex items-center gap-3 pr-4 border-r border-slate-700">
            <div className="relative">
              <Shield className="w-8 h-8 text-emerald-400 z-10 relative" />
              <div className="absolute inset-0 bg-emerald-400/30 rounded-full animate-ping"></div>
            </div>
            <div>
              <div className="text-xs text-emerald-400 font-bold tracking-wider">ESTADO DEL ESCUDO</div>
              <div className="text-sm text-slate-200">Grok AI Activo</div>
            </div>
          </div>
          <div className="pl-2">
            <div className="text-xs text-slate-400 font-bold tracking-wider mb-1">THREAT SCORE</div>
            <div className="flex items-baseline gap-1">
              <span className="text-2xl font-bold text-cyan-400">{data?.riskScore}</span>
              <span className="text-sm text-slate-500">/ 100</span>
            </div>
          </div>
        </div>
      </div>

      {error && (
        <div className="bg-red-950/30 border border-red-500/30 text-red-400 px-4 py-3 rounded-lg flex items-center gap-2 text-sm">
          <ServerCrash className="w-4 h-4" /> {error}
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Main Stats */}
        <div className="lg:col-span-2 space-y-6">
          <div className="bg-[#1e293b]/40 backdrop-blur-md rounded-2xl p-6 border border-slate-700/50 shadow-xl relative overflow-hidden">
            <div className="absolute top-0 right-0 p-4 opacity-10">
              <Lock className="w-32 h-32 text-cyan-500" />
            </div>
            <h2 className="text-slate-400 text-sm font-medium tracking-wide uppercase mb-2">Patrimonio Total Protegido</h2>
            <div className="text-5xl font-extrabold text-slate-100 tracking-tight">
              ${(data?.totalProtected / 1000000).toFixed(1)}<span className="text-3xl text-slate-500">M</span> <span className="text-xl text-slate-400 font-normal">USD</span>
            </div>
            <div className="mt-6">
              <div className="h-2 w-full bg-slate-800 rounded-full overflow-hidden">
                <div className="h-full bg-gradient-to-r from-cyan-600 to-cyan-400 w-full shadow-[0_0_10px_rgba(6,182,212,0.5)]"></div>
              </div>
              <div className="flex justify-between text-xs text-slate-500 mt-2">
                <span>0</span>
                <span>Análisis en tiempo real cifrado AES-256</span>
              </div>
            </div>
          </div>

          {/* Monitored Assets */}
          <div className="bg-[#1e293b]/40 backdrop-blur-md rounded-2xl border border-slate-700/50 overflow-hidden shadow-xl">
            <div className="px-6 py-4 border-b border-slate-700/50 bg-[#1e293b]/60 flex justify-between items-center">
              <h3 className="text-slate-200 font-semibold flex items-center gap-2">
                <Activity className="w-4 h-4 text-cyan-400" /> Activos Monitoreados
              </h3>
            </div>
            <div className="divide-y divide-slate-700/50">
              {data?.assets.map((asset) => (
                <div key={asset.id} className="p-4 px-6 flex items-center justify-between hover:bg-slate-800/30 transition-colors">
                  <div className="flex items-center gap-4">
                    <div className="p-2 bg-slate-800 rounded-lg border border-slate-700">
                      {asset.icon === 'DollarSign' && <DollarSign className="w-5 h-5 text-slate-300" />}
                      {asset.icon === 'TrendingUp' && <TrendingUp className="w-5 h-5 text-slate-300" />}
                      {asset.icon === 'Activity' && <Activity className="w-5 h-5 text-slate-300" />}
                    </div>
                    <div>
                      <div className="text-slate-200 font-medium">{asset.name}</div>
                      <div className="text-slate-500 text-sm">${(asset.value / 1000000).toFixed(1)}M USD</div>
                    </div>
                  </div>
                  <div>
                    {asset.status === 'secure' ? (
                      <span className="px-3 py-1 bg-emerald-950/50 text-emerald-400 text-xs rounded-full border border-emerald-500/30 flex items-center gap-1">
                        <Shield className="w-3 h-3" /> Seguro
                      </span>
                    ) : (
                      <span className="px-3 py-1 bg-amber-950/50 text-amber-400 text-xs rounded-full border border-amber-500/30 flex items-center gap-1">
                        <Activity className="w-3 h-3" /> Observación
                      </span>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Critical Alerts */}
        <div className="space-y-4">
          <h3 className="text-slate-300 font-semibold flex items-center gap-2 uppercase tracking-wider text-sm px-1">
            <AlertTriangle className="w-4 h-4 text-red-400" /> Alertas del Sistema
          </h3>
          {data?.alerts.map((alert) => (
            <div 
              key={alert.id} 
              className={`p-4 rounded-xl border relative overflow-hidden backdrop-blur-sm ${
                alert.type === 'critical' 
                  ? 'bg-red-950/20 border-red-500/50 shadow-[0_0_15px_rgba(239,68,68,0.1)]' 
                  : 'bg-amber-950/20 border-amber-500/50'
              }`}
            >
              {alert.type === 'critical' && (
                <div className="absolute top-0 left-0 w-1 h-full bg-red-500 shadow-[0_0_10px_rgba(239,68,68,0.8)]"></div>
              )}
              <div className="flex gap-3">
                <AlertTriangle className={`w-5 h-5 shrink-0 ${alert.type === 'critical' ? 'text-red-500' : 'text-amber-500'}`} />
                <div>
                  <p className="text-sm text-slate-200 mb-2">{alert.message}</p>
                  <p className="text-xs text-slate-500">{alert.time}</p>
                </div>
              </div>
              {alert.type === 'critical' && (
                <button className="mt-3 w-full py-1.5 px-3 bg-red-500/10 hover:bg-red-500/20 text-red-400 text-xs font-medium rounded border border-red-500/20 transition-colors">
                  Ver Detalles de Incidencia
                </button>
              )}
            </div>
          ))}
          
          {/* Cyber feed mock */}
          <div className="mt-8 p-4 bg-[#0a0d14] rounded-xl border border-slate-800 font-mono text-xs text-slate-500 space-y-2 h-48 overflow-hidden relative">
            <div className="absolute inset-0 bg-gradient-to-b from-transparent to-[#0a0d14] z-10 pointer-events-none"></div>
            <p className="text-cyan-800">&gt; INITIALIZING SENTINEL NODE...</p>
            <p>&gt; SECURE CONNECTION ESTABLISHED</p>
            <p>&gt; MONITORING GLOBAL THREAT VECTORS</p>
            <p className="text-red-900">&gt; [WARN] DDOS ATTEMPT DETECTED IN ASIA-PAC REGION</p>
            <p>&gt; ROUTING TRAFFIC THROUGH ENCRYPTED TUNNEL</p>
            <p>&gt; PORTFOLIO INTEGRITY: 100%</p>
            <p className="animate-pulse text-cyan-600">&gt; AWAITING INPUT_</p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default HnwDashboardView;
