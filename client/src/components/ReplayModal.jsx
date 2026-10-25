import React, { useState } from 'react';
import { History, Swords, Shield, X, ChevronRight, Play, RotateCcw } from 'lucide-react';

export default function ReplayModal({ isOpen, onClose, battleHistory = [], countries = {} }) {
  const [selectedBattleIdx, setSelectedBattleIdx] = useState(0);

  if (!isOpen) return null;

  const currentBattle = battleHistory[selectedBattleIdx] || null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-md">
      <div className="w-full max-w-4xl bg-tactical-950 border border-tactical-700 rounded-2xl shadow-2xl overflow-hidden flex flex-col max-h-[90vh]">
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 bg-tactical-900 border-b border-tactical-800">
          <div className="flex items-center gap-3">
            <History className="w-6 h-6 text-amber-400" />
            <h2 className="font-display text-lg font-bold text-white tracking-wide">
              CAMPAIGN BATTLE ARCHIVE & REPLAY
            </h2>
          </div>
          <button
            onClick={onClose}
            className="p-1.5 rounded-lg bg-tactical-800 hover:bg-tactical-700 text-slate-400 hover:text-white"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Content */}
        <div className="flex-1 flex flex-col md:flex-row overflow-hidden">
          {/* Battle List Sidebar */}
          <div className="w-full md:w-72 border-r border-tactical-800 bg-tactical-950 overflow-y-auto custom-scrollbar p-3 space-y-2">
            <div className="text-xs font-mono font-bold text-slate-400 uppercase tracking-wider px-2 py-1">
              Recorded Skirmishes ({battleHistory.length})
            </div>

            {battleHistory.length === 0 ? (
              <div className="text-xs text-slate-500 p-4 text-center">
                No battles recorded yet in this campaign.
              </div>
            ) : (
              battleHistory.map((b, idx) => {
                const attCountry = countries[b.attackerCountryId] || { name: b.attackerCountryId, flag: '🚩' };
                const defCountry = countries[b.defenderCountryId] || { name: b.defenderCountryId, flag: '🎯' };

                let badge = 'text-slate-400';
                if (b.status === 'conquered') badge = 'text-emerald-400 font-bold';
                else if (b.status === 'retreated') badge = 'text-amber-400';
                else if (b.status === 'attacker_defeated') badge = 'text-rose-400';

                return (
                  <button
                    key={idx}
                    onClick={() => setSelectedBattleIdx(idx)}
                    className={`w-full p-2.5 rounded-xl border text-left transition-all ${
                      selectedBattleIdx === idx
                        ? 'bg-tactical-800 border-blue-500 shadow-md'
                        : 'bg-tactical-900/60 border-tactical-800 hover:border-tactical-700'
                    }`}
                  >
                    <div className="flex items-center justify-between text-[11px] font-mono text-slate-500 mb-1">
                      <span>Battle #{idx + 1}</span>
                      <span>R{b.roundNumber || 1}</span>
                    </div>
                    <div className="text-xs font-semibold text-slate-200 truncate">
                      {attCountry.flag} {attCountry.name} vs {defCountry.flag} {defCountry.name}
                    </div>
                    <div className={`text-[10px] uppercase tracking-wider mt-1 ${badge}`}>
                      {b.status}
                    </div>
                  </button>
                );
              })
            )}
          </div>

          {/* Battle Details & Round-by-Round Playback */}
          <div className="flex-1 p-6 overflow-y-auto custom-scrollbar bg-tactical-900/40">
            {currentBattle ? (
              <div className="space-y-6">
                {/* Battle Header */}
                <div className="p-4 rounded-xl bg-tactical-900 border border-tactical-700 flex items-center justify-between">
                  <div>
                    <div className="text-xs font-mono uppercase text-slate-400">Battle Report</div>
                    <div className="font-display text-lg font-bold text-white">
                      {currentBattle.outcome || 'Combat Encounter'}
                    </div>
                  </div>
                  <div className="text-right text-xs font-mono">
                    <div className="text-slate-400">Total Dice Clashes</div>
                    <div className="text-cyan-400 font-bold text-base">
                      {currentBattle.rounds?.length || 0} rounds
                    </div>
                  </div>
                </div>

                {/* Round by Round Dice Rolls Breakdown */}
                <div className="space-y-3">
                  <h4 className="text-xs font-bold text-slate-300 uppercase tracking-wider">
                    Clash Sequences
                  </h4>

                  {currentBattle.rounds && currentBattle.rounds.length > 0 ? (
                    currentBattle.rounds.map((round, rIdx) => (
                      <div
                        key={rIdx}
                        className="p-3.5 rounded-xl bg-tactical-950 border border-tactical-800 space-y-2 text-xs"
                      >
                        <div className="flex justify-between items-center text-slate-400 font-mono text-[11px] border-b border-tactical-800/80 pb-1">
                          <span>Volley #{rIdx + 1}</span>
                          <span>
                            Casualties: Att -{round.attackerLosses} | Def -{round.defenderLosses}
                          </span>
                        </div>

                        <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-2 pt-1">
                          {round.pairings && round.pairings.map((p, pIdx) => (
                            <div
                              key={pIdx}
                              className="p-2 rounded bg-tactical-900 border border-tactical-800 flex items-center justify-between font-mono"
                            >
                              <span className="text-blue-400 font-bold">Die: {p.attackerDie}</span>
                              <span className="text-slate-500 text-[10px]">vs</span>
                              <span className="text-red-400 font-bold">Die: {p.defenderDie}</span>
                            </div>
                          ))}
                        </div>
                      </div>
                    ))
                  ) : (
                    <div className="text-slate-500 text-xs">No rounds recorded.</div>
                  )}
                </div>
              </div>
            ) : (
              <div className="text-center py-24 text-slate-500 text-sm">
                Select a skirmish from the sidebar to inspect tactical telemetry.
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

