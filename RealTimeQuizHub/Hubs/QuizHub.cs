using Microsoft.AspNetCore.SignalR;
using RealTimeQuizHub.Hubs;
using RealTimeQuizHub.Hubs.Interfaces;
using RealTimeQuizHub.Services.Interfaces;

namespace RealTimeQuizHub.Hubs
{
    public class QuizHub : Hub<IQuizClient>
    {
        // Храним прогресс всех пользователей в памяти
        private static readonly Dictionary<string, LeaderboardItem> _players
            = new();

        private readonly IQuizSessionService _sessionService;
        public QuizHub(IQuizSessionService sessionService)
        {
            _sessionService = sessionService;
        }

        // Клиент вызывает сразу после старта (или при вводе ника)
        public async Task RegisterUser(string quizId, string nickname)
        {
            // первый вход: добавляем в словарь
            if (!_players.ContainsKey(nickname))
            {
                var session = await _sessionService.GetQuizSessionAsync(quizId);
                _players[nickname] = new LeaderboardItem
                {
                    Nickname = nickname,
                    CorrectAnswers = session.CorrectAnswers,
                    TotalQuestions = session.TotalQuestions,
                    CurrentQuestionIndex = session.CurrentQuestionIndex
                };
            }
            await Clients.Caller.UserJoined(nickname);
            // обновляем всем
            await Clients.All.BroadcastLeaderboard(_players.Values.ToList());
        }

        // Клиент вызывает при каждом ответе, передавая обновлённые данные
        public async Task UpdateProgress(string quizId, string nickname, int currentIndex, int correct)
        {
            if (_players.ContainsKey(nickname))
            {
                var item = _players[nickname];
                item.CurrentQuestionIndex = currentIndex;
                item.CorrectAnswers = correct;
                // totalQuestions можно брать из item или из сервиса
            }
            // рассылаем всем актуальный лидерборд
            await Clients.All.BroadcastLeaderboard(_players.Values.ToList());
        }

        // По желанию: сбросить по завершении
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            // если хотите удалять пользователя при закрытии страницы:
            // _players.Remove(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
