using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Jobs;
using GarageControl.Core.ViewModels.Orders;

namespace GarageControl.Core.Contracts
{
    public interface IPDFGeneratorService
    {
        Task<byte[]> GenerateInvoicePdfAsync(OrderInvoiceVM order);
    }
}