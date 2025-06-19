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

        public async Task<QuizSession> StartQuizAsync(string quizId)
        {
            var questions = await _questionService.GetAllQuestionsAsync();
            var quizSession = new QuizSession
            {
                QuizId = quizId,
                TotalQuestions = questions.Count,
                CurrentQuestionIndex = 1,
                CurrentQuestion = questions[0]
            };
            _quizSessions[quizId] = quizSession;
            return quizSession;
        }

        public async Task<Question> GetNextQuestionAsync(string quizId)
        {
            if (_quizSessions.TryGetValue(quizId, out var session))
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

        public async Task<bool> SubmitAnswerAsync(string quizId, string answer)
        {
            if (_quizSessions.TryGetValue(quizId, out var session))
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

            Console.WriteLine($"Quiz SESSION with ID {quizId} not found");

            return false;
        }

        public Task<QuizSession> GetQuizSessionAsync(string quizId)
        {
            _quizSessions.TryGetValue(quizId, out var session);

            Console.WriteLine($"Quiz SESSION with ID {quizId} retrieved: \n{session}");

            return Task.FromResult(session);
        }

        public Task EndQuizAsync(string quizId)
        {
            _quizSessions.Remove(quizId);
            return Task.CompletedTask;
        }
    }
}
