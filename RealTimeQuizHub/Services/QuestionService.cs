using RealTimeQuizHub.Models;
using RealTimeQuizHub.Repository.Interfaces;
using RealTimeQuizHub.Services.Interfaces;

namespace RealTimeQuizHub.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly IQuestionRepository _questionRepository;

        public QuestionService(IQuestionRepository questionRepository)
        {
            _questionRepository = questionRepository;
        }

        public async Task<bool> AddQuestionAsync(Question question)
        {
            if (question == null)
            {
                return false;
            }
            return await _questionRepository.AddQuestionAsync(question);
        }

        public async Task<bool> DeleteQuestionAsync(int questionId)
        {
            return await _questionRepository.DeleteQuestionAsync(questionId);
        }

        public async Task<List<Question>> GetAllQuestionsAsync()
        {
            var questions = await _questionRepository.GetAllAsync();
            if (questions == null || questions.Count == 0)
            {
                throw new Exception("No questions found.");
            }
            return questions;
        }

        public async Task<Question> GetQuestionByIdAsync(int questionId)
        {
            var question = await _questionRepository.GetQuestionByIdAsync(questionId);
            if (question == null)
            {
                throw new Exception($"Question with ID {questionId} not found.");
            }
            return question;
        }

        public async Task<bool> UpdateQuestionAsync(int id, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("Question text cannot be null or empty.", nameof(text));
            }
            var existingQuestion = await _questionRepository.GetQuestionByIdAsync(id);
            if (existingQuestion == null)
            {
                throw new Exception($"Question with ID {id} not found.");
            }
            return await _questionRepository.UpdateQuestionAsync(existingQuestion);
        }
    }
}
