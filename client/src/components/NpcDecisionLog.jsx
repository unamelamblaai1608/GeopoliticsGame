import React, { useState } from 'react';
import { Bot, ChevronDown, ChevronUp, Shield, Target, Zap, ArrowRight, CornerDownRight } from 'lucide-react';

export default function NpcDecisionLog({ decisionLog = [], players = [] }) {
  const [isCollapsed, setIsCollapsed] = useState(false);

  // Show newest decisions first
  const reversedLogs = [...decisionLog].reverse();

  const getPlayer = (id) => players.find(p => p.id === id) || { name: 'NPC', color: '#64748b' };

  return (
    <div className="bg-tactical-900 border border-tactical-700 rounded-xl overflow-hidden shadow-xl flex flex-col">
      {/* Header */}
      <div className="flex items-center justify-between px-4 py-3 bg-tactical-950/80 border-b border-tactical-800">
        <div className="flex items-center gap-2">
          <Bot className="w-5 h-5 text-amber-400" />
          <h3 className="font-display text-sm font-bold text-white tracking-wide">
            NPC STRATEGIC INTELLIGENCE & JUSTIFICATIONS
          </h3>
          <span className="text-[10px] bg-tactical-800 text-slate-300 px-2 py-0.5 rounded-full font-mono">
            {decisionLog.length} logs
          </span>
        </div>

        <button
          onClick={() => setIsCollapsed(!isCollapsed)}
          className="text-xs text-slate-400 hover:text-white flex items-center gap-1"
        >
          {isCollapsed ? <ChevronDown className="w-4 h-4" /> : <ChevronUp className="w-4 h-4" />}
        </button>
      </div>

      {/* Body */}
      {!isCollapsed && (
        <div className="p-3 max-h-80 overflow-y-auto custom-scrollbar space-y-3 bg-tactical-950/40">
          {reversedLogs.length === 0 ? (
            <div className="text-center py-6 text-xs text-slate-500">
              No NPC moves executed yet. All decisions will stream here with tactical justifications.
            </div>
          ) : (
            reversedLogs.map((log, logIdx) => {
              const player = getPlayer(log.playerId);
              return (
                <div
                  key={logIdx}
                  className="p-3 rounded-lg bg-tactical-950/90 border border-tactical-800 space-y-2 text-xs"
                >
                  {/* Commander Header */}
                  <div className="flex items-center justify-between border-b border-tactical-800/80 pb-1.5">
                    <div className="flex items-center gap-2 font-bold">
                      <div
                        className="w-2.5 h-2.5 rounded-full"
                        style={{ backgroundColor: player.color }}
                      ></div>
                      <span className="text-slate-200">{log.playerName}</span>
                      <span className="text-[10px] text-slate-400 font-normal">
                        ({player.title || player.personality})
                      </span>
                    </div>

                    <div className="text-[10px] font-mono text-slate-500">
                      R{log.roundNumber} : T{log.turnNumber}
                    </div>
                  </div>

                  {/* Decision Steps */}
                  <div className="space-y-1.5 pl-2">
                    {log.steps.map((step, sIdx) => {
                      let icon = <CornerDownRight className="w-3.5 h-3.5 text-slate-400 shrink-0" />;
                      let color = 'text-slate-300';

                      if (step.action === 'reinforce') {
                        icon = <Shield className="w-3.5 h-3.5 text-blue-400 shrink-0" />;
                        color = 'text-blue-300';
                      } else if (step.action === 'attack_declare') {
                        icon = <Target className="w-3.5 h-3.5 text-amber-400 shrink-0" />;
                        color = 'text-amber-200';
                      } else if (step.action === 'conquest') {
                        icon = <Zap className="w-3.5 h-3.5 text-emerald-400 shrink-0" />;
                        color = 'text-emerald-300 font-semibold';
                      } else if (step.action === 'retreat' || step.action === 'defeat') {
                        icon = <ArrowRight className="w-3.5 h-3.5 text-rose-400 shrink-0" />;
                        color = 'text-rose-300';
                      }

                      return (
                        <div key={sIdx} className="flex items-start gap-2">
                          <span className="mt-0.5">{icon}</span>
                          <span className={`leading-relaxed text-[11px] ${color}`}>
                            {step.text}
                          </span>
                        </div>
                      );
                    })}
                  </div>
                </div>
              );
            })
          )}
        </div>
      )}
    </div>
  );
}

