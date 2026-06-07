// Quiz id is passed from the quiz list page (?quizId=ID); fall back to the
// legacy single-session id so older links keep working.
const params = new URLSearchParams(window.location.search);
const quizId = params.get('quizId') || params.get('id') || 'default-session';
let QUESTION_TIME = 30;
const URGENT_THRESHOLD = 10;

// ===== AUTH GUARD =====
const TOKEN_KEY = 'quizhub_token';
const USER_KEY = 'quizhub_user';

const authToken = localStorage.getItem(TOKEN_KEY);
if (!authToken) {
    // Not logged in — bounce to the auth page.
    window.location.replace('./login.html');
}

let currentUser = null;
try {
    currentUser = JSON.parse(localStorage.getItem(USER_KEY) || 'null');
} catch {
    currentUser = null;
}

// Match the countdown to this quiz's configured timer (if it has one).
(async () => {
    if (!/^\d+$/.test(quizId)) return;
    try {
        const res = await fetch(`/api/quizzes/${quizId}`, {
            headers: { 'Authorization': `Bearer ${authToken}` }
        });
        if (res.ok) {
            const quiz = await res.json();
            if (quiz.hasTimer && quiz.timerSecondsPerQuestion > 0) {
                QUESTION_TIME = quiz.timerSecondsPerQuestion;
            }
        }
    } catch { /* keep default */ }
})();

function logout() {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    window.location.replace('./login.html');
}

let nickname = '';
let totalQuestions = 0;
let currentQuestionIndex = 1;

// Per-answer results collected over the session, used to record the quiz score.
const answerResults = [];

const startArea         = document.getElementById('startArea');
const startBtn          = document.getElementById('startBtn');
const nickInput         = document.getElementById('nickInput');
const quizArea          = document.getElementById('quizArea');
const resultArea        = document.getElementById('resultArea');
const questionContainer = document.getElementById('questionContainer');
const submitBtn         = document.getElementById('submitBtn');
const leaderboardList   = document.getElementById('leaderboardList');
const progressBar       = document.getElementById('progressBar');
const progressLabel     = document.getElementById('progressLabel');
const connectionDot     = document.getElementById('connectionDot');
const timerNumber       = document.getElementById('timerNumber');
const timerFill         = document.getElementById('timerFill');

// Prefill the nickname with the logged-in user's name.
if (currentUser && currentUser.name) {
    nickInput.value = currentUser.name;
}

// ===== TIMER =====
let timerInterval = null;
let timeLeft = QUESTION_TIME;

function startTimer() {
    stopTimer();
    timeLeft = QUESTION_TIME;
    renderTimer();
    timerInterval = setInterval(() => {
        timeLeft--;
        if (timeLeft <= 0) {
            renderTimer();
            stopTimer();
            handleTimeout();
            return;
        }
        renderTimer();
    }, 1000);
}

function stopTimer() {
    if (timerInterval) {
        clearInterval(timerInterval);
        timerInterval = null;
    }
}

function renderTimer() {
    const display = Math.max(0, timeLeft);
    timerNumber.textContent = display;
    const pct = (display / QUESTION_TIME) * 100;
    timerFill.style.width = `${pct}%`;
    const urgent = display <= URGENT_THRESHOLD;
    timerNumber.classList.toggle('urgent', urgent);
    timerFill.classList.toggle('urgent', urgent);
}

function handleTimeout() {
    const sel = document.querySelector('input[name="answer"]:checked');
    if (sel) {
        submitBtn.click();
    } else {
        submitAnswer(null);
    }
}

// ===== SIGNALR =====
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/quizHub", { accessTokenFactory: () => localStorage.getItem(TOKEN_KEY) })
    .withAutomaticReconnect()
    .build();

const prevScores = {};
const knownPlayers = new Set();

