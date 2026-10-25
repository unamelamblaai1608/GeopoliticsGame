import React from 'react';
import { Shield, Crosshair, Users, Crown, Zap, AlertTriangle, Compass } from 'lucide-react';

export default function WorldMap({
  game,
  selectedAttackerId,
  selectedDefenderId,
  onSelectAttacker,
  onSelectDefender,
  isPlayerTurn,
  isSpectator,
  activeBattle
}) {
  if (!game || !game.countries) return null;

  const countriesList = Object.values(game.countries);
  const humanPlayer = game.players.find(p => p.isHuman);
  const activePlayer = game.players.find(p => p.id === game.currentTurnPlayerId);

  // Helper to get owner details
  const getOwner = (ownerId) => {
    return game.players.find(p => p.id === ownerId) || { name: 'Neutral', color: '#64748b' };
  };

  const handleCountryClick = (c) => {
    if (isSpectator) return;
    if (activeBattle && activeBattle.status === 'ongoing') return;

    const isOwnedByHuman = c.ownerId === humanPlayer?.id;

    if (!isPlayerTurn) return;

    if (isOwnedByHuman) {
      // Toggle or select attacker country
      if (c.soldiers >= 2) {
        onSelectAttacker(c.id === selectedAttackerId ? null : c.id);
        if (selectedDefenderId === c.id) onSelectDefender(null);
      }
    } else {
      // Clicked enemy country
      if (selectedAttackerId) {
        onSelectDefender(c.id === selectedDefenderId ? null : c.id);
      }
    }
  };

  return (
    <div className="relative w-full aspect-[950/620] min-h-[500px] max-h-[740px] bg-[#070d18] border-2 border-tactical-700/80 rounded-2xl overflow-hidden shadow-2xl select-none">
      {/* 1. Real World Map SVG Vector Background */}
      <img
        src="/world-map.svg"
        alt="Real World Geopolitical Map"
        className="absolute inset-0 w-full h-full object-fill pointer-events-none select-none opacity-90"
      />

      {/* 2. Tactical Coordinate Graticules (Latitude & Longitude Grid) */}
      <svg className="absolute inset-0 w-full h-full pointer-events-none opacity-20" viewBox="0 0 950 620">
        {/* Equator & Tropics */}
        <line x1="0" y1="310" x2="950" y2="310" stroke="#38bdf8" strokeWidth="1" strokeDasharray="6 6" />
        <line x1="0" y1="210" x2="950" y2="210" stroke="#38bdf8" strokeWidth="0.5" strokeDasharray="4 8" />
        <line x1="0" y1="410" x2="950" y2="410" stroke="#38bdf8" strokeWidth="0.5" strokeDasharray="4 8" />
        
        {/* Meridians */}
        <line x1="237" y1="0" x2="237" y2="620" stroke="#38bdf8" strokeWidth="0.5" strokeDasharray="4 8" />
        <line x1="475" y1="0" x2="475" y2="620" stroke="#38bdf8" strokeWidth="1" strokeDasharray="6 6" />
        <line x1="712" y1="0" x2="712" y2="620" stroke="#38bdf8" strokeWidth="0.5" strokeDasharray="4 8" />

        {/* Tactical Crosshair Marks */}
        <circle cx="475" cy="310" r="16" fill="none" stroke="#38bdf8" strokeWidth="1" />
        <line x1="475" y1="290" x2="475" y2="330" stroke="#38bdf8" strokeWidth="1" />
        <line x1="455" y1="310" x2="495" y2="310" stroke="#38bdf8" strokeWidth="1" />
      </svg>

      {/* 3. Radar Sweep Effect */}
      <div className="absolute -top-[50%] -left-[50%] w-[200%] h-[200%] rounded-full border border-blue-500/10 pointer-events-none animate-radar">
        <div className="w-1/2 h-1/2 bg-gradient-to-br from-blue-500/5 to-transparent"></div>
      </div>

      {/* 4. Active Ballistic Strike / Invasion Vector Line */}
      {selectedAttackerId && selectedDefenderId && (
        <svg className="absolute inset-0 w-full h-full pointer-events-none z-10">
          {(() => {
            const att = game.countries[selectedAttackerId];
            const def = game.countries[selectedDefenderId];
            if (!att || !def) return null;

            // Curved arc for realistic ballistic trajectory
            const mx = (att.x + def.x) / 2;
            const my = Math.min(att.y, def.y) - 8;

            return (
              <g>
                <path
                  d={`M ${att.x}% ${att.y}% Q ${mx}% ${my}% ${def.x}% ${def.y}%`}
                  fill="none"
                  stroke="#ef4444"
                  strokeWidth="3"
                  strokeDasharray="6 4"
                  className="animate-pulse"
                />
                <circle cx={`${att.x}%`} cy={`${att.y}%`} r="6" fill="#38bdf8" />
                <circle cx={`${def.x}%`} cy={`${def.y}%`} r="7" fill="#ef4444" className="animate-ping" />
              </g>
            );
          })()}
        </svg>
      )}

      {/* 5. Countries at their Precise Real-World Geographic Locations */}
      {countriesList.map((country) => {
        const owner = getOwner(country.ownerId);
        const isHuman = country.ownerId === humanPlayer?.id;
        const isAttacker = selectedAttackerId === country.id;
        const isDefender = selectedDefenderId === country.id;
        const isTurnOwner = activePlayer && activePlayer.id === country.ownerId;
        const conquerBonus = Math.max(2, Math.floor(country.initialSoldiers / 2));

        // Border styling based on selection
        let borderClass = 'border-slate-700/80';
        let glowClass = '';
        if (isAttacker) {
          borderClass = 'border-cyan-400 ring-4 ring-cyan-400/40 scale-110';
          glowClass = 'neon-glow';
        } else if (isDefender) {
          borderClass = 'border-red-500 ring-4 ring-red-500/50 scale-110';
          glowClass = 'neon-glow-red';
        } else if (isTurnOwner) {
          borderClass = 'border-amber-400/90 ring-2 ring-amber-400/30';
        }

        return (
          <div
            key={country.id}
            onClick={() => handleCountryClick(country)}
            style={{
              left: `${country.x}%`,
              top: `${country.y}%`,
              transform: 'translate(-50%, -50%)'
            }}
            className={`absolute z-20 cursor-pointer transition-all duration-300 group`}
          >
            {/* Geographic Anchor Dot (pinpointing the real territory centroid) */}
            <div
              className="absolute left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 w-3 h-3 rounded-full border-2 border-white/90 shadow-md pointer-events-none -z-10"
              style={{ backgroundColor: owner.color }}
            >
              {isTurnOwner && (
                <div
                  className="absolute -inset-2 rounded-full animate-ping opacity-75"
                  style={{ backgroundColor: owner.color }}
                ></div>
              )}
            </div>

            {/* Country Tactical Badge */}
            <div
              className={`relative flex items-center gap-2 px-2.5 py-1.5 rounded-lg bg-tactical-950/95 backdrop-blur-md border ${borderClass} ${glowClass} shadow-xl hover:border-cyan-400 hover:scale-110 transition-all`}
            >
              {/* Owner Faction Bar */}
              <div
                className="w-1.5 h-6 rounded-full shrink-0"
                style={{ backgroundColor: owner.color }}
                title={`Commander: ${owner.name}`}
              ></div>

              {/* Flag & Name */}
              <div className="flex flex-col text-left">
                <div className="flex items-center gap-1.5">
                  <span className="text-sm leading-none">{country.flag}</span>
                  <span className="font-display text-[11px] font-bold tracking-tight text-white group-hover:text-cyan-300 whitespace-nowrap">
                    {country.name}
                  </span>
                </div>
                <div className="text-[9px] text-slate-400 font-mono leading-none mt-0.5">
                  {(country.population / 1_000_000).toFixed(0)}M pop • <span className="text-amber-400">+{conquerBonus} bonus</span>
                </div>
              </div>

              {/* Soldier Troops Counter */}
              <div
                className="flex items-center justify-center min-w-[24px] h-6 px-1.5 rounded font-mono text-xs font-black text-white shadow-inner"
                style={{
                  backgroundColor: owner.color,
                  boxShadow: `0 0 8px ${owner.color}80`
                }}
              >
                {country.soldiers}
              </div>

              {/* Selection Status Crosshairs */}
              {isAttacker && (
                <div className="absolute -top-2.5 -right-2 bg-cyan-400 text-black p-0.5 rounded-full shadow-md animate-spin">
                  <Crosshair className="w-3.5 h-3.5" />
                </div>
              )}
              {isDefender && (
                <div className="absolute -top-2.5 -right-2 bg-red-600 text-white p-0.5 rounded-full shadow-md animate-bounce">
                  <AlertTriangle className="w-3.5 h-3.5" />
                </div>
              )}
            </div>

            {/* Detailed Intelligence Popover on Hover */}
            <div className="hidden group-hover:block absolute bottom-full left-1/2 -translate-x-1/2 mb-2 w-56 p-3 bg-tactical-950/95 backdrop-blur-md border border-tactical-600 rounded-xl shadow-2xl z-30 pointer-events-none text-xs">
              <div className="flex items-center justify-between font-bold text-slate-200 border-b border-tactical-800 pb-1.5 mb-1.5">
                <span className="flex items-center gap-1.5">
                  <span className="text-base">{country.flag}</span>
                  <span>{country.name}</span>
                </span>
                <span className="text-[10px] uppercase tracking-wider text-cyan-400 font-mono">
                  {country.continent}
                </span>
              </div>

              <div className="space-y-1.5 text-[11px]">
                <div className="flex justify-between">
                  <span className="text-slate-400">Commander:</span>
                  <span className="font-semibold" style={{ color: owner.color }}>{owner.name}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-slate-400">Population:</span>
                  <span className="font-mono text-slate-200">{(country.population / 1_000_000).toFixed(1)}M</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-slate-400">Active Garrison:</span>
                  <span className="font-bold text-white font-mono">{country.soldiers} soldiers</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-slate-400">Base Soldier Capacity:</span>
                  <span className="font-mono text-slate-300">{country.initialSoldiers}</span>
                </div>
                <div className="flex justify-between text-amber-400 font-bold pt-1 border-t border-tactical-800/80">
                  <span>Conquest Dividend:</span>
                  <span>+{conquerBonus} soldiers</span>
                </div>
              </div>
            </div>
          </div>
        );
      })}

      {/* Map Tactical Telemetry & Coordinates Bar */}
      <div className="absolute bottom-3 left-3 bg-tactical-950/85 backdrop-blur-md border border-tactical-700/80 rounded-xl px-3.5 py-2 flex items-center gap-4 text-xs z-10 shadow-lg">
        <div className="flex items-center gap-2">
          <div className="w-2.5 h-2.5 rounded-full bg-blue-500 shadow-blue-500/50 shadow"></div>
          <span className="text-slate-200 font-semibold text-[11px]">Your Faction</span>
        </div>
        <div className="flex items-center gap-2">
          <div className="w-2.5 h-2.5 rounded-full bg-red-500 shadow-red-500/50 shadow"></div>
          <span className="text-slate-300 font-medium text-[11px]">NPC Factions</span>
        </div>
        <div className="flex items-center gap-1.5 text-amber-400 font-mono text-[11px] pl-2 border-l border-tactical-800">
          <Crown className="w-3.5 h-3.5" />
          <span>Round {game.roundNumber} / Turn {game.turnNumber}</span>
        </div>
      </div>

      {/* Compass Rose */}
      <div className="absolute top-3 right-3 bg-tactical-950/80 backdrop-blur-md border border-tactical-700/60 rounded-xl p-2 text-cyan-400 flex items-center gap-1.5 text-[10px] font-mono shadow-lg">
        <Compass className="w-4 h-4 animate-spin-slow" />
        <span>GLOBAL SAT-FEED</span>
      </div>
    </div>
  );
}
