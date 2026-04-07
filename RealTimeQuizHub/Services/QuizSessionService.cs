using RealTimeQuizHub.Models;
using RealTimeQuizHub.Services.Interfaces;

namespace RealTimeQuizHub.Services
{
    public class QuizSessionService : IQuizSessionService
    {
        private readonly IQuestionService _questionService;
        private readonly static Dictionary<string, QuizSession> _quizSessions = new();
        public QuizSessionService(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        private static string BuildKey(string quizId, string nickname) =>
            $"{quizId}:{nickname}";

        public async Task<QuizSession> StartQuizAsync(string quizId, string nickname)
        {
            var questions = await _questionService.GetAllQuestionsAsync();
            var quizSession = new QuizSession
            {
                QuizId = quizId,
                Nickname = nickname,
                TotalQuestions = questions.Count,
                CurrentQuestionIndex = 1,
                CurrentQuestion = questions[0]
            };
            var key = BuildKey(quizId, nickname); // "quiz1:Alice"
            _quizSessions[key] = quizSession;
            return quizSession;
        }

        public async Task<Question> GetNextQuestionAsync(string quizId, string nickname)
        {
            var key = BuildKey(quizId, nickname);

            if (_quizSessions.TryGetValue(key, out var session))
            {
                session.CurrentQuestionIndex++;
                if (session.CurrentQuestionIndex <= session.TotalQuestions)
                {
                    session.CurrentQuestion = await _questionService.GetQuestionByIdAsync(session.CurrentQuestionIndex);
                    return session.CurrentQuestion;
                }
            }
            return null;
        }

        public async Task<bool> SubmitAnswerAsync(string quizId, string answer, string nickname)
        {
            var key = BuildKey(quizId, nickname);

            if (_quizSessions.TryGetValue(key, out var session))
            {
                var userAnsw = session.CurrentQuestion.Answers.FirstOrDefault(a => a.Text == answer);

                if (userAnsw == null)
                {
                    return false;
                }

                if (userAnsw.IsCorrect)
                {
                    session.CorrectAnswers++;
                    return true;
                }
            }

            Console.WriteLine($"Quiz SESSION with ID {key} not found");

            return false;
        }

        public Task<QuizSession?> GetQuizSessionAsync(string quizId, string nickname)
        {
            var key = BuildKey(quizId, nickname);

            _quizSessions.TryGetValue(key, out var session);

            if(session == null)
            {
                return Task.FromResult<QuizSession?>(null);
            }

            Console.WriteLine($"Quiz SESSION with ID {key} retrieved, session nick: {session.Nickname}");

            return Task.FromResult<QuizSession?>(session);
        }

        public Task EndQuizAsync(string quizId, string nickname)
        {
            var key = BuildKey(quizId, nickname);
            _quizSessions.Remove(key);
            return Task.CompletedTask;
        }
    }
}
