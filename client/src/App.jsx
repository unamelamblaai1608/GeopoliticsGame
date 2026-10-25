import React, { useState, useEffect, useCallback } from 'react';
import Navbar from './components/Navbar';
import WorldMap from './components/WorldMap';
import BattleArena from './components/BattleArena';
import TurnControls from './components/TurnControls';
import CommentaryTicker from './components/CommentaryTicker';
import NpcDecisionLog from './components/NpcDecisionLog';
import AnalyticsModal from './components/AnalyticsModal';
import ReplayModal from './components/ReplayModal';
import AuthModal from './components/AuthModal';
import NewGameModal from './components/NewGameModal';

import {
  getCurrentUser,
  getGames,
  getGame,
  sendGameAction,
  logout
} from './api';
import { sound } from './utils/audio';
import confetti from 'canvas-confetti';
import { ShieldAlert, Trophy, Eye, Plus, Sparkles } from 'lucide-react';

export default function App() {
  const [currentUser, setCurrentUser] = useState(null);
  const [games, setGames] = useState([]);
  const [activeGame, setActiveGame] = useState(null);
  const [loading, setLoading] = useState(true);
  const [isSpectator, setIsSpectator] = useState(false);

  // Selection state for map combat
  const [selectedAttackerId, setSelectedAttackerId] = useState(null);
  const [selectedDefenderId, setSelectedDefenderId] = useState(null);

  // Fast-forward / simulation state
  const [isSimulatingNpc, setIsSimulatingNpc] = useState(false);

  // Modals
  const [showAuthModal, setShowAuthModal] = useState(false);
  const [showNewGameModal, setShowNewGameModal] = useState(false);
  const [showAnalyticsModal, setShowAnalyticsModal] = useState(false);
  const [showReplayModal, setShowReplayModal] = useState(false);

  // Optional Gemini API key in local storage
  const [customApiKey, setCustomApiKey] = useState(
    localStorage.getItem('geopolitics_gemini_key') || ''
  );

  // Check initial user session & URL parameters
  useEffect(() => {
    async function init() {
      setLoading(true);
      try {
        const user = await getCurrentUser();
        setCurrentUser(user);

        // Check if a specific game ID is requested via URL query params
        const urlParams = new URLSearchParams(window.location.search);
        const requestedGameId = urlParams.get('game');

        if (requestedGameId) {
          const gameData = await getGame(requestedGameId);
          setActiveGame(gameData.game);
          setIsSpectator(gameData.spectatorMode);
        } else if (user) {
          const userGames = await getGames();
          setGames(userGames);
          if (userGames.length > 0) {
            setActiveGame(userGames[0]);
            setIsSpectator(false);
          } else {
            // First time user: open new game modal!
            setShowNewGameModal(true);
          }
        }
      } catch (err) {
        console.error('Initialization error:', err);
      } finally {
        setLoading(false);
      }
    }
    init();
  }, []);

  const refreshGame = useCallback(async (gameId) => {
    if (!gameId) return;
    try {
      const data = await getGame(gameId);
      setActiveGame(data.game);
      setIsSpectator(data.spectatorMode);
    } catch (err) {
      console.error('Failed to refresh game:', err);
    }
  }, []);

  // Poll for spectator updates or live state when viewing
  useEffect(() => {
    if (!activeGame || !isSpectator) return;
    const interval = setInterval(() => {
      refreshGame(activeGame._id);
    }, 3000);
    return () => clearInterval(interval);
  }, [activeGame, isSpectator, refreshGame]);

  // Auth callbacks
  const handleAuthSuccess = async (user) => {
    setCurrentUser(user);
    const userGames = await getGames();
    setGames(userGames);
    if (userGames.length > 0) {
      setActiveGame(userGames[0]);
      setIsSpectator(false);
    } else {
      setShowNewGameModal(true);
    }
  };

  const handleLogout = () => {
    logout();
    setCurrentUser(null);
    setGames([]);
    setActiveGame(null);
    setIsSpectator(true);
  };

  const handleGameCreated = (newGame) => {
    setGames(prev => [newGame, ...prev]);
    setActiveGame(newGame);
    setIsSpectator(false);
    setSelectedAttackerId(null);
    setSelectedDefenderId(null);
  };

  const handleSelectGame = (gameId) => {
    const selected = games.find(g => g._id === gameId);
    if (selected) {
      setActiveGame(selected);
      setIsSpectator(false);
      setSelectedAttackerId(null);
      setSelectedDefenderId(null);
    }
  };

  // Turn Action Handlers
  const handleReinforce = async (allocations) => {
    if (!activeGame) return;
    try {
      const res = await sendGameAction(activeGame._id, 'reinforce', { allocations });
      setActiveGame(res.game);
    } catch (err) {
      alert(err.message);
    }
  };

  const handleTransfer = async (fromCountryId, toCountryId, count) => {
    if (!activeGame) return;
    try {
      const res = await sendGameAction(activeGame._id, 'transfer', { fromCountryId, toCountryId, count });
      setActiveGame(res.game);
    } catch (err) {
      alert(err.message);
    }
  };

  const handleStartAttack = async () => {
    if (!activeGame || !selectedAttackerId || !selectedDefenderId) return;
    try {
      const res = await sendGameAction(activeGame._id, 'start_attack', {
        fromCountryId: selectedAttackerId,
        toCountryId: selectedDefenderId
      });
      setActiveGame(res.game);
    } catch (err) {
      alert(err.message);
    }
  };

  const handleClashStep = async () => {
    if (!activeGame) return;
    const res = await sendGameAction(activeGame._id, 'clash_step');
    setActiveGame(res.game);
    return res;
  };

  const handleStopAttack = async () => {
    if (!activeGame) return;
    const res = await sendGameAction(activeGame._id, 'stop_attack');
    setActiveGame(res.game);
    setSelectedAttackerId(null);
    setSelectedDefenderId(null);
  };

  const handleConquestTransfer = async (count) => {
    if (!activeGame) return;
    const res = await sendGameAction(activeGame._id, 'conquest_transfer', { count });
    setActiveGame(res.game);
    setSelectedAttackerId(null);
    setSelectedDefenderId(null);
  };

  const handleEndTurn = async () => {
    if (!activeGame) return;
    try {
      const res = await sendGameAction(activeGame._id, 'end_turn');
      setActiveGame(res.game);
      setSelectedAttackerId(null);
      setSelectedDefenderId(null);
    } catch (err) {
      alert(err.message);
    }
  };

  const handleStepNpc = async () => {
    if (!activeGame || isSimulatingNpc) return;
    setIsSimulatingNpc(true);
    sound.playDiceRoll();
    try {
      const res = await sendGameAction(activeGame._id, 'step_npc');
      setActiveGame(res.game);
    } catch (err) {
      console.error(err);
    } finally {
      setIsSimulatingNpc(false);
    }
  };

  const handleFastForward = async () => {
    if (!activeGame || isSimulatingNpc) return;
    setIsSimulatingNpc(true);
    sound.playDiceRoll();
    try {
      const res = await sendGameAction(activeGame._id, 'fast_forward');
      setActiveGame(res.game);
    } catch (err) {
      console.error(err);
    } finally {
      setIsSimulatingNpc(false);
    }
  };

  const handleSaveApiKey = (key) => {
    setCustomApiKey(key);
    localStorage.setItem('geopolitics_gemini_key', key);
  };

  const isPlayerTurn = activeGame && activeGame.currentTurnPlayerId === `player_${currentUser?.id}`;

  return (
    <div className="min-h-screen bg-tactical-950 text-slate-100 flex flex-col font-sans">
      {/* Navigation */}
      <Navbar
        currentUser={currentUser}
        games={games}
        activeGame={activeGame}
        onSelectGame={handleSelectGame}
        onOpenNewGame={() => setShowNewGameModal(true)}
        onOpenAuth={() => setShowAuthModal(true)}
        onLogout={handleLogout}
        onOpenAnalytics={() => setShowAnalyticsModal(true)}
        onOpenReplay={() => setShowReplayModal(true)}
        isSpectator={isSpectator}
      />

      {/* Spectator Mode Alert Banner */}
      {isSpectator && activeGame && (
        <div className="bg-amber-950/80 border-b border-amber-600/60 px-4 py-2 text-xs text-amber-200 flex flex-wrap items-center justify-between gap-3">
          <div className="flex items-center gap-2">
            <Eye className="w-4 h-4 text-amber-400 animate-pulse" />
            <span>
              <strong>LIVE TRANSMISSION:</strong> You are spectating Commander{' '}
              <strong>{activeGame.ownerUsername}</strong>'s campaign. Turns execute automatically.
            </span>
          </div>

          {!currentUser ? (
            <button
              onClick={() => setShowAuthModal(true)}
              className="px-3 py-1 bg-amber-500 hover:bg-amber-400 text-black font-bold rounded text-xs transition"
            >
              Sign Up to Start Your Campaign
            </button>
          ) : (
            <button
              onClick={() => setShowNewGameModal(true)}
              className="px-3 py-1 bg-amber-500 hover:bg-amber-400 text-black font-bold rounded text-xs transition"
            >
              Start Your Own Match
            </button>
          )}
        </div>
      )}

      {/* Main War Room Body */}
      <main className="flex-1 max-w-7xl w-full mx-auto p-4 lg:p-8 space-y-6">
        {loading ? (
          <div className="text-center py-32 space-y-3">
            <div className="w-12 h-12 border-4 border-blue-500 border-t-transparent rounded-full animate-spin mx-auto"></div>
            <div className="font-display text-sm tracking-wider text-slate-400">
              SYNCHRONIZING GLOBAL SATELLITES...
            </div>
          </div>
        ) : !activeGame ? (
          /* Empty State: No Game Loaded */
          <div className="text-center py-24 p-8 rounded-2xl bg-tactical-900/60 border border-tactical-800 max-w-xl mx-auto space-y-5">
            <div className="w-16 h-16 rounded-2xl bg-blue-600/20 border border-blue-500/40 flex items-center justify-center mx-auto text-blue-400">
              <ShieldAlert className="w-8 h-8" />
            </div>
            <div>
              <h2 className="font-display text-xl font-bold text-white mb-1">
                NO ACTIVE WAR ROOM FOUND
              </h2>
              <p className="text-xs text-slate-400 leading-relaxed">
                Log in or enlist as a commander to configure your nation, deploy garrisons, and embark on world conquest against 9 AI factions!
              </p>
            </div>
            <div className="flex justify-center gap-3">
              {currentUser ? (
                <button
                  onClick={() => setShowNewGameModal(true)}
                  className="px-6 py-2.5 rounded-lg bg-blue-600 hover:bg-blue-500 font-bold font-display text-xs tracking-wider shadow-lg shadow-blue-600/30 flex items-center gap-2"
                >
                  <Plus className="w-4 h-4" />
                  INITIALIZE MATCH
                </button>
              ) : (
                <button
                  onClick={() => setShowAuthModal(true)}
                  className="px-6 py-2.5 rounded-lg bg-blue-600 hover:bg-blue-500 font-bold font-display text-xs tracking-wider shadow-lg shadow-blue-600/30 flex items-center gap-2"
                >
                  LOG IN / SIGN UP
                </button>
              )}
            </div>
          </div>
        ) : (
          /* Active War Room View */
          <>
            {/* Live AI Commentary Ticker */}
            <CommentaryTicker
              commentary={activeGame.recentCommentary}
              onSaveApiKey={handleSaveApiKey}
              customApiKey={customApiKey}
            />

            {/* Supreme World Conquest Victory Alert */}
            {activeGame.status === 'completed' && (
              <div className="p-6 rounded-2xl bg-gradient-to-r from-emerald-950 via-tactical-900 to-emerald-950 border-2 border-emerald-500 text-center shadow-2xl space-y-3">
                <Trophy className="w-16 h-16 text-amber-400 mx-auto animate-bounce" />
                <h2 className="font-display text-2xl font-black text-white tracking-widest uppercase">
                  WORLD CONQUERED: {activeGame.winnerName} WINS!
                </h2>
                <p className="text-xs text-slate-300 max-w-lg mx-auto leading-relaxed">
                  Every sovereign nation on the map has capitulated. A new global order has been established after {activeGame.roundNumber} rounds and {activeGame.turnNumber} maneuvers!
                </p>
                {currentUser && (
                  <button
                    onClick={() => setShowNewGameModal(true)}
                    className="px-6 py-2.5 rounded-lg bg-emerald-600 hover:bg-emerald-500 text-white font-bold font-display text-xs tracking-wider shadow-lg shadow-emerald-600/30"
                  >
                    START NEW CAMPAIGN
                  </button>
                )}
              </div>
            )}

            {/* Interactive World Map */}
            <WorldMap
              game={activeGame}
              selectedAttackerId={selectedAttackerId}
              selectedDefenderId={selectedDefenderId}
              onSelectAttacker={setSelectedAttackerId}
              onSelectDefender={setSelectedDefenderId}
              isPlayerTurn={isPlayerTurn}
              isSpectator={isSpectator}
              activeBattle={activeGame.activeBattle}
            />

            {/* Active Dice Combat Arena (When attack is initiated) */}
            {activeGame.activeBattle && (
              <BattleArena
                game={activeGame}
                activeBattle={activeGame.activeBattle}
                onClashStep={handleClashStep}
                onStopAttack={handleStopAttack}
                onConquestTransfer={handleConquestTransfer}
                isSpectator={isSpectator}
              />
            )}

            {/* Turn Controls & Actions */}
            <TurnControls
              game={activeGame}
              isPlayerTurn={isPlayerTurn}
              isSpectator={isSpectator}
              selectedAttackerId={selectedAttackerId}
              selectedDefenderId={selectedDefenderId}
              onStartAttack={handleStartAttack}
              onReinforce={handleReinforce}
              onTransfer={handleTransfer}
              onEndTurn={handleEndTurn}
              onStepNpc={handleStepNpc}
              onFastForward={handleFastForward}
              isSimulatingNpc={isSimulatingNpc}
            />

            {/* NPC Strategic Decision Log */}
            <NpcDecisionLog
              decisionLog={activeGame.npcDecisionLog}
              players={activeGame.players}
            />
          </>
        )}
      </main>

      {/* Modals */}
      <AuthModal
        isOpen={showAuthModal}
        onClose={() => setShowAuthModal(false)}
        onSuccess={handleAuthSuccess}
      />

      <NewGameModal
        isOpen={showNewGameModal}
        onClose={() => setShowNewGameModal(false)}
        onGameCreated={handleGameCreated}
      />

      <AnalyticsModal
        isOpen={showAnalyticsModal}
        onClose={() => setShowAnalyticsModal(false)}
        currentUser={currentUser}
      />

      <ReplayModal
        isOpen={showReplayModal}
        onClose={() => setShowReplayModal(false)}
        battleHistory={activeGame?.battleHistory || []}
        countries={activeGame?.countries || {}}
      />
    </div>
  );
}
