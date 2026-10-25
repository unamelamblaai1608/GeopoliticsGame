import React, { useState } from 'react';
import { User, Lock, Mail, ShieldCheck, X } from 'lucide-react';
import { login, register } from '../api';

export default function AuthModal({ isOpen, onClose, onSuccess }) {
  const [isRegister, setIsRegister] = useState(false);
  const [identifier, setIdentifier] = useState('');
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  if (!isOpen) return null;

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      let data;
      if (isRegister) {
        data = await register(username, email, password);
      } else {
        data = await login(identifier, password);
      }
      onSuccess(data.user);
      onClose();
    } catch (err) {
      setError(err.message || 'Authentication failed');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-md">
      <div className="w-full max-w-md bg-tactical-950 border border-tactical-700 rounded-2xl shadow-2xl overflow-hidden p-6">
        {/* Header */}
        <div className="flex items-center justify-between pb-4 border-b border-tactical-800">
          <div className="flex items-center gap-2 text-blue-400 font-display font-bold text-lg">
            <ShieldCheck className="w-6 h-6" />
            <span>{isRegister ? 'COMMANDER ENLISTMENT' : 'COMMAND IDENTIFICATION'}</span>
          </div>
          <button
            onClick={onClose}
            className="p-1 rounded-lg text-slate-400 hover:text-white"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {error && (
          <div className="mt-4 p-3 rounded-lg bg-red-950/80 border border-red-700 text-xs text-red-300">
            {error}
          </div>
        )}

        <div className="mt-4">
          <button
            type="button"
            onClick={() => {
              setIsRegister(false);
              setIdentifier('commander');
              setPassword('password123');
            }}
            className="w-full py-2 px-3 rounded-lg bg-tactical-900 hover:bg-tactical-800 border border-cyan-500/40 text-cyan-300 text-xs font-semibold flex items-center justify-center gap-1.5 transition shadow-sm"
          >
            <span>⚡ Use Testing Credentials</span>
            <span className="font-mono text-[11px] text-slate-400">(commander / password123)</span>
          </button>
        </div>

        <form onSubmit={handleSubmit} className="mt-4 space-y-4 text-xs">
          {isRegister ? (
            <>
              <div>
                <label className="block text-slate-400 mb-1 font-medium">Commander Callsign (Username)</label>
                <div className="relative">
                  <User className="absolute left-3 top-2.5 w-4 h-4 text-slate-500" />
                  <input
                    type="text"
                    required
                    value={username}
                    onChange={(e) => setUsername(e.target.value)}
                    placeholder="e.g. CommanderPretorius"
                    className="w-full pl-9 pr-3 py-2 bg-tactical-900 border border-tactical-700 rounded-lg text-white focus:outline-none focus:border-blue-500 font-sans"
                  />
                </div>
              </div>

              <div>
                <label className="block text-slate-400 mb-1 font-medium">Military Email</label>
                <div className="relative">
                  <Mail className="absolute left-3 top-2.5 w-4 h-4 text-slate-500" />
                  <input
                    type="email"
                    required
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    placeholder="commander@geopolitics.war"
                    className="w-full pl-9 pr-3 py-2 bg-tactical-900 border border-tactical-700 rounded-lg text-white focus:outline-none focus:border-blue-500 font-sans"
                  />
                </div>
              </div>
            </>
          ) : (
            <div>
              <label className="block text-slate-400 mb-1 font-medium">Username or Email</label>
              <div className="relative">
                <User className="absolute left-3 top-2.5 w-4 h-4 text-slate-500" />
                <input
                  type="text"
                  required
                  value={identifier}
                  onChange={(e) => setIdentifier(e.target.value)}
                  placeholder="Callsign or email..."
                  className="w-full pl-9 pr-3 py-2 bg-tactical-900 border border-tactical-700 rounded-lg text-white focus:outline-none focus:border-blue-500 font-sans"
                />
              </div>
            </div>
          )}

          <div>
            <label className="block text-slate-400 mb-1 font-medium">Security Password</label>
            <div className="relative">
              <Lock className="absolute left-3 top-2.5 w-4 h-4 text-slate-500" />
              <input
                type="password"
                required
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="••••••••"
                className="w-full pl-9 pr-3 py-2 bg-tactical-900 border border-tactical-700 rounded-lg text-white focus:outline-none focus:border-blue-500 font-sans"
              />
            </div>
          </div>

          <button
            type="submit"
            disabled={loading}
            className="w-full py-2.5 mt-2 rounded-lg bg-blue-600 hover:bg-blue-500 disabled:opacity-50 text-white font-bold font-display text-sm tracking-wide shadow-lg shadow-blue-600/30 transition"
          >
            {loading ? 'AUTHENTICATING...' : isRegister ? 'ENLIST & COMMENCE' : 'ENTER WAR ROOM'}
          </button>
        </form>

        <div className="mt-4 pt-4 border-t border-tactical-800 text-center text-xs text-slate-400">
          {isRegister ? (
            <span>
              Already hold high command credentials?{' '}
              <button
                type="button"
                onClick={() => { setIsRegister(false); setError(''); }}
                className="text-cyan-400 hover:underline font-bold"
              >
                Log In
              </button>
            </span>
          ) : (
            <span>
              New recruit to global geopolitics?{' '}
              <button
                type="button"
                onClick={() => { setIsRegister(true); setError(''); }}
                className="text-cyan-400 hover:underline font-bold"
              >
                Sign Up Now
              </button>
            </span>
          )}
        </div>
      </div>
    </div>
  );
}

