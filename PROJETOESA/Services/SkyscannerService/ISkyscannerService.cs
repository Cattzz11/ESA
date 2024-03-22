using Newtonsoft.Json.Linq;
using PROJETOESA.Models.ViewModels;
using PROJETOESA.Models;
using System.Net.Http;

namespace PROJETOESA.Services.SkyscannerService
{
    public interface ISkyscannerService
    {
        Task<List<Trip>> GetRoundtripAsync(FlightData data);

        Task<List<TripDetailsViewModel>> GetRoundtripPremiumAsync(FlightData data);

        Task<TripDetailsViewModel> GetTripDetailsAsync(string token, string itineraryId);

        Task<List<Country>> GetEverywhereAsync(FlightData data);

        Task<string> GetOneWayAsync(FlightData data);

        Task<List<Calendar>> GetCalendarAsync(FlightData data);

        Task<List<CustomGetDataModel>> GetDataAsync(Country data);

        Task<List<City>> GetAirportListAsync(Country country);

        Task<List<Trip>> GetSugestionsCompanyAsyncTest();

        Task<List<Trip>> GetSugestionsCompanyAsync(string carrierId);

        Task<List<City>> GetFavouriteDestinationsAsync();

        Task<List<Trip>> GetSugestionsDestinationsAsync();

        Task<List<Carrier>> GetFavoriteAirlineAsync();
    }
}
