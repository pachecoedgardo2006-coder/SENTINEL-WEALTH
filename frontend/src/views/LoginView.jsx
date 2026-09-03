import React, { useState } from 'react';
import { Shield } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { login } from '../api/sentinel';
import { setSession } from '../api/session';

const LoginView = () => {
  const navigate = useNavigate();
  const [username, setUsername] = useState('restrepo');
  const [password, setPassword] = useState('demo123');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const submit = async (event) => {
    event.preventDefault();
    setError('');
    setLoading(true);
    try {
      const user = await login(username, password);
      setSession(user);
      navigate('/');
    } catch {
      setError('Usuario o contraseña incorrectos.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-[#0b0f19] text-slate-200 flex items-center justify-center px-4">
      <form onSubmit={submit} className="w-full max-w-md bg-[#1e293b]/60 border border-slate-700/50 rounded-2xl p-8 space-y-5">
        <div className="flex items-center gap-3 mb-2">
          <div className="w-10 h-10 rounded-full bg-gradient-to-tr from-cyan-500 to-blue-600 flex items-center justify-center">
            <Shield className="w-5 h-5 text-white" />
          </div>
          <div>
            <div className="text-lg font-bold tracking-wider">SENTINELWEALTH</div>
            <div className="text-xs text-slate-400">Login demo — sin seguridad real</div>
          </div>
        </div>

        <label className="block text-sm text-slate-400">
          Usuario
          <input
            className="mt-1 w-full bg-[#0b0f19] border border-slate-700 rounded-xl px-3 py-2 text-slate-100"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
          />
        </label>
        <label className="block text-sm text-slate-400">
          Contraseña
          <input
            type="password"
            className="mt-1 w-full bg-[#0b0f19] border border-slate-700 rounded-xl px-3 py-2 text-slate-100"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />
        </label>

        {error && <p className="text-sm text-red-400">{error}</p>}

        <button
          type="submit"
          disabled={loading}
          className="w-full py-2.5 rounded-xl bg-cyan-600 hover:bg-cyan-500 text-white font-semibold disabled:opacity-50"
        >
          {loading ? 'Entrando…' : 'Entrar al Wealth Shield'}
        </button>

        <div className="text-xs text-slate-500 space-y-1 pt-2 border-t border-slate-700/50">
          <p><b className="text-slate-300">restrepo</b> / demo123 — Family Office USD 12.5M</p>
          <p><b className="text-slate-300">mejia</b> / demo123 — HNW Medellín USD 3.2M</p>
          <p>El escáner ciudadano no pide login: /scanner</p>
        </div>
      </form>
    </div>
  );
};

export default LoginView;
