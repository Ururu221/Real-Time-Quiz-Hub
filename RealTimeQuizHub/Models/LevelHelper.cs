namespace RealTimeQuizHub.Models
{
    // Maps a user's total score onto a named level (Ukrainian labels).
    //   0–499      → Новачок
    //   500–1499   → Знавець
    //   1500–3999  → Досвідчений
    //   4000–7999  → Експерт
    //   8000+      → Майстер вікторин
    public static class LevelHelper
    {
        public static string GetLevel(int totalScore)
        {
            if (totalScore >= 8000) return "Майстер вікторин";
            if (totalScore >= 4000) return "Експерт";
            if (totalScore >= 1500) return "Досвідчений";
            if (totalScore >= 500) return "Знавець";
            return "Новачок";
        }
    }
}
