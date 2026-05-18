using Restaurant.Api.Models;

namespace Restaurant.Api.Services;

public interface IInvoiceService
{
    byte[] BuildInvoicePdf(Order order);
}
