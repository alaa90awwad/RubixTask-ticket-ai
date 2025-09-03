namespace TicketAI.Backend.Models
{
    public class SuggestResponse
    {
        public string Category { get; set; } = "General Feedback";
        public string Model { get; set; } = "gpt-4o-mini";
        public bool Coerced { get; set; } = false; 
    }
}
