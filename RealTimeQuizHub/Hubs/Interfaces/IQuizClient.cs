namespace RealTimeQuizHub.Hubs.Interfaces
{
    public interface IQuizClient
    {
        // Обновить весь лидерборд
        Task BroadcastLeaderboard(List<LeaderboardItem> items);

        // (по желанию) сообщить, что пользователь начал викторину
        Task UserJoined(string nickname);

        // (по желанию) уведомить, что викторина закончена
        Task QuizFinished();
    }
}
