﻿using RealTimeQuizHub.Models;

namespace RealTimeQuizHub.Services.Interfaces
{
    public interface IQuestionService
    {
        Task<List<Question>> GetAllQuestionsAsync();
        Task<Question> GetQuestionByIdAsync(int questionId);
        Task<bool> AddQuestionAsync(Question question);
        Task<bool> UpdateQuestionAsync(int id, string text);
        Task<bool> DeleteQuestionAsync(int questionId);
    }
}
