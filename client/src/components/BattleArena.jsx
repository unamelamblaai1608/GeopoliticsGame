import React, { useState } from 'react';
import { Swords, Shield, AlertCircle, Trophy, UserCheck, Play, FastForward, Square, Check } from 'lucide-react';
import { sound } from '../utils/audio';
import confetti from 'canvas-confetti';

// Visual Die Component rendering 1 to 6 pips
function Die({ value, side = 'attacker', rolling = false }) {
  const pipPositions = {
    1: ['center'],
    2: ['top-right', 'bottom-left'],
    3: ['top-right', 'center', 'bottom-left'],
    4: ['top-left', 'top-right', 'bottom-left', 'bottom-right'],
    5: ['top-left', 'top-right', 'center', 'bottom-left', 'bottom-right'],
    6: ['top-left', 'top-right', 'center-left', 'center-right', 'bottom-left', 'bottom-right']
  };

  const pips = pipPositions[value] || ['center'];

  const bgClass = side === 'attacker'
    ? 'bg-gradient-to-br from-blue-500 to-indigo-700 text-white border-blue-300 shadow-blue-500/50'
    : 'bg-gradient-to-br from-red-500 to-rose-700 text-white border-red-300 shadow-red-500/50';

  return (
    <div
      className={`relative w-12 h-12 rounded-xl border-2 shadow-lg flex items-center justify-center p-2 transform transition-transform ${bgClass} ${
        rolling ? 'animate-dice-roll' : 'scale-100'
      }`}
    >
      <div className="relative w-full h-full grid grid-cols-3 grid-rows-3">
        {pips.includes('top-left') && <div className="col-start-1 row-start-1 w-2 h-2 rounded-full bg-white mx-auto my-auto shadow-sm"></div>}
        {pips.includes('top-right') && <div className="col-start-3 row-start-1 w-2 h-2 rounded-full bg-white mx-auto my-auto shadow-sm"></div>}
        {pips.includes('center-left') && <div className="col-start-1 row-start-2 w-2 h-2 rounded-full bg-white mx-auto my-auto shadow-sm"></div>}
        {pips.includes('center') && <div className="col-start-2 row-start-2 w-2 h-2 rounded-full bg-white mx-auto my-auto shadow-sm"></div>}
        {pips.includes('center-right') && <div className="col-start-3 row-start-2 w-2 h-2 rounded-full bg-white mx-auto my-auto shadow-sm"></div>}
        {pips.includes('bottom-left') && <div className="col-start-1 row-start-3 w-2 h-2 rounded-full bg-white mx-auto my-auto shadow-sm"></div>}
        {pips.includes('bottom-right') && <div className="col-start-3 row-start-3 w-2 h-2 rounded-full bg-white mx-auto my-auto shadow-sm"></div>}
      </div>
    </div>
  );
}

