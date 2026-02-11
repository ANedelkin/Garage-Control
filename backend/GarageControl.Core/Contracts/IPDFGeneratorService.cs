using GarageControl.Core.ViewModels;

namespace GarageControl.Core.Contracts
{
    public interface IPDFGeneratorService
    {
        Task<byte[]> GenerateInvoicePdfAsync(OrderInvoiceVM order);
    }
}