connection.on("BroadcastLeaderboard", items => {
    const sorted = [...items].sort((a, b) => b.correctAnswers - a.correctAnswers);

    // FLIP: snapshot old positions before DOM swap
    const oldPositions = {};
    leaderboardList.querySelectorAll('.lb-item').forEach(el => {
        const key = el.dataset.key;
        if (key) oldPositions[key] = el.getBoundingClientRect().top;
    });

    leaderboardList.innerHTML = '';

    sorted.forEach((it, i) => {
        const li = document.createElement('li');
        li.className = 'lb-item' + (it.nickname === nickname ? ' is-me' : '');
        li.dataset.key = it.nickname;
        const rank = String(i + 1).padStart(2, '0');
        const prevScore = prevScores[it.nickname] ?? 0;
        const delta = it.correctAnswers - prevScore;

        li.innerHTML = `
            <span class="lb-rank">${rank}</span>
            <div class="lb-info">
                <div class="lb-name">${escapeHtml(it.nickname)}</div>
                <div class="lb-progress">Q ${it.currentQuestionIndex}/${it.totalQuestions}</div>
            </div>
            <span class="lb-score-wrap">
                <span class="lb-score">${it.correctAnswers}</span>
                ${delta > 0 ? `<span class="lb-delta">+${delta}</span>` : ''}
            </span>
        `;

        if (!knownPlayers.has(it.nickname)) {
            li.classList.add('is-new');
            knownPlayers.add(it.nickname);
        }

        leaderboardList.appendChild(li);
    });

    // FLIP: invert, then play
    leaderboardList.querySelectorAll('.lb-item').forEach(el => {
        const key = el.dataset.key;
        const oldTop = oldPositions[key];
        if (oldTop === undefined) return;
        const newTop = el.getBoundingClientRect().top;
        const dy = oldTop - newTop;
        if (dy === 0) return;
        el.style.transform = `translateY(${dy}px)`;
        requestAnimationFrame(() => {
            el.classList.add('flip-animating');
            el.style.transform = '';
        });
        el.addEventListener('transitionend', () => {
            el.classList.remove('flip-animating');
        }, { once: true });
    });

    // Animate score delta — show then fade
    leaderboardList.querySelectorAll('.lb-delta').forEach(deltaEl => {
        requestAnimationFrame(() => {
            deltaEl.classList.add('show');
            setTimeout(() => {
                deltaEl.classList.remove('show');
                deltaEl.classList.add('fade');
            }, 900);
        });
    });

    // Snapshot scores for next broadcast
    sorted.forEach(it => { prevScores[it.nickname] = it.correctAnswers; });
});

connection.start()
    .then(() => connectionDot.classList.add('connected'))
    .catch(err => console.error('SignalR error:', err));

// ===== START =====
startBtn.addEventListener('click', async () => {
    nickname = nickInput.value.trim();
    if (!nickname) {
        nickInput.focus();
        nickInput.classList.remove('shake');
        void nickInput.offsetWidth;
        nickInput.classList.add('shake');
        return;
    }

    startBtn.disabled = true;
    startBtn.querySelector('span').textContent = 'Завантаження…';

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
    document.body.classList.add('quiz-active');
    updateProgress();
    renderQuestion(session.currentQuestion);
});

nickInput.addEventListener('keydown', e => {
    if (e.key === 'Enter') startBtn.click();
});

// ===== RENDER QUESTION =====
const LETTERS = ['A', 'B', 'C', 'D', 'E', 'F'];

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

        const letter = document.createElement('span');
        letter.className = 'answer-letter';
        letter.textContent = LETTERS[i] || String(i + 1);

        const text = document.createElement('span');
        text.className = 'answer-text';
        text.textContent = a.text;

        label.append(radio, letter, text);
        list.appendChild(label);
    });

    questionContainer.append(title, list);
    submitBtn.disabled = false;
    startTimer();
}

// ===== SUBMIT =====
submitBtn.addEventListener('click', () => {
    const sel = document.querySelector('input[name="answer"]:checked');
    if (!sel) return;
    submitAnswer(sel.value);
});

async function submitAnswer(answerValue) {
    stopTimer();
    submitBtn.disabled = true;

    const list = document.querySelector('.answers-list');
    if (list) list.classList.add('locked');

    try {
        const res = await fetch(`/api/quiz/${quizId}/submit`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ answer: answerValue ?? '', nickname })
        });
        const data = await res.json();
        const isCorrect = data.isCorrect === true || String(data.isCorrect).toLowerCase() === 'true';

        // Capture correctness + seconds left for server-side score calculation.
        answerResults.push({ isCorrect, timeRemaining: Math.max(0, timeLeft) });

        showFeedback(isCorrect);
        flashSelectedRow(isCorrect);

        const stateRes = await fetch(`/api/quiz/${quizId}?nickname=${encodeURIComponent(nickname)}`);
        const state = await stateRes.json();
        currentQuestionIndex = state.currentQuestionIndex;

        await connection.invoke("UpdateProgress", quizId, nickname, currentQuestionIndex, state.correctAnswers);

        await new Promise(r => setTimeout(r, 900));

        const nextRes = await fetch(`/api/quiz/${quizId}/next?nickname=${encodeURIComponent(nickname)}`);

        if (nextRes.ok) {
            const nextQ = await nextRes.json();
            updateProgress();
            renderQuestion(nextQ);
        } else {
            showResult();
        }
    } catch (error) {
        console.error('Помилка:', error);
        submitBtn.disabled = false;
        if (list) list.classList.remove('locked');
    }
}

function flashSelectedRow(isCorrect) {
    const selectedLabel = document.querySelector('.answer-label:has(input:checked)');
    if (!selectedLabel) return;
    selectedLabel.classList.add(isCorrect ? 'reveal-correct' : 'reveal-wrong');
}

// ===== PROGRESS =====
function updateProgress() {
    const pct = ((currentQuestionIndex - 1) / totalQuestions) * 100;
    progressBar.style.width = `${pct}%`;
    const cur = String(currentQuestionIndex).padStart(2, '0');
    const tot = String(totalQuestions).padStart(2, '0');
    progressLabel.innerHTML = `Question <strong>${cur}</strong> &mdash; of ${tot}`;
}

