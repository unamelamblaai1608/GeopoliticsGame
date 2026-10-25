import React, { useState, useEffect } from 'react';
import { Globe2, Flag, Users, ChevronRight, X, Sparkles, Shield } from 'lucide-react';
import { getCountries, createGame } from '../api';

export default function NewGameModal({ isOpen, onClose, onGameCreated }) {
  const [countries, setCountries] = useState([]);
  const [selectedCountryId, setSelectedCountryId] = useState('za'); // South Africa preselected!
  const [title, setTitle] = useState('South African Global Initiative');
  const [loading, setLoading] = useState(false);
  const [filterContinent, setFilterContinent] = useState('All');
  const [search, setSearch] = useState('');

  useEffect(() => {
    if (isOpen) {
      getCountries().then(setCountries).catch(console.error);
    }
  }, [isOpen]);

  if (!isOpen) return null;

  const continents = ['All', 'Africa', 'Europe', 'Asia', 'South America', 'North America', 'Oceania'];

  const filtered = countries.filter(c => {
    const matchesContinent = filterContinent === 'All' || c.continent.includes(filterContinent);
    const matchesSearch = c.name.toLowerCase().includes(search.toLowerCase());
    return matchesContinent && matchesSearch;
  });

  const selectedCountry = countries.find(c => c.id === selectedCountryId);

  const handleCreate = async () => {
    if (!selectedCountryId) return;
    setLoading(true);
    try {
      const newGame = await createGame(selectedCountryId, title || 'Global Campaign');
      onGameCreated(newGame);
      onClose();
    } catch (err) {
      alert(err.message || 'Error creating game');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-md">
      <div className="w-full max-w-4xl bg-tactical-950 border border-tactical-700 rounded-2xl shadow-2xl overflow-hidden flex flex-col max-h-[92vh]">
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 bg-tactical-900 border-b border-tactical-800">
          <div className="flex items-center gap-3">
            <Globe2 className="w-6 h-6 text-blue-400" />
            <div>
              <h2 className="font-display text-lg font-bold text-white tracking-wide">
                COMMENCE NEW GLOBAL CONQUEST
              </h2>
              <p className="text-xs text-slate-400">
                Choose your home nation. 9 rival nations will be randomly drawn into your arena.
              </p>
            </div>
          </div>
          <button
            onClick={onClose}
            className="p-1.5 rounded-lg bg-tactical-800 hover:bg-tactical-700 text-slate-400 hover:text-white"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Content Body */}
        <div className="p-6 overflow-y-auto custom-scrollbar flex-1 space-y-5">
          {/* Campaign Title Input */}
          <div className="p-4 rounded-xl bg-tactical-900 border border-tactical-800">
            <label className="block text-xs font-bold uppercase tracking-wider text-slate-300 mb-1.5">
              Operation Codename / Match Title
            </label>
            <input
              type="text"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              placeholder="e.g. Operation Southern Shield"
              className="w-full px-3.5 py-2 bg-tactical-950 border border-tactical-700 rounded-lg text-sm text-white font-display focus:outline-none focus:border-blue-500"
            />
          </div>

          {/* Staging Country Selector */}
          <div>
            <div className="flex flex-wrap items-center justify-between gap-3 mb-3">
              <label className="text-xs font-bold uppercase tracking-wider text-slate-300">
                Select Your Sovereign Nation
              </label>

              {/* Continents Filter */}
              <div className="flex flex-wrap gap-1 text-xs">
                {continents.map(cont => (
                  <button
                    key={cont}
                    onClick={() => setFilterContinent(cont)}
                    className={`px-2.5 py-1 rounded-md text-[11px] font-medium transition ${
                      filterContinent === cont
                        ? 'bg-blue-600 text-white font-bold'
                        : 'bg-tactical-900 text-slate-400 hover:text-slate-200'
                    }`}
                  >
                    {cont}
                  </button>
                ))}
              </div>
            </div>

            {/* Countries Grid */}
            <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-3 max-h-72 overflow-y-auto custom-scrollbar pr-1">
              {filtered.map(c => {
                const isSelected = selectedCountryId === c.id;
                const isSouthAfrica = c.id === 'za';

                return (
                  <button
                    key={c.id}
                    onClick={() => setSelectedCountryId(c.id)}
                    className={`p-3 rounded-xl border text-left transition-all relative flex flex-col justify-between ${
                      isSelected
                        ? 'bg-blue-950/80 border-blue-400 ring-2 ring-blue-500/50 shadow-lg'
                        : 'bg-tactical-900/80 border-tactical-800 hover:border-tactical-600'
                    }`}
                  >
                    {isSouthAfrica && (
                      <span className="absolute -top-2 -right-2 bg-amber-500 text-black text-[9px] font-bold px-1.5 py-0.5 rounded-full shadow">
                        Case Study Star
                      </span>
                    )}

                    <div className="flex items-center gap-2 mb-2">
                      <span className="text-2xl">{c.flag}</span>
                      <div className="truncate">
                        <div className="font-display text-xs font-bold text-white truncate">
                          {c.name}
                        </div>
                        <div className="text-[10px] text-slate-400">{c.continent}</div>
                      </div>
                    </div>

                    <div className="flex items-center justify-between pt-2 border-t border-tactical-800/80 text-[11px]">
                      <span className="text-slate-400 font-mono">
                        {(c.population / 1_000_000).toFixed(1)}M pop
                      </span>
                      <span className="flex items-center font-mono font-bold text-cyan-400">
                        <Users className="w-3 h-3 mr-0.5" />
                        {c.initialSoldiers} troops
                      </span>
                    </div>
                  </button>
                );
              })}
            </div>
          </div>

          {/* Selected Country Briefing */}
          {selectedCountry && (
            <div className="p-4 rounded-xl bg-gradient-to-r from-blue-950/60 to-tactical-900 border border-blue-500/40 flex flex-wrap items-center justify-between gap-4">
              <div className="flex items-center gap-3">
                <span className="text-4xl">{selectedCountry.flag}</span>
                <div>
                  <div className="text-xs uppercase text-blue-400 font-mono font-bold">
                    Designated Flagship Territory
                  </div>
                  <div className="font-display text-base font-bold text-white">
                    {selectedCountry.name}
                  </div>
                  <div className="text-xs text-slate-400">
                    Population: {(selectedCountry.population / 1_000_000).toFixed(2)} Million
                  </div>
                </div>
              </div>

              <div className="text-right">
                <div className="text-xs text-slate-400">Initial Standing Army</div>
                <div className="font-mono text-2xl font-extrabold text-cyan-300">
                  {selectedCountry.initialSoldiers} Soldiers
                </div>
              </div>
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="px-6 py-4 bg-tactical-900 border-t border-tactical-800 flex justify-between items-center">
          <button
            onClick={onClose}
            className="px-4 py-2 text-xs font-medium text-slate-400 hover:text-white"
          >
            Cancel
          </button>

          <button
            onClick={handleCreate}
            disabled={loading || !selectedCountryId}
            className="px-6 py-2.5 rounded-lg bg-blue-600 hover:bg-blue-500 active:scale-95 disabled:opacity-50 text-white font-bold font-display text-sm tracking-wide shadow-lg shadow-blue-600/40 transition flex items-center gap-2"
          >
            <span>{loading ? 'MOBILIZING NATIONS...' : 'COMMENCE CAMPAIGN'}</span>
            <ChevronRight className="w-4 h-4" />
          </button>
        </div>
      </div>
    </div>
  );
}

