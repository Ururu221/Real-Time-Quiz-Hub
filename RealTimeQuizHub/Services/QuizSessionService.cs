using RealTimeQuizHub.Models;
using RealTimeQuizHub.Services.Interfaces;

namespace RealTimeQuizHub.Services
{
    public class QuizSessionService : IQuizSessionService
    {
        private readonly IJsonQuestionService _questionService;
        private readonly Dictionary<string, QuizSession> _quizSessions = new();
        public QuizSessionService(IJsonQuestionService questionService)
        {
            _questionService = questionService;
        }

        public async Task<QuizSession> StartQuizAsync(string quizId)
        {
            var questions = await _questionService.GetAllAsync();
            var quizSession = new QuizSession
            {
                QuizId = quizId,
                TotalQuestions = questions.Count,
                CurrentQuestionIndex = 0,
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
                if (session.CurrentQuestionIndex < session.TotalQuestions)
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
            return false;
        }

        public Task<QuizSession> GetQuizSessionAsync(string quizId)
        {
            _quizSessions.TryGetValue(quizId, out var session);
            return Task.FromResult(session);
        }

        public Task EndQuizAsync(string quizId)
        {
            _quizSessions.Remove(quizId);
            return Task.CompletedTask;
        }
    }
}
