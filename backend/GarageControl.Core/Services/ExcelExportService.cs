using ClosedXML.Excel;
using GarageControl.Core.Contracts;
using GarageControl.Core.ViewModels.Clients;
using GarageControl.Core.ViewModels.Jobs;
using GarageControl.Core.ViewModels.Orders;
using GarageControl.Core.ViewModels.Parts;
using GarageControl.Core.ViewModels.Workers;
using GarageControl.Core.ViewModels.Vehicles;

namespace GarageControl.Core.Services
{
    public class ExcelExportService : IExcelExportService
    {
        // ── helpers ────────────────────────────────────────────────────────────

        private static byte[] ToBytes(XLWorkbook workbook)
        {
            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return ms.ToArray();
        }

        /// <summary>
        /// Writes a bold/styled header row followed by data rows, then auto-fits columns.
        /// </summary>
        private static void WriteTable(
            IXLWorksheet ws,
            IReadOnlyList<string> headers,
            IEnumerable<IReadOnlyList<object?>> rows)
        {
            for (int col = 0; col < headers.Count; col++)
            {
                var cell = ws.Cell(1, col + 1);
                cell.Value = headers[col];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#D9E1F2");
            }

            int rowIndex = 2;
            foreach (var row in rows)
            {
                for (int col = 0; col < row.Count; col++)
                {
                    var value = row[col];
                    var cell = ws.Cell(rowIndex, col + 1);
                    switch (value)
                    {
                        case decimal d:  cell.Value = (double)d;  break;
                        case double db:  cell.Value = db;          break;
                        case DateTime dt: cell.Value = dt;         break;
                        case int i:      cell.Value = i;           break;
                        default:         cell.Value = value?.ToString() ?? ""; break;
                    }
                }
                rowIndex++;
            }

            ws.Columns().AdjustToContents();
        }

        // ── exports ────────────────────────────────────────────────────────────

        public Task<byte[]> ExportOrdersAsync(List<(OrderListVM Order, List<JobListVM> Jobs)> ordersWithJobs)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Orders");

            var headers = new[]
            {
                "Client", "Car", "Reg #", "Km",
                "Job Type", "Mechanic", "Status",
                "Start Time", "Labor Cost (€)", "Parts Cost (€)", "Total Cost (€)"
            };

            var rows = new List<IReadOnlyList<object?>>();

            foreach (var (order, jobs) in ordersWithJobs)
            {
                if (jobs.Count == 0)
                {
                    rows.Add(new object?[] { order.ClientName, order.CarName, order.CarRegistrationNumber, order.Kilometers, "", "", "", "", "", "", "" });
                }
                else
                {
                    foreach (var job in jobs)
                    {
                        var statusLabel = job.Status switch
                        {
                            "pending"    => "Pending",
                            "inprogress" => "In Progress",
                            _            => "Done"
                        };
                        rows.Add(new object?[]
                        {
                            order.ClientName, order.CarName, order.CarRegistrationNumber, order.Kilometers,
                            job.Type, job.MechanicName, statusLabel,
                            job.StartTime == default ? "" : job.StartTime.ToString("dd/MM/yyyy HH:mm"),
                            job.LaborCost, job.PartsCost, job.LaborCost + job.PartsCost
                        });
                    }
                }
            }

            WriteTable(ws, headers, rows);
            return Task.FromResult(ToBytes(wb));
        }

        public Task<byte[]> ExportClientsAsync(IEnumerable<ClientVM> clients)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Clients");

            WriteTable(ws,
                new[] { "Name", "Phone", "Email", "Address", "Registration #" },
                clients.Select(c => (IReadOnlyList<object?>)new object?[]
                {
                    c.Name, c.PhoneNumber, c.Email ?? "", c.Address ?? "", c.RegistrationNumber ?? ""
                }));