// ===== RESULT =====
async function showResult() {
    stopTimer();
    document.body.classList.remove('quiz-active');

    const res = await fetch(`/api/quiz/${quizId}?nickname=${encodeURIComponent(nickname)}`);
    const session = await res.json();

    await fetch(`/api/quiz/${quizId}/end?nickname=${encodeURIComponent(nickname)}`, { method: 'POST' });

    const pct = session.correctAnswers / session.totalQuestions;
    document.getElementById('resultEmoji').textContent = pct >= 0.8 ? '🏆' : pct >= 0.5 ? '🎯' : '💪';
    document.getElementById('resultScore').textContent = session.correctAnswers;
    document.getElementById('resultScoreOf').textContent = `/ ${session.totalQuestions}`;
    document.getElementById('resultNick').textContent = nickname;
    document.getElementById('resultPct').textContent = `${Math.round(pct * 100)}% correct`;

    quizArea.classList.add('d-none');
    resultArea.classList.remove('d-none');

    // Persist the score, surface points + new badges, and show the quiz
    // leaderboard. Only real quizzes (numeric id) are recorded.
    if (/^\d+$/.test(quizId)) {
        await recordQuizCompletion();
    }
}

async function recordQuizCompletion() {
    try {
        const res = await fetch('/api/scores/complete', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${localStorage.getItem(TOKEN_KEY)}`
            },
            body: JSON.stringify({ quizId: Number(quizId), answers: answerResults })
        });
        if (res.ok) {
            const result = await res.json();
            showPointsAndBadges(result);
        }
    } catch (err) {
        console.error('Не вдалося зберегти результат:', err);
    }

    await renderQuizLeaderboard();
}

function showPointsAndBadges(result) {
    const pointsEl = document.getElementById('resultPoints');
    pointsEl.textContent = `+${result.score} очок · ${result.level}`;
    pointsEl.classList.remove('d-none');

    if (result.newBadges && result.newBadges.length) {
        const badgesEl = document.getElementById('resultBadges');
        badgesEl.innerHTML = '<span class="result-badges-label">Нові бейджі:</span>' +
            result.newBadges.map(b =>
                `<span class="result-badge">${escapeHtml(b.iconEmoji)} ${escapeHtml(b.name)}</span>`
            ).join('');
        badgesEl.classList.remove('d-none');
    }
}

async function renderQuizLeaderboard() {
    try {
        const res = await fetch(`/api/quizzes/${quizId}/leaderboard`, {
            headers: { 'Authorization': `Bearer ${localStorage.getItem(TOKEN_KEY)}` }
        });
        if (!res.ok) return;

        const entries = await res.json();
        if (!entries.length) return;

        const list = document.getElementById('quizLeaderboard');
        list.innerHTML = entries.map((e, i) => `
            <li class="lb-item">
                <span class="lb-rank">${String(i + 1).padStart(2, '0')}</span>
                <div class="lb-info">
                    ${playerNameWithTooltip(e)}
                    <div class="lb-progress">${escapeHtml(e.level)}</div>
                </div>
                <span class="lb-score-wrap"><span class="lb-score">${e.score}</span></span>
            </li>
        `).join('');
        document.getElementById('quizLeaderboardWrap').classList.remove('d-none');
    } catch (err) {
        console.error('Не вдалося завантажити рейтинг вікторини:', err);
    }
}

// Username chip with a profile tooltip (level, totals, badges) on hover/focus.
function playerNameWithTooltip(entry) {
    const badges = entry.badges || [];
    const badgeRows = badges.length
        ? badges.map(b => `
            <li class="pt-badge">
                <span class="pt-badge-icon">${escapeHtml(b.iconEmoji)}</span>
                <span class="pt-badge-name">${escapeHtml(b.name)}</span>
            </li>`).join('')
        : '<li class="pt-badge pt-badge--empty">Ще немає бейджів</li>';

    return `
        <span class="lb-player" tabindex="0">
            <span class="lb-name">${escapeHtml(entry.name)}</span>
            <span class="player-tooltip" role="tooltip">
                <span class="pt-head">
                    <span class="pt-name">${escapeHtml(entry.name)}</span>
                    <span class="pt-level">${escapeHtml(entry.level)}</span>
                </span>
                <span class="pt-stats">
                    <span class="pt-stat"><b>${entry.totalScore ?? 0}</b> очок</span>
                    <span class="pt-stat"><b>${entry.quizzesCompleted ?? 0}</b> вікторин</span>
                    <span class="pt-stat"><b>${entry.wins ?? 0}</b> перемог</span>
                </span>
                <ul class="pt-badges">${badgeRows}</ul>
            </span>
        </span>`;
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
    toastEl.innerHTML = isCorrect
        ? '<span class="mark">+</span> Правильно'
        : '<span class="mark">&times;</span> Неправильно';
    requestAnimationFrame(() => toastEl.classList.add('show'));
    setTimeout(() => toastEl.classList.remove('show'), 1800);
}

function escapeHtml(str) {
    return str.replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
}
