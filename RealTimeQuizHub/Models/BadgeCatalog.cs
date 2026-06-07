namespace RealTimeQuizHub.Models
{
    // Single source of truth for the built-in badges. The names are used both as
    // the seed data (HasData) and as keys when BadgeService decides what to award,
    // so they live here as constants to keep the two in sync.
    public static class BadgeCatalog
    {
        public const string FirstWin = "Перша перемога";
        public const string Unbeatable = "Непереможний";
        public const string Lightning = "Блискавка";
        public const string Perfectionist = "Перфекціоніст";
        public const string Enthusiast = "Ентузіаст";

        // Win counts / quiz counts that unlock the cumulative badges.
        public const int UnbeatableWins = 3;
        public const int EnthusiastQuizzes = 5;

        public static readonly Badge[] Seed =
        {
            new Badge { Id = 1, Name = FirstWin,      Description = "Виграйте першу вікторину",                 IconEmoji = "🏆" },
            new Badge { Id = 2, Name = Unbeatable,    Description = "Виграйте 3 вікторини загалом",            IconEmoji = "🔥" },
            new Badge { Id = 3, Name = Lightning,     Description = "Виграйте вікторину в кімнаті з таймером", IconEmoji = "⚡" },
            new Badge { Id = 4, Name = Perfectionist, Description = "Завершіть вікторину зі 100% правильних",  IconEmoji = "🎯" },
            new Badge { Id = 5, Name = Enthusiast,    Description = "Завершіть 5 вікторин загалом",            IconEmoji = "📚" },
        };
    }
}
