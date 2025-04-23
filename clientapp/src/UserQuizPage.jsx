import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';

function UserQuizPage() {
  const navigate = useNavigate();
  const [questions, setQuestions] = useState([]);
  const [currentQuestion, setCurrentQuestion] = useState(null);
  const [sessionId, setSessionId] = useState(null);
  const [selectedAnswer, setSelectedAnswer] = useState('');
  const [feedback, setFeedback] = useState('');
  const [waitingForNext, setWaitingForNext] = useState(false);
  const [message, setMessage] = useState('');

  useEffect(() => {
    // Проверяем, авторизован ли пользователь (есть ли токен)
    const token = localStorage.getItem('token');
    if (!token) {
      navigate('/login');
      return;
    }
    // Загружаем все доступные вопросы (тесты)
    fetch('https://localhost:5001/api/questions', {
      headers: { 'Authorization': 'Bearer ' + token }
    })
      .then(res => res.ok ? res.json() : Promise.reject('Failed to load questions'))
      .then(data => setQuestions(data))
      .catch(err => console.error('Ошибка загрузки вопросов:', err));
  }, [navigate]);

  // Начать выбранный тест
  const startQuiz = async (quizId) => {
    setMessage('');  // очистить прошлые сообщения
    try {
      const token = localStorage.getItem('token');
      const res = await fetch('https://localhost:5001/api/quiz/start', {
        method: 'POST',
        headers: {
          'Authorization': 'Bearer ' + token,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(quizId)  // передаем ID теста в теле запроса
      });
      if (!res.ok) {
        throw new Error('Не удалось начать тест');
      }
      const session = await res.json();
      // Если API вернул объект с ID сессии – используем его; иначе используем переданный quizId
      const newSessionId = session.id || quizId;
      setSessionId(newSessionId);
      // Загружаем первый вопрос теста сразу после старта
      const nextRes = await fetch(`https://localhost:5001/api/quiz/${newSessionId}/next`, {
        headers: { 'Authorization': 'Bearer ' + token }
      });
      if (!nextRes.ok) {
        throw new Error('Не удалось загрузить первый вопрос');
      }
      const questionData = await nextRes.json();
      setCurrentQuestion(questionData);
      setSelectedAnswer('');
      setFeedback('');
      setWaitingForNext(false);
    } catch (err) {
      console.error('Ошибка запуска теста:', err);
      alert('Ошибка запуска теста');
    }
  };

  // Отправить ответ на текущий вопрос
  const submitAnswer = async () => {
    if (!selectedAnswer) {
      alert('Пожалуйста, выберите ответ');
      return;
    }
    try {
      const token = localStorage.getItem('token');
      const res = await fetch(`https://localhost:5001/api/quiz/${sessionId}/submit`, {
        method: 'POST',
        headers: {
          'Authorization': 'Bearer ' + token,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(selectedAnswer)  // отправляем ответ (строкой)
      });
      if (res.ok) {
        // Ответ правильный
        setFeedback('Ответ верный!');
        setWaitingForNext(true);
      } else if (res.status === 400) {
        // Ответ неправильный
        setFeedback('Ответ неверный, попробуйте еще раз.');
      } else {
        // Другая ошибка
        throw new Error('Ошибка при отправке ответа');
      }
    } catch (err) {
      console.error('Ошибка проверки ответа:', err);
      alert('Ошибка при отправке ответа');
    }
  };

  // Загрузить следующий вопрос
  const loadNextQuestion = async () => {
    try {
      const token = localStorage.getItem('token');
      const res = await fetch(`https://localhost:5001/api/quiz/${sessionId}/next`, {
        headers: { 'Authorization': 'Bearer ' + token }
      });
      if (!res.ok) {
        if (res.status === 404) {
          // Вопросы закончились – завершаем тест
          await fetch(`https://localhost:5001/api/quiz/${sessionId}/end`, {
            method: 'POST',
            headers: { 'Authorization': 'Bearer ' + token }
          });
          setMessage('Тест завершен.');
          setSessionId(null);
          setCurrentQuestion(null);
        } else {
          throw new Error('Не удалось загрузить следующий вопрос');
        }
      } else {
        const questionData = await res.json();
        setCurrentQuestion(questionData);
        setSelectedAnswer('');
        setFeedback('');
        setWaitingForNext(false);
      }
    } catch (err) {
      console.error('Ошибка получения следующего вопроса:', err);
      alert('Ошибка загрузки следующего вопроса');
    }
  };

  // Досрочное завершение теста
  const endQuiz = async () => {
    if (!sessionId) return;
    try {
      const token = localStorage.getItem('token');
      await fetch(`https://localhost:5001/api/quiz/${sessionId}/end`, {
        method: 'POST',
        headers: { 'Authorization': 'Bearer ' + token }
      });
    } catch (err) {
      console.error('Ошибка завершения теста:', err);
    }
    // Сброс состояния теста
    setMessage('Тест завершен.');
    setSessionId(null);
    setCurrentQuestion(null);
    setSelectedAnswer('');
    setFeedback('');
    setWaitingForNext(false);
  };

  // Обработчик кнопки "Стать админом" (пример вызова)
  const handleBecomeAdmin = () => {
    alert('Функция "Стать админом" пока не реализована.');
    // Здесь мог бы быть вызов API для повышения привилегий, если бы он существовал
  };

  return (
    <div className="container mt-4">
      <h2>Доступные тесты</h2>
      {/* Статус пользователя и кнопка "Стать админом" */}
      <div className="mb-3">
        Статус пользователя: <strong>{localStorage.getItem('isAdmin') === 'true' ? 'Администратор' : 'Обычный пользователь'}</strong>
        <button className="btn btn-outline-primary btn-sm ms-3" onClick={handleBecomeAdmin}>
          Стать админом
        </button>
      </div>

      {/* Общее сообщение (например, о завершении теста) */}
      {message && <div className="alert alert-info">{message}</div>}

      {/* Либо список тестов, либо интерфейс текущего теста */}
      {!sessionId ? (
        // Список доступных тестов (отображается, когда никакой тест не начат)
        <ul className="list-group">
          {questions.map(q => (
            <li key={q.id} className="list-group-item d-flex justify-content-between align-items-center">
              {q.text || q.name}
              <button className="btn btn-primary btn-sm" onClick={() => startQuiz(q.id)}>
                Начать тест
              </button>
            </li>
          ))}
        </ul>
      ) : (
        // Интерфейс прохождения теста (отображается, когда тест запущен)
        <div>
          <h4>Вопрос:</h4>
          {currentQuestion && (
            <div className="mb-3">
              <p><strong>{currentQuestion.text}</strong></p>
              {currentQuestion.answers && currentQuestion.answers.length > 0 ? (
                // Варианты ответа (множественный выбор)
                <div>
                  {currentQuestion.answers.map(ans => (
                    <div key={ans.id} className="form-check">
                      <input
                        type="radio"
                        name="answer"
                        value={ans.text}
                        className="form-check-input"
                        checked={selectedAnswer === ans.text}
                        onChange={() => setSelectedAnswer(ans.text)}
                        disabled={waitingForNext}
                      />
                      <label className="form-check-label">
                        {ans.text}
                      </label>
                    </div>
                  ))}
                </div>
              ) : (
                // Поле для свободного ответа (если нет готовых вариантов)
                <input
                  type="text"
                  className="form-control"
                  value={selectedAnswer}
                  onChange={e => setSelectedAnswer(e.target.value)}
                  disabled={waitingForNext}
                />
              )}
            </div>
          )}

          {/* Сообщение о результате ответа на текущий вопрос */}
          {feedback && (
            <div className="mb-3">
              <em>{feedback}</em>
            </div>
          )}

          {/* Кнопки действий: Ответить / Следующий вопрос / Завершить тест */}
          <div className="mb-3">
            {!waitingForNext ? (
              <button className="btn btn-success me-2" onClick={submitAnswer}>
                Ответить
              </button>
            ) : (
              <button className="btn btn-primary me-2" onClick={loadNextQuestion}>
                Следующий вопрос
              </button>
            )}
            <button className="btn btn-secondary" onClick={endQuiz}>
              Завершить тест
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

export default UserQuizPage;
