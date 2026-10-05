import { useCallback, useEffect, useState } from 'react';
import { api } from '../api.js';
import { useAuth } from '../auth.jsx';
import Errors from '../components/Errors.jsx';

const money = new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' });
const emptyFilters = { search: '', categoryId: '', minPrice: '', maxPrice: '', sortBy: 'CreatedDate', sortDirection: 'Desc' };
const emptyProduct = { name: '', description: '', price: '', stock: 0, categoryId: '', isActive: true };

export default function ProductsPage() {
  const { isAdmin } = useAuth();
  const [categories, setCategories] = useState([]);
  const [draft, setDraft] = useState(emptyFilters);   // formda yazılan
  const [applied, setApplied] = useState(emptyFilters); // sunucuya giden
  const [page, setPage] = useState(1);
  const [result, setResult] = useState({ items: [], totalCount: 0, totalPages: 0 });
  const [editing, setEditing] = useState(null);
  const [error, setError] = useState(null);

  const load = useCallback(
    () => api('/products', { query: { ...applied, page, pageSize: 8 } }).then(setResult).catch(setError),
    [applied, page]
  );
  useEffect(() => { load(); }, [load]);
  useEffect(() => { api('/categories').then(setCategories).catch(() => {}); }, []);

  const run = async (fn) => {
    setError(null);
    try { await fn(); await load(); } catch (err) { setError(err); }
  };

  const save = (e) => {
    e.preventDefault();
    const { id, categoryName, createdDate, ...body } = editing;
    body.price = Number(body.price);
    body.stock = Number(body.stock);
    run(async () => {
      await api(id ? `/products/${id}` : '/products', { method: id ? 'PUT' : 'POST', body });
      setEditing(null);
    });
  };

  const changeStock = (p) => {
    const value = window.prompt(`${p.name} için yeni stok değeri`, p.stock);
    if (value !== null) run(() => api(`/products/${p.id}/stock`, { method: 'PATCH', body: { stock: Number(value) } }));
  };

  const field = (k) => ({ value: editing[k], onChange: (e) => setEditing({ ...editing, [k]: e.target.value }) });

  return (
    <>
      <form className="panel filters" onSubmit={(e) => { e.preventDefault(); setPage(1); setApplied(draft); }}>
        <input placeholder="Ürün ara" value={draft.search} onChange={(e) => setDraft({ ...draft, search: e.target.value })} />
        <select value={draft.categoryId} onChange={(e) => setDraft({ ...draft, categoryId: e.target.value })}>
          <option value="">Tüm kategoriler</option>
          {categories.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
        </select>
        <input type="number" placeholder="En az ₺" value={draft.minPrice} onChange={(e) => setDraft({ ...draft, minPrice: e.target.value })} />
        <input type="number" placeholder="En çok ₺" value={draft.maxPrice} onChange={(e) => setDraft({ ...draft, maxPrice: e.target.value })} />
        <select value={draft.sortBy} onChange={(e) => setDraft({ ...draft, sortBy: e.target.value })}>
          <option value="CreatedDate">Eklenme tarihi</option><option value="Name">Ad</option>
          <option value="Price">Fiyat</option><option value="Stock">Stok</option>
        </select>
        <select value={draft.sortDirection} onChange={(e) => setDraft({ ...draft, sortDirection: e.target.value })}>
          <option value="Desc">Azalan</option><option value="Asc">Artan</option>
        </select>
        <button>Filtrele</button>
      </form>

      <section className="panel">
        <div className="head">
          <h1>Ürünler <span className="muted">({result.totalCount})</span></h1>
          {isAdmin && <button onClick={() => setEditing({ ...emptyProduct, categoryId: categories[0]?.id ?? '' })}>Ürün ekle</button>}
        </div>
        <Errors error={error} />

        {editing && (
          <form className="editor" onSubmit={save}>
            <label>Ad<input {...field('name')} required /></label>
            <label>Açıklama<input {...field('description')} /></label>
            <div className="row">
              <label>Fiyat<input type="number" step="0.01" {...field('price')} required /></label>
              <label>Stok<input type="number" {...field('stock')} required /></label>
            </div>
            <label>Kategori
              <select {...field('categoryId')} required>
                {categories.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
              </select>
            </label>
            <label className="check"><input type="checkbox" checked={editing.isActive} onChange={(e) => setEditing({ ...editing, isActive: e.target.checked })} /> Satışta</label>
            <div className="row inline"><button>Kaydet</button><button type="button" className="ghost" onClick={() => setEditing(null)}>Vazgeç</button></div>
          </form>
        )}

        <div className="scroll">
          <table>
            <thead><tr><th>Ürün</th><th>Kategori</th><th className="num">Fiyat</th><th className="num">Stok</th>{isAdmin && <th />}</tr></thead>
            <tbody>
              {result.items.map((p) => (
                <tr key={p.id}>
                  <td>{p.name} {!p.isActive && <em className="badge off">pasif</em>}</td>
                  <td>{p.categoryName}</td>
                  <td className="num">{money.format(p.price)}</td>
                  <td className="num">{p.stock}</td>
                  {isAdmin && (
                    <td className="actions">
                      <button className="ghost" onClick={() => setEditing(p)}>Düzenle</button>
                      <button className="ghost" onClick={() => changeStock(p)}>Stok</button>
                      <button className="ghost danger" onClick={() => run(() => api(`/products/${p.id}`, { method: 'DELETE' }))}>Sil</button>
                    </td>
                  )}
                </tr>
              ))}
              {result.items.length === 0 && <tr><td colSpan={5} className="muted">Bu filtrelerle ürün bulunamadı. Filtreleri temizleyip tekrar deneyin.</td></tr>}
            </tbody>
          </table>
        </div>

        <div className="pager">
          <button className="ghost" disabled={page <= 1} onClick={() => setPage(page - 1)}>Önceki</button>
          <span>Sayfa {page} / {Math.max(result.totalPages, 1)}</span>
          <button className="ghost" disabled={page >= result.totalPages} onClick={() => setPage(page + 1)}>Sonraki</button>
        </div>
      </section>
    </>
  );
}
