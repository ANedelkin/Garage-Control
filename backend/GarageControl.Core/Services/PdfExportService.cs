using GarageControl.Core.Contracts;
using GarageControl.Core.ViewModels.Clients;
using GarageControl.Core.ViewModels.Jobs;
using GarageControl.Core.ViewModels.Orders;
using GarageControl.Core.ViewModels.Parts;
using GarageControl.Core.ViewModels.Workers;
using GarageControl.Core.ViewModels.Vehicles;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Tables;
using MigraDocCore.Rendering;
using System.IO;

namespace GarageControl.Core.Services
{
    public class PdfExportService : IPdfExportService
    {
        public Task<byte[]> ExportOrdersAsync(List<(OrderListVM Order, List<JobListVM> Jobs)> ordersWithJobs)
        {
            var doc = CreateDocument("Orders Report");
            var section = doc.LastSection;

            var table = section.AddTable();
            table.Borders.Width = 0.5;
            
            table.AddColumn("2cm"); // ID
            table.AddColumn("4cm"); // Client
            table.AddColumn("4cm"); // Car
            table.AddColumn("3cm"); // Plate
            table.AddColumn("3cm"); // Date
            table.AddColumn("3cm"); // Status

            var header = table.AddRow();
            header.HeadingFormat = true;
            header.Format.Font.Bold = true;
            header.Cells[0].AddParagraph("ID");
            header.Cells[1].AddParagraph("Client");
            header.Cells[2].AddParagraph("Car");
            header.Cells[3].AddParagraph("Plate");
            header.Cells[4].AddParagraph("Date");
            header.Cells[5].AddParagraph("Status");

            foreach (var (order, jobs) in ordersWithJobs)
            {
                var row = table.AddRow();
                row.Cells[0].AddParagraph(order.Id.Substring(0, 8));
                row.Cells[1].AddParagraph(order.ClientName);
                row.Cells[2].AddParagraph(order.CarName);
                row.Cells[3].AddParagraph(order.CarRegistrationNumber);
                row.Cells[4].AddParagraph(order.Date.ToString("dd.MM.yyyy"));
                row.Cells[5].AddParagraph(order.IsDone ? "Done" : "Active");
            }

            return Task.FromResult(Render(doc));
        }

        public Task<byte[]> ExportClientsAsync(IEnumerable<ClientVM> clients)
        {
            var doc = CreateDocument("Clients Report");
            var section = doc.LastSection;

            var table = section.AddTable();
            table.Borders.Width = 0.5;

            table.AddColumn("4cm"); // Name
            table.AddColumn("3cm"); // Phone
            table.AddColumn("5cm"); // Email
            table.AddColumn("7cm"); // Address

            var header = table.AddRow();
            header.HeadingFormat = true;
            header.Format.Font.Bold = true;
            header.Cells[0].AddParagraph("Name");
            header.Cells[1].AddParagraph("Phone");
            header.Cells[2].AddParagraph("Email");
            header.Cells[3].AddParagraph("Address");

            foreach (var client in clients)
            {
                var row = table.AddRow();
                row.Cells[0].AddParagraph(client.Name);
                row.Cells[1].AddParagraph(client.PhoneNumber ?? "-");
                row.Cells[2].AddParagraph(client.Email ?? "-");
                row.Cells[3].AddParagraph(client.Address ?? "-");
            }

            return Task.FromResult(Render(doc));
        }

        public Task<byte[]> ExportWorkersAsync(IEnumerable<WorkerVM> workers, List<string> exportTypes)
        {
            var doc = CreateDocument("Workers Report");
            var section = doc.LastSection;

            if (exportTypes.Contains("details"))
            {
                section.AddParagraph("Workers Details").Format.Font.Size = 14;
                var table = section.AddTable();
                table.Borders.Width = 0.5;
                table.AddColumn("4cm");
                table.AddColumn("4cm");
                table.AddColumn("6cm");
                table.AddColumn("4cm");

                var header = table.AddRow();
                header.Format.Font.Bold = true;
                header.Cells[0].AddParagraph("Name");
                header.Cells[1].AddParagraph("Username");
                header.Cells[2].AddParagraph("Email");
                header.Cells[3].AddParagraph("Hired On");

                foreach (var w in workers)
                {
                    var row = table.AddRow();
                    row.Cells[0].AddParagraph(w.Name);
                    row.Cells[1].AddParagraph(w.Username);
                    row.Cells[2].AddParagraph(w.Email ?? "-");
                    row.Cells[3].AddParagraph(w.HiredOn.ToString("dd.MM.yyyy"));
                }
                section.AddParagraph().Format.SpaceAfter = "1cm";
            }

            return Task.FromResult(Render(doc));
        }

