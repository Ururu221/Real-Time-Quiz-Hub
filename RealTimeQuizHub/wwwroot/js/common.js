// ===== SHARED HELPERS =====
// Token storage + authenticated fetch, reused across quiz list / play / admin pages.

const TOKEN_KEY = 'quizhub_token';
const USER_KEY = 'quizhub_user';

function getToken() {
    return localStorage.getItem(TOKEN_KEY);
}

function getUser() {
    try {
        return JSON.parse(localStorage.getItem(USER_KEY) || 'null');
    } catch {
        return null;
    }
}

function isAdmin() {
    const u = getUser();
    return !!(u && u.isAdmin);
}

function logout() {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    window.location.replace('./login.html');
}

// Bounce to login if there is no token. Call at the top of protected pages.
function requireAuth() {
    if (!getToken()) {
        window.location.replace('./login.html');
        return false;
    }
    return true;
}

// fetch wrapper that always attaches the JWT and redirects to login on 401.
async function apiFetch(url, options = {}) {
    const headers = Object.assign({}, options.headers, {
        'Authorization': `Bearer ${getToken()}`
    });
    if (options.body && !headers['Content-Type']) {
        headers['Content-Type'] = 'application/json';
    }

    const res = await fetch(url, Object.assign({}, options, { headers }));
    if (res.status === 401) {
        logout();
        throw new Error('Сесія завершилась. Увійдіть знову.');
    }
    return res;
}

function escapeHtml(str) {
    return String(str ?? '').replace(/[&<>"']/g, c =>
        ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
}

// Ukrainian plural for "питання" (question):
//   1 → "питання", 2–4 → "питання", 5+ and 11–19 → "питань".
function formatPytannia(n) {
    const mod10 = n % 10;
    const mod100 = n % 100;
    if (mod100 >= 11 && mod100 <= 19) return `${n} питань`;
    if (mod10 === 1) return `${n} питання`;
    if (mod10 >= 2 && mod10 <= 4) return `${n} питання`;
    return `${n} питань`;
}

// Ukrainian plural for "вікторина" (quiz):
//   1 → "вікторина", 2–4 → "вікторини", 5+ and 11–19 → "вікторин".
function formatViktoryn(n) {
    const mod10 = n % 10;
    const mod100 = n % 100;
    if (mod100 >= 11 && mod100 <= 19) return `${n} вікторин`;
    if (mod10 === 1) return `${n} вікторина`;
    if (mod10 >= 2 && mod10 <= 4) return `${n} вікторини`;
    return `${n} вікторин`;
}

// ===== LEADERBOARD / PLAYER TOOLTIP HELPERS =====
// Shared by the global lobby leaderboard and the quiz result leaderboard.

// A username chip that reveals a profile tooltip on hover/focus.
// `entry` carries: name, level, totalScore, quizzesCompleted, wins, badges[].
function playerNameWithTooltip(entry) {
    const badges = (entry.badges || []);
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