            return Task.FromResult(ToBytes(wb));
        }

        public Task<byte[]> ExportWorkersAsync(IEnumerable<WorkerVM> workers, List<string> exportTypes)
        {
            using var wb = new XLWorkbook();

            if (exportTypes.Contains("details"))
            {
                var ws = wb.Worksheets.Add("Workers Details");
                WriteTable(ws,
                    new[] { "Name", "Username", "Email", "Access", "Job Types", "Hired On" },
                    workers.Select(w =>
                    {
                        var selected = w.Accesses.Where(a => a.IsSelected).ToList();
                        var accessLabel = selected.Count == 0 ? "-"
                                        : selected.Count == w.Accesses.Count ? "Full"
                                        : string.Join(", ", selected.Select(a => a.Name));

                        return (IReadOnlyList<object?>)new object?[]
                        {
                            w.Name, w.Username, w.Email, accessLabel,
                            string.Join(", ", w.JobTypeNames),
                            w.HiredOn.ToString("dd.MM.yyyy")
                        };
                    }));
            }

            if (exportTypes.Contains("schedules"))
            {
                var ws = wb.Worksheets.Add("Workers Schedules");
                var rows = new List<IReadOnlyList<object?>>();
                var days = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

                foreach (var worker in workers)
                {
                    foreach (var s in worker.Schedules.OrderBy(s => s.DayOfWeek).ThenBy(s => s.StartTime))
                    {
                        rows.Add(new object?[]
                        {
                            worker.Name, days[s.DayOfWeek], s.StartTime, s.EndTime
                        });
                    }
                }

                WriteTable(ws, new[] { "Worker", "Day", "Start Time", "End Time" }, rows);
            }

            if (exportTypes.Contains("leaves"))
            {
                var ws = wb.Worksheets.Add("Workers Leaves");
                var rows = new List<IReadOnlyList<object?>>();

                foreach (var worker in workers)
                {
                    foreach (var l in worker.Leaves.OrderBy(l => l.StartDate))
                    {
                        rows.Add(new object?[]
                        {
                            worker.Name, l.StartDate.ToString("dd.MM.yyyy"), l.EndDate.ToString("dd.MM.yyyy")
                        });
                    }
                }

                WriteTable(ws, new[] { "Worker", "Start Date", "End Date" }, rows);
            }

            return Task.FromResult(ToBytes(wb));
        }

        public Task<byte[]> ExportToDoAsync(IEnumerable<JobToDoVM> jobs, string workerName)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("To-Do List");

            WriteTable(ws,
                new[] { "Client", "Car", "Plate", "Job Type", "Status", "Start", "End", "Description" },
                jobs.Select(j => (IReadOnlyList<object?>)new object?[]
                {
                    j.ClientName, j.CarName, j.CarRegistrationNumber, j.TypeName,
                    j.Status, j.StartTime.ToString("dd.MM.yyyy HH:mm"), j.EndTime.ToString("dd.MM.yyyy HH:mm"),
                    j.Description
                }));

            return Task.FromResult(ToBytes(wb));
        }

        public Task<byte[]> ExportPartsAsync(List<PartVM> parts)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Parts Stock");

            WriteTable(ws,
                new[] { "Name", "Part Number", "Current Qty", "Availability Balance", "Parts to Client", "Min Qty", "Unit Price (€)" },
                parts.Select(p => (IReadOnlyList<object?>)new object?[]
                {
                    p.Name, p.PartNumber, p.Quantity, p.AvailabilityBalance, p.PartsToSend, p.MinimumQuantity, p.Price
                }));

            return Task.FromResult(ToBytes(wb));
        }

        public Task<byte[]> ExportClientDetailsAsync(ClientVM client)
        {
            using var wb = new XLWorkbook();

            var wsInfo = wb.Worksheets.Add("Client Info");
            WriteTable(wsInfo,
                new[] { "Name", "Phone", "Email", "Address", "Registration #" },
                new[]
                {
                    (IReadOnlyList<object?>)new object?[]
                    {
                        client.Name, client.PhoneNumber, client.Email ?? "", client.Address ?? "", client.RegistrationNumber ?? ""
                    }
                });

            var wsCars = wb.Worksheets.Add("Cars");
            WriteTable(wsCars,
                new[] { "Make", "Model", "Registration #", "Kilometers" },
                (client.Cars ?? new()).Select(c => (IReadOnlyList<object?>)new object?[]
                {
                    c.MakeName ?? "", c.ModelName ?? "", c.RegistrationNumber ?? "", c.Kilometers
                }));

            return Task.FromResult(ToBytes(wb));
        }

        public Task<byte[]> ExportWorkerScheduleAsync(WorkerVM worker)
        {
            using var wb = new XLWorkbook();
            string[] dayNames = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

            var wsSchedule = wb.Worksheets.Add("Schedule");
            WriteTable(wsSchedule,
                new[] { "Day", "Start Time", "End Time" },
                worker.Schedules.Select(s => (IReadOnlyList<object?>)new object?[]
                {
                    s.DayOfWeek >= 0 && s.DayOfWeek < dayNames.Length ? dayNames[s.DayOfWeek] : s.DayOfWeek.ToString(),
                    s.StartTime,
                    s.EndTime
                }));

            var wsLeaves = wb.Worksheets.Add("Leaves");
            WriteTable(wsLeaves,
                new[] { "Start Date", "End Date" },
                worker.Leaves.Select(l => (IReadOnlyList<object?>)new object?[]
                {
                    l.StartDate.ToString("dd.MM.yyyy"),
                    l.EndDate.ToString("dd.MM.yyyy")
                }));

            return Task.FromResult(ToBytes(wb));
        }

        public Task<byte[]> ExportJobAsync(JobDetailsVM job)
        {
            using var wb = new XLWorkbook();

            var wsInfo = wb.Worksheets.Add("Job Details");
            WriteTable(wsInfo,
                new[] { "Client", "Car", "Reg #", "Job Type", "Mechanic", "Labor Cost (€)", "Start Time", "End Time", "Description" },
                new[]
                {
                    (IReadOnlyList<object?>)new object?[]
                    {
                        job.ClientName, job.CarName, job.CarRegistrationNumber,
                        job.JobTypeName ?? "", job.MechanicName ?? "", job.LaborCost,
                        job.StartTime == default ? "" : job.StartTime.ToString("dd/MM/yyyy HH:mm"),
                        job.EndTime   == default ? "" : job.EndTime.ToString("dd/MM/yyyy HH:mm"),
                        job.Description
                    }
                });

            var wsParts = wb.Worksheets.Add("Parts Used");
            WriteTable(wsParts,
                new[] { "Part Name", "Planned Qty", "Sent Qty", "Used Qty", "Requested Qty", "Unit Price (€)", "Total (€)" },
                job.Parts.Select(p => (IReadOnlyList<object?>)new object?[]
                {
                    p.PartName,
                    p.PlannedQuantity,
                    p.SentQuantity,
                    p.UsedQuantity,
                    p.RequestedQuantity,
                    p.Price,
                    (decimal)p.UsedQuantity * p.Price
                }));

            return Task.FromResult(ToBytes(wb));
        }

        public Task<byte[]> ExportJobTypesAsync(IEnumerable<JobTypeVM> jobTypes)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Job Types");

            WriteTable(ws,
                new[] { "Name", "Description" },
                jobTypes.Select(jt => (IReadOnlyList<object?>)new object?[]
                {
                    jt.Name, jt.Description ?? ""
                }));

            return Task.FromResult(ToBytes(wb));
        }

        public Task<byte[]> ExportCarsAsync(IEnumerable<VehicleVM> cars)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Cars");

            WriteTable(ws,
                new[] { "Make", "Model", "Registration #", "Kilometers", "Owner" },
                cars.Select(c => (IReadOnlyList<object?>)new object?[]
                {
                    c.MakeName ?? "", c.ModelName ?? "", c.RegistrationNumber, c.Kilometers, c.OwnerName ?? ""
                }));

            return Task.FromResult(ToBytes(wb));
        }
    }
}
