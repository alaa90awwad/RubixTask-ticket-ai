using TicketAI.Backend.Models;

namespace TicketAI.Backend.Services
{
    public interface ICategorySuggester
    {
        Task<SuggestResponse> SuggestAsync(SuggestRequest request, CancellationToken ct = default);
    }
}
