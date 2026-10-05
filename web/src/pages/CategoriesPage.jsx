import { useCallback, useEffect, useState } from 'react';
import { api } from '../api.js';
import { useAuth } from '../auth.jsx';
import Errors from '../components/Errors.jsx';

export default function CategoriesPage() {
  const { isAdmin } = useAuth();
  const [items, setItems] = useState([]);
  const [name, setName] = useState('');
  const [error, setError] = useState(null);

  const load = useCallback(() => api('/categories').then(setItems).catch(setError), []);
  useEffect(() => { load(); }, [load]);

  const run = async (fn) => {
    setError(null);
    try { await fn(); await load(); } catch (err) { setError(err); }
  };

  return (
    <section className="panel">
      <h1>Kategoriler</h1>
      {isAdmin && (
        <form className="row inline" onSubmit={(e) => { e.preventDefault(); run(async () => { await api('/categories', { method: 'POST', body: { name } }); setName(''); }); }}>
          <input placeholder="Yeni kategori adı" value={name} onChange={(e) => setName(e.target.value)} required />
          <button>Kategori ekle</button>
        </form>
      )}
      <Errors error={error} />
      <ul className="list">
        {items.map((c) => (
          <li key={c.id}>
            <span>{c.name} {!c.isActive && <em className="badge off">pasif</em>}</span>
            {isAdmin && <button className="ghost danger" onClick={() => run(() => api(`/categories/${c.id}`, { method: 'DELETE' }))}>Sil</button>}
          </li>
        ))}
        {items.length === 0 && <li className="muted">Henüz kategori yok.</li>}
      </ul>
    </section>
  );
}
