import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';

function QuestionEditor() {
  const { id } = useParams();
  const isEdit = id !== undefined;
  const navigate = useNavigate();
  const [questionText, setQuestionText] = useState('');
  const [answers, setAnswers] = useState([]);
  const [initialAnswers, setInitialAnswers] = useState([]);

  useEffect(() => {
    const token = localStorage.getItem('token');
    const isAdmin = localStorage.getItem('isAdmin');
    if (!token || isAdmin !== 'true') {
      navigate('/login');
      return;
    }
    if (isEdit) {
      // Загружаем данные вопроса и ответы
      fetch(`https://localhost:5001/api/admin/questions/${id}`, {
        headers: { 'Authorization': 'Bearer ' + token }
      })
        .then(res => res.ok ? res.json() : Promise.reject('Failed to load'))
        .then(data => {
          setQuestionText(data.text);
          if (data.answers) {
            setAnswers(data.answers);
            setInitialAnswers(data.answers);
          }
        })
        .catch(err => console.error('Error fetching question:', err));
    }
  }, [isEdit, id, navigate]);

  const handleAddAnswer = () => {
    setAnswers(prev => [...prev, { id: null, text: '', isCorrect: false }]);
  };

  const handleRemoveAnswer = (index) => {
    setAnswers(prev => prev.filter((_, idx) => idx !== index));
  };

  const handleAnswerTextChange = (index, newText) => {
    setAnswers(prev => prev.map((ans, idx) => idx === index ? { ...ans, text: newText } : ans));
  };

  const handleCorrectChange = (index) => {
    setAnswers(prev =>
      prev.map((ans, idx) => ({ ...ans, isCorrect: idx === index }))
    );
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    const token = localStorage.getItem('token');
    try {
      if (isEdit) {
        // Обновление существующего вопроса
        const resQ = await fetch(`https://localhost:5001/api/admin/questions/${id}`, {
          method: 'PUT',
          headers: {
            'Authorization': 'Bearer ' + token,
            'Content-Type': 'application/json'
          },
          body: JSON.stringify({ text: questionText })
        });
        if (!resQ.ok) throw new Error('Failed to update question');
        // Удаляем ответы, которые были в исходном списке, но удалены пользователем
        for (let orig of initialAnswers) {
          if (!answers.find(ans => ans.id === orig.id)) {
            await fetch(`https://localhost:5001/api/admin/answers/${orig.id}`, {
              method: 'DELETE',
              headers: { 'Authorization': 'Bearer ' + token }
            });
          }
        }
        // Добавляем новые ответы или обновляем существующие
        for (let ans of answers) {
          if (ans.id && initialAnswers.find(x => x.id === ans.id)) {
            // Обновляем существующий ответ
            await fetch(`https://localhost:5001/api/admin/answers/${ans.id}`, {
              method: 'PUT',
              headers: {
                'Authorization': 'Bearer ' + token,
                'Content-Type': 'application/json'
              },
              body: JSON.stringify({ text: ans.text, isCorrect: ans.isCorrect })
            });
          }
          if (!ans.id) {
            // Добавляем новый ответ
            await fetch('https://localhost:5001/api/admin/answers', {
              method: 'POST',
              headers: {
                'Authorization': 'Bearer ' + token,
                'Content-Type': 'application/json'
              },
              body: JSON.stringify({ questionId: Number(id), text: ans.text, isCorrect: ans.isCorrect })
            });
          }
        }
      } else {
        // Создание нового вопроса
        const resQ = await fetch('https://localhost:5001/api/admin/questions', {
          method: 'POST',
          headers: {
            'Authorization': 'Bearer ' + token,
            'Content-Type': 'application/json'
          },
          body: JSON.stringify({ text: questionText })
        });
        if (!resQ.ok) throw new Error('Failed to create question');
        const newQuestion = await resQ.json();
        // Добавляем ответы к новому вопросу
        for (let ans of answers) {
          await fetch('https://localhost:5001/api/admin/answers', {
            method: 'POST',
            headers: {
              'Authorization': 'Bearer ' + token,
              'Content-Type': 'application/json'
            },
            body: JSON.stringify({ questionId: newQuestion.id, text: ans.text, isCorrect: ans.isCorrect })
          });
        }
      }
      navigate('/admin/questions');
    } catch (error) {
      console.error('Error saving question:', error);
      alert('Ошибка при сохранении');
    }
  };

  return (
    <div className="container mt-4">
      <h2>{isEdit ? 'Редактировать вопрос' : 'Новый вопрос'}</h2>
      <form onSubmit={handleSubmit}>
        <div className="mb-3">
          <label className="form-label">Текст вопроса:</label>
          <input type="text" className="form-control" value={questionText}
                 onChange={e => setQuestionText(e.target.value)} required />
        </div>
        <div className="mb-3">
          <h5>Варианты ответа:</h5>
          {answers.map((ans, idx) => (
            <div className="mb-2 d-flex align-items-center" key={idx}>
              <input type="text" className="form-control me-2" style={{ flex: 1 }}
                     value={ans.text} onChange={e => handleAnswerTextChange(idx, e.target.value)}
                     placeholder={`Ответ ${idx + 1}`} required />
              <input type="radio" name="correctAnswer" className="form-check-input me-2"
                     checked={ans.isCorrect} onChange={() => handleCorrectChange(idx)} />
              <label className="form-check-label me-3">Правильный</label>
              <button type="button" className="btn btn-outline-danger btn-sm"
                      onClick={() => handleRemoveAnswer(idx)}>
                Удалить
              </button>
            </div>
          ))}
          <button type="button" className="btn btn-secondary" onClick={handleAddAnswer}>
            Добавить ответ
          </button>
        </div>
        <button type="submit" className="btn btn-primary">Сохранить</button>
        <button type="button" className="btn btn-link ms-2" onClick={() => navigate('/admin/questions')}>
          Отмена
        </button>
      </form>
    </div>
  );
}

export default QuestionEditor;
