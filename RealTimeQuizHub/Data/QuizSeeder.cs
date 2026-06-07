using RealTimeQuizHub.Models;

namespace RealTimeQuizHub.Data
{
    // One-time, idempotent seeding of sample quizzes. Run with: dotnet run -- seed
    // Quizzes already present (matched by title) are skipped, so it is safe to re-run.
    public static class QuizSeeder
    {
        public static int Seed(AppDbContext db)
        {
            var added = 0;
            foreach (var quiz in BuildQuizzes())
            {
                if (db.Quizzes.Any(q => q.Title == quiz.Title))
                {
                    continue; // already seeded
                }
                db.Quizzes.Add(quiz);
                added++;
            }
            if (added > 0)
            {
                db.SaveChanges();
            }
            return added;
        }

        private static List<Quiz> BuildQuizzes() => new()
        {
            // ── Вікторина на 5 питань (без таймера) ──
            MakeQuiz(
                "Географія світу",
                "Перевірте свої знання про країни, столиці та континенти.",
                hasTimer: false, seconds: 30, impact: 0.0,
                ("Яка столиця Франції?",
                    ("Париж", true), ("Лондон", false), ("Берлін", false), ("Мадрид", false)),
                ("Яка найбільша за площею країна світу?",
                    ("Росія", true), ("Канада", false), ("Китай", false), ("Бразилія", false)),
                ("На якому континенті розташована пустеля Сахара?",
                    ("Африка", true), ("Азія", false), ("Австралія", false), ("Південна Америка", false)),
                ("Яке море омиває південь України?",
                    ("Чорне", true), ("Балтійське", false), ("Червоне", false), ("Середземне", false)),
                ("Яка столиця Японії?",
                    ("Токіо", true), ("Пекін", false), ("Сеул", false), ("Бангкок", false))
            ),

            // ── Вікторина на 6 питань (з таймером) ──
            MakeQuiz(
                "Історія України",
                "Ключові постаті та події в історії України.",
                hasTimer: true, seconds: 20, impact: 0.5,
                ("У якому році Україна проголосила незалежність?",
                    ("1991", true), ("1989", false), ("1996", false), ("2004", false)),
                ("Хто був першим Президентом незалежної України?",
                    ("Леонід Кравчук", true), ("Леонід Кучма", false), ("Віктор Ющенко", false), ("Михайло Грушевський", false)),
                ("Яке місто є столицею України?",
                    ("Київ", true), ("Львів", false), ("Харків", false), ("Одеса", false)),
                ("Хто очолив національно-визвольну війну 1648 року?",
                    ("Богдан Хмельницький", true), ("Іван Мазепа", false), ("Петро Сагайдачний", false), ("Тарас Шевченко", false)),
                ("Який документ ухвалили 28 червня 1996 року?",
                    ("Конституцію України", true), ("Акт незалежності", false), ("Декларацію про суверенітет", false), ("Перший Універсал", false)),
                ("Хто написав збірку «Кобзар»?",
                    ("Тарас Шевченко", true), ("Іван Франко", false), ("Леся Українка", false), ("Григорій Сковорода", false))
            ),

            // ── Вікторина на 10 питань (з таймером) ──
            MakeQuiz(
                "Загальні знання",
                "Десять різнопланових питань на ерудицію.",
                hasTimer: true, seconds: 30, impact: 0.5,
                ("Скільки днів у звичайному (невисокосному) році?",
                    ("365", true), ("364", false), ("366", false), ("360", false)),
                ("Яка хімічна формула води?",
                    ("H2O", true), ("CO2", false), ("O2", false), ("NaCl", false)),
                ("Скільки планет у Сонячній системі?",
                    ("8", true), ("9", false), ("7", false), ("10", false)),
                ("Який орган качає кров у тілі людини?",
                    ("Серце", true), ("Печінка", false), ("Легені", false), ("Нирки", false)),
                ("Скільки кольорів у веселці?",
                    ("7", true), ("5", false), ("6", false), ("8", false)),
                ("Яка найбільша планета Сонячної системи?",
                    ("Юпітер", true), ("Сатурн", false), ("Земля", false), ("Марс", false)),
                ("Скільки ніг у павука?",
                    ("8", true), ("6", false), ("10", false), ("4", false)),
                ("Який метал є рідким за кімнатної температури?",
                    ("Ртуть", true), ("Залізо", false), ("Золото", false), ("Мідь", false)),
                ("Скільки сторін у трикутника?",
                    ("3", true), ("4", false), ("5", false), ("2", false)),
                ("Яка столиця Італії?",
                    ("Рим", true), ("Мілан", false), ("Венеція", false), ("Неаполь", false))
            ),

            // ── Вікторина на 3 питання (без таймера) ──
            MakeQuiz(
                "Космос",
                "Коротка вікторина про космос для початківців.",
                hasTimer: false, seconds: 30, impact: 0.0,
                ("Як називається зірка, навколо якої обертається Земля?",
                    ("Сонце", true), ("Місяць", false), ("Сіріус", false), ("Полярна зоря", false)),
                ("Яку планету називають «Червоною планетою»?",
                    ("Марс", true), ("Венера", false), ("Юпітер", false), ("Меркурій", false)),
                ("Хто першим полетів у космос?",
                    ("Юрій Гагарін", true), ("Ніл Армстронг", false), ("Валентина Терешкова", false), ("Олексій Леонов", false))
            ),
        };

        // Builds a Quiz with its ordered questions and answers in one object graph.
        // Each question carries exactly four answer options (A–D).
        private static Quiz MakeQuiz(
            string title,
            string description,
            bool hasTimer,
            int seconds,
            double impact,
            params (string Text,
                    (string Text, bool IsCorrect) A,
                    (string Text, bool IsCorrect) B,
                    (string Text, bool IsCorrect) C,
                    (string Text, bool IsCorrect) D)[] questions)
        {
            var quiz = new Quiz
            {
                Title = title,
                Description = description,
                HasTimer = hasTimer,
                TimerSecondsPerQuestion = seconds,
                TimerScoreImpact = hasTimer ? impact : 0.0,
                CreatedByUserId = 1,
                CreatedAt = DateTime.UtcNow,
                QuizQuestions = new List<QuizQuestion>()
            };

            for (var i = 0; i < questions.Length; i++)
            {
                var q = questions[i];
                var options = new[] { q.A, q.B, q.C, q.D };
                quiz.QuizQuestions.Add(new QuizQuestion
                {
                    OrderIndex = i,
                    Question = new Question
                    {
                        Name = q.Text,
                        Answers = options
                            .Select(a => new Answer { Text = a.Text, IsCorrect = a.IsCorrect })
                            .ToList()
                    }
                });
            }

            return quiz;
        }
    }
}
