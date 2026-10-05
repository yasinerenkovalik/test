import { NavLink, Navigate, Route, Routes } from 'react-router-dom';
import { useAuth } from './auth.jsx';
import AuthPage from './pages/AuthPage.jsx';
import ProductsPage from './pages/ProductsPage.jsx';
import CategoriesPage from './pages/CategoriesPage.jsx';

export default function App() {
  const { user, ready, logout } = useAuth();
  if (!ready) return <p className="muted pad">Yükleniyor…</p>;

  return (
    <>
      <header className="bar">
        <strong className="brand">Katalog</strong>
        <nav>
          <NavLink to="/">Ürünler</NavLink>
          <NavLink to="/categories">Kategoriler</NavLink>
        </nav>
        <div className="who">
          {user ? (
            <>
              <span>{user.email} <em className="badge">{user.role}</em></span>
              <button className="ghost" onClick={logout}>Çıkış yap</button>
            </>
          ) : (
            <>
              <NavLink to="/login">Giriş yap</NavLink>
              <NavLink to="/register">Kayıt ol</NavLink>
            </>
          )}
        </div>
      </header>
      <main>
        <Routes>
          <Route path="/" element={<ProductsPage />} />
          <Route path="/categories" element={<CategoriesPage />} />
          <Route path="/login" element={user ? <Navigate to="/" /> : <AuthPage mode="login" />} />
          <Route path="/register" element={user ? <Navigate to="/" /> : <AuthPage mode="register" />} />
        </Routes>
      </main>
    </>
  );
}
