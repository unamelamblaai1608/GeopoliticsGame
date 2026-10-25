import React, { useState } from 'react';
import {
  ShieldAlert,
  Swords,
  FastForward,
  Play,
  RotateCcw,
  ArrowRightLeft,
  ChevronRight,
  Sparkles,
  Zap
} from 'lucide-react';
import { sound } from '../utils/audio';

export default function TurnControls({
  game,
  isPlayerTurn,
  isSpectator,
  selectedAttackerId,
  selectedDefenderId,
  onStartAttack,
  onReinforce,
  onTransfer,
  onEndTurn,
  onStepNpc,
  onFastForward,
  isSimulatingNpc
}) {
  const [transferFrom, setTransferFrom] = useState('');
  const [transferTo, setTransferTo] = useState('');
  const [transferCount, setTransferCount] = useState(1);
  const [showTransferModal, setShowTransferModal] = useState(false);

  // Reinforcement allocations state: { [countryId]: count }
  const [allocations, setAllocations] = useState({});

  if (!game) return null;

  const humanPlayer = game.players.find(p => p.isHuman);
  const activePlayer = game.players.find(p => p.id === game.currentTurnPlayerId);
  const ownedCountries = Object.values(game.countries).filter(c => c.ownerId === humanPlayer?.id);

  const pendingReinforcements = game.pendingReinforcements || 0;
  const needsReinforcement = isPlayerTurn && pendingReinforcements > 0;

  // Initialize reinforcement allocations if not set
  const currentTotalAllocated = Object.values(allocations).reduce((a, b) => a + (b || 0), 0);

  const handleAllocationChange = (countryId, val) => {
    const num = Math.max(0, parseInt(val, 10) || 0);
    setAllocations(prev => ({
      ...prev,
      [countryId]: num
    }));
  };

  const handleReinforceSubmit = () => {
    sound.playReinforce();
    onReinforce(allocations);
    setAllocations({});
  };

  const handleTransferSubmit = () => {
    if (!transferFrom || !transferTo || transferCount <= 0) return;
    onTransfer(transferFrom, transferTo, transferCount);
    setShowTransferModal(false);
  };

  return (
    <div className="bg-tactical-900 border border-tactical-700 rounded-xl p-4 shadow-xl">
      {/* Turn Banner */}
      <div className="flex flex-wrap items-center justify-between pb-3 border-b border-tactical-800 gap-3">
        <div className="flex items-center gap-3">
          <div
            className="w-3.5 h-3.5 rounded-full"
            style={{ backgroundColor: activePlayer?.color || '#3b82f6' }}
          ></div>
          <div>
            <div className="text-xs font-mono uppercase tracking-wider text-slate-400">
              Active Initiative
            </div>
            <div className="font-display text-base font-bold text-white flex items-center gap-2">
              {isPlayerTurn ? (
                <span className="text-blue-400">YOUR COMMAND PHASE</span>
              ) : (
                <span>{activePlayer?.name || 'Enemy maneuvers'}</span>
              )}
              {isPlayerTurn && (
                <span className="text-xs bg-blue-500/20 text-blue-300 border border-blue-500/40 px-2 py-0.5 rounded-full">
                  Action Required
                </span>
              )}
            </div>
          </div>
        </div>

        <div className="flex items-center gap-4 text-xs font-mono">
          <div className="bg-tactical-950 px-3 py-1.5 rounded-lg border border-tactical-800 text-slate-300">
            Round: <span className="text-amber-400 font-bold">{game.roundNumber}</span>
          </div>
          <div className="bg-tactical-950 px-3 py-1.5 rounded-lg border border-tactical-800 text-slate-300">
            Turn: <span className="text-cyan-400 font-bold">{game.turnNumber}</span>
          </div>
        </div>
      </div>

      {/* PHASE 1: REINFORCEMENTS REQUIRED */}
      {needsReinforcement && !isSpectator && (
        <div className="my-4 p-4 rounded-xl bg-gradient-to-r from-blue-950/70 to-tactical-950 border border-blue-500/50">
          <div className="flex items-center justify-between mb-3">
            <div className="flex items-center gap-2 text-blue-400 font-bold text-sm">
              <Sparkles className="w-4 h-4 animate-spin" />
              DEPLOY ROUND {game.roundNumber} REINFORCEMENTS
            </div>
            <div className="text-xs font-mono">
              Troops to allocate:{' '}
              <span className="text-amber-400 font-extrabold text-sm">
                {pendingReinforcements - currentTotalAllocated}
              </span>{' '}
              / {pendingReinforcements}
            </div>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-3 mb-4">
            {ownedCountries.map(country => (
              <div
                key={country.id}
                className="p-2.5 rounded-lg bg-tactical-900 border border-tactical-700 flex items-center justify-between"
              >
                <div>
                  <div className="text-xs font-bold text-slate-200">
                    {country.flag} {country.name}
                  </div>
                  <div className="text-[11px] text-slate-400">Current: {country.soldiers}</div>
                </div>
                <div className="flex items-center gap-1.5">
                  <input
                    type="number"
                    min="0"
                    max={pendingReinforcements}
                    value={allocations[country.id] || ''}
                    placeholder="0"
                    onChange={(e) => handleAllocationChange(country.id, e.target.value)}
                    className="w-14 px-2 py-1 bg-tactical-950 border border-tactical-600 rounded text-center text-sm font-mono text-cyan-300 focus:outline-none focus:border-blue-400"
                  />
                </div>
              </div>
            ))}
          </div>

          <button
            onClick={handleReinforceSubmit}
            disabled={currentTotalAllocated !== pendingReinforcements}
            className="w-full py-2.5 rounded-lg bg-blue-600 hover:bg-blue-500 active:scale-[0.99] disabled:opacity-40 disabled:cursor-not-allowed text-white font-bold font-display text-sm tracking-wide shadow-lg shadow-blue-600/30 transition-all"
          >
            CONFIRM & DEPLOY {currentTotalAllocated} SOLDIERS
          </button>
        </div>
      )}

      {/* PHASE 2: ATTACK & MANEUVER PHASE */}
      {isPlayerTurn && !needsReinforcement && !isSpectator && (
        <div className="my-4 space-y-4">
          {/* Attack Bar */}
          <div className="p-4 rounded-xl bg-tactical-950 border border-tactical-800 flex flex-wrap items-center justify-between gap-4">
            <div className="flex items-center gap-3">
              <div className="p-2 rounded-lg bg-red-500/10 border border-red-500/30 text-red-400">
                <Swords className="w-5 h-5" />
              </div>
              <div>
                <div className="text-xs font-bold uppercase tracking-wider text-slate-300">
                  Combat Target Selection
                </div>
                <div className="text-xs text-slate-400">
                  {selectedAttackerId && selectedDefenderId ? (
                    <span className="text-cyan-300 font-semibold">
                      {game.countries[selectedAttackerId]?.name} ({game.countries[selectedAttackerId]?.soldiers} troops)
                      {' '}&rarr;{' '}
                      <span className="text-red-400">
                        {game.countries[selectedDefenderId]?.name} ({game.countries[selectedDefenderId]?.soldiers} troops)
                      </span>
                    </span>
                  ) : selectedAttackerId ? (
                    <span className="text-amber-400">
                      Attacking from {game.countries[selectedAttackerId]?.name}. Now select an enemy territory.
                    </span>
                  ) : (
                    <span>Click one of your territories with &ge; 2 soldiers on the map to begin.</span>
                  )}
                </div>
              </div>
            </div>

            <div className="flex items-center gap-3">
              {selectedAttackerId && selectedDefenderId && !game.hasAttackedThisTurn && (
                <button
                  onClick={onStartAttack}
                  className="px-5 py-2 rounded-lg bg-red-600 hover:bg-red-500 active:scale-95 text-white font-bold font-display text-xs tracking-wider shadow-lg shadow-red-600/40 flex items-center gap-2"
                >
                  <Swords className="w-4 h-4" />
                  INITIATE INVASION
                </button>
              )}

              {ownedCountries.length > 1 && !game.activeBattle && (
                <button
                  onClick={() => setShowTransferModal(!showTransferModal)}
                  className="px-4 py-2 rounded-lg bg-tactical-800 hover:bg-tactical-700 border border-tactical-600 text-slate-200 text-xs font-semibold flex items-center gap-1.5"
                >
                  <ArrowRightLeft className="w-3.5 h-3.5 text-cyan-400" />
                  Transfer Reserves
                </button>
              )}

              <button
                onClick={onEndTurn}
                className="px-4 py-2 rounded-lg bg-slate-800 hover:bg-slate-700 border border-slate-600 text-slate-200 text-xs font-semibold flex items-center gap-1.5"
              >
                <span>{game.hasAttackedThisTurn ? 'Conclude Turn' : 'Skip Turn'}</span>
                <ChevronRight className="w-3.5 h-3.5" />
              </button>
            </div>
          </div>

          {/* Transfer Reserves Modal / Drawer */}
          {showTransferModal && (
            <div className="p-4 rounded-xl bg-tactical-950 border border-tactical-700 space-y-3">
              <div className="text-xs font-bold text-slate-300 uppercase tracking-wider flex items-center gap-1.5">
                <ArrowRightLeft className="w-4 h-4 text-cyan-400" />
                Redeploy Garrison Troops (Between Conquered Nations)
              </div>
              <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
                <div>
                  <label className="text-[11px] text-slate-400 block mb-1">From Territory</label>
                  <select
                    value={transferFrom}
                    onChange={(e) => setTransferFrom(e.target.value)}
                    className="w-full px-2.5 py-1.5 bg-tactical-900 border border-tactical-700 rounded text-xs text-slate-200"
                  >
                    <option value="">Select origin...</option>
                    {ownedCountries.filter(c => c.soldiers > 1).map(c => (
                      <option key={c.id} value={c.id}>
                        {c.flag} {c.name} ({c.soldiers} soldiers)
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="text-[11px] text-slate-400 block mb-1">To Territory</label>
                  <select
                    value={transferTo}
                    onChange={(e) => setTransferTo(e.target.value)}
                    className="w-full px-2.5 py-1.5 bg-tactical-900 border border-tactical-700 rounded text-xs text-slate-200"
                  >
                    <option value="">Select destination...</option>
                    {ownedCountries.filter(c => c.id !== transferFrom).map(c => (
                      <option key={c.id} value={c.id}>
                        {c.flag} {c.name} ({c.soldiers} soldiers)
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="text-[11px] text-slate-400 block mb-1">
                    Soldiers (Max: {transferFrom ? Math.max(0, game.countries[transferFrom].soldiers - 1) : 0})
                  </label>
                  <input
                    type="number"
                    min="1"
                    max={transferFrom ? Math.max(1, game.countries[transferFrom].soldiers - 1) : 1}
                    value={transferCount}
                    onChange={(e) => setTransferCount(parseInt(e.target.value, 10))}
                    className="w-full px-2.5 py-1.5 bg-tactical-900 border border-tactical-700 rounded text-xs text-slate-200"
                  />
                </div>
              </div>

              <div className="flex justify-end gap-2 pt-2">
                <button
                  onClick={() => setShowTransferModal(false)}
                  className="px-3 py-1.5 text-xs text-slate-400 hover:text-white"
                >
                  Cancel
                </button>
                <button
                  onClick={handleTransferSubmit}
                  className="px-4 py-1.5 bg-blue-600 hover:bg-blue-500 rounded text-xs font-bold text-white shadow"
                >
                  Execute Transfer
                </button>
              </div>
            </div>
          )}
        </div>
      )}

      {/* PHASE 3: NPC TURN CONTROLS & FAST-FORWARD */}
      {!isPlayerTurn && !isSpectator && (
        <div className="my-4 p-4 rounded-xl bg-tactical-950 border border-tactical-800 flex flex-wrap items-center justify-between gap-4">
          <div className="flex items-center gap-3">
            <div className="p-2 rounded-lg bg-amber-500/10 border border-amber-500/30 text-amber-400">
              <Zap className="w-5 h-5" />
            </div>
            <div>
              <div className="text-xs font-bold text-slate-200">
                SIMULATING GLOBAL NPC STRATEGY
              </div>
              <div className="text-xs text-slate-400">
                NPCs are evaluating territory strengths, frontline borders, and attack dividends.
              </div>
            </div>
          </div>

          <div className="flex items-center gap-3">
            <button
              onClick={onStepNpc}
              disabled={isSimulatingNpc}
              className="px-4 py-2 rounded-lg bg-tactical-800 hover:bg-tactical-700 border border-tactical-600 active:scale-95 disabled:opacity-50 text-slate-200 text-xs font-semibold flex items-center gap-2"
            >
              <Play className="w-3.5 h-3.5 fill-slate-300" />
              Step 1 Move
            </button>

            <button
              onClick={onFastForward}
              disabled={isSimulatingNpc}
              className="px-5 py-2 rounded-lg bg-gradient-to-r from-amber-600 to-orange-600 hover:from-amber-500 hover:to-orange-500 active:scale-95 disabled:opacity-50 text-white font-bold font-display text-xs tracking-wider shadow-lg shadow-amber-600/30 flex items-center gap-2"
            >
              <FastForward className="w-4 h-4 fill-white" />
              {isSimulatingNpc ? 'SIMULATING MOVES...' : 'FAST-FORWARD TO MY TURN'}
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

