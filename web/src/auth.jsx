import { createContext, useContext, useEffect, useState } from 'react';
import { api, tokens } from './api.js';

const AuthContext = createContext(null);
export const useAuth = () => useContext(AuthContext);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [ready, setReady] = useState(false);

  useEffect(() => {
    const load = tokens.get() ? api('/users/me').then(setUser).catch(() => tokens.set(null)) : Promise.resolve();
    load.finally(() => setReady(true));
  }, []);

  const authenticate = async (path, body) => {
    tokens.set(await api(path, { method: 'POST', body }));
    setUser(await api('/users/me'));
  };

  const value = {
    user,
    ready,
    isAdmin: user?.role === 'Admin',
    login: (body) => authenticate('/auth/login', body),
    register: (body) => authenticate('/auth/register', body),
    logout: () => { tokens.set(null); setUser(null); },
  };
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
