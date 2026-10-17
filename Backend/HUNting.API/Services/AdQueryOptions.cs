namespace HUNting.API.Services
{
    public class AdQueryOptions
    {
        public int Page { get; set; } = 1;
        public int Take { get; set; } = 12;
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public string? StatusCode { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? OwnerUserId { get; set; }
    }
}
