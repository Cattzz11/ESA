using PROJETOESA.Models;

namespace PROJETOESA.Services.AeroDataBoxService
{
    public interface IAeroDataBoxService
    {
        Task<AircraftData> GetFlightStatusAsync(string flightIATA);
        Task<AircraftData> GetFlightStatusTestAsync(string flightIATA);
    }
}
