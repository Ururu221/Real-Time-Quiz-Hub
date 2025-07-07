// === Настройки и глобальные переменные ===
const quizId = 'default-session';
let nickname = '';
let totalQuestions = 0;
let currentQuestionIndex = 1;

// Элементы DOM
const startArea = document.getElementById('startArea');
const startBtn = document.getElementById('startBtn');
const nickInput = document.getElementById('nickInput');
const quizArea = document.getElementById('quizArea');
const questionContainer = document.getElementById('questionContainer');
const submitBtn = document.getElementById('submitBtn');
const leaderboardBody = document.getElementById('leaderboardBody');

// === 1. Инициализация SignalR ===
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/quizHub")
    .build();

connection.on("BroadcastLeaderboard", items => {
    leaderboardBody.innerHTML = '';
    items.forEach(it => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
      <td>${it.nickname}</td>
      <td>${it.correctAnswers}</td>
      <td>${it.currentQuestionIndex}</td>
      <td>${it.totalQuestions}</td>
    `;
        leaderboardBody.appendChild(tr);
    });
});

connection.start()
    .then(() => console.log("SignalR: connected"))
    .catch(err => console.error(err));

// === 2. Старт викторины ===
startBtn.addEventListener('click', async () => {
    nickname = nickInput.value.trim();
    if (!nickname) {
        alert('Введите ник!');
        return;
    }

    // Регистрируем пользователя в хабе
    await connection.invoke("RegisterUser", quizId, nickname);

    // Запрашиваем начало сессии у API
    const res = await fetch('/api/quiz/start', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(quizId)
    });
    const session = await res.json();
    totalQuestions = session.totalQuestions;
    currentQuestionIndex = session.currentQuestionIndex;

    // Переключаем UI
    startArea.classList.add('d-none');
    renderQuestion(session.currentQuestion, currentQuestionIndex);
    quizArea.classList.remove('d-none');
});

// === 3. Рендер вопроса ===
function renderQuestion(q, idx) {
    questionContainer.innerHTML = '';

    const title = document.createElement('h5');
    title.textContent = `Вопрос ${idx}/${totalQuestions}: ${q.name}`;
    questionContainer.appendChild(title);

    const list = document.createElement('div');
    list.className = 'list-group mb-3';

    q.answers.forEach(a => {
        const label = document.createElement('label');
        label.className = 'list-group-item d-flex align-items-center';
        const radio = document.createElement('input');
        radio.type = 'radio';
        radio.name = 'answer';
        radio.value = a.text;
        radio.className = 'form-check-input me-2';
        label.appendChild(radio);
        label.appendChild(document.createTextNode(a.text));
        list.appendChild(label);
    });

    questionContainer.appendChild(list);
}

// === 4. Отправка ответа ===
submitBtn.addEventListener('click', async () => {
    const sel = document.querySelector('input[name="answer"]:checked');
    if (!sel) {
        alert('Please select an answer');
        return;
    }

    // Отправляем ответ на сервер
    await fetch(`/api/quiz/${quizId}/submit`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(sel.value)
    });

    // Получаем обновлённое состояние сессии
    const stateRes = await fetch(`/api/quiz/${quizId}`);
    const state = await stateRes.json();
    currentQuestionIndex = state.currentQuestionIndex;

    // Шлём прогресс в лидерборд
    await connection.invoke(
        "UpdateProgress",
        quizId,
        nickname,
        currentQuestionIndex,
        state.correctAnswers
    );

    // Запрашиваем следующий вопрос
    const nextRes = await fetch(`/api/quiz/${quizId}/next`);
    if (nextRes.ok) {
        const nextQ = await nextRes.json();
        renderQuestion(nextQ, currentQuestionIndex);
    } else {
        showResult();
    }
});

// === 5. Показ результата ===
async function showResult() {
    const res = await fetch(`/api/quiz/${quizId}`);
    const session = await res.json();

    questionContainer.innerHTML = '';
    submitBtn.classList.add('d-none');

    const div = document.createElement('div');
    div.className = 'alert alert-success';
    div.textContent = `You got ${session.correctAnswers} of ${session.totalQuestions}`;
    questionContainer.appendChild(div);
}
