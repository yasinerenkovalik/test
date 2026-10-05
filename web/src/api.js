const BASE = '/api';

export const tokens = {
  get: () => JSON.parse(localStorage.getItem('tokens') || 'null'),
  set: (t) => (t ? localStorage.setItem('tokens', JSON.stringify(t)) : localStorage.removeItem('tokens')),
};

export class ApiError extends Error {
  constructor(body) {
    super(body.message);
    this.status = body.statusCode;
    this.errors = body.errors ?? [];
  }
}

let refreshing = null;
function refresh() {
  const t = tokens.get();
  if (!t) return Promise.resolve(false);
  // Aynı anda gelen 401'lerde tek bir refresh isteği yapılır.
  refreshing ??= fetch(`${BASE}/auth/refresh-token`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken: t.refreshToken }),
  })
    .then(async (r) => {
      if (!r.ok) { tokens.set(null); return false; }
      tokens.set((await r.json()).data);
      return true;
    })
    .finally(() => { refreshing = null; });
  return refreshing;
}

export async function api(path, { method = 'GET', body, query } = {}, retry = true) {
  const qs = query
    ? '?' + new URLSearchParams(Object.entries(query).filter(([, v]) => v !== '' && v != null))
    : '';
  const t = tokens.get();
  const res = await fetch(`${BASE}${path}${qs}`, {
    method,
    headers: {
      ...(body && { 'Content-Type': 'application/json' }),
      ...(t && { Authorization: `Bearer ${t.accessToken}` }),
    },
    body: body && JSON.stringify(body),
  });
  if (res.status === 401 && retry && t && (await refresh())) return api(path, { method, body, query }, false);

  const json = await res.json().catch(() => null);
  if (!res.ok) throw new ApiError(json ?? { message: 'İstek başarısız oldu.', statusCode: res.status });
  return json.data;
}