export default function BattleArena({
  game,
  activeBattle,
  onClashStep,
  onResolveBattle,
  onStopAttack,
  onConquestTransfer,
  isSpectator
}) {
  const [isRolling, setIsRolling] = useState(false);
  const [conquestTransferCount, setConquestTransferCount] = useState(0);

  if (!activeBattle) return null;

  const attackerCountry = game.countries[activeBattle.fromCountryId];
  const defenderCountry = game.countries[activeBattle.toCountryId];
  const attackerPlayer = game.players.find(p => p.id === attackerCountry?.ownerId);
  const defenderPlayer = game.players.find(p => p.id === defenderCountry?.ownerId);

  const lastRound = activeBattle.rounds && activeBattle.rounds.length > 0
    ? activeBattle.rounds[activeBattle.rounds.length - 1]
    : null;

  const maxTransferable = Math.max(0, (attackerCountry?.soldiers || 1) - 1);
  const isConquered = activeBattle.status === 'conquered';
  const isDefeated = activeBattle.status === 'attacker_defeated';
  const isOngoing = activeBattle.status === 'ongoing';

  const handleRollClick = async () => {
    if (isRolling || !isOngoing || isSpectator) return;
    setIsRolling(true);
    sound.playDiceRoll();

    setTimeout(async () => {
      try {
        const res = await onClashStep();
        sound.playClash();
        if (res?.clash?.attackerLosses > 0 || res?.clash?.defenderLosses > 0) {
          sound.playHit();
        }
        if (res?.game?.activeBattle?.status === 'conquered') {
          sound.playConquest();
          confetti({ particleCount: 70, spread: 60, origin: { y: 0.6 } });
        }
      } finally {
        setIsRolling(false);
      }
    }, 500);
  };

  const handleResolveClick = async () => {
    if (isRolling || !isOngoing || isSpectator) return;
    setIsRolling(true);
    sound.playDiceRoll();

    try {
      while (true) {
        const res = await onClashStep();
        if (!res?.game?.activeBattle || res.game.activeBattle.status !== 'ongoing') {
          if (res?.game?.activeBattle?.status === 'conquered') {
            sound.playConquest();
            confetti({ particleCount: 100, spread: 80, origin: { y: 0.6 } });
          }
          break;
        }
      }
    } finally {
      setIsRolling(false);
    }
  };

  const handleTransferSubmit = () => {
    onConquestTransfer(conquestTransferCount);
  };

  return (
    <div className="relative w-full bg-tactical-950 border-2 border-tactical-700 rounded-xl p-5 shadow-2xl overflow-hidden">
      {/* Background Combat Accents */}
      <div className="absolute top-0 left-0 right-0 h-1 bg-gradient-to-r from-blue-500 via-amber-500 to-red-500"></div>

      {/* Header Combat Arena Status */}
      <div className="flex items-center justify-between pb-4 border-b border-tactical-800">
        <div className="flex items-center gap-2">
          <Swords className="w-6 h-6 text-red-500 animate-pulse" />
          <h2 className="font-display text-lg font-bold text-white tracking-wider">
            COMBAT ARENA: TACTICAL CLASH
          </h2>
        </div>
        <div className="text-xs font-mono text-slate-400">
          Round Clash: <span className="text-amber-400 font-bold">{activeBattle.rounds?.length || 0}</span>
        </div>
      </div>

      {/* Opposing Forces Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6 my-6">
        {/* Attacker Panel */}
        <div className="flex items-center justify-between p-4 rounded-xl bg-tactical-900 border border-blue-500/40 shadow-inner">
          <div className="flex items-center gap-3">
            <span className="text-4xl">{attackerCountry?.flag}</span>
            <div>
              <div className="text-xs uppercase tracking-wider text-blue-400 font-bold">Attacker</div>
              <div className="font-display text-lg font-bold text-white">{attackerCountry?.name}</div>
              <div className="text-xs text-slate-400">Commander: {attackerPlayer?.name}</div>
            </div>
          </div>
          <div className="text-right">
            <div className="text-xs text-slate-400">Troops Active / Garrison</div>
            <div className="font-mono text-2xl font-extrabold text-blue-400">
              {Math.max(0, (attackerCountry?.soldiers || 1) - 1)}{' '}
              <span className="text-xs text-slate-400">/ 1 held</span>
            </div>
          </div>
        </div>

        {/* Defender Panel */}
        <div className="flex items-center justify-between p-4 rounded-xl bg-tactical-900 border border-red-500/40 shadow-inner">
          <div className="flex items-center gap-3">
            <span className="text-4xl">{defenderCountry?.flag}</span>
            <div>
              <div className="text-xs uppercase tracking-wider text-red-400 font-bold">Defender</div>
              <div className="font-display text-lg font-bold text-white">{defenderCountry?.name}</div>
              <div className="text-xs text-slate-400">Commander: {defenderPlayer?.name}</div>
            </div>
          </div>
          <div className="text-right">
            <div className="text-xs text-slate-400">Defending Troops</div>
            <div className="font-mono text-2xl font-extrabold text-red-400">
              {defenderCountry?.soldiers || 0}
            </div>
          </div>
        </div>
      </div>

      {/* Active Clash Dice Table */}
      {lastRound && lastRound.pairings && lastRound.pairings.length > 0 && (
        <div className="my-6 p-4 rounded-xl bg-tactical-900/60 border border-tactical-800">
          <div className="text-xs font-semibold text-slate-400 uppercase tracking-wider mb-3 text-center">
            Dice Clash Pairings (1 Die = 1 Soldier)
          </div>

          <div className="space-y-3 max-h-56 overflow-y-auto custom-scrollbar pr-2">
            {lastRound.pairings.map((pair, idx) => {
              let outcomeBadge = (
                <span className="text-xs font-bold text-slate-400 bg-slate-800 px-2 py-1 rounded">
                  Tie (No Casualty)
                </span>
              );

              if (pair.outcome === 'attacker_won') {
                outcomeBadge = (
                  <span className="text-xs font-bold text-emerald-400 bg-emerald-950/70 border border-emerald-800 px-2 py-1 rounded">
                    Attacker Won (-1 Def)
                  </span>
                );
              } else if (pair.outcome === 'defender_won') {
                outcomeBadge = (
                  <span className="text-xs font-bold text-rose-400 bg-rose-950/70 border border-rose-800 px-2 py-1 rounded">
                    Defender Won (-1 Att)
                  </span>
                );
              }

              return (
                <div
                  key={idx}
                  className="flex items-center justify-between p-2 rounded-lg bg-tactical-950/70 border border-tactical-800"
                >
                  <div className="flex items-center gap-3">
                    <span className="text-xs font-mono text-slate-400">Squad {idx + 1}</span>
                    <Die value={pair.attackerDie} side="attacker" rolling={isRolling} />
                  </div>

                  <div className="flex flex-col items-center">
                    <span className="text-xs text-slate-400 mb-1">vs</span>
                    {outcomeBadge}
                  </div>

                  <div className="flex items-center gap-3">
                    <Die value={pair.defenderDie} side="defender" rolling={isRolling} />
                  </div>
                </div>
              );
            })}
          </div>

          {/* Casualty Summary Footer */}
          <div className="mt-3 pt-3 border-t border-tactical-800 flex justify-between text-xs font-mono">
            <span className="text-blue-400">
              Casualties: -{lastRound.attackerLosses} ({attackerCountry?.soldiers} left)
            </span>
            <span className="text-red-400">
              Casualties: -{lastRound.defenderLosses} ({defenderCountry?.soldiers} left)
            </span>
          </div>
        </div>
      )}

      {/* Post-Conquest Territory Allocation Interface */}
      {isConquered && (
        <div className="my-6 p-5 rounded-xl bg-gradient-to-r from-emerald-950/80 via-tactical-900 to-emerald-950/80 border-2 border-emerald-500 shadow-2xl text-center">
          <Trophy className="w-12 h-12 text-amber-400 mx-auto mb-2 animate-bounce" />
          <h3 className="font-display text-xl font-bold text-white mb-1">
            TERRITORY CONQUERED: {defenderCountry?.name}!
          </h3>
          <p className="text-sm text-slate-300 max-w-lg mx-auto mb-4">
            You secured <span className="text-emerald-400 font-bold">+{activeBattle.conquerBonus} veteran soldiers</span> from the annexation!
            Select additional troops to mobilize into your new province:
          </p>

          <div className="flex flex-col items-center max-w-sm mx-auto mb-4">
            <div className="flex items-center justify-between w-full text-xs text-slate-400 mb-1">
              <span>Retain at base (min 1)</span>
              <span className="font-bold text-emerald-400">Transfer: +{conquestTransferCount} troops</span>
            </div>
            <input
              type="range"
              min="0"
              max={maxTransferable}
              value={conquestTransferCount}
              onChange={(e) => setConquestTransferCount(parseInt(e.target.value, 10))}
              className="w-full h-2 bg-tactical-800 rounded-lg appearance-none cursor-pointer accent-emerald-500"
            />
            <div className="flex justify-between w-full text-[11px] text-slate-500 mt-1">
              <span>0 additional</span>
              <span>Max: {maxTransferable} additional</span>
            </div>
          </div>

          {!isSpectator && (
            <button
              onClick={handleTransferSubmit}
              className="px-6 py-2.5 rounded-lg bg-emerald-600 hover:bg-emerald-500 text-white font-bold font-display text-sm tracking-wide shadow-lg shadow-emerald-600/30 transition-all flex items-center gap-2 mx-auto"
            >
              <Check className="w-4 h-4" />
              CONFIRM GARRISON & CONQUEST
            </button>
          )}
        </div>
      )}

      {/* Attacker Defeated Banner */}
      {isDefeated && (
        <div className="my-6 p-4 rounded-xl bg-rose-950/80 border border-rose-600 text-center">
          <AlertCircle className="w-10 h-10 text-rose-400 mx-auto mb-2" />
          <h3 className="font-display text-lg font-bold text-white mb-1">
            INVASION REPULSED
          </h3>
          <p className="text-xs text-slate-300 max-w-md mx-auto">
            Your attacking column was whittled down to a single soldier. Under the rules of engagement, 
            the assault has concluded. Your base territory retains its final soldier.
          </p>
        </div>
      )}

      {/* Combat Controls */}
      {isOngoing && !isSpectator && (
        <div className="flex flex-wrap items-center justify-center gap-4 pt-4 border-t border-tactical-800">
          <button
            onClick={handleRollClick}
            disabled={isRolling}
            className="px-6 py-2.5 rounded-lg bg-blue-600 hover:bg-blue-500 active:scale-95 disabled:opacity-50 text-white font-bold font-display text-sm tracking-wide shadow-lg shadow-blue-600/40 transition-all flex items-center gap-2"
          >
            <Play className="w-4 h-4 fill-white" />
            {isRolling ? 'ROLLING DICE...' : 'ROLL NEXT CLASH'}
          </button>

          <button
            onClick={handleResolveClick}
            disabled={isRolling}
            className="px-5 py-2.5 rounded-lg bg-tactical-800 hover:bg-tactical-700 border border-tactical-600 active:scale-95 disabled:opacity-50 text-slate-200 font-medium text-sm transition-all flex items-center gap-2"
          >
            <FastForward className="w-4 h-4 text-amber-400" />
            RESOLVE TO CONCLUSION
          </button>

          <button
            onClick={onStopAttack}
            disabled={isRolling}
            className="px-4 py-2.5 rounded-lg bg-rose-950 hover:bg-rose-900 border border-rose-800 active:scale-95 disabled:opacity-50 text-rose-300 font-medium text-sm transition-all flex items-center gap-2"
          >
            <Square className="w-4 h-4" />
            CEASEFIRE / STOP ATTACK
          </button>
        </div>
      )}
    </div>
  );
}

