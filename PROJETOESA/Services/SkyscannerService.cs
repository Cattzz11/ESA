using PROJETOESA.Controllers;
using PROJETOESA.Models;

namespace PROJETOESA.Services
{
    public class SkyscannerService
    {
        private readonly HttpClient _httpClient;

        public SkyscannerService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("SkyscannerAPI");
        }

        public async Task<string> GetFlightsAsync(FlightData data)
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(data.fromEntityId)) queryParams.Add($"fromEntityId={data.fromEntityId}");
            if (!string.IsNullOrEmpty(data.toEntityId)) queryParams.Add($"toEntityId={data.toEntityId}");
            if (!string.IsNullOrEmpty(data.departDate)) queryParams.Add($"departDate={data.departDate}");
            if (!string.IsNullOrEmpty(data.returnDate)) queryParams.Add($"returnDate={data.returnDate}");
            if (!string.IsNullOrEmpty(data.market)) queryParams.Add($"market={data.market}");
            if (!string.IsNullOrEmpty(data.locale)) queryParams.Add($"locale={data.locale}");
            if (!string.IsNullOrEmpty(data.currency)) queryParams.Add($"currency={data.currency}");
            if (data.Adults.HasValue) queryParams.Add($"adults={data.Adults}");
            if (data.Children.HasValue) queryParams.Add($"children={data.Children}");
            if (data.Infants.HasValue) queryParams.Add($"infants={data.Infants}");
            if (!string.IsNullOrEmpty(data.cabinClass)) queryParams.Add($"cabinClass={data.cabinClass}");

            var queryString = string.Join("&", queryParams);
            var response = await _httpClient.GetAsync($"flights/search-roundtrip?{queryString}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

    }
}