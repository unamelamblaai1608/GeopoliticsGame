import React, { useState } from 'react';
import {
  Globe,
  Plus,
  Share2,
  Volume2,
  VolumeX,
  BarChart3,
  History,
  User,
  LogOut,
  ChevronDown,
  Shield,
  Check
} from 'lucide-react';
import { sound } from '../utils/audio';

export default function Navbar({
  currentUser,
  games = [],
  activeGame,
  onSelectGame,
  onOpenNewGame,
  onOpenAuth,
  onLogout,
  onOpenAnalytics,
  onOpenReplay,
  isSpectator
}) {
  const [isMuted, setIsMuted] = useState(sound.isMuted());
  const [copied, setCopied] = useState(false);
  const [showGameDropdown, setShowGameDropdown] = useState(false);

  const handleMuteToggle = () => {
    const muted = sound.toggleMute();
    setIsMuted(muted);
  };

  const handleShare = () => {
    if (!activeGame) return;
    const url = `${window.location.origin}?game=${activeGame._id}`;
    navigator.clipboard.writeText(url).then(() => {
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    });
  };

  return (
    <header className="sticky top-0 z-40 w-full bg-tactical-950/90 backdrop-blur-md border-b border-tactical-800 px-4 lg:px-8 py-3">
      <div className="max-w-7xl mx-auto flex flex-wrap items-center justify-between gap-4">
        {/* Brand & Logo */}
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-gradient-to-tr from-blue-600 to-cyan-400 p-0.5 shadow-lg shadow-blue-500/20 flex items-center justify-center">
            <div className="w-full h-full bg-tactical-950 rounded-[10px] flex items-center justify-center">
              <Globe className="w-5 h-5 text-cyan-400" />
            </div>
          </div>

          <div>
            <div className="font-display text-lg font-extrabold tracking-wider text-white flex items-center gap-2">
              <span>GEOPOLITICS</span>
              <span className="text-[10px] font-mono uppercase bg-blue-500/20 text-cyan-400 border border-blue-500/30 px-2 py-0.5 rounded-full">
                Risk Engine 2.0
              </span>
            </div>
            <div className="text-[11px] text-slate-400 font-sans">
              Single-Player Geopolitical Hegemony
            </div>
          </div>
        </div>

        {/* Center: Active Game Switcher */}
        {currentUser && games.length > 0 && (
          <div className="relative">
            <button
              onClick={() => setShowGameDropdown(!showGameDropdown)}
              className="px-3.5 py-1.5 rounded-lg bg-tactical-900 border border-tactical-700 hover:border-tactical-600 text-xs font-semibold text-slate-200 flex items-center gap-2 shadow-sm"
            >
              <Shield className="w-3.5 h-3.5 text-blue-400" />
              <span className="truncate max-w-[160px]">
                {activeGame ? activeGame.title : 'Select War Room'}
              </span>
              <ChevronDown className="w-3.5 h-3.5 text-slate-400" />
            </button>

            {showGameDropdown && (
              <div className="absolute left-0 mt-2 w-64 p-2 bg-tactical-900 border border-tactical-700 rounded-xl shadow-2xl z-50 space-y-1">
                <div className="text-[10px] uppercase font-mono text-slate-400 px-2 py-1 font-bold">
                  Your Active Campaigns ({games.length})
                </div>
                {games.map(g => (
                  <button
                    key={g._id}
                    onClick={() => {
                      onSelectGame(g._id);
                      setShowGameDropdown(false);
                    }}
                    className={`w-full p-2 rounded-lg text-left text-xs transition flex items-center justify-between ${
                      activeGame?._id === g._id
                        ? 'bg-blue-600 text-white font-bold'
                        : 'text-slate-300 hover:bg-tactical-800'
                    }`}
                  >
                    <span className="truncate">{g.title}</span>
                    <span className="text-[10px] font-mono opacity-80">R{g.roundNumber}</span>
                  </button>
                ))}
              </div>
            )}
          </div>
        )}

        {/* Right Navigation Controls */}
        <div className="flex items-center gap-2.5">
          {/* New Game Button */}
          {currentUser && (
            <button
              onClick={onOpenNewGame}
              className="px-3 py-1.5 rounded-lg bg-blue-600 hover:bg-blue-500 active:scale-95 text-white font-bold font-display text-xs tracking-wider shadow-md shadow-blue-600/30 flex items-center gap-1.5 transition"
            >
              <Plus className="w-4 h-4" />
              <span>NEW MATCH</span>
            </button>
          )}

          {/* Share Game Link */}
          {activeGame && (
            <button
              onClick={handleShare}
              className="p-2 rounded-lg bg-tactical-900 border border-tactical-700 hover:border-tactical-600 text-slate-300 hover:text-white transition relative"
              title="Share Match URL"
            >
              {copied ? <Check className="w-4 h-4 text-emerald-400" /> : <Share2 className="w-4 h-4" />}
            </button>
          )}

          {/* Battle Replay */}
          {activeGame && (
            <button
              onClick={onOpenReplay}
              className="p-2 rounded-lg bg-tactical-900 border border-tactical-700 hover:border-tactical-600 text-slate-300 hover:text-amber-400 transition"
              title="Campaign Battle History & Replays"
            >
              <History className="w-4 h-4" />
            </button>
          )}

          {/* Analytics Modal */}
          <button
            onClick={onOpenAnalytics}
            className="p-2 rounded-lg bg-tactical-900 border border-tactical-700 hover:border-tactical-600 text-slate-300 hover:text-cyan-400 transition"
            title="Strategic Analytics & Leaderboard"
          >
            <BarChart3 className="w-4 h-4" />
          </button>

          {/* Audio Mute Toggle */}
          <button
            onClick={handleMuteToggle}
            className="p-2 rounded-lg bg-tactical-900 border border-tactical-700 hover:border-tactical-600 text-slate-300 hover:text-white transition"
            title={isMuted ? 'Unmute Sound FX' : 'Mute Sound FX'}
          >
            {isMuted ? <VolumeX className="w-4 h-4 text-rose-400" /> : <Volume2 className="w-4 h-4 text-emerald-400" />}
          </button>

          {/* User Authentication Menu */}
          {currentUser ? (
            <div className="flex items-center gap-2 pl-2 border-l border-tactical-800">
              <div className="text-right hidden sm:block">
                <div className="text-xs font-bold text-white leading-none">{currentUser.username}</div>
                <div className="text-[10px] text-cyan-400 font-mono">Commander</div>
              </div>

              <button
                onClick={onLogout}
                className="p-2 rounded-lg bg-tactical-900 border border-tactical-700 hover:bg-red-950/60 hover:border-red-600 text-slate-400 hover:text-red-300 transition"
                title="Log Out"
              >
                <LogOut className="w-4 h-4" />
              </button>
            </div>
          ) : (
            <button
              onClick={onOpenAuth}
              className="px-4 py-1.5 rounded-lg bg-tactical-800 hover:bg-tactical-700 border border-tactical-600 text-slate-200 font-semibold text-xs transition"
            >
              Log In / Register
            </button>
          )}
        </div>
      </div>
    </header>
  );
}

