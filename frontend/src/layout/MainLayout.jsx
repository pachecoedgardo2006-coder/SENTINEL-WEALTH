import React from 'react';
import { Outlet } from 'react-router-dom';
import Navbar from '../components/Navbar';

const MainLayout = () => {
  return (
    <div className="min-h-screen bg-[#0b0f19] text-slate-200 font-sans selection:bg-cyan-500/30">
      {/* Background ambient glow */}
      <div className="fixed inset-0 z-0 overflow-hidden pointer-events-none">
        <div className="absolute top-[-10%] left-[-10%] w-[40%] h-[40%] bg-cyan-900/20 rounded-full blur-[120px]"></div>
        <div className="absolute bottom-[-10%] right-[-10%] w-[40%] h-[40%] bg-emerald-900/10 rounded-full blur-[120px]"></div>
      </div>

      {/* Main Content */}
      <div className="relative z-10 flex flex-col min-h-screen">
        <Navbar />
        
        <main className="flex-grow">
          <Outlet />
        </main>

        <footer className="py-6 text-center text-slate-500 text-xs border-t border-slate-800/50 mt-auto bg-[#0b0f19]/80 backdrop-blur-sm">
          <p>© 2026 SentinelWealth AI. Powered by xAI Grok & .NET 10.</p>
        </footer>
      </div>
    </div>
  );
};

export default MainLayout;
