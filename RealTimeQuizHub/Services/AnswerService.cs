using RealTimeQuizHub.Models;
using RealTimeQuizHub.Repository.Interfaces;
using RealTimeQuizHub.Services.Interfaces;

namespace RealTimeQuizHub.Services
{
    public class AnswerService : IAnswerService
    {
        private readonly IAnswerRepository _answerRepository;
        public AnswerService(IAnswerRepository answerRepository)
        {
            _answerRepository = answerRepository;
        }

        public async Task<bool> AddAnswerAsync(Answer answer)
        {
            if (answer == null)
            {
                return false;
            }
            return await _answerRepository.AddAnswerAsync(answer);
        }

        public async Task<bool> DeleteAnswerAsync(int answerId)
        {
            return await _answerRepository.DeleteAnswerAsync(answerId);
        }

        public async Task<List<Answer>> GetAllAnswersAsync()
        {
            var answers = await _answerRepository.GetAllAsync();
            if (answers == null || answers.Count == 0)
            {
                throw new KeyNotFoundException("No answers found.");
            }
            return answers;
        }

        public async Task<Answer> GetAnswerByIdAsync(int answerId)
        {
            var answer = await _answerRepository.GetAnswerByIdAsync(answerId);
            if (answer == null)
            {
                throw new KeyNotFoundException($"Answer with ID {answerId} not found.");
            }
            return answer;
        }

        public async Task<bool> UpdateAnswerAsync(Answer answer)
        {
            if (answer == null)
            {
                return false;
            }
            var existingAnswer = await _answerRepository.GetAnswerByIdAsync(answer.Id);
            if (existingAnswer == null)
            {
                throw new KeyNotFoundException($"Answer with ID {answer.Id} not found.");
            }
            return await _answerRepository.UpdateAnswerAsync(answer);
        }
        public async Task<List<Answer>> GetAnswersByQuestionIdAsync(int questionId)
        {
            var answers = await _answerRepository.GetAllAsync();
            if (answers == null || answers.Count == 0)
            {
                throw new KeyNotFoundException($"No answers found for question ID {questionId}.");
            }
            return answers.Where(a => a.QuestionId == questionId).ToList();
        }
    }
}
