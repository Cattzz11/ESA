using PROJETOESA.Controllers;
using PROJETOESA.Models.ViewModels;
using PROJETOESA.Models;


namespace PROJETOESA.Services.FlightService
{
    public interface IFlightService
    {
        Task<List<FlightsItinerary>> GetLiveFlightData();

        Task<List<FlightsItinerary>> LoadFlightsAsync();

        Task<(string latitude, string longitude)> GetCoordenatesAsync(string locationName);

        Task<string> GetAirlinesDataAsync();

        Task<List<Trip>> GetFlightsAsync(AddressComponents origin, AddressComponents destination);

        Task<List<TripViewModel>> GetFlightsPremiumAsync(AddressComponents origin, AddressComponents destination);

        Task<List<City>> PopulateCoordinatesAsync();
    }
}
