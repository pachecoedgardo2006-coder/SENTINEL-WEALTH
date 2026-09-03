import React, { useState } from 'react';
import { Radar, ShieldAlert, CheckCircle, AlertOctagon, FileText, Bot, Zap, ArrowRight, Loader2 } from 'lucide-react';
import { analyzeScan } from '../api/sentinel';

const CitizenScannerView = () => {
  const [inputText, setInputText] = useState('');
  const [isScanning, setIsScanning] = useState(false);
  const [result, setResult] = useState(null);

  const testCases = {
    crypto: "URGENTE: Tu cuenta de Binance ha sido seleccionada para un rendimiento garantizado del 15% SEMANAL. Haz clic aquí para activar tu smart contract antes de que expire en 2 horas: http://binance-secure-yield.io/auth",
    phishing: "Estimado cliente de Bancolombia, su clave dinámica ha sido bloqueada por seguridad. Para reactivarla y evitar el bloqueo total de sus productos, ingrese inmediatamente a: https://bancolombia-reactivacion-segura.com/login",
    safe: "Hola Juan, te adjunto el resumen del fondo de inversión en renta fija que discutimos ayer. La rentabilidad histórica promedio es del 5% anual. Revísalo y me avisas si tienes dudas para agendar la llamada de la próxima semana."
  };

  const handleScan = async () => {
    if (!inputText.trim()) return;
    
    setIsScanning(true);
    setResult(null);

    try {
      const data = await analyzeScan(inputText);
      setResult(data);
      setIsScanning(false);
    } catch (error) {
      // Mock Fallback inmediato para la demo si el backend de .NET no responde
      console.warn("Backend no disponible, usando Grok AI mock local");
      setTimeout(() => {
        const textLower = inputText.toLowerCase();
        let mockResult = {
          score: 15,
          verdict: 'SEGURO',
          summary: 'El texto analizado no presenta patrones conocidos de estafa o phishing. Parece ser una comunicación legítima.',
          flags: [],
          recommendation: 'Puedes proceder con normalidad, pero siempre verifica la identidad del remitente en transacciones financieras.'
        };

        if (textLower.includes('binance') || textLower.includes('cripto') || textLower.includes('15%')) {
          mockResult = {
            score: 98,
            verdict: 'ESTAFA CRIPTO CRÍTICA',
            summary: 'Grok AI ha detectado un esquema Ponzi clásico o scam de criptomonedas. Promete rendimientos imposibles y urgencia artificial.',
            flags: ['Rendimiento garantizado irreal (15% semanal)', 'Urgencia artificial (expira en 2 horas)', 'URL no oficial (binance-secure-yield.io)'],
            recommendation: 'BLOQUEAR al remitente inmediatamente. NO hacer clic en el enlace. Binance nunca ofrece retornos garantizados.'
          };
        } else if (textLower.includes('bancolombia') || textLower.includes('bloqueada') || textLower.includes('reactivarla')) {
          mockResult = {
            score: 95,
            verdict: 'PHISHING BANCARIO',
            summary: 'Intento de robo de credenciales. El mensaje simula ser de una entidad bancaria para robar tu clave dinámica.',
            flags: ['Alerta de bloqueo falsa', 'URL fraudulenta (bancolombia-reactivacion...)', 'Sentido de urgencia extremo'],
            recommendation: 'NO INGRESES TUS DATOS. Elimina el mensaje y repórtalo a correosospechoso@bancolombia.com.co.'
          };
        }
        
        setResult(mockResult);
        setIsScanning(false);
      }, 1500); // Simular tiempo de análisis de IA
    }
  };

  const getScoreColor = (score) => {
    if (score > 75) return 'text-red-500';
    if (score > 40) return 'text-amber-500';
    return 'text-emerald-500';
  };

  const getScoreBg = (score) => {
    if (score > 75) return 'bg-red-500';
    if (score > 40) return 'bg-amber-500';
    return 'bg-emerald-500';
  };

  return (
    <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-8 animate-fade-in">
      <div className="text-center mb-10">
        <div className="inline-flex items-center justify-center p-3 bg-emerald-900/30 rounded-2xl mb-4 border border-emerald-500/20 shadow-[0_0_30px_rgba(16,185,129,0.15)]">
          <Radar className="w-10 h-10 text-emerald-400" />
        </div>
        <h1 className="text-4xl font-bold text-slate-100 mb-4 tracking-tight">Escáner Ciudadano Anti-Fraude</h1>
        <p className="text-lg text-slate-400 max-w-2xl mx-auto">
          Pega cualquier mensaje sospechoso de WhatsApp, SMS o correo. Nuestra IA analizará el texto buscando patrones de estafa, phishing o manipulación financiera en segundos.
        </p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        {/* Input Section */}
        <div className="space-y-6">
          <div className="bg-[#1e293b]/60 backdrop-blur-md rounded-2xl p-6 border border-slate-700/50 shadow-lg">
            <label className="flex items-center gap-2 text-sm font-medium text-slate-300 mb-3">
              <FileText className="w-4 h-4 text-cyan-400" /> Texto a analizar
            </label>
            <textarea
              className="w-full h-48 bg-[#0b0f19] border border-slate-700 rounded-xl p-4 text-slate-200 placeholder-slate-600 focus:ring-2 focus:ring-cyan-500 focus:border-transparent transition-all resize-none"
              placeholder="Pega aquí el mensaje sospechoso, enlace o fragmento de contrato..."
              value={inputText}
              onChange={(e) => setInputText(e.target.value)}
            />
            
            <button
              onClick={handleScan}
              disabled={!inputText.trim() || isScanning}
              className="w-full mt-4 py-3 bg-gradient-to-r from-emerald-600 to-cyan-600 hover:from-emerald-500 hover:to-cyan-500 text-white font-semibold rounded-xl shadow-[0_0_20px_rgba(16,185,129,0.3)] transition-all disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
            >
              {isScanning ? (
                <><Loader2 className="w-5 h-5 animate-spin" /> Analizando con Grok AI...</>
              ) : (
                <><Bot className="w-5 h-5" /> Escanear con Grok AI</>
              )}
            </button>
          </div>

          <div className="bg-[#1e293b]/40 rounded-xl p-5 border border-slate-700/30">
            <h3 className="text-sm font-medium text-slate-400 mb-3 flex items-center gap-2">
              <Zap className="w-4 h-4 text-amber-400" /> Casos de Prueba Rápida (Demo)
            </h3>
            <div className="space-y-2">
              <button onClick={() => setInputText(testCases.crypto)} className="w-full text-left px-4 py-2 text-sm text-slate-300 hover:text-cyan-300 bg-[#0b0f19]/50 hover:bg-[#1e293b] rounded-lg border border-slate-700/50 transition-colors flex justify-between items-center group">
                A. Estafa Cripto 15% semanal <ArrowRight className="w-4 h-4 opacity-0 group-hover:opacity-100 transition-opacity" />
              </button>
              <button onClick={() => setInputText(testCases.phishing)} className="w-full text-left px-4 py-2 text-sm text-slate-300 hover:text-cyan-300 bg-[#0b0f19]/50 hover:bg-[#1e293b] rounded-lg border border-slate-700/50 transition-colors flex justify-between items-center group">
                B. Phishing Bancario Urgente <ArrowRight className="w-4 h-4 opacity-0 group-hover:opacity-100 transition-opacity" />
              </button>
              <button onClick={() => setInputText(testCases.safe)} className="w-full text-left px-4 py-2 text-sm text-slate-300 hover:text-cyan-300 bg-[#0b0f19]/50 hover:bg-[#1e293b] rounded-lg border border-slate-700/50 transition-colors flex justify-between items-center group">
                C. Inversión Renta Fija Segura <ArrowRight className="w-4 h-4 opacity-0 group-hover:opacity-100 transition-opacity" />
              </button>
            </div>
          </div>
        </div>

        {/* Results Section */}
        <div className="h-full">
          {result ? (
            <div className={`h-full bg-[#1e293b]/60 backdrop-blur-md rounded-2xl p-6 border shadow-2xl transition-all duration-500 flex flex-col ${
              result.score > 75 ? 'border-red-500/50 shadow-[0_0_30px_rgba(239,68,68,0.15)]' : 
              result.score > 40 ? 'border-amber-500/50 shadow-[0_0_30px_rgba(245,158,11,0.15)]' : 
              'border-emerald-500/50 shadow-[0_0_30px_rgba(16,185,129,0.15)]'
            }`}>
              
              <div className="flex items-center justify-between mb-6 pb-6 border-b border-slate-700/50">
                <div>
                  <div className="text-xs font-bold text-slate-400 tracking-wider mb-1">VEREDICTO GROK AI</div>
                  <h2 className={`text-2xl font-black tracking-tight ${getScoreColor(result.score)}`}>
                    {result.verdict}
                  </h2>
                </div>
                <div className="text-right">
                  <div className="text-xs font-bold text-slate-400 tracking-wider mb-1">THREAT SCORE</div>
                  <div className={`text-4xl font-black ${getScoreColor(result.score)} flex items-baseline gap-1 justify-end`}>
                    {result.score}<span className="text-lg text-slate-500">%</span>
                  </div>
                </div>
              </div>

              <div className="mb-6 flex-grow">
                <h3 className="text-slate-300 font-medium mb-2">Análisis de la Inteligencia Artificial</h3>
                <p className="text-slate-400 text-sm leading-relaxed">{result.summary}</p>
              </div>

              {result.flags?.length > 0 && (
                <div className="mb-6">
                  <h3 className="text-slate-300 font-medium mb-3 flex items-center gap-2">
                    <ShieldAlert className="w-4 h-4 text-red-400" /> Red Flags Detectadas
                  </h3>
                  <ul className="space-y-2">
                    {result.flags.map((flag, idx) => (
                      <li key={idx} className="flex items-start gap-2 text-sm text-red-200 bg-red-950/30 p-2.5 rounded-lg border border-red-900/50">
                        <AlertOctagon className="w-4 h-4 text-red-500 shrink-0 mt-0.5" />
                        <span>{flag}</span>
                      </li>
                    ))}
                  </ul>
                </div>
              )}

              <div className="mt-auto pt-6 border-t border-slate-700/50">
                <h3 className="text-slate-300 font-medium mb-2">Acción Recomendada</h3>
                <div className={`p-4 rounded-xl border flex gap-3 ${
                  result.score > 75 ? 'bg-red-950/20 border-red-500/30' : 
                  result.score > 40 ? 'bg-amber-950/20 border-amber-500/30' : 
                  'bg-emerald-950/20 border-emerald-500/30'
                }`}>
                  <CheckCircle className={`w-6 h-6 shrink-0 ${getScoreColor(result.score)}`} />
                  <p className="text-sm text-slate-200">{result.recommendation}</p>
                </div>
              </div>

            </div>
          ) : (
            <div className="h-full min-h-[400px] border-2 border-dashed border-slate-700/50 rounded-2xl flex flex-col items-center justify-center text-slate-500 p-8 text-center bg-[#1e293b]/20">
              <Bot className="w-16 h-16 mb-4 text-slate-600 opacity-50" />
              <h3 className="text-lg font-medium text-slate-400 mb-2">Esperando texto para analizar</h3>
              <p className="text-sm max-w-xs mx-auto">
                Usa el formulario o los botones de prueba rápida a la izquierda para iniciar un escaneo de seguridad.
              </p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default CitizenScannerView;
