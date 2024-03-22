using PROJETOESA.Models.ViewModels;
using PROJETOESA.Models;
using System.Net.Http;

namespace PROJETOESA.Services.DataService

{
    public interface IDataService
    {
        Task<List<TripViewModel>> GetFlightsByUserAsync(string userId);

        Task<List<CityViewModel>> GetAllCitiesAsync();

        Task<List<Country>> PopulateBDCountries();
        Task<List<City>> PopulateBDCity();
    }
}
