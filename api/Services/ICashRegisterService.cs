using Restaurant.Api.Dtos;

namespace Restaurant.Api.Services;

public interface ICashRegisterService
{
    Task<CashRegisterDto?> GetCurrentAsync();
    Task<IReadOnlyList<CashRegisterDto>> GetClosedAsync();
    Task<CashRegisterDto> OpenAsync(OpenCashRegisterRequest request);
    Task<CashRegisterDto?> CloseAsync();
}
