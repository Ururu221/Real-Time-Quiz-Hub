import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';

function LoginPage() {
  const [email, setEmail]       = useState('');
  const [password, setPassword] = useState('');
  const navigate = useNavigate();

  const handleSubmit = async e => {
    e.preventDefault();
    try {

      console.log('Отправка:', email, password);

      const res = await fetch('https://localhost:5001/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          Email: email,       
          Password: password
        })
      });
      if (!res.ok) {
        const errorText = await res.text(); 
        console.error('Ошибка входа:', errorText);
        alert('Ошибка входа, смотри консоль');
        return;
      }
      const data = await res.json();

      console.log('Успешный вход', data);

      localStorage.setItem('token', data.token);
      localStorage.setItem('isAdmin', data.isAdmin);
      navigate(data.isAdmin ? '/admin/questions' : '/');
    } catch {
      alert('Ошибка запроса');
    }
  };

  return (
    <div className="container mt-4" style={{ maxWidth: '400px' }}>
      <h2>Вход</h2>
      <form onSubmit={handleSubmit}>
        <div className="mb-3">
          <label className="form-label">Email</label>
          <input 
            type="email" 
            className="form-control" 
            value={email}
            onChange={e => setEmail(e.target.value)}
            required />
        </div>
        <div className="mb-3">
          <label className="form-label">Пароль</label>
          <input 
            type="password" 
            className="form-control" 
            value={password}
            onChange={e => setPassword(e.target.value)}
            required />
        </div>
        <button type="submit" className="btn btn-primary">Войти</button>
        <button 
          type="button" 
          className="btn btn-link" 
          onClick={() => navigate('/register')}>
          Регистрация
        </button>
      </form>
    </div>
  );
}

export default LoginPage;
