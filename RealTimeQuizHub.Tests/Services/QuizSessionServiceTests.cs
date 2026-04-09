using Moq;
using RealTimeQuizHub.Models;
using RealTimeQuizHub.Services;
using RealTimeQuizHub.Services.Interfaces;
using System.Reflection;

namespace RealTimeQuizHub.Tests.Services
{
    public class QuizSessionServiceTests : IDisposable
    {
        private readonly Mock<IQuestionService> _questionServiceMock;
        private readonly QuizSessionService _service;

        // Тестові дані
        private readonly List<Question> _fakeQuestions = new()
        {
            new Question
            {
                Id = 1, Name = "Що таке C#?",
                Answers = new List<Answer>
                {
                    new Answer { Id = 1, Text = "Мова програмування", IsCorrect = true },
                    new Answer { Id = 2, Text = "База даних",         IsCorrect = false },
                    new Answer { Id = 3, Text = "Операційна система", IsCorrect = false }
                }
            },
            new Question
            {
                Id = 2, Name = "Що таке SignalR?",
                Answers = new List<Answer>
                {
                    new Answer { Id = 4, Text = "Real-time бібліотека", IsCorrect = true },
                    new Answer { Id = 5, Text = "ORM фреймворк",        IsCorrect = false }
                }
            },
            new Question
            {
                Id = 3, Name = "Що таке xUnit?",
                Answers = new List<Answer>
                {
                    new Answer { Id = 6, Text = "Фреймворк для тестів", IsCorrect = true },
                    new Answer { Id = 7, Text = "UI бібліотека",        IsCorrect = false }
                }
            }
        };

        public QuizSessionServiceTests()
        {
            _questionServiceMock = new Mock<IQuestionService>();

            _questionServiceMock
                .Setup(s => s.GetAllQuestionsAsync())
                .ReturnsAsync(_fakeQuestions);

            _questionServiceMock
                .Setup(s => s.GetQuestionByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => _fakeQuestions.First(q => q.Id == id));

            _service = new QuizSessionService(_questionServiceMock.Object);
        }

        // Очищення статичного словника між тестами
        public void Dispose()
        {
            var field = typeof(QuizSessionService)
                .GetField("_quizSessions", BindingFlags.Static | BindingFlags.NonPublic);
            var dict = field!.GetValue(null) as System.Collections.IDictionary;
            dict!.Clear();
        }

        // ===== StartQuizAsync =====

        [Fact]
        public async Task StartQuizAsync_МаєПовернутиСесіюЗПравильнимиДаними()
        {
            var session = await _service.StartQuizAsync("quiz1", "Alice");

            Assert.Equal("quiz1", session.QuizId);
            Assert.Equal("Alice", session.Nickname);
            Assert.Equal(3, session.TotalQuestions);
            Assert.Equal(1, session.CurrentQuestionIndex);
            Assert.Equal(_fakeQuestions[0].Name, session.CurrentQuestion.Name);
        }

        [Fact]
        public async Task StartQuizAsync_МаєСтворитиОкремуСесіюДляКожногоНікнейму()
        {
            var sessionAlice = await _service.StartQuizAsync("quiz1", "Alice");
            var sessionBob   = await _service.StartQuizAsync("quiz1", "Bob");

            Assert.Equal("Alice", sessionAlice.Nickname);
            Assert.Equal("Bob", sessionBob.Nickname);
            // Сесії незалежні — це різні об'єкти
            Assert.NotSame(sessionAlice, sessionBob);
        }

        [Fact]
        public async Task StartQuizAsync_МаєВикликатиGetAllQuestionsAsync()
        {
            await _service.StartQuizAsync("quiz1", "Alice");

            _questionServiceMock.Verify(s => s.GetAllQuestionsAsync(), Times.Once);
        }

        // ===== GetQuizSessionAsync =====

        [Fact]
        public async Task GetQuizSessionAsync_МаєПовернутиСесіюПіслястарту()
        {
            await _service.StartQuizAsync("quiz1", "Alice");
            var session = await _service.GetQuizSessionAsync("quiz1", "Alice");

            Assert.NotNull(session);
            Assert.Equal("Alice", session!.Nickname);
        }

        [Fact]
        public async Task GetQuizSessionAsync_МаєПовернутиNullЯкщоСесіяНеІснує()
        {
            var session = await _service.GetQuizSessionAsync("quiz1", "NonExistent");

            Assert.Null(session);
        }

        [Fact]
        public async Task GetQuizSessionAsync_НеМаєПовертатиСесіюІншогоКористувача()
        {
            await _service.StartQuizAsync("quiz1", "Alice");

            // Боб шукає свою сесію — має отримати null, бо він не стартував
            var session = await _service.GetQuizSessionAsync("quiz1", "Bob");

            Assert.Null(session);
        }

        // ===== GetNextQuestionAsync =====

        [Fact]
        public async Task GetNextQuestionAsync_МаєПовернутиНаступнеПитання()
        {
            await _service.StartQuizAsync("quiz1", "Alice");

            var nextQuestion = await _service.GetNextQuestionAsync("quiz1", "Alice");

            Assert.NotNull(nextQuestion);
            Assert.Equal(_fakeQuestions[1].Name, nextQuestion!.Name);
        }

