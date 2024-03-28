using Newtonsoft.Json.Linq;
using PROJETOESA.Models.ViewModels;
using PROJETOESA.Models;
using System.Net.Http;

namespace PROJETOESA.Services.SkyscannerService
{
    public interface ISkyscannerService
    {
        Task<List<TripViewModel>> GetRoundtripAsync(FlightData data);

        Task<List<TripViewModel>> GetRoundtripPremiumAsync(FlightData data);

        Task<TripViewModel> GetTripDetailsAsync(string token, string itineraryId, string sessionId);

        Task<List<Country>> GetEverywhereAsync(FlightData data);

        Task<string> GetOneWayAsync(FlightData data);

        Task<List<Calendar>> GetCalendarAsync(FlightData data);

        Task<List<CustomGetDataModel>> GetDataAsync(string data);

        Task<List<City>> GetAirportListAsync(Country country);

        Task<List<TripViewModel>> GetSugestionsCompanyAsyncTest();

        Task<List<TripViewModel>> GetSugestionsCompanyAsync(string carrierId);

        Task<List<City>> GetFavouriteDestinationsAsync();

        Task<List<TripViewModel>> GetSugestionsDestinationsAsync();

        Task<List<Carrier>> GetFavoriteAirlineAsync();
    }
}
