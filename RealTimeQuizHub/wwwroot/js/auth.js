// ===== AUTH PAGE =====
// Handles login + registration, stores the JWT in localStorage,
// then redirects to the main quiz page.

const TOKEN_KEY = 'quizhub_token';
const USER_KEY = 'quizhub_user';

// If already logged in, skip straight to the quiz.
if (localStorage.getItem(TOKEN_KEY)) {
    window.location.replace('./index.html');
}

const tabLogin      = document.getElementById('tabLogin');
const tabRegister   = document.getElementById('tabRegister');
const loginForm     = document.getElementById('loginForm');
const registerForm  = document.getElementById('registerForm');
const authError     = document.getElementById('authError');

// ===== TAB SWITCHING =====
function showLogin() {
    tabLogin.classList.add('is-active');
    tabRegister.classList.remove('is-active');
    loginForm.classList.remove('d-none');
    registerForm.classList.add('d-none');
    hideError();
}

function showRegister() {
    tabRegister.classList.add('is-active');
    tabLogin.classList.remove('is-active');
    registerForm.classList.remove('d-none');
    loginForm.classList.add('d-none');
    hideError();
}

tabLogin.addEventListener('click', showLogin);
tabRegister.addEventListener('click', showRegister);

// ===== ERROR HELPERS =====
function showError(msg) {
    authError.textContent = msg;
    authError.classList.remove('d-none');
}

function hideError() {
    authError.classList.add('d-none');
    authError.textContent = '';
}

// ===== SHARED =====
function persistAndRedirect(data) {
    localStorage.setItem(TOKEN_KEY, data.token);
    localStorage.setItem(USER_KEY, JSON.stringify(data.user));
    window.location.replace('./index.html');
}

async function readError(res, fallback) {
    try {
        const body = await res.json();
        return body.message || fallback;
    } catch {
        return fallback;
    }
}

// ===== REGISTER =====
registerForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    hideError();

    const name = document.getElementById('regName').value.trim();
    const email = document.getElementById('regEmail').value.trim();
    const password = document.getElementById('regPassword').value;
    const isAdmin = document.getElementById('regIsAdmin').checked;

    if (!name || !email || !password) {
        showError('Заповни всі поля.');
        return;
    }

    const btn = document.getElementById('registerBtn');
    btn.disabled = true;

    try {
        const res = await fetch('/api/auth/register', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ name, email, password, isAdmin })
        });

        if (!res.ok) {
            showError(await readError(res, 'Не вдалося зареєструватися.'));
            return;
        }

        persistAndRedirect(await res.json());
    } catch {
        showError('Сервер недоступний. Спробуй пізніше.');
    } finally {
        btn.disabled = false;
    }
});

// ===== LOGIN =====
loginForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    hideError();

    const email = document.getElementById('loginEmail').value.trim();
    const password = document.getElementById('loginPassword').value;

    if (!email || !password) {
        showError('Введи email і пароль.');
        return;
    }

    const btn = document.getElementById('loginBtn');
    btn.disabled = true;

    try {
        const res = await fetch('/api/auth/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, password })
        });

        if (!res.ok) {
            showError(await readError(res, 'Невірний email або пароль.'));
            return;
        }

        persistAndRedirect(await res.json());
    } catch {
        showError('Сервер недоступний. Спробуй пізніше.');
    } finally {
        btn.disabled = false;
    }
});
