using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RealTimeQuizHub.Hubs;
using RealTimeQuizHub.Hubs.Interfaces;
using RealTimeQuizHub.Services.Interfaces;

namespace RealTimeQuizHub.Hubs
{
    [Authorize]
    public class QuizHub : Hub<IQuizClient>
    {
        // Live in-game leaderboard, in memory only and isolated per quiz:
        // quizId -> (nickname -> that player's live entry). Because the outer key
        // is the quiz, a result from one quiz can never appear in another. The
        // whole structure is cleared on app restart; the persistent Global Rating
        // lives separately in PostgreSQL.
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, LeaderboardItem>> _playersByQuiz
            = new();

        private readonly IQuizSessionService _sessionService;
        public QuizHub(IQuizSessionService sessionService)
        {
            _sessionService = sessionService;
        }

        // The live board for a single quiz (created on first access).
        private static ConcurrentDictionary<string, LeaderboardItem> BoardFor(string quizId)
            => _playersByQuiz.GetOrAdd(quizId, _ => new ConcurrentDictionary<string, LeaderboardItem>());

        // SignalR group that scopes a broadcast to one quiz, so clients only ever
        // receive the leaderboard for the quiz they actually joined.
        private static string GroupFor(string quizId) => $"quiz:{quizId}";

        public async Task RegisterUser(string quizId, string nickname)
        {
            var session = await _sessionService.GetQuizSessionAsync(quizId, nickname)
                       ?? await _sessionService.StartQuizAsync(quizId, nickname);

            var board = BoardFor(quizId);
            // Refresh from the session each time so a reconnect to the same quiz
            // reflects real progress instead of resurrecting a stale snapshot.
            board.AddOrUpdate(nickname,
                _ => new LeaderboardItem
                {
                    Nickname = nickname,
                    CorrectAnswers = session.CorrectAnswers,
                    TotalQuestions = session.TotalQuestions,
                    CurrentQuestionIndex = session.CurrentQuestionIndex
                },
                (_, existing) =>
                {
                    existing.CorrectAnswers = session.CorrectAnswers;
                    existing.TotalQuestions = session.TotalQuestions;
                    existing.CurrentQuestionIndex = session.CurrentQuestionIndex;
                    return existing;
                });

            await Groups.AddToGroupAsync(Context.ConnectionId, GroupFor(quizId));
            await Clients.Caller.UserJoined(nickname);
            await Clients.Group(GroupFor(quizId)).BroadcastLeaderboard(board.Values.ToList());
        }

        public async Task UpdateProgress(string quizId, string nickname, int currentIndex, int correct)
        {
            var board = BoardFor(quizId);
            if (board.TryGetValue(nickname, out var item))
            {
                item.CurrentQuestionIndex = currentIndex;
                item.CorrectAnswers = correct;
            }
            await Clients.Group(GroupFor(quizId)).BroadcastLeaderboard(board.Values.ToList());
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }
    }
}
