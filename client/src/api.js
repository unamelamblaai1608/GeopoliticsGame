const API_BASE = '/api';

function getAuthHeader() {
  const token = localStorage.getItem('geopolitics_token');
  return token ? { Authorization: `Bearer ${token}` } : {};
}

export async function apiRequest(endpoint, options = {}) {
  const url = `${API_BASE}${endpoint}`;
  const headers = {
    'Content-Type': 'application/json',
    ...getAuthHeader(),
    ...options.headers
  };

  const response = await fetch(url, {
    ...options,
    headers
  });

  const data = await response.json().catch(() => ({}));

  if (!response.ok) {
    throw new Error(data.error || `Request failed with status ${response.status}`);
  }

  return data;
}

// Authentication
export async function register(username, email, password) {
  const data = await apiRequest('/auth/register', {
    method: 'POST',
    body: JSON.stringify({ username, email, password })
  });
  if (data.token) {
    localStorage.setItem('geopolitics_token', data.token);
  }
  return data;
}

export async function login(identifier, password) {
  const data = await apiRequest('/auth/login', {
    method: 'POST',
    body: JSON.stringify({ identifier, password })
  });
  if (data.token) {
    localStorage.setItem('geopolitics_token', data.token);
  }
  return data;
}

export function logout() {
  localStorage.removeItem('geopolitics_token');
}

export async function getCurrentUser() {
  const token = localStorage.getItem('geopolitics_token');
  if (!token) return null;
  try {
    return await apiRequest('/auth/me');
  } catch (err) {
    localStorage.removeItem('geopolitics_token');
    return null;
  }
}

// Countries
export async function getCountries() {
  return await apiRequest('/countries');
}

// Games
export async function getGames() {
  return await apiRequest('/games');
}

export async function getGame(id) {
  return await apiRequest(`/games/${id}`);
}

export async function createGame(selectedCountryId, title) {
  return await apiRequest('/games', {
    method: 'POST',
    body: JSON.stringify({ selectedCountryId, title })
  });
}

export async function sendGameAction(gameId, actionType, payload = {}) {
  return await apiRequest(`/games/${gameId}/action`, {
    method: 'POST',
    body: JSON.stringify({ actionType, payload })
  });
}

// Analytics
export async function getPlayerAnalytics() {
  return await apiRequest('/analytics/me');
}

export async function getPlatformAnalytics() {
  return await apiRequest('/analytics/platform');
}

