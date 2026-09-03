import React from 'react';
import { Shield, Radar } from 'lucide-react';
import { NavLink } from 'react-router-dom';

const Navbar = () => {
  return (
    <nav className="bg-[#1e293b]/80 backdrop-blur-md border-b border-cyan-900/50 sticky top-0 z-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex items-center justify-between h-16">
          <div className="flex items-center gap-3">
            <div className="w-8 h-8 rounded-full bg-gradient-to-tr from-cyan-500 to-blue-600 flex items-center justify-center shadow-[0_0_15px_rgba(6,182,212,0.5)]">
              <Shield className="w-5 h-5 text-white" />
            </div>
            <span className="text-xl font-bold bg-clip-text text-transparent bg-gradient-to-r from-cyan-100 to-cyan-400 tracking-wider">
              SENTINEL<span className="font-light">WEALTH</span> AI
            </span>
          </div>
          
          <div className="flex space-x-4">
            <NavLink
              to="/"
              className={({ isActive }) => `flex items-center gap-2 px-4 py-2 rounded-md text-sm font-medium transition-all duration-300 ${
                isActive
                  ? 'bg-cyan-950/50 text-cyan-400 border border-cyan-500/30 shadow-[0_0_10px_rgba(6,182,212,0.2)]'
                  : 'text-slate-400 hover:text-cyan-300 hover:bg-slate-800/50'
              }`}
            >
              <Shield className="w-4 h-4" />
              Wealth Shield HNW
            </NavLink>
            <NavLink
              to="/scanner"
              className={({ isActive }) => `flex items-center gap-2 px-4 py-2 rounded-md text-sm font-medium transition-all duration-300 ${
                isActive
                  ? 'bg-emerald-950/50 text-emerald-400 border border-emerald-500/30 shadow-[0_0_10px_rgba(16,185,129,0.2)]'
                  : 'text-slate-400 hover:text-emerald-300 hover:bg-slate-800/50'
              }`}
            >
              <Radar className="w-4 h-4" />
              Escáner Ciudadano Anti-Fraude
            </NavLink>
          </div>
        </div>
      </div>
    </nav>
  );
};

export default Navbar;
