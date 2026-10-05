import { useState } from 'react';
import { useAuth } from '../auth.jsx';
import Errors from '../components/Errors.jsx';

export default function AuthPage({ mode }) {
  const { login, register } = useAuth();
  const isRegister = mode === 'register';
  const [form, setForm] = useState({ firstName: '', lastName: '', email: '', password: '' });
  const [error, setError] = useState(null);
  const set = (k) => (e) => setForm({ ...form, [k]: e.target.value });

  const submit = async (e) => {
    e.preventDefault();
    setError(null);
    try {
      await (isRegister ? register(form) : login({ email: form.email, password: form.password }));
    } catch (err) { setError(err); }
  };

  return (
    <form className="panel narrow" onSubmit={submit}>
      <h1>{isRegister ? 'Hesap oluştur' : 'Giriş yap'}</h1>
      {isRegister && (
        <div className="row">
          <label>Ad<input value={form.firstName} onChange={set('firstName')} required /></label>
          <label>Soyad<input value={form.lastName} onChange={set('lastName')} required /></label>
        </div>
      )}
      <label>E-posta<input type="email" value={form.email} onChange={set('email')} required /></label>
      <label>Şifre<input type="password" value={form.password} onChange={set('password')} required /></label>
      <Errors error={error} />
      <button>{isRegister ? 'Kayıt ol' : 'Giriş yap'}</button>
    </form>
  );
}
