namespace RealTimeQuizHub.Models
{
    // A catalogue of achievements users can earn.
    public class Badge
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string IconEmoji { get; set; } = string.Empty;
    }
}
