using RealTimeQuizHub.Models;
using RealTimeQuizHub.Models.Dtos;
using RealTimeQuizHub.Repository.Interfaces;
using RealTimeQuizHub.Services.Interfaces;

namespace RealTimeQuizHub.Services
{
    public class QuizService : IQuizService
    {
        private readonly IQuizRepository _quizRepository;
        private readonly IQuestionRepository _questionRepository;

        public QuizService(IQuizRepository quizRepository, IQuestionRepository questionRepository)
        {
            _quizRepository = quizRepository;
            _questionRepository = questionRepository;
        }

        public async Task<List<QuizListDto>> GetAllAsync()
        {
            var quizzes = await _quizRepository.GetAllAsync();
            return quizzes.Select(ToListDto).ToList();
        }

        public async Task<QuizListDto?> GetByIdAsync(int id)
        {
            var quiz = await _quizRepository.GetByIdAsync(id);
            return quiz == null ? null : ToListDto(quiz);
        }

        public async Task<List<Question>> GetQuizQuestionsAsync(int quizId)
        {
            var quiz = await _quizRepository.GetByIdAsync(quizId);
            if (quiz == null)
            {
                return new List<Question>();
            }

            // Only the questions linked to THIS quiz, in their stored order.
            return quiz.QuizQuestions
                .OrderBy(qq => qq.OrderIndex)
                .Select(qq => qq.Question)
                .Where(q => q != null)
                .Select(q => q!)
                .ToList();
        }

        public async Task<Quiz> CreateQuizAsync(CreateQuizDto dto, int createdByUserId)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                throw new ArgumentException("Введіть назву вікторини.", nameof(dto));
            }
            if (dto.TimerScoreImpact < 0.0 || dto.TimerScoreImpact > 1.0)
            {
                throw new ArgumentException("Вплив таймера має бути в межах від 0.0 до 1.0.", nameof(dto));
            }

            var quiz = new Quiz
            {
                Title = dto.Title.Trim(),
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
                HasTimer = dto.HasTimer,
                TimerSecondsPerQuestion = dto.TimerSecondsPerQuestion,
                TimerScoreImpact = dto.HasTimer ? dto.TimerScoreImpact : 0.0,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow,
                QuizQuestions = new List<QuizQuestion>()
            };

            if (dto.Questions != null && dto.Questions.Count > 0)
            {
                BuildNewQuestions(dto.Questions, quiz);
            }
            else if (dto.QuestionIds != null && dto.QuestionIds.Count > 0)
            {
                await LinkExistingQuestionsAsync(dto.QuestionIds, quiz);
            }
            else
            {
                throw new ArgumentException("Додайте хоча б одне питання.", nameof(dto));
            }

            // A single SaveChanges over the whole graph (quiz → questions → answers
            // → quiz_questions) runs inside one transaction: if any insert fails,
            // EF Core rolls the entire operation back.
            return await _quizRepository.AddAsync(quiz);
        }

        // Creates brand-new Question + Answer rows and links them to the quiz.
        private static void BuildNewQuestions(List<NewQuestionDto> questions, Quiz quiz)
        {
            for (int i = 0; i < questions.Count; i++)
            {
                var q = questions[i];
                var humanIndex = i + 1;

                if (string.IsNullOrWhiteSpace(q.Text))
                {
                    throw new ArgumentException($"Питання {humanIndex}: введіть текст питання.");
                }

                var answers = q.Answers?
                    .Where(a => a != null && !string.IsNullOrWhiteSpace(a.Text))
                    .ToList() ?? new List<NewAnswerDto>();

                if (answers.Count < 2)
                {
                    throw new ArgumentException($"Питання {humanIndex}: додайте хоча б 2 варіанти відповіді.");
                }
                if (!answers.Any(a => a.IsCorrect))
                {
                    throw new ArgumentException($"Питання {humanIndex}: позначте правильну відповідь.");
                }

                var question = new Question
                {
                    Name = q.Text.Trim(),
                    Answers = answers.Select(a => new Answer
                    {
                        Text = a.Text.Trim(),
                        IsCorrect = a.IsCorrect
                    }).ToList()
                };

                quiz.QuizQuestions.Add(new QuizQuestion
                {
                    Question = question,
                    OrderIndex = i
                });
            }
        }

        // Legacy path: link already-existing questions by id (kept for room creation).
        private async Task LinkExistingQuestionsAsync(List<int> questionIds, Quiz quiz)
        {
            var orderedIds = questionIds.Distinct().ToList();
            for (int i = 0; i < orderedIds.Count; i++)
            {
                var questionId = orderedIds[i];
                var question = await _questionRepository.GetQuestionByIdAsync(questionId);
                if (question == null)
                {
                    throw new ArgumentException($"Question with ID {questionId} does not exist.", nameof(questionIds));
                }

                quiz.QuizQuestions.Add(new QuizQuestion
                {
                    QuestionId = questionId,
                    OrderIndex = i
                });
            }
        }

        public Task<bool> DeleteQuizAsync(int id)
        {
            return _quizRepository.DeleteAsync(id);
        }

        private static QuizListDto ToListDto(Quiz q) => new()
        {
            Id = q.Id,
            Title = q.Title,
            Description = q.Description,
            QuestionCount = q.QuizQuestions.Count,
            HasTimer = q.HasTimer,
            TimerSecondsPerQuestion = q.TimerSecondsPerQuestion,
            TimerScoreImpact = q.TimerScoreImpact,
            CreatedByUserId = q.CreatedByUserId,
            CreatedAt = q.CreatedAt
        };
    }
}