        public Task<byte[]> ExportToDoAsync(IEnumerable<JobToDoVM> jobs, string workerName)
        {
            var doc = CreateDocument($"To-Do List: {workerName}");
            var section = doc.LastSection;

            var table = section.AddTable();
            table.Borders.Width = 0.5;

            table.AddColumn("3cm"); // Client
            table.AddColumn("3cm"); // Car
            table.AddColumn("3cm"); // Type
            table.AddColumn("2.5cm"); // Status
            table.AddColumn("3cm"); // Start
            table.AddColumn("4.5cm"); // Description

            var header = table.AddRow();
            header.Format.Font.Bold = true;
            header.Cells[0].AddParagraph("Client");
            header.Cells[1].AddParagraph("Car");
            header.Cells[2].AddParagraph("Type");
            header.Cells[3].AddParagraph("Status");
            header.Cells[4].AddParagraph("Start");
            header.Cells[5].AddParagraph("Description");

            foreach (var j in jobs)
            {
                var row = table.AddRow();
                row.Cells[0].AddParagraph(j.ClientName);
                row.Cells[1].AddParagraph(j.CarName);
                row.Cells[2].AddParagraph(j.TypeName);
                row.Cells[3].AddParagraph(j.Status);
                row.Cells[4].AddParagraph(j.StartTime.ToString("dd.MM HH:mm"));
                row.Cells[5].AddParagraph(j.Description);
            }

            return Task.FromResult(Render(doc));
        }

        public Task<byte[]> ExportPartsAsync(List<PartVM> parts)
        {
            var doc = CreateDocument("Parts Stock Report");
            var section = doc.LastSection;

            var table = section.AddTable();
            table.Borders.Width = 0.5;

            table.AddColumn("8cm"); // Name
            table.AddColumn("3cm"); // Quantity
            table.AddColumn("3cm"); // Price
            table.AddColumn("5cm"); // Category

            var header = table.AddRow();
            header.Format.Font.Bold = true;
            header.Cells[0].AddParagraph("Name");
            header.Cells[1].AddParagraph("Quantity");
            header.Cells[2].AddParagraph("Price");
            header.Cells[3].AddParagraph("Category");

            foreach (var p in parts)
            {
                var row = table.AddRow();
                row.Cells[0].AddParagraph(p.Name);
                row.Cells[1].AddParagraph(p.Quantity.ToString());
                row.Cells[2].AddParagraph(p.Price.ToString("C"));
                row.Cells[3].AddParagraph(p.PartNumber ?? "-");
            }

            return Task.FromResult(Render(doc));
        }

        public Task<byte[]> ExportJobTypesAsync(IEnumerable<JobTypeVM> jobTypes)
        {
            var doc = CreateDocument("Job Types Report");
            var section = doc.LastSection;

            var table = section.AddTable();
            table.Borders.Width = 0.5;

            table.AddColumn("6cm"); // Name
            table.AddColumn("13cm"); // Description

            var header = table.AddRow();
            header.Format.Font.Bold = true;
            header.Cells[0].AddParagraph("Name");
            header.Cells[1].AddParagraph("Description");

            foreach (var jt in jobTypes)
            {
                var row = table.AddRow();
                row.Cells[0].AddParagraph(jt.Name);
                row.Cells[1].AddParagraph(jt.Description ?? "-");
            }

            return Task.FromResult(Render(doc));
        }

        public Task<byte[]> ExportCarsAsync(IEnumerable<VehicleVM> cars)
        {
            var doc = CreateDocument("Cars Report");
            var section = doc.LastSection;

            var table = section.AddTable();
            table.Borders.Width = 0.5;

            table.AddColumn("4cm"); // Make
            table.AddColumn("4cm"); // Model
            table.AddColumn("3cm"); // Plate
            table.AddColumn("5cm"); // VIN
            table.AddColumn("3cm"); // Owner

            var header = table.AddRow();
            header.Format.Font.Bold = true;
            header.Cells[0].AddParagraph("Make");
            header.Cells[1].AddParagraph("Model");
            header.Cells[2].AddParagraph("Plate");
            header.Cells[3].AddParagraph("VIN");
            header.Cells[4].AddParagraph("Owner");

            foreach (var c in cars)
            {
                var row = table.AddRow();
                row.Cells[0].AddParagraph(c.MakeName ?? "-");
                row.Cells[1].AddParagraph(c.ModelName ?? "-");
                row.Cells[2].AddParagraph(c.RegistrationNumber);
                row.Cells[3].AddParagraph(c.VIN ?? "-");
                row.Cells[4].AddParagraph(c.OwnerName ?? "-");
            }

            return Task.FromResult(Render(doc));
        }

        private Document CreateDocument(string title)
        {
            var doc = new Document();
            var section = doc.AddSection();
            section.PageSetup.PageFormat = PageFormat.A4;
            section.PageSetup.Orientation = Orientation.Portrait;

            var header = section.AddParagraph(title);
            header.Format.Font.Size = 18;
            header.Format.Font.Bold = true;
            header.Format.SpaceAfter = "0.5cm";
            header.Format.Alignment = ParagraphAlignment.Center;

            return doc;
        }

        private byte[] Render(Document doc)
        {
            var renderer = new PdfDocumentRenderer(true) { Document = doc };
            renderer.RenderDocument();
            using var ms = new MemoryStream();
            renderer.PdfDocument.Save(ms);
            return ms.ToArray();
        }
    }
}
