namespace RealTimeQuizHub.Hubs
{
    public class LeaderboardItem
    {
        public string Nickname { get; set; } = "";
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public int CurrentQuestionIndex { get; set; }
    }
}
