import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';

function RegisterPage() {
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');           
  const [password, setPassword] = useState('');
  const [confirm, setConfirm] = useState('');
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (password !== confirm) {
      alert('Пароли не совпадают');
      return;
    }
    try {
      const res = await fetch('https://localhost:5001/api/auth/register', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          Name: username,
          Email: email,       
          Password: password
        })
      });
      if (!res.ok) {
        const errorText = await res.text();
        alert(`Ошибка регистрации: ${errorText}`);
        return;
      }
      alert('Регистрация успешно выполнена. Теперь вы можете войти.');
      navigate('/login');
    } catch (error) {
      console.error('Register error:', error);
      alert('Ошибка запроса');
    }
  };

  return (
    <div className="container mt-4" style={{ maxWidth: '400px' }}>
      <h2>Регистрация</h2>
      <form onSubmit={handleSubmit}>
        <div className="mb-3">
          <label className="form-label">Имя пользователя</label>
          <input
            type="text"
            className="form-control"
            value={username}
            onChange={e => setUsername(e.target.value)}
            required
          />
        </div>

        <div className="mb-3">
          <label className="form-label">Email</label>   {/* 3. Поле Email */}
          <input
            type="email"
            className="form-control"
            value={email}
            onChange={e => setEmail(e.target.value)}
            required
          />
        </div>

        <div className="mb-3">
          <label className="form-label">Пароль</label>
          <input
            type="password"
            className="form-control"
            value={password}
            onChange={e => setPassword(e.target.value)}
            required
          />
        </div>

        <div className="mb-3">
          <label className="form-label">Повторите пароль</label>
          <input
            type="password"
            className="form-control"
            value={confirm}
            onChange={e => setConfirm(e.target.value)}
            required
          />
        </div>

        <button type="submit" className="btn btn-primary">Зарегистрироваться</button>
        <button type="button" className="btn btn-link" onClick={() => navigate('/login')}>
          Вход
        </button>
      </form>
    </div>
  );
}

export default RegisterPage;
