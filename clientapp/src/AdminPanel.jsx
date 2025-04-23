import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';

function AdminPanel() {
  const [questions, setQuestions] = useState([]);
  const navigate = useNavigate();

  useEffect(() => {
    const token = localStorage.getItem('token');
    const isAdmin = localStorage.getItem('isAdmin');
    if (!token || isAdmin !== 'true') {
      navigate('/login');
      return;
    }
    // Загружаем список вопросов
    fetch('https://localhost:5001/api/admin/questions', {
      headers: { 'Authorization': 'Bearer ' + token }
    })
      .then(res => res.ok ? res.json() : Promise.reject('Failed to load'))
      .then(data => setQuestions(data))
      .catch(err => console.error('Error fetching questions:', err));
  }, [navigate]);

  const handleDelete = (id) => {
    const token = localStorage.getItem('token');
    fetch(`https://localhost:5001/api/admin/questions/${id}`, {
      method: 'DELETE',
      headers: { 'Authorization': 'Bearer ' + token }
    })
      .then(res => {
        if (res.ok) {
          setQuestions(prev => prev.filter(q => q.id !== id));
        } else {
          alert('Не удалось удалить вопрос');
        }
      });
  };

  return (
    <div className="container mt-4">
      <h2>Админ-панель: Вопросы</h2>
      <button className="btn btn-success mb-3" onClick={() => navigate('/admin/questions/new')}>
        Добавить вопрос
      </button>
      <table className="table table-bordered">
        <thead>
          <tr>
            <th>ID</th>
            <th>Текст вопроса</th>
            <th>Действия</th>
          </tr>
        </thead>
        <tbody>
          {questions.map(q => (
            <tr key={q.id}>
              <td>{q.id}</td>
              <td>{q.text}</td>
              <td>
                <button className="btn btn-primary me-2"
                        onClick={() => navigate(`/admin/questions/edit/${q.id}`)}>
                  Редактировать
                </button>
                <button className="btn btn-danger" onClick={() => handleDelete(q.id)}>
                  Удалить
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export default AdminPanel;
