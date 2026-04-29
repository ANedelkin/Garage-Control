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

            
            table.AddColumn("3cm"); // Client
            table.AddColumn("3cm"); // Car
            table.AddColumn("2.5cm"); // Reg #
            table.AddColumn("1.5cm"); // Km
            table.AddColumn("2.5cm"); // Job Type
            table.AddColumn("2.5cm"); // Mechanic
            table.AddColumn("2cm"); // Status
            table.AddColumn("2.5cm"); // Start Time
            table.AddColumn("1.8cm"); // Labor Cost
            table.AddColumn("1.8cm"); // Parts Cost
            table.AddColumn("1.8cm"); // Total Cost

            var header = table.AddRow();
            header.HeadingFormat = true;
            StyleHeader(header);
            header.Cells[0].AddParagraph("Client");
            header.Cells[1].AddParagraph("Car");
            header.Cells[2].AddParagraph("Reg #");
            header.Cells[3].AddParagraph("Km");
            header.Cells[4].AddParagraph("Job Type");
            header.Cells[5].AddParagraph("Mechanic");
            header.Cells[6].AddParagraph("Status");
            header.Cells[7].AddParagraph("Start Time");
            header.Cells[8].AddParagraph("Labor (€)");
            header.Cells[9].AddParagraph("Parts (€)");
            header.Cells[10].AddParagraph("Total (€)");

            foreach (var (order, jobs) in ordersWithJobs)
            {
                if (jobs.Count == 0)
                {
                    var row = table.AddRow();
                    row.Cells[0].AddParagraph(order.ClientName);
                    row.Cells[1].AddParagraph(order.CarName);
                    row.Cells[2].AddParagraph(order.CarRegistrationNumber);
                    row.Cells[3].AddParagraph(order.Kilometers.ToString());
                }
                else
                {
                    foreach (var job in jobs)
                    {
                        var row = table.AddRow();
                        row.Cells[0].AddParagraph(order.ClientName);
                        row.Cells[1].AddParagraph(order.CarName);
                        row.Cells[2].AddParagraph(order.CarRegistrationNumber);
                        row.Cells[3].AddParagraph(order.Kilometers.ToString());
                        row.Cells[4].AddParagraph(job.Type);
                        row.Cells[5].AddParagraph(job.MechanicName ?? "-");
                        row.Cells[6].AddParagraph(job.Status switch {
                            "pending" => "Pending",
                            "inprogress" => "In Progress",
                            _ => "Done"
                        });
                        row.Cells[7].AddParagraph(job.StartTime == default ? "" : job.StartTime.ToString("dd/MM/yyyy HH:mm"));
                        row.Cells[8].AddParagraph(job.LaborCost.ToString("F2"));
                        row.Cells[9].AddParagraph(job.PartsCost.ToString("F2"));
                        row.Cells[10].AddParagraph((job.LaborCost + job.PartsCost).ToString("F2"));
                    }
                }
            }

            return Task.FromResult(Render(doc));
        }

        public Task<byte[]> ExportClientsAsync(IEnumerable<ClientVM> clients)
        {
            var doc = CreateDocument("Clients Report");
            var section = doc.LastSection;

            var table = section.AddTable();


            table.AddColumn("4cm"); // Name
            table.AddColumn("3cm"); // Phone
            table.AddColumn("5cm"); // Email
            table.AddColumn("5cm"); // Address
            table.AddColumn("4cm"); // Registration #

            var header = table.AddRow();
            header.HeadingFormat = true;
            StyleHeader(header);
            header.Cells[0].AddParagraph("Name");
            header.Cells[1].AddParagraph("Phone");
            header.Cells[2].AddParagraph("Email");
            header.Cells[3].AddParagraph("Address");
            header.Cells[4].AddParagraph("Registration #");

            foreach (var client in clients)
            {
                var row = table.AddRow();
                row.Cells[0].AddParagraph(client.Name);
                row.Cells[1].AddParagraph(client.PhoneNumber ?? "-");
                row.Cells[2].AddParagraph(client.Email ?? "-");
                row.Cells[3].AddParagraph(client.Address ?? "-");
                row.Cells[4].AddParagraph(client.RegistrationNumber ?? "-");
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
                section.AddParagraph().Format.SpaceAfter = "0.2cm";
                var table = section.AddTable();

                table.AddColumn("4cm");
                table.AddColumn("4cm");
                table.AddColumn("5cm");
                table.AddColumn("4cm");
                table.AddColumn("4cm");
                table.AddColumn("3cm");

                var header = table.AddRow();
                StyleHeader(header);
                header.Cells[0].AddParagraph("Name");
                header.Cells[1].AddParagraph("Username");
                header.Cells[2].AddParagraph("Email");
                header.Cells[3].AddParagraph("Access");
                header.Cells[4].AddParagraph("Job Types");
                header.Cells[5].AddParagraph("Hired On");

                foreach (var w in workers)
                {
                    var selected = w.Accesses.Where(a => a.IsSelected).ToList();
                    var accessLabel = selected.Count == 0 ? "-"
                                    : selected.Count == w.Accesses.Count ? "Full"
                                    : string.Join(", ", selected.Select(a => a.Name));

                    var row = table.AddRow();
                    row.Cells[0].AddParagraph(w.Name);
                    row.Cells[1].AddParagraph(w.Username);
                    row.Cells[2].AddParagraph(w.Email ?? "-");
                    row.Cells[3].AddParagraph(accessLabel);
                    row.Cells[4].AddParagraph(string.Join(", ", w.JobTypeNames));
                    row.Cells[5].AddParagraph(w.HiredOn.ToString("dd.MM.yyyy"));
                }
                section.AddParagraph().Format.SpaceAfter = "1cm";
            }

            if (exportTypes.Contains("schedules"))
            {
                section.AddParagraph("Workers Schedules").Format.Font.Size = 14;
                section.AddParagraph().Format.SpaceAfter = "0.2cm";
                var table = section.AddTable();

                table.AddColumn("6cm");
                table.AddColumn("4cm");
                table.AddColumn("4cm");
                table.AddColumn("4cm");

                var header = table.AddRow();
                StyleHeader(header);
                header.Cells[0].AddParagraph("Worker");
                header.Cells[1].AddParagraph("Day");
                header.Cells[2].AddParagraph("Start Time");
                header.Cells[3].AddParagraph("End Time");

                var days = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
                foreach (var worker in workers)
                {
                    foreach (var s in worker.Schedules.OrderBy(s => s.DayOfWeek).ThenBy(s => s.StartTime))
                    {
                        var row = table.AddRow();
                        row.Cells[0].AddParagraph(worker.Name);
                        row.Cells[1].AddParagraph(days[s.DayOfWeek]);
                        row.Cells[2].AddParagraph(s.StartTime);
                        row.Cells[3].AddParagraph(s.EndTime);
                    }
                }
                section.AddParagraph().Format.SpaceAfter = "1cm";
            }

            if (exportTypes.Contains("leaves"))
            {
                section.AddParagraph("Workers Leaves").Format.Font.Size = 14;
                section.AddParagraph().Format.SpaceAfter = "0.2cm";
                var table = section.AddTable();

                table.AddColumn("8cm");
                table.AddColumn("4cm");
                table.AddColumn("4cm");

                var header = table.AddRow();
                StyleHeader(header);
                header.Cells[0].AddParagraph("Worker");
                header.Cells[1].AddParagraph("Start Date");
                header.Cells[2].AddParagraph("End Date");

                foreach (var worker in workers)
                {
                    foreach (var l in worker.Leaves.OrderBy(l => l.StartDate))
                    {
                        var row = table.AddRow();
                        row.Cells[0].AddParagraph(worker.Name);
                        row.Cells[1].AddParagraph(l.StartDate.ToString("dd.MM.yyyy"));
                        row.Cells[2].AddParagraph(l.EndDate.ToString("dd.MM.yyyy"));
                    }
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


            table.AddColumn("3cm"); // Client
            table.AddColumn("3cm"); // Car
            table.AddColumn("2.5cm"); // Plate
            table.AddColumn("3cm"); // Type
            table.AddColumn("2.5cm"); // Status
            table.AddColumn("3cm"); // Start
            table.AddColumn("3cm"); // End
            table.AddColumn("4cm"); // Description

            var header = table.AddRow();
            StyleHeader(header);
            header.Cells[0].AddParagraph("Client");
            header.Cells[1].AddParagraph("Car");
            header.Cells[2].AddParagraph("Plate");
            header.Cells[3].AddParagraph("Job Type");
            header.Cells[4].AddParagraph("Status");
            header.Cells[5].AddParagraph("Start");
            header.Cells[6].AddParagraph("End");
            header.Cells[7].AddParagraph("Description");

            foreach (var j in jobs)
            {
                var row = table.AddRow();
                row.Cells[0].AddParagraph(j.ClientName);
                row.Cells[1].AddParagraph(j.CarName);
                row.Cells[2].AddParagraph(j.CarRegistrationNumber);
                row.Cells[3].AddParagraph(j.TypeName);
                row.Cells[4].AddParagraph(j.Status);
                row.Cells[5].AddParagraph(j.StartTime.ToString("dd.MM HH:mm"));
                row.Cells[6].AddParagraph(j.EndTime.ToString("dd.MM HH:mm"));
                row.Cells[7].AddParagraph(j.Description);
            }

            return Task.FromResult(Render(doc));
        }

        public Task<byte[]> ExportPartsAsync(List<PartVM> parts)
        {
            var doc = CreateDocument("Parts Stock Report");
            var section = doc.LastSection;

            var table = section.AddTable();


            table.AddColumn("6cm"); // Name
            table.AddColumn("3.5cm"); // Part Number
            table.AddColumn("2.5cm"); // Current Qty
            table.AddColumn("2.5cm"); // Avail. Balance
            table.AddColumn("2.5cm"); // Pts to Client
            table.AddColumn("2.5cm"); // Min Qty
            table.AddColumn("3cm"); // Unit Price

            var header = table.AddRow();
            StyleHeader(header);
            header.Cells[0].AddParagraph("Name");
            header.Cells[1].AddParagraph("Part Number");
            header.Cells[2].AddParagraph("Current Qty");
            header.Cells[3].AddParagraph("Avail. Balance");
            header.Cells[4].AddParagraph("Pts to Client");
            header.Cells[5].AddParagraph("Min Qty");
            header.Cells[6].AddParagraph("Unit Price (€)");

            foreach (var p in parts)
            {
                var row = table.AddRow();
                row.Cells[0].AddParagraph(p.Name);
                row.Cells[1].AddParagraph(p.PartNumber ?? "-");
                row.Cells[2].AddParagraph(p.Quantity.ToString());
                row.Cells[3].AddParagraph(p.AvailabilityBalance.ToString());
                row.Cells[4].AddParagraph(p.PartsToSend.ToString());
                row.Cells[5].AddParagraph(p.MinimumQuantity.ToString());
                row.Cells[6].AddParagraph(p.Price.ToString("F2"));
            }

            return Task.FromResult(Render(doc));
        }

        public Task<byte[]> ExportJobTypesAsync(IEnumerable<JobTypeVM> jobTypes)
        {
            var doc = CreateDocument("Job Types Report");
            var section = doc.LastSection;

            var table = section.AddTable();


            table.AddColumn("6cm"); // Name
            table.AddColumn("13cm"); // Description

            var header = table.AddRow();
            StyleHeader(header);
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

        public Task<byte[]> ExportClientDetailsAsync(ClientVM client)
        {
            var doc = CreateDocument($"Client: {client.Name}");
            var section = doc.LastSection;

            section.AddParagraph("Client Info").Format.Font.Size = 14;
            section.AddParagraph().Format.SpaceAfter = "0.2cm";
            var tableInfo = section.AddTable();

            tableInfo.AddColumn("6cm");
            tableInfo.AddColumn("3.5cm");
            tableInfo.AddColumn("5.5cm");
            tableInfo.AddColumn("6cm");
            tableInfo.AddColumn("4cm");

            var headerInfo = tableInfo.AddRow();
            StyleHeader(headerInfo);
            headerInfo.Cells[0].AddParagraph("Name");
            headerInfo.Cells[1].AddParagraph("Phone");
            headerInfo.Cells[2].AddParagraph("Email");
            headerInfo.Cells[3].AddParagraph("Address");
            headerInfo.Cells[4].AddParagraph("Registration #");

            var rowInfo = tableInfo.AddRow();
            rowInfo.Cells[0].AddParagraph(client.Name);
            rowInfo.Cells[1].AddParagraph(client.PhoneNumber ?? "-");
            rowInfo.Cells[2].AddParagraph(client.Email ?? "-");
            rowInfo.Cells[3].AddParagraph(client.Address ?? "-");
            rowInfo.Cells[4].AddParagraph(client.RegistrationNumber ?? "-");

            section.AddParagraph().Format.SpaceAfter = "1cm";
            section.AddParagraph("Cars").Format.Font.Size = 14;
            section.AddParagraph().Format.SpaceAfter = "0.2cm";
            var tableCars = section.AddTable();

            tableCars.AddColumn("6cm");
            tableCars.AddColumn("6cm");
            tableCars.AddColumn("6cm");
            tableCars.AddColumn("4cm");

            var headerCars = tableCars.AddRow();
            StyleHeader(headerCars);
            headerCars.Cells[0].AddParagraph("Make");
            headerCars.Cells[1].AddParagraph("Model");
            headerCars.Cells[2].AddParagraph("Registration #");
            headerCars.Cells[3].AddParagraph("Kilometers");

            foreach (var c in client.Cars ?? new())
            {
                var row = tableCars.AddRow();
                row.Cells[0].AddParagraph(c.MakeName ?? "-");
                row.Cells[1].AddParagraph(c.ModelName ?? "-");
                row.Cells[2].AddParagraph(c.RegistrationNumber ?? "-");
                row.Cells[3].AddParagraph(c.Kilometers.ToString());
            }

            return Task.FromResult(Render(doc));
        }

        public Task<byte[]> ExportWorkerScheduleAsync(WorkerVM worker)
        {
            var doc = CreateDocument($"Schedule: {worker.Name}");
            var section = doc.LastSection;
            string[] dayNames = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

            section.AddParagraph("Schedule").Format.Font.Size = 14;
            section.AddParagraph().Format.SpaceAfter = "0.2cm";
            var tableSchedule = section.AddTable();

            tableSchedule.AddColumn("6cm");
            tableSchedule.AddColumn("6cm");
            tableSchedule.AddColumn("6cm");

            var headerSchedule = tableSchedule.AddRow();
            StyleHeader(headerSchedule);
            headerSchedule.Cells[0].AddParagraph("Day");
            headerSchedule.Cells[1].AddParagraph("Start Time");
            headerSchedule.Cells[2].AddParagraph("End Time");

            foreach (var s in worker.Schedules)
            {
                var row = tableSchedule.AddRow();
                row.Cells[0].AddParagraph(s.DayOfWeek >= 0 && s.DayOfWeek < dayNames.Length ? dayNames[s.DayOfWeek] : s.DayOfWeek.ToString());
                row.Cells[1].AddParagraph(s.StartTime);
                row.Cells[2].AddParagraph(s.EndTime);
            }

            section.AddParagraph().Format.SpaceAfter = "1cm";
            section.AddParagraph("Leaves").Format.Font.Size = 14;
            section.AddParagraph().Format.SpaceAfter = "0.2cm";
            var tableLeaves = section.AddTable();

            tableLeaves.AddColumn("6cm");
            tableLeaves.AddColumn("6cm");

            var headerLeaves = tableLeaves.AddRow();
            StyleHeader(headerLeaves);
            headerLeaves.Cells[0].AddParagraph("Start Date");
            headerLeaves.Cells[1].AddParagraph("End Date");

            foreach (var l in worker.Leaves)
            {
                var row = tableLeaves.AddRow();
                row.Cells[0].AddParagraph(l.StartDate.ToString("dd.MM.yyyy"));
                row.Cells[1].AddParagraph(l.EndDate.ToString("dd.MM.yyyy"));
            }

            return Task.FromResult(Render(doc));
        }

        public Task<byte[]> ExportJobAsync(JobDetailsVM job)
        {
            var doc = CreateDocument($"Job Details: {job.JobTypeName}");
            var section = doc.LastSection;

            section.AddParagraph("Details").Format.Font.Size = 14;
            section.AddParagraph().Format.SpaceAfter = "0.2cm";
            var tableInfo = section.AddTable();

            tableInfo.AddColumn("3cm");
            tableInfo.AddColumn("3cm");
            tableInfo.AddColumn("2.5cm");
            tableInfo.AddColumn("3cm");
            tableInfo.AddColumn("3cm");
            tableInfo.AddColumn("2cm");
            tableInfo.AddColumn("2.5cm");
            tableInfo.AddColumn("2.5cm");
            tableInfo.AddColumn("4cm");

            var headerInfo = tableInfo.AddRow();
            StyleHeader(headerInfo);
            headerInfo.Cells[0].AddParagraph("Client");
            headerInfo.Cells[1].AddParagraph("Car");
            headerInfo.Cells[2].AddParagraph("Reg #");
            headerInfo.Cells[3].AddParagraph("Job Type");
            headerInfo.Cells[4].AddParagraph("Mechanic");
            headerInfo.Cells[5].AddParagraph("Labor (€)");
            headerInfo.Cells[6].AddParagraph("Start Time");
            headerInfo.Cells[7].AddParagraph("End Time");
            headerInfo.Cells[8].AddParagraph("Description");

            var rowInfo = tableInfo.AddRow();
            rowInfo.Cells[0].AddParagraph(job.ClientName);
            rowInfo.Cells[1].AddParagraph(job.CarName);
            rowInfo.Cells[2].AddParagraph(job.CarRegistrationNumber);
            rowInfo.Cells[3].AddParagraph(job.JobTypeName ?? "");
            rowInfo.Cells[4].AddParagraph(job.MechanicName ?? "");
            rowInfo.Cells[5].AddParagraph(job.LaborCost.ToString("F2"));
            rowInfo.Cells[6].AddParagraph(job.StartTime == default ? "" : job.StartTime.ToString("dd/MM/yyyy HH:mm"));
            rowInfo.Cells[7].AddParagraph(job.EndTime == default ? "" : job.EndTime.ToString("dd/MM/yyyy HH:mm"));
            rowInfo.Cells[8].AddParagraph(job.Description);

            section.AddParagraph().Format.SpaceAfter = "1cm";
            section.AddParagraph("Parts Used").Format.Font.Size = 14;
            section.AddParagraph().Format.SpaceAfter = "0.2cm";
            var tableParts = section.AddTable();

            tableParts.AddColumn("6cm");
            tableParts.AddColumn("2.5cm");
            tableParts.AddColumn("2.5cm");
            tableParts.AddColumn("2.5cm");
            tableParts.AddColumn("2.5cm");
            tableParts.AddColumn("2.5cm");
            tableParts.AddColumn("2.5cm");

            var headerParts = tableParts.AddRow();
            StyleHeader(headerParts);
            headerParts.Cells[0].AddParagraph("Part Name");
            headerParts.Cells[1].AddParagraph("Planned Qty");
            headerParts.Cells[2].AddParagraph("Sent Qty");
            headerParts.Cells[3].AddParagraph("Used Qty");
            headerParts.Cells[4].AddParagraph("Req. Qty");
            headerParts.Cells[5].AddParagraph("Price (€)");
            headerParts.Cells[6].AddParagraph("Total (€)");

            foreach (var p in job.Parts)
            {
                var row = tableParts.AddRow();
                row.Cells[0].AddParagraph(p.PartName);
                row.Cells[1].AddParagraph(p.PlannedQuantity.ToString());
                row.Cells[2].AddParagraph(p.SentQuantity.ToString());
                row.Cells[3].AddParagraph(p.UsedQuantity.ToString());
                row.Cells[4].AddParagraph(p.RequestedQuantity.ToString());
                row.Cells[5].AddParagraph(p.Price.ToString("F2"));
                row.Cells[6].AddParagraph(((decimal)p.UsedQuantity * p.Price).ToString("F2"));
            }

            return Task.FromResult(Render(doc));
        }

        public Task<byte[]> ExportCarsAsync(IEnumerable<VehicleVM> cars)
        {
            var doc = CreateDocument("Cars Report");
            var section = doc.LastSection;

            var table = section.AddTable();


            table.AddColumn("4cm"); // Make
            table.AddColumn("4cm"); // Model
            table.AddColumn("4cm"); // Registration #
            table.AddColumn("3cm"); // Kilometers
            table.AddColumn("6cm"); // Owner

            var header = table.AddRow();
            StyleHeader(header);
            header.Cells[0].AddParagraph("Make");
            header.Cells[1].AddParagraph("Model");
            header.Cells[2].AddParagraph("Registration #");
            header.Cells[3].AddParagraph("Kilometers");
            header.Cells[4].AddParagraph("Owner");

            foreach (var c in cars)
            {
                var row = table.AddRow();
                row.Cells[0].AddParagraph(c.MakeName ?? "-");
                row.Cells[1].AddParagraph(c.ModelName ?? "-");
                row.Cells[2].AddParagraph(c.RegistrationNumber);
                row.Cells[3].AddParagraph(c.Kilometers.ToString());
                row.Cells[4].AddParagraph(c.OwnerName ?? "-");
            }

            return Task.FromResult(Render(doc));
        }

        private Document CreateDocument(string title)
        {
            var doc = new Document();
            var style = doc.Styles["Normal"];
            style.Font.Name = "Arial";
            style.Font.Size = 10;

            var section = doc.AddSection();
            section.PageSetup.PageFormat = PageFormat.A4;
            section.PageSetup.Orientation = Orientation.Landscape;
            section.PageSetup.TopMargin = "1.5cm";
            section.PageSetup.BottomMargin = "1.5cm";
            section.PageSetup.LeftMargin = "1.5cm";
            section.PageSetup.RightMargin = "1.5cm";

            var header = section.AddParagraph(title);
            header.Format.Font.Size = 18;
            header.Format.Font.Bold = true;
            header.Format.SpaceAfter = "0.8cm";
            header.Format.Alignment = ParagraphAlignment.Center;
            header.Format.Font.Color = new Color(0, 0, 0);

            return doc;
        }

        private void StyleHeader(Row row)
        {
            row.HeadingFormat = true;
            row.Format.Font.Bold = true;
            row.Format.Font.Color = new Color(0, 0, 0);
            row.Shading.Color = new Color(255, 204, 153);
            for (int i = 0; i < row.Cells.Count; i++)
            {
                row.Cells[i].Format.Alignment = ParagraphAlignment.Center;
                row.Cells[i].VerticalAlignment = VerticalAlignment.Center;
                row.Cells[i].Format.SpaceBefore = "0.15cm";
                row.Cells[i].Format.SpaceAfter = "0.15cm";
            }
        }

        private byte[] Render(Document doc)
        {
            foreach (Section section in doc.Sections)
            {
                foreach (DocumentObject element in section.Elements)
                {
                    if (element is Table table)
                    {
                        table.Borders.Width = 0.5;
                        table.Borders.Color = new Color(255, 100, 0);
                        table.LeftPadding = "0.15cm";
                        table.RightPadding = "0.15cm";
                        table.TopPadding = "0.15cm";
                        table.BottomPadding = "0.15cm";
                        
                        double totalWidth = 0;
                        foreach (Column column in table.Columns)
                        {
                            totalWidth += column.Width.Centimeter;
                        }

                        if (totalWidth > 0)
                        {
                            double targetWidth = 26.7; 
                            double ratio = targetWidth / totalWidth;
                            foreach (Column column in table.Columns)
                            {
                                column.Width = MigraDocCore.DocumentObjectModel.Unit.FromCentimeter(column.Width.Centimeter * ratio);
                            }
                        }

                        int rowIndex = 0;
                        foreach (Row row in table.Rows)
                        {
                            if (row.HeadingFormat) continue;

                            if (rowIndex % 2 != 0)
                            {
                                row.Shading.Color = new Color(255, 245, 235);
                            }
                            row.VerticalAlignment = VerticalAlignment.Center;
                            rowIndex++;

                            // Force text wrapping for long words without spaces
                            foreach (Cell cell in row.Cells)
                            {
                                foreach (DocumentObject cellElement in cell.Elements)
                                {
                                    if (cellElement is Paragraph paragraph)
                                    {
                                        foreach (DocumentObject paraElement in paragraph.Elements)
                                        {
                                            if (paraElement is Text textObj)
                                            {
                                                if (!string.IsNullOrEmpty(textObj.Content))
                                                {
                                                    var words = textObj.Content.Split(' ');
                                                    for (int w = 0; w < words.Length; w++)
                                                    {
                                                        if (words[w].Length > 10)
                                                        {
                                                            var sb = new System.Text.StringBuilder();
                                                            for (int c = 0; c < words[w].Length; c++)
                                                            {
                                                                sb.Append(words[w][c]);
                                                                // Insert zero-width space every 10 characters to allow breaking long words
                                                                if ((c + 1) % 10 == 0 && c < words[w].Length - 1)
                                                                {
                                                                    sb.Append('\u200B');
                                                                }
                                                            }
                                                            words[w] = sb.ToString();
                                                        }
                                                    }
                                                    textObj.Content = string.Join(" ", words);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var renderer = new PdfDocumentRenderer(true) { Document = doc };
            renderer.RenderDocument();
            using var ms = new MemoryStream();
            renderer.PdfDocument.Save(ms);
            return ms.ToArray();
        }
    }
}
