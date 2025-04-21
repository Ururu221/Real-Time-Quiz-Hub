const quizId = 'default-session';

let totalQuestions = 0;    // сохраняется после старта
let correctAnswers = 0;    // считаем по ходу

// Buttons and containers
const startBtn = document.getElementById('startBtn');
const quizArea = document.getElementById('quizArea');
const questionContainer = document.getElementById('questionContainer');
const submitBtn = document.getElementById('submitBtn');

// 1) Начало викторины: запрашиваем старт у API
async function startQuiz() {
    const res = await fetch('/api/quiz/start', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(quizId)
    });
    const session = await res.json();
    totalQuestions = session.totalQuestions;             // сохраняем общее число
    renderQuestion(session.currentQuestion);             // показываем первый вопрос

    // переключаем видимость блоков
    document.getElementById('startArea').classList.add('d-none');
    quizArea.classList.remove('d-none');
}

// 2) Рендер вопроса и вариантов
function renderQuestion(q) {
    // Очищаем контейнер
    questionContainer.innerHTML = '';

    // Вопрос
    const h5 = document.createElement('h5');
    h5.textContent = q.name;
    questionContainer.appendChild(h5);

    // Список вариантов (Bootstrap list-group)
    const list = document.createElement('div');
    list.className = 'list-group';

    q.answers.forEach(a => {
        const label = document.createElement('label');
        label.className = 'list-group-item d-flex align-items-center';
        // радио-кнопка
        const radio = document.createElement('input');
        radio.type = 'radio';
        radio.name = 'answer';
        radio.value = a.text;
        radio.className = 'form-check-input me-2';
        label.appendChild(radio);
        // текст варианта
        label.appendChild(document.createTextNode(a.text));
        list.appendChild(label);
    });

    questionContainer.appendChild(list);
}

// 3) Отправка ответа и получение следующего вопроса
async function submitAnswer() {
    // найдём выбранный вариант
    const sel = document.querySelector('input[name="answer"]:checked');
    if (!sel) {
        alert('Please select an answer');
        return;
    }
    // отправляем на /api/quiz/{id}/submit
    await fetch(`/api/quiz/${quizId}/submit`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(sel.value)
    });

    // запрашиваем /api/quiz/{id}/next
    const nextRes = await fetch(`/api/quiz/${quizId}/next`);
    if (nextRes.ok) {
        const nextQ = await nextRes.json();
        renderQuestion(nextQ);
    } else {
        // если вопросов больше нет — показываем результат
        showResult();
    }
}

// 4) Вывод финального результата
async function showResult() {
    // берём финальную сессию
    const res = await fetch(`/api/quiz/${quizId}`);
    const session = await res.json();
    // чистим зону викторины
    questionContainer.innerHTML = '';
    submitBtn.classList.add('d-none');

    // выводим алерт с результатом
    const div = document.createElement('div');
    div.className = 'alert alert-success';
    div.textContent = `You got ${session.correctAnswers} of ${session.totalQuestions}`;
    questionContainer.appendChild(div);
}

// 5) Подключаем события
startBtn.addEventListener('click', startQuiz);
submitBtn.addEventListener('click', submitAnswer);
