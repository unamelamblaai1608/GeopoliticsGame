import React, { useState, useEffect } from 'react';
import { BarChart3, Globe, Trophy, Users, ShieldAlert, Award, X, Sparkles } from 'lucide-react';
import { getPlayerAnalytics, getPlatformAnalytics } from '../api';

export default function AnalyticsModal({ isOpen, onClose, currentUser }) {
  const [tab, setTab] = useState('player'); // 'player' or 'platform'
  const [playerStats, setPlayerStats] = useState(null);
  const [platformStats, setPlatformStats] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!isOpen) return;
    setLoading(true);

    Promise.all([
      currentUser ? getPlayerAnalytics().catch(() => null) : Promise.resolve(null),
      getPlatformAnalytics().catch(() => null)
    ]).then(([pStats, globStats]) => {
      setPlayerStats(pStats);
      setPlatformStats(globStats);
      setLoading(false);
    });
  }, [isOpen, currentUser]);

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-md">
      <div className="w-full max-w-3xl bg-tactical-950 border border-tactical-700 rounded-2xl shadow-2xl overflow-hidden flex flex-col max-h-[90vh]">
        {/* Modal Header */}
        <div className="flex items-center justify-between px-6 py-4 bg-tactical-900 border-b border-tactical-800">
          <div className="flex items-center gap-3">
            <BarChart3 className="w-6 h-6 text-cyan-400" />
            <h2 className="font-display text-lg font-bold text-white tracking-wide">
              WAR ROOM STRATEGIC ANALYTICS
            </h2>
          </div>
          <button
            onClick={onClose}
            className="p-1.5 rounded-lg bg-tactical-800 hover:bg-tactical-700 text-slate-400 hover:text-white"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Tab Switcher */}
        <div className="flex border-b border-tactical-800 bg-tactical-950/60 px-6 pt-3 gap-4">
          <button
            onClick={() => setTab('player')}
            disabled={!currentUser}
            className={`pb-3 text-sm font-semibold transition-all border-b-2 flex items-center gap-2 ${
              tab === 'player'
                ? 'border-blue-500 text-blue-400'
                : 'border-transparent text-slate-400 hover:text-slate-200'
            } ${!currentUser ? 'opacity-40 cursor-not-allowed' : ''}`}
          >
            <Award className="w-4 h-4" />
            Commander Dossier {currentUser ? `(${currentUser.username})` : '(Login Required)'}
          </button>

          <button
            onClick={() => setTab('platform')}
            className={`pb-3 text-sm font-semibold transition-all border-b-2 flex items-center gap-2 ${
              tab === 'platform'
                ? 'border-cyan-500 text-cyan-400'
                : 'border-transparent text-slate-400 hover:text-slate-200'
            }`}
          >
            <Globe className="w-4 h-4" />
            Global Platform Metrics & Leaderboard
          </button>
        </div>

        {/* Modal Content */}
        <div className="p-6 overflow-y-auto custom-scrollbar flex-1 space-y-6">
          {loading ? (
            <div className="text-center py-12 text-slate-400">Loading intelligence metrics...</div>
          ) : tab === 'player' && playerStats ? (
            <div className="space-y-6">
              {/* Top Key Metrics Grid */}
              <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
                <div className="p-4 rounded-xl bg-tactical-900 border border-tactical-800 text-center">
                  <div className="text-xs text-slate-400 mb-1">Win Rate</div>
                  <div className="font-mono text-2xl font-extrabold text-emerald-400">
                    {playerStats.winRate}
                  </div>
                  <div className="text-[10px] text-slate-500 mt-1">
                    {playerStats.gamesWon}W / {playerStats.gamesPlayed} matches
                  </div>
                </div>

                <div className="p-4 rounded-xl bg-tactical-900 border border-tactical-800 text-center">
                  <div className="text-xs text-slate-400 mb-1">Combat Win Rate</div>
                  <div className="font-mono text-2xl font-extrabold text-blue-400">
                    {playerStats.battleWinRate}
                  </div>
                  <div className="text-[10px] text-slate-500 mt-1">
                    {playerStats.battlesWon}W / {playerStats.battlesLost}L
                  </div>
                </div>

                <div className="p-4 rounded-xl bg-tactical-900 border border-tactical-800 text-center">
                  <div className="text-xs text-slate-400 mb-1">Casualty Ratio (K/D)</div>
                  <div className="font-mono text-2xl font-extrabold text-amber-400">
                    {playerStats.kdRatio}
                  </div>
                  <div className="text-[10px] text-slate-500 mt-1">
                    {playerStats.soldiersKilled} killed / {playerStats.soldiersLost} lost
                  </div>
                </div>

                <div className="p-4 rounded-xl bg-tactical-900 border border-tactical-800 text-center">
                  <div className="text-xs text-slate-400 mb-1">Conquered Lands</div>
                  <div className="font-mono text-2xl font-extrabold text-cyan-400">
                    {playerStats.countriesConquered}
                  </div>
                  <div className="text-[10px] text-slate-500 mt-1">Territories Annexed</div>
                </div>
              </div>

              {/* Dice Luck Distribution */}
              <div className="p-4 rounded-xl bg-tactical-900 border border-tactical-800">
                <h4 className="text-xs font-bold text-slate-300 uppercase tracking-wider mb-3">
                  Dice Roll Distribution (Your Luck Index)
                </h4>
                <div className="grid grid-cols-6 gap-2 text-center">
                  {[1, 2, 3, 4, 5, 6].map(die => {
                    const count = playerStats.diceRolls?.[die] || 0;
                    return (
                      <div key={die} className="p-2.5 rounded-lg bg-tactical-950 border border-tactical-800">
                        <div className="text-xs font-mono font-bold text-slate-400">Die {die}</div>
                        <div className="text-lg font-bold font-mono text-cyan-300">{count}</div>
                      </div>
                    );
                  })}
                </div>
              </div>

              {/* Top Starting Nations */}
              {playerStats.topStartingCountries && playerStats.topStartingCountries.length > 0 && (
                <div className="p-4 rounded-xl bg-tactical-900 border border-tactical-800">
                  <h4 className="text-xs font-bold text-slate-300 uppercase tracking-wider mb-2">
                    Favorite Staging Nations
                  </h4>
                  <div className="flex flex-wrap gap-2">
                    {playerStats.topStartingCountries.map((c, i) => (
                      <span
                        key={i}
                        className="px-3 py-1 rounded-full bg-tactical-950 border border-tactical-700 text-xs text-slate-300 font-mono"
                      >
                        {c.country}: <span className="text-amber-400 font-bold">{c.count} games</span>
                      </span>
                    ))}
                  </div>
                </div>
              )}
            </div>
          ) : tab === 'platform' && platformStats ? (
            <div className="space-y-6">
              {/* Platform Overview Grid */}
              <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
                <div className="p-4 rounded-xl bg-tactical-900 border border-tactical-800 text-center">
                  <div className="text-xs text-slate-400 mb-1">Registered Commanders</div>
                  <div className="font-mono text-2xl font-extrabold text-blue-400">
                    {platformStats.totalUsers}
                  </div>
                </div>

                <div className="p-4 rounded-xl bg-tactical-900 border border-tactical-800 text-center">
                  <div className="text-xs text-slate-400 mb-1">Total Campaigns</div>
                  <div className="font-mono text-2xl font-extrabold text-cyan-400">
                    {platformStats.totalGames}
                  </div>
                  <div className="text-[10px] text-slate-500 mt-1">
                    {platformStats.activeGames} active / {platformStats.completedGames} won
                  </div>
                </div>

                <div className="p-4 rounded-xl bg-tactical-900 border border-tactical-800 text-center">
                  <div className="text-xs text-slate-400 mb-1">Avg War Length</div>
                  <div className="font-mono text-2xl font-extrabold text-amber-400">
                    {platformStats.avgRoundsPerGame}
                  </div>
                  <div className="text-[10px] text-slate-500 mt-1">Rounds per campaign</div>
                </div>

                <div className="p-4 rounded-xl bg-tactical-900 border border-tactical-800 text-center">
                  <div className="text-xs text-slate-400 mb-1">Most Picked Staging</div>
                  <div className="font-mono text-base font-bold text-emerald-400 truncate">
                    {platformStats.popularCountries?.[0]?.name || 'South Africa'}
                  </div>
                </div>
              </div>

              {/* Global Leaderboard */}
              <div className="p-4 rounded-xl bg-tactical-900 border border-tactical-800">
                <h4 className="text-xs font-bold text-slate-300 uppercase tracking-wider mb-3 flex items-center gap-2">
                  <Trophy className="w-4 h-4 text-amber-400" />
                  Global Supreme Commanders Leaderboard
                </h4>
                <div className="overflow-x-auto">
                  <table className="w-full text-xs text-left">
                    <thead className="bg-tactical-950 text-slate-400 font-mono">
                      <tr>
                        <th className="p-2.5">Rank</th>
                        <th className="p-2.5">Commander</th>
                        <th className="p-2.5">World Conquests</th>
                        <th className="p-2.5">Territories Won</th>
                        <th className="p-2.5">Casualties Inflicted</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-tactical-800 font-sans">
                      {platformStats.leaderboard && platformStats.leaderboard.length > 0 ? (
                        platformStats.leaderboard.map((user, idx) => (
                          <tr key={idx} className="hover:bg-tactical-800/40">
                            <td className="p-2.5 font-mono font-bold text-amber-400">#{idx + 1}</td>
                            <td className="p-2.5 font-semibold text-white">{user.username}</td>
                            <td className="p-2.5 font-mono text-emerald-400 font-bold">{user.gamesWon}</td>
                            <td className="p-2.5 font-mono text-slate-300">{user.countriesConquered}</td>
                            <td className="p-2.5 font-mono text-cyan-300">{user.soldiersKilled}</td>
                          </tr>
                        ))
                      ) : (
                        <tr>
                          <td colSpan="5" className="p-4 text-center text-slate-500">
                            No campaigns concluded yet. Lead the charge and claim #1!
                          </td>
                        </tr>
                      )}
                    </tbody>
                  </table>
                </div>
              </div>
            </div>
          ) : (
            <div className="text-center py-12 text-slate-400">No data available.</div>
          )}
        </div>
      </div>
    </div>
  );
}

