using GarageControl.Core.Contracts;
using GarageControl.Core.ViewModels.Orders;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.Rendering;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class PDFGeneratorService : IPDFGeneratorService
{
    private const string JobIndent = "0.5cm";
    private const string DetailIndent = "1cm";
    private const string PartIndent = "1.5cm";

    public Task<byte[]> GenerateInvoicePdfAsync(OrderInvoiceViewModel order)
    {
        var document = new Document();
        var section = document.AddSection();

        // Header
        var header = section.AddParagraph($"Invoice for Order #{order.OrderId}");
        header.Format.Font.Size = 16;
        header.Format.Font.Bold = true;
        header.Format.SpaceAfter = "1cm";

        // Client & car info
        section.AddParagraph($"Client: {order.ClientName}");
        section.AddParagraph($"Car: {order.CarName} ({order.CarRegistrationNumber})");
        section.AddParagraph($"Kilometers: {order.Kilometers}");
        section.AddParagraph().AddLineBreak();

        section.AddParagraph($"Workshop: {order.WorkshopName}");
        section.AddParagraph($"Address: {order.WorkshopAddress}");
        section.AddParagraph($"Phone: {order.WorkshopPhone}");
        section.AddParagraph($"Email: {order.WorkshopEmail}");
        section.AddParagraph($"Registration Number: {order.WorkshopRegistrationNumber}");
        section.AddParagraph().AddLineBreak();

        // Jobs title
        section.AddParagraph("Jobs:")
               .Format.Font.Bold = true;

        foreach (var job in order.Jobs)
        {
            // Job title
            var jobTitle = section.AddParagraph(job.JobTypeName);
            jobTitle.Format.Font.Bold = true;
            jobTitle.Format.LeftIndent = JobIndent;
            jobTitle.Format.SpaceBefore = "0.3cm";

            // Job details
            AddIndentedParagraph(section, $"Description: {job.Description}", DetailIndent);
            AddIndentedParagraph(section, $"Mechanic: {job.MechanicName}", DetailIndent);
            AddIndentedParagraph(section, $"Labor cost: {job.LaborCost:C}", DetailIndent);

            var partsTotal = job.Parts.Sum(p => p.Price * (decimal)p.UsedQuantity);
            AddIndentedParagraph(section, $"Parts total: {partsTotal:C}", DetailIndent);

            // Parts
            foreach (var part in job.Parts)
            {
                AddIndentedParagraph(
                    section,
                    $"{part.PartName} x{part.UsedQuantity} @ {part.Price:C} = {(part.Price * (decimal)part.UsedQuantity):C}",
                    PartIndent
                );
            }
        }

        // Total
        var total = order.Jobs.Sum(j =>
            j.LaborCost + j.Parts.Sum(p => p.Price * (decimal)p.UsedQuantity)
        );

        var totalParagraph = section.AddParagraph($"Total: {total:C}");
        totalParagraph.Format.Font.Bold = true;
        totalParagraph.Format.SpaceBefore = "0.8cm";

        // Footer
        var footer = section.AddParagraph("Thank you for choosing us!");
        footer.Format.Alignment = ParagraphAlignment.Center;
        footer.Format.SpaceBefore = "1cm";

        var renderer = new PdfDocumentRenderer(unicode: true)
        {
            Document = document
        };

        renderer.RenderDocument();

        using var memoryStream = new MemoryStream();
        renderer.PdfDocument.Save(memoryStream, false);

        return Task.FromResult(memoryStream.ToArray());
    }

    private static void AddIndentedParagraph(
        Section section,
        string text,
        string leftIndent)
    {
        var paragraph = section.AddParagraph(text);
        paragraph.Format.LeftIndent = leftIndent;
    }
}
