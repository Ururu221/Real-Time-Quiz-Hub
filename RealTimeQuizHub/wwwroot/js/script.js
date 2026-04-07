const quizId = 'default-session';
let nickname = '';
let totalQuestions = 0;
let currentQuestionIndex = 1;

const startArea       = document.getElementById('startArea');
const startBtn        = document.getElementById('startBtn');
const nickInput       = document.getElementById('nickInput');
const quizArea        = document.getElementById('quizArea');
const resultArea      = document.getElementById('resultArea');
const questionContainer = document.getElementById('questionContainer');
const submitBtn       = document.getElementById('submitBtn');
const leaderboardList = document.getElementById('leaderboardList');
const progressBar     = document.getElementById('progressBar');
const progressLabel   = document.getElementById('progressLabel');
const connectionDot   = document.getElementById('connectionDot');

// ===== SIGNALR =====
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/quizHub")
    .withAutomaticReconnect()
    .build();

connection.on("BroadcastLeaderboard", items => {
    // Сортуємо за кількістю правильних відповідей
    const sorted = [...items].sort((a, b) => b.correctAnswers - a.correctAnswers);
    leaderboardList.innerHTML = '';
    sorted.forEach((it, i) => {
        const li = document.createElement('li');
        li.className = 'lb-item' + (it.nickname === nickname ? ' is-me' : '');
        li.innerHTML = `
            <span class="lb-rank">${i + 1}</span>
            <div class="lb-info">
                <div class="lb-name">${escapeHtml(it.nickname)}</div>
                <div class="lb-progress">Питання ${it.currentQuestionIndex} / ${it.totalQuestions}</div>
            </div>
            <span class="lb-score">${it.correctAnswers}</span>
        `;
        leaderboardList.appendChild(li);
    });
});

connection.start()
    .then(() => connectionDot.classList.add('connected'))
    .catch(err => console.error('SignalR error:', err));

// ===== СТАРТ =====
startBtn.addEventListener('click', async () => {
    nickname = nickInput.value.trim();
    if (!nickname) {
        nickInput.focus();
        nickInput.style.borderColor = 'var(--error)';
        setTimeout(() => nickInput.style.borderColor = '', 1000);
        return;
    }

    startBtn.disabled = true;
    startBtn.querySelector('span').textContent = 'Завантаження...';

    await connection.invoke("RegisterUser", quizId, nickname);

    const res = await fetch('/api/quiz/start', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ quizId, nickname })
    });
    const session = await res.json();
    totalQuestions = session.totalQuestions;
    currentQuestionIndex = session.currentQuestionIndex;

    startArea.classList.add('d-none');
    quizArea.classList.remove('d-none');
    updateProgress();
    renderQuestion(session.currentQuestion);
});

nickInput.addEventListener('keydown', e => {
    if (e.key === 'Enter') startBtn.click();
});

// ===== РЕНДЕР ПИТАННЯ =====
function renderQuestion(q) {
    questionContainer.innerHTML = '';

    const title = document.createElement('p');
    title.className = 'question-text';
    title.textContent = q.name;

    const list = document.createElement('div');
    list.className = 'answers-list';

    q.answers.forEach((a, i) => {
        const label = document.createElement('label');
        label.className = 'answer-label';

        const radio = document.createElement('input');
        radio.type = 'radio';
        radio.name = 'answer';
        radio.value = a.text;

        const radioCustom = document.createElement('span');
        radioCustom.className = 'answer-radio-custom';

        const text = document.createElement('span');
        text.className = 'answer-text';
        text.textContent = a.text;

        label.appendChild(radio);
        label.appendChild(radioCustom);
        label.appendChild(text);

        // Затримана поява варіантів
        label.style.animationDelay = `${i * 60}ms`;
        label.style.animation = 'fadeIn 0.3s ease both';

        list.appendChild(label);
    });

    questionContainer.appendChild(title);
    questionContainer.appendChild(list);

    submitBtn.disabled = false;
}

// ===== ВІДПРАВКА ВІДПОВІДІ =====
submitBtn.addEventListener('click', async () => {
    const sel = document.querySelector('input[name="answer"]:checked');
    if (!sel) return;

    submitBtn.disabled = true;

    try {
        console.log('1. Відправка відповіді...');
        const res = await fetch(`/api/quiz/${quizId}/submit`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ answer: sel.value, nickname })
        });
        const data = await res.json();

        console.log('Сирі дані з сервера:', data);

        const isCorrect = data.isCorrect === true || String(data.isCorrect).toLowerCase() === 'true';

        console.log('2. Відповідь оброблено (boolean):', isCorrect);

        showFeedback(isCorrect);

        console.log('3. Отримання стану...');
        const stateRes = await fetch(`/api/quiz/${quizId}?nickname=${encodeURIComponent(nickname)}`);
        const state = await stateRes.json();
        console.log('4. Стан:', state);
        currentQuestionIndex = state.currentQuestionIndex;

        await connection.invoke("UpdateProgress", quizId, nickname, currentQuestionIndex, state.correctAnswers);

        await new Promise(r => setTimeout(r, 700));

        console.log('5. Запит наступного питання...');
        const nextRes = await fetch(`/api/quiz/${quizId}/next?nickname=${encodeURIComponent(nickname)}`);
        console.log('6. Статус next:', nextRes.status, nextRes.ok);
        
        if (nextRes.ok) {
            const nextQ = await nextRes.json();
            console.log('7. Наступне питання:', nextQ);
            updateProgress();
            renderQuestion(nextQ);
        } else {
            console.log('8. Показ результатів');
            showResult();
        }
    } catch (error) {
        console.error('Помилка:', error);
        submitBtn.disabled = false; // Розблоковуємо кнопку при помилці
    }
});

// ===== ПРОГРЕС =====
function updateProgress() {
    const pct = ((currentQuestionIndex - 1) / totalQuestions) * 100;
    progressBar.style.width = `${pct}%`;
    progressLabel.textContent = `${currentQuestionIndex} / ${totalQuestions}`;
}

// ===== РЕЗУЛЬТАТ =====
async function showResult() {
    const res = await fetch(`/api/quiz/${quizId}?nickname=${encodeURIComponent(nickname)}`);
    const session = await res.json();

    await fetch(`/api/quiz/${quizId}/end?nickname=${encodeURIComponent(nickname)}`, { method: 'POST' });

    const pct = session.correctAnswers / session.totalQuestions;
    document.getElementById('resultEmoji').textContent = pct >= 0.8 ? '🏆' : pct >= 0.5 ? '🎯' : '💪';
    document.getElementById('resultScore').textContent = `${session.correctAnswers} / ${session.totalQuestions}`;
    document.getElementById('resultNick').textContent = nickname;

    quizArea.classList.add('d-none');
    resultArea.classList.remove('d-none');
}

// ===== TOAST FEEDBACK =====
let toastEl = null;
function showFeedback(isCorrect) {
    if (!toastEl) {
        toastEl = document.createElement('div');
        toastEl.className = 'feedback-toast';
        document.body.appendChild(toastEl);
    }
    toastEl.className = `feedback-toast ${isCorrect ? 'correct' : 'wrong'}`;
    toastEl.textContent = isCorrect ? '✓ Правильно!' : '✗ Неправильно';
    requestAnimationFrame(() => toastEl.classList.add('show'));
    setTimeout(() => toastEl.classList.remove('show'), 1800);
}

function escapeHtml(str) {
    return str.replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
}
