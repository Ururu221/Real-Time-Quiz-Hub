// ===== QUIZ LIST (main page) =====
// Lists available quizzes as cards. Admins also get a "create quiz" button.

if (requireAuth()) {
    init();
}

function init() {
    const user = getUser();
    const userTag = document.getElementById('userTag');
    if (user && user.name) {
        userTag.textContent = user.isAdmin ? `${user.name} · admin` : user.name;
    }

    // "Створити вікторину" only for admins (read from the stored JWT user claims).
    if (isAdmin()) {
        document.getElementById('createQuizBtn').classList.remove('d-none');
    }

    loadQuizzes();
    renderGlobalLeaderboard();
}

async function loadQuizzes() {
    const stateEl = document.getElementById('quizzesState');
    const gridEl = document.getElementById('quizzesGrid');

    try {
        const res = await apiFetch('/api/quizzes');
        if (!res.ok) {
            stateEl.textContent = 'Не вдалося завантажити вікторини.';
            return;
        }

        const quizzes = await res.json();
        if (!quizzes.length) {
            stateEl.textContent = 'Вікторин ще немає. Адміністратор має створити першу вікторину.';
            return;
        }

        stateEl.classList.add('d-none');
        gridEl.innerHTML = quizzes.map(renderCard).join('');

        // Wire up admin delete buttons.
        gridEl.querySelectorAll('.quiz-delete').forEach(btn => {
            btn.addEventListener('click', () => deleteQuiz(btn.dataset.id, btn.dataset.title));
        });
    } catch (err) {
        stateEl.textContent = err.message || 'Сервер недоступний.';
    }
}

async function deleteQuiz(id, title) {
    if (!confirm(`Видалити вікторину «${title}»? Цю дію не можна скасувати.`)) {
        return;
    }
    try {
        const res = await apiFetch(`/api/quizzes/${id}`, { method: 'DELETE' });
        if (!res.ok && res.status !== 204) {
            let msg = 'Не вдалося видалити вікторину.';
            try { const b = await res.json(); msg = b.message || msg; } catch {}
            alert(msg);
            return;
        }
        loadQuizzes();
    } catch (err) {
        alert(err.message || 'Сервер недоступний.');
    }
}

function renderCard(quiz) {
    const desc = quiz.description
        ? `<p class="room-card-desc">${escapeHtml(quiz.description)}</p>`
        : '';

    const timer = quiz.hasTimer
        ? `<span class="room-card-pill room-card-pill--ghost">🕐 З таймером (${quiz.timerSecondsPerQuestion}s на питання)</span>`
        : '';

    return `
        <div class="room-card">
            <div class="room-card-top">
                <span class="room-card-pill">${formatPytannia(quiz.questionCount)}</span>
                ${timer}
            </div>
            <h3 class="room-card-name">${escapeHtml(quiz.title)}</h3>
            ${desc}
            <div class="room-card-foot">
                <a class="btn btn--sm" href="./play.html?quizId=${quiz.id}">
                    <span>Пройти вікторину</span>
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M5 12h14M12 5l7 7-7 7"/></svg>
                </a>
                ${isAdmin() ? `<button type="button" class="quiz-delete" data-id="${quiz.id}" data-title="${escapeHtml(quiz.title)}" title="Видалити вікторину">✕ Видалити</button>` : ''}
            </div>
        </div>
    `;
}

// Global leaderboard — top 10 users by total score, each with a profile tooltip.
async function renderGlobalLeaderboard() {
    const list = document.getElementById('globalLeaderboard');
    const note = document.querySelector('.lobby-lb .lb-note');

    try {
        const res = await apiFetch('/api/leaderboard');
        if (!res.ok) {
            if (note) note.textContent = 'Не вдалося завантажити рейтинг.';
            return;
        }

        const entries = (await res.json()).slice(0, 10);
        if (!entries.length) {
            list.innerHTML = '';
            if (note) note.textContent = 'Рейтинг порожній — зіграйте першу вікторину!';
            return;
        }

        if (note) note.classList.add('d-none');
        list.innerHTML = entries.map((e, i) => `
            <li class="lb-item">
                <span class="lb-rank">${String(i + 1).padStart(2, '0')}</span>
                <div class="lb-info">
                    ${playerNameWithTooltip(e)}
                    <div class="lb-progress">${escapeHtml(e.level)}</div>
                </div>
                <span class="lb-score-wrap"><span class="lb-score">${e.totalScore}</span></span>
            </li>
        `).join('');
    } catch (err) {
        if (note) note.textContent = err.message || 'Сервер недоступний.';
    }
}
