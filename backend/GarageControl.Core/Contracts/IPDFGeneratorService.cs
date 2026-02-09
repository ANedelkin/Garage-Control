using GarageControl.Core.ViewModels.Orders;

namespace GarageControl.Core.Contracts
{
    public interface IPDFGeneratorService
    {
        Task<byte[]> GenerateInvoicePdfAsync(OrderInvoiceViewModel order);
    }
}