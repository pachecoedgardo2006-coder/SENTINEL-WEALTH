const KEY = 'sentinel_user';

export function getSession() {
  try {
    const raw = localStorage.getItem(KEY);
    return raw ? JSON.parse(raw) : null;
  } catch {
    return null;
  }
}

export function setSession(user) {
  localStorage.setItem(KEY, JSON.stringify(user));
}

export function clearSession() {
  localStorage.removeItem(KEY);
}

export function authHeaders() {
  const session = getSession();
  return session?.id ? { 'X-User-Id': session.id } : {};
}
