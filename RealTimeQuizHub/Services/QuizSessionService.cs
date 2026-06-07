using RealTimeQuizHub.Models;
using RealTimeQuizHub.Services.Interfaces;

namespace RealTimeQuizHub.Services
{
    public class QuizSessionService : IQuizSessionService
    {
        private readonly IQuestionService _questionService;
        private readonly IQuizService _quizService;
        private readonly static Dictionary<string, QuizSession> _quizSessions = new();
        public QuizSessionService(IQuestionService questionService, IQuizService quizService)
        {
            _questionService = questionService;
            _quizService = quizService;
        }

        private static string BuildKey(string quizId, string nickname) =>
            $"{quizId}:{nickname}";

        public async Task<QuizSession> StartQuizAsync(string quizId, string nickname)
        {
            // Load ONLY the questions that belong to this quiz (via quiz_questions).
            // A numeric quizId is a real quiz; the legacy "default-session" string
            // falls back to every question for backward compatibility.
            List<Question> questions;
            if (int.TryParse(quizId, out var id))
            {
                questions = await _quizService.GetQuizQuestionsAsync(id);
                if (questions.Count == 0)
                {
                    throw new InvalidOperationException($"Quiz {id} has no questions.");
                }
            }
            else
            {
                questions = await _questionService.GetAllQuestionsAsync();
            }

            var quizSession = new QuizSession
            {
                QuizId = quizId,
                Nickname = nickname,
                Questions = questions,
                TotalQuestions = questions.Count,
                CurrentQuestionIndex = 1,
                CurrentQuestion = questions[0]
            };
            var key = BuildKey(quizId, nickname); // "quiz1:Alice"
            _quizSessions[key] = quizSession;
            return quizSession;
        }

        public Task<Question?> GetNextQuestionAsync(string quizId, string nickname)
        {
            var key = BuildKey(quizId, nickname);

            if (_quizSessions.TryGetValue(key, out var session))
            {
                session.CurrentQuestionIndex++;
                if (session.CurrentQuestionIndex <= session.TotalQuestions)
                {
                    // Index into THIS quiz's ordered question list (1-based).
                    session.CurrentQuestion = session.Questions[session.CurrentQuestionIndex - 1];
                    return Task.FromResult<Question?>(session.CurrentQuestion);
                }
            }
            return Task.FromResult<Question?>(null);
        }

        public async Task<bool> SubmitAnswerAsync(string quizId, string answer, string nickname)
        {
            await Task.CompletedTask;
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