        [Fact]
        public async Task GetNextQuestionAsync_МаєПовернутиNullКолиПитаньБільшеНемає()
        {
            await _service.StartQuizAsync("quiz1", "Alice");

            // Проходимо всі питання
            await _service.GetNextQuestionAsync("quiz1", "Alice"); // питання 2
            await _service.GetNextQuestionAsync("quiz1", "Alice"); // питання 3
            var result = await _service.GetNextQuestionAsync("quiz1", "Alice"); // питань більше нема

            Assert.Null(result);
        }

        [Fact]
        public async Task GetNextQuestionAsync_НеМаєВпливатиНаСесіюІншогоКористувача()
        {
            await _service.StartQuizAsync("quiz1", "Alice");
            await _service.StartQuizAsync("quiz1", "Bob");

            // Alice переходить на наступне питання
            await _service.GetNextQuestionAsync("quiz1", "Alice");

            // Bob має залишатись на першому питанні
            var bobSession = await _service.GetQuizSessionAsync("quiz1", "Bob");
            Assert.Equal(1, bobSession!.CurrentQuestionIndex);
        }

        // ===== SubmitAnswerAsync =====

        [Fact]
        public async Task SubmitAnswerAsync_МаєПовернутиTrueПриПравильнійВідповіді()
        {
            await _service.StartQuizAsync("quiz1", "Alice");

            var result = await _service.SubmitAnswerAsync("quiz1", "Мова програмування", "Alice");

            Assert.True(result);
        }

        [Fact]
        public async Task SubmitAnswerAsync_МаєПовернутиFalseПриНеправильнійВідповіді()
        {
            await _service.StartQuizAsync("quiz1", "Alice");

            var result = await _service.SubmitAnswerAsync("quiz1", "База даних", "Alice");

            Assert.False(result);
        }

        [Fact]
        public async Task SubmitAnswerAsync_МаєПовернутиFalseЯкщоВідповідьНеІснує()
        {
            await _service.StartQuizAsync("quiz1", "Alice");

            var result = await _service.SubmitAnswerAsync("quiz1", "Відповідь якої немає", "Alice");

            Assert.False(result);
        }

        [Fact]
        public async Task SubmitAnswerAsync_МаєЗбільшитиCorrectAnswersПриПравильнійВідповіді()
        {
            await _service.StartQuizAsync("quiz1", "Alice");

            await _service.SubmitAnswerAsync("quiz1", "Мова програмування", "Alice");

            var session = await _service.GetQuizSessionAsync("quiz1", "Alice");
            Assert.Equal(1, session!.CorrectAnswers);
        }

        [Fact]
        public async Task SubmitAnswerAsync_НеМаєЗбільшитиCorrectAnswersПриНеправильнійВідповіді()
        {
            await _service.StartQuizAsync("quiz1", "Alice");

            await _service.SubmitAnswerAsync("quiz1", "База даних", "Alice");

            var session = await _service.GetQuizSessionAsync("quiz1", "Alice");
            Assert.Equal(0, session!.CorrectAnswers);
        }

        [Fact]
        public async Task SubmitAnswerAsync_МаєПовернутиFalseЯкщоСесіяНеІснує()
        {
            var result = await _service.SubmitAnswerAsync("quiz1", "Мова програмування", "Ghost");

            Assert.False(result);
        }

        // ===== EndQuizAsync =====

        [Fact]
        public async Task EndQuizAsync_МаєВидалитиСесіюПіслязавершення()
        {
            await _service.StartQuizAsync("quiz1", "Alice");

            await _service.EndQuizAsync("quiz1", "Alice");

            var session = await _service.GetQuizSessionAsync("quiz1", "Alice");
            Assert.Null(session);
        }

        [Fact]
        public async Task EndQuizAsync_НеМаєВпливатиНаСесіюІншогоКористувача()
        {
            await _service.StartQuizAsync("quiz1", "Alice");
            await _service.StartQuizAsync("quiz1", "Bob");

            await _service.EndQuizAsync("quiz1", "Alice");

            var bobSession = await _service.GetQuizSessionAsync("quiz1", "Bob");
            Assert.NotNull(bobSession);
        }

        // ===== Ізоляція між різними quizId =====

        [Fact]
        public async Task Сесії_РізнихКвізівМаютьБутиНезалежними()
        {
            await _service.StartQuizAsync("quiz-A", "Alice");
            await _service.StartQuizAsync("quiz-B", "Alice");

            await _service.GetNextQuestionAsync("quiz-A", "Alice");

            var sessionA = await _service.GetQuizSessionAsync("quiz-A", "Alice");
            var sessionB = await _service.GetQuizSessionAsync("quiz-B", "Alice");

            Assert.Equal(2, sessionA!.CurrentQuestionIndex);
            Assert.Equal(1, sessionB!.CurrentQuestionIndex);
        }
    }
}
