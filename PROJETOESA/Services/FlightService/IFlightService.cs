using PROJETOESA.Controllers;
using PROJETOESA.Models.ViewModels;
using PROJETOESA.Models;


namespace PROJETOESA.Services.FlightService
{
    public interface IFlightService
    {
        Task<List<FlightsItinerary>> GetLiveFlightData();

        Task<List<FlightsItinerary>> LoadFlightsAsync();

        Task<(string latitude, string longitude)> GetCoordinatesAsync(string locationName);

        Task<string> GenerateMapUrl();

        Task<string> GetAirlinesDataAsync();

        Task<List<Trip>> GetFlightsAsync(AddressComponents origin, AddressComponents destination);

        Task<List<TripDetailsViewModel>> GetFlightsPremiumAsync(AddressComponents origin, AddressComponents destination);

        Task<List<City>> PopulateCoordinatesAsync();
    }
}
