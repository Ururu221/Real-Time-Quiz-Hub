using Microsoft.AspNetCore.SignalR;
using RealTimeQuizHub.Hubs;
using RealTimeQuizHub.Hubs.Interfaces;
using RealTimeQuizHub.Services.Interfaces;

namespace RealTimeQuizHub.Hubs
{
    public class QuizHub : Hub<IQuizClient>
    {
        private static readonly Dictionary<string, LeaderboardItem> _players
            = new();

        private readonly IQuizSessionService _sessionService;
        public QuizHub(IQuizSessionService sessionService)
        {
            _sessionService = sessionService;
        }

        public async Task RegisterUser(string quizId, string nickname)
        {

            var session = await _sessionService.GetQuizSessionAsync(quizId, nickname)
                       ?? await _sessionService.StartQuizAsync(quizId, nickname);

            if (!_players.ContainsKey(nickname))
            {
                _players[nickname] = new LeaderboardItem
                {
                    Nickname = nickname,
                    CorrectAnswers = session.CorrectAnswers,
                    TotalQuestions = session.TotalQuestions,
                    CurrentQuestionIndex = session.CurrentQuestionIndex
                };
            }
            await Clients.Caller.UserJoined(nickname);
            await Clients.All.BroadcastLeaderboard(_players.Values.ToList());
        }

        public async Task UpdateProgress(string quizId, string nickname, int currentIndex, int correct)
        {
            if (_players.ContainsKey(nickname))
            {
                var item = _players[nickname];
                item.CurrentQuestionIndex = currentIndex;
                item.CorrectAnswers = correct;
            }
            await Clients.All.BroadcastLeaderboard(_players.Values.ToList());
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }
    }
}
