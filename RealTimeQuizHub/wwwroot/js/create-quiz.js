// ===== ADMIN: CREATE QUIZ =====
// One-step quiz creation: basic info + timer + questions (with answers).

const MIN_ANSWERS = 2;
const MAX_ANSWERS = 6;

let questionGroupCounter = 0; // ensures each question's radios form a unique group

const els = {};

function init() {
    els.form            = document.getElementById('quizForm');
    els.title           = document.getElementById('quizTitle');
    els.desc            = document.getElementById('quizDesc');

    els.hasTimer        = document.getElementById('hasTimer');
    els.timerSettings   = document.getElementById('timerSettings');
    els.timerSeconds    = document.getElementById('timerSeconds');
    els.timerImpact     = document.getElementById('timerImpact');
    els.timerImpactValue = document.getElementById('timerImpactValue');
    els.timerPreview    = document.getElementById('timerPreview');

    els.questionsContainer = document.getElementById('questionsContainer');
    els.addQuestionBtn  = document.getElementById('addQuestionBtn');

    els.error           = document.getElementById('formError');
    els.success         = document.getElementById('formSuccess');
    els.submitBtn       = document.getElementById('submitBtn');

    els.hasTimer.addEventListener('change', syncTimerVisibility);
    els.timerImpact.addEventListener('input', () => {
        els.timerImpactValue.textContent = Number(els.timerImpact.value).toFixed(1);
        updateTimerPreview();
    });
    els.timerSeconds.addEventListener('input', updateTimerPreview);

    els.addQuestionBtn.addEventListener('click', () => addQuestionBlock());
    els.form.addEventListener('submit', onSubmit);

    // Start with one empty question.
    addQuestionBlock();
    syncTimerVisibility();
}

// ===== TIMER =====
function syncTimerVisibility() {
    const on = els.hasTimer.checked;
    els.timerSettings.classList.toggle('d-none', !on);
    if (on) updateTimerPreview();
}

function updateTimerPreview() {
    const total = parseInt(els.timerSeconds.value, 10) || 30;
    const impact = parseFloat(els.timerImpact.value) || 0;
    const exampleAnswerTime = Math.max(1, Math.round(total / 3));
    const timeRemaining = total - exampleAnswerTime;
    const bonus = impact * (timeRemaining / total) * 100;
    els.timerPreview.textContent =
        `При відповіді за ${exampleAnswerTime}с з ${total}с — бонус +${Math.round(bonus)}%`;
}

// ===== QUESTIONS =====
function addQuestionBlock() {
    const groupName = `correct-${++questionGroupCounter}`;

    const block = document.createElement('div');
    block.className = 'q-block';
    block.dataset.group = groupName;

    const head = document.createElement('div');
    head.className = 'q-block-head';

    const title = document.createElement('span');
    title.className = 'q-block-title';
    title.textContent = 'Питання';

    const removeBtn = document.createElement('button');
    removeBtn.type = 'button';
    removeBtn.className = 'q-remove';
    removeBtn.textContent = '✕ Видалити питання';
    removeBtn.addEventListener('click', () => {
        block.remove();
        renumberQuestions();
    });

    head.append(title, removeBtn);

    const textInput = document.createElement('input');
    textInput.type = 'text';
    textInput.className = 'nick-input q-text';
    textInput.maxLength = 300;
    textInput.placeholder = 'Текст питання';

    const answersList = document.createElement('div');
    answersList.className = 'answers-edit';

    const addAnswerBtn = document.createElement('button');
    addAnswerBtn.type = 'button';
    addAnswerBtn.className = 'add-btn add-answer-btn';
    addAnswerBtn.textContent = '+ Додати варіант відповіді';
    addAnswerBtn.addEventListener('click', () => addAnswerRow(answersList, groupName));

    block.append(head, textInput, answersList, addAnswerBtn);
    els.questionsContainer.appendChild(block);

    // Two answers by default (the minimum).
    addAnswerRow(answersList, groupName);
    addAnswerRow(answersList, groupName);

    renumberQuestions();
}

