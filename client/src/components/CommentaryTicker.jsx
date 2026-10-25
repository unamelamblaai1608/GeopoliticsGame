import React, { useState } from 'react';
import { Radio, Sparkles, MessageSquare, Key, ChevronDown, ChevronUp, Bell } from 'lucide-react';

export default function CommentaryTicker({ commentary = [], onSaveApiKey, customApiKey }) {
  const [isExpanded, setIsExpanded] = useState(false);
  const [showKeyModal, setShowKeyModal] = useState(false);
  const [apiKeyInput, setApiKeyInput] = useState(customApiKey || '');

  const latest = commentary && commentary.length > 0 ? commentary[0] : null;

  const handleKeySave = () => {
    onSaveApiKey(apiKeyInput.trim());
    setShowKeyModal(false);
  };

  return (
    <div className="bg-tactical-900 border border-tactical-700 rounded-xl overflow-hidden shadow-lg">
      {/* Live Ticker Bar */}
      <div className="flex items-center justify-between px-4 py-2.5 bg-gradient-to-r from-tactical-950 via-tactical-900 to-tactical-950 border-b border-tactical-800">
        <div className="flex items-center gap-2.5 overflow-hidden">
          <div className="flex items-center gap-1.5 px-2 py-0.5 rounded-full bg-red-600/20 border border-red-500/40 text-[10px] font-bold text-red-400 tracking-wider uppercase shrink-0">
            <span className="w-1.5 h-1.5 rounded-full bg-red-500 animate-ping"></span>
            WAR ROOM INTEL
          </div>

          <div className="text-xs font-mono text-slate-200 truncate">
            {latest ? latest.text : 'Awaiting geopolitical maneuvers and satellite intelligence...'}
          </div>
        </div>

        <div className="flex items-center gap-2 shrink-0 ml-3">
          <button
            onClick={() => setShowKeyModal(true)}
            className="p-1.5 rounded-lg bg-tactical-800 hover:bg-tactical-700 text-slate-300 hover:text-cyan-400 transition"
            title="Configure Gemini LLM API Key (Optional)"
          >
            <Key className="w-3.5 h-3.5" />
          </button>

          <button
            onClick={() => setIsExpanded(!isExpanded)}
            className="p-1.5 rounded-lg bg-tactical-800 hover:bg-tactical-700 text-slate-300 flex items-center gap-1 text-xs"
          >
            <MessageSquare className="w-3.5 h-3.5 text-blue-400" />
            <span className="text-[11px] font-medium">{commentary.length}</span>
            {isExpanded ? <ChevronUp className="w-3.5 h-3.5" /> : <ChevronDown className="w-3.5 h-3.5" />}
          </button>
        </div>
      </div>

      {/* Expanded Commentary Feed */}
      {isExpanded && (
        <div className="p-3 bg-tactical-950/90 max-h-60 overflow-y-auto custom-scrollbar space-y-2 border-t border-tactical-800 text-xs">
          {commentary.map((entry) => {
            let badgeClass = 'bg-slate-800 text-slate-300 border-slate-700';
            if (entry.type === 'conquest' || entry.type === 'victory') {
              badgeClass = 'bg-emerald-950 text-emerald-400 border-emerald-800';
            } else if (entry.type === 'clash' || entry.type === 'defeat') {
              badgeClass = 'bg-rose-950 text-rose-400 border-rose-800';
            } else if (entry.type === 'attack_declare') {
              badgeClass = 'bg-amber-950 text-amber-400 border-amber-800';
            }

            return (
              <div
                key={entry.id}
                className="p-2.5 rounded-lg bg-tactical-900/70 border border-tactical-800/80 flex items-start gap-2.5"
              >
                <span className={`px-1.5 py-0.5 rounded text-[10px] font-mono font-bold uppercase border shrink-0 ${badgeClass}`}>
                  {entry.type}
                </span>
                <div className="flex-1 text-slate-300 font-sans leading-relaxed">
                  {entry.text}
                </div>
                <div className="text-[10px] font-mono text-slate-500 shrink-0">
                  {new Date(entry.timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' })}
                </div>
              </div>
            );
          })}
        </div>
      )}

      {/* Optional Gemini API Key Dialog */}
      {showKeyModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-sm">
          <div className="w-full max-w-md p-6 bg-tactical-900 border border-tactical-700 rounded-xl shadow-2xl">
            <div className="flex items-center gap-2 mb-3 text-cyan-400 font-bold font-display text-base">
              <Sparkles className="w-5 h-5" />
              CONFIGURE GEMINI LLM COMMENTARY
            </div>
            <p className="text-xs text-slate-300 mb-4 leading-relaxed">
              The game comes with an intelligent built-in generative commentary engine. If you'd like to stream live Google Gemini commentary directly, paste your Gemini API key below. It will be stored locally in your browser.
            </p>

            <input
              type="password"
              placeholder="AIzaSy..."
              value={apiKeyInput}
              onChange={(e) => setApiKeyInput(e.target.value)}
              className="w-full px-3 py-2 bg-tactical-950 border border-tactical-600 rounded-lg text-xs font-mono text-white mb-4 focus:outline-none focus:border-cyan-400"
            />

            <div className="flex justify-end gap-3">
              <button
                onClick={() => setShowKeyModal(false)}
                className="px-4 py-2 text-xs text-slate-400 hover:text-white"
              >
                Cancel
              </button>
              <button
                onClick={handleKeySave}
                className="px-5 py-2 rounded-lg bg-blue-600 hover:bg-blue-500 text-xs font-bold text-white shadow"
              >
                Save & Enable
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

