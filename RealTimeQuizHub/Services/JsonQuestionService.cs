using RealTimeQuizHub.Models;
using RealTimeQuizHub.Services.Interfaces;
using System.Text.Json;

namespace RealTimeQuizHub.Services
{
    public class JsonQuestionService : IQuestionService
    {
        private readonly string _filePath;

        public JsonQuestionService(string filePath)
        {
            _filePath = filePath;
        }

        public async Task<List<Question>> GetAllAsync()
        {
            var json = await File.ReadAllTextAsync(_filePath);
            var questions = JsonSerializer.Deserialize<List<Question>>(json);
            return questions ?? throw new Exception("Failed to load JSON file.");
        }

        public async Task<Question> GetQuestionByIdAsync(int questionId)
        {
            var questions = await GetAllAsync();
            var question = questions.FirstOrDefault(q => q.Id == questionId);
            return question ?? throw new Exception($"Question with ID {questionId} not found.");
        }
    }
}