function addAnswerRow(answersList, groupName) {
    if (answersList.querySelectorAll('.answer-row').length >= MAX_ANSWERS) {
        return;
    }

    const row = document.createElement('div');
    row.className = 'answer-row';

    const radio = document.createElement('input');
    radio.type = 'radio';
    radio.name = groupName;
    radio.className = 'answer-correct';
    radio.title = 'Правильна відповідь';

    const text = document.createElement('input');
    text.type = 'text';
    text.className = 'nick-input answer-text';
    text.maxLength = 100;
    text.placeholder = 'Варіант відповіді';

    const remove = document.createElement('button');
    remove.type = 'button';
    remove.className = 'answer-remove';
    remove.textContent = '✕';
    remove.title = 'Видалити варіант';
    remove.addEventListener('click', () => {
        if (answersList.querySelectorAll('.answer-row').length > MIN_ANSWERS) {
            row.remove();
        }
    });

    row.append(radio, text, remove);
    answersList.appendChild(row);
}

function renumberQuestions() {
    const blocks = els.questionsContainer.querySelectorAll('.q-block');
    blocks.forEach((block, i) => {
        block.querySelector('.q-block-title').textContent = `Питання ${i + 1}`;
        // Allow deleting a question only when more than one exists.
        block.querySelector('.q-remove').style.display = blocks.length > 1 ? '' : 'none';
    });
}

// ===== COLLECT + VALIDATE =====
function collectQuestions() {
    return Array.from(els.questionsContainer.querySelectorAll('.q-block')).map(block => {
        const text = block.querySelector('.q-text').value.trim();
        const answers = Array.from(block.querySelectorAll('.answer-row')).map(row => ({
            text: row.querySelector('.answer-text').value.trim(),
            isCorrect: row.querySelector('.answer-correct').checked
        }));
        return { text, answers };
    });
}

function validate(questions) {
    if (!els.title.value.trim()) {
        return 'Введіть назву вікторини';
    }
    if (questions.length === 0) {
        return 'Додайте хоча б одне питання';
    }
    for (let i = 0; i < questions.length; i++) {
        const num = i + 1;
        const q = questions[i];
        if (!q.text) {
            return `Питання ${num}: введіть текст питання`;
        }
        const filled = q.answers.filter(a => a.text);
        if (filled.length < 2) {
            return `Питання ${num}: додайте хоча б 2 варіанти відповіді`;
        }
        if (!filled.some(a => a.isCorrect)) {
            return `Питання ${num}: позначте правильну відповідь`;
        }
    }
    return null;
}

// ===== SUBMIT =====
function showError(msg) {
    els.success.classList.add('d-none');
    els.error.textContent = msg;
    els.error.classList.remove('d-none');
}

function showSuccess(msg) {
    els.error.classList.add('d-none');
    els.success.textContent = msg;
    els.success.classList.remove('d-none');
}

function hideMessages() {
    els.error.classList.add('d-none');
    els.success.classList.add('d-none');
}

async function onSubmit(e) {
    e.preventDefault();
    hideMessages();

    const questions = collectQuestions();
    const err = validate(questions);
    if (err) {
        showError(err);
        return;
    }

    const hasTimer = els.hasTimer.checked;
    const payload = {
        title: els.title.value.trim(),
        description: els.desc.value.trim() || null,
        hasTimer,
        timerSecondsPerQuestion: hasTimer ? (parseInt(els.timerSeconds.value, 10) || 30) : 30,
        timerScoreImpact: hasTimer ? (parseFloat(els.timerImpact.value) || 0) : 0,
        // Only send filled-in answers.
        questions: questions.map(q => ({
            text: q.text,
            answers: q.answers.filter(a => a.text)
        }))
    };

    els.submitBtn.disabled = true;
    try {
        const res = await apiFetch('/api/quizzes', {
            method: 'POST',
            body: JSON.stringify(payload)
        });

        if (!res.ok) {
            let msg = 'Не вдалося створити вікторину.';
            try { const b = await res.json(); msg = b.message || msg; } catch {}
            showError(msg);
            els.submitBtn.disabled = false;
            return;
        }

        // Success — keep the form intact, show message, then go to the list.
        showSuccess('Вікторину успішно створено!');
        setTimeout(() => window.location.replace('./index.html'), 1200);
    } catch (err) {
        showError(err.message || 'Сервер недоступний.');
        els.submitBtn.disabled = false;
    }
}

// ===== BOOTSTRAP =====
// Runs last so init() can safely touch the `els`/`const` declared above.
if (requireAuth()) {
    if (!isAdmin()) {
        window.location.replace('./index.html');
    } else {
        init();
    }
}
