using ClosedXML.Excel;
using GarageControl.Core.Contracts;
using GarageControl.Core.ViewModels.Clients;
using GarageControl.Core.ViewModels.Jobs;
using GarageControl.Core.ViewModels.Orders;
using GarageControl.Core.ViewModels.Parts;
using GarageControl.Core.ViewModels.Workers;
using GarageControl.Core.ViewModels.Vehicles;
using GarageControl.Core.Enums;

namespace GarageControl.Core.Services
{
    public static class ExcelStyles
    {
        public static readonly XLColor BorderColor = XLColor.FromArgb(255, 100, 0); // Orange Border
        public static readonly XLColor HeaderBgColor = XLColor.FromArgb(255, 204, 153); // Light Orange Header
        public static readonly XLColor AlternateRowBgColor = XLColor.FromArgb(255, 245, 235); // Soft Alternate BG
    }

    public static class ExcelFormats
    {
        public const string Date = "dd.MM.yyyy";
        public const string DateTime = "dd.MM.yyyy HH:mm";
        public const string Currency = "#,##0.00"; 
        public const string Integer = "#,##0";
        public const string Decimal = "#,##0.00";
    }

    public record ExcelColumn<T>(
        string Header,
        Func<T, object?> ValueSelector,
        string? NumberFormat = null
    );

    public class ExcelExportService : IExcelExportService
    {
        // ── core helpers ───────────────────────────────────────────────────────

        private static byte[] ToBytes(XLWorkbook workbook)
        {
            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return ms.ToArray();
        }

        private static void WriteSheet<T>(
            IXLWorksheet ws,
            IEnumerable<T> items,
            IReadOnlyList<ExcelColumn<T>> columns)
        {
            var totalColumns = columns.Count;

            // Apply column-wide configurations and write headers
            for (int col = 0; col < totalColumns; col++)
            {
                var xlCol = ws.Column(col + 1);
                
                // Set native Excel number formatting for the entire column at once
                if (columns[col].NumberFormat != null)
                {
                    xlCol.Style.NumberFormat.Format = columns[col].NumberFormat;
                }

                // Write header text
                var cell = ws.Cell(1, col + 1);
                cell.Value = columns[col].Header;
            }

            // Batch style the header row at once
            var headerRange = ws.Range(1, 1, 1, totalColumns);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = ExcelStyles.HeaderBgColor;

            // Write rows
            int rowIndex = 2;
            foreach (var item in items)
            {
                for (int col = 0; col < totalColumns; col++)
                {
                    var cell = ws.Cell(rowIndex, col + 1);
                    var value = columns[col].ValueSelector(item);
                    cell.Value = value != null ? XLCellValue.FromObject(value) : Blank.Value;
                }

                // Batch style alternating rows in a single range call instead of cell-by-cell
                if (rowIndex % 2 != 0)
                {
                    ws.Range(rowIndex, 1, rowIndex, totalColumns).Style.Fill.BackgroundColor = ExcelStyles.AlternateRowBgColor;
                }
                rowIndex++;
            }

            // Batch style borders for the entire sheet data area in a single block range
            if (rowIndex > 2)
            {
                var dataRange = ws.Range(1, 1, rowIndex - 1, totalColumns);
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorderColor = ExcelStyles.BorderColor;
                dataRange.Style.Border.OutsideBorderColor = ExcelStyles.BorderColor;
            }

            ws.Columns().AdjustToContents();
        }

        private static byte[] ExportSingleSheet<T>(
            string sheetName,
            IEnumerable<T> items,
            IReadOnlyList<ExcelColumn<T>> columns)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            WriteSheet(ws, items, columns);
            return ToBytes(wb);
        }

        // ── exports implementations ────────────────────────────────────────────

        private record OrderExportRow(
            string Client,
            string Car,
            string RegNumber,
            int Kilometers,
            string JobType,
            string Mechanic,
            string Status,
            DateTime? StartTime,
            decimal? LaborCost,
            decimal? PartsCost
        )
        {
            public decimal? TotalCost => (LaborCost ?? 0) + (PartsCost ?? 0);
        }

        public byte[] ExportOrders(List<(OrderListVM Order, List<JobListVM> Jobs)> ordersWithJobs)
        {
            IEnumerable<OrderExportRow> MapRows()
            {
                foreach (var (order, jobs) in ordersWithJobs)
                {
                    if (jobs.Count == 0)
                    {
                        yield return new OrderExportRow(
                            order.ClientName,
                            order.CarName,
                            order.CarRegistrationNumber,
                            order.Kilometers,
                            "",
                            "",
                            "",
                            null,
                            null,
                            null
                        );
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
                            yield return new OrderExportRow(
                                order.ClientName,
                                order.CarName,
                                order.CarRegistrationNumber,
                                order.Kilometers,
                                job.Type,
                                job.MechanicName,
                                statusLabel,
                                job.StartTime == default ? null : job.StartTime,
                                job.LaborCost,
                                job.PartsCost
                            );
                        }
                    }
                }
            }

            return ExportSingleSheet("Orders", MapRows(), new ExcelColumn<OrderExportRow>[]
            {
                new("Client", r => r.Client),
                new("Car", r => r.Car),
                new("Reg #", r => r.RegNumber),
                new("Km", r => r.Kilometers, ExcelFormats.Integer),
                new("Job Type", r => r.JobType),
                new("Mechanic", r => r.Mechanic),
                new("Status", r => r.Status),
                new("Start Time", r => r.StartTime, ExcelFormats.DateTime),
                new("Labor Cost", r => r.LaborCost, ExcelFormats.Currency),
                new("Parts Cost", r => r.PartsCost, ExcelFormats.Currency),
                new("Total Cost", r => r.TotalCost, ExcelFormats.Currency)
            });
        }

        public byte[] ExportClients(IEnumerable<ClientVM> clients)
        {
            return ExportSingleSheet("Clients", clients, new ExcelColumn<ClientVM>[]
            {
                new("Name", c => c.Name),
                new("Phone", c => c.PhoneNumber),
                new("Email", c => c.Email ?? ""),
                new("Address", c => c.Address ?? ""),
                new("Registration #", c => c.RegistrationNumber ?? "")
            });
        }

        private record WorkerDetailsRow(
            string Name,
            string Username,
            string Email,
            string Access,
            string JobTypes,
            DateTime HiredOn
        );

        private record WorkerScheduleRow(
            string Worker,
            string Day,
            string StartTime,
            string EndTime
        );

        private record WorkerLeaveRow(
            string Worker,
            DateTime StartDate,
            DateTime EndDate
        );

        public byte[] ExportWorkers(IEnumerable<WorkerVM> workers, WorkerExportFlags exportFlags)
        {
            using var wb = new XLWorkbook();

            if (exportFlags.HasFlag(WorkerExportFlags.Details))
            {
                var ws = wb.Worksheets.Add("Workers Details");
                
                // Lazy select projection passed directly without .ToList() allocation
                var details = workers.Select(w =>
                {
                    var selected = w.Accesses.Where(a => a.IsSelected).ToList();
                    var accessLabel = selected.Count == 0 ? "-"
                                    : selected.Count == w.Accesses.Count ? "Full"
                                    : string.Join(", ", selected.Select(a => a.Name));
                    return new WorkerDetailsRow(
                        w.Name,
                        w.Username,
                        w.Email,
                        accessLabel,
                        string.Join(", ", w.JobTypeNames),
                        w.HiredOn
                    );
                });

                WriteSheet(ws, details, new ExcelColumn<WorkerDetailsRow>[]
                {
                    new("Name", x => x.Name),
                    new("Username", x => x.Username),
                    new("Email", x => x.Email),
                    new("Access", x => x.Access),
                    new("Job Types", x => x.JobTypes),
                    new("Hired On", x => x.HiredOn, ExcelFormats.Date)
                });
            }

            if (exportFlags.HasFlag(WorkerExportFlags.Schedules))
            {
                var ws = wb.Worksheets.Add("Workers Schedules");
                var days = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
                
                // Lazy mapping to avoid intermediate list buffers
                var schedules = workers.SelectMany(w => w.Schedules
                    .OrderBy(s => s.DayOfWeek)
                    .ThenBy(s => s.StartTime)
                    .Select(s => new WorkerScheduleRow(
                        w.Name,
                        days[s.DayOfWeek],
                        s.StartTime,
                        s.EndTime
                    )));

                WriteSheet(ws, schedules, new ExcelColumn<WorkerScheduleRow>[]
                {
                    new("Worker", x => x.Worker),
                    new("Day", x => x.Day),
                    new("Start Time", x => x.StartTime),
                    new("End Time", x => x.EndTime)
                });
            }

            if (exportFlags.HasFlag(WorkerExportFlags.Leaves))
            {
                var ws = wb.Worksheets.Add("Workers Leaves");
                
                var leaves = workers.SelectMany(w => w.Leaves
                    .OrderBy(l => l.StartDate)
                    .Select(l => new WorkerLeaveRow(
                        w.Name,
                        l.StartDate,
                        l.EndDate
                    )));

                WriteSheet(ws, leaves, new ExcelColumn<WorkerLeaveRow>[]
                {
                    new("Worker", x => x.Worker),
                    new("Start Date", x => x.StartDate, ExcelFormats.Date),
                    new("End Date", x => x.EndDate, ExcelFormats.Date)
                });
            }

            return ToBytes(wb);
        }

        public byte[] ExportToDo(IEnumerable<JobToDoVM> jobs, string workerName)
        {
            return ExportSingleSheet("To-Do List", jobs, new ExcelColumn<JobToDoVM>[]
            {
                new("Client", j => j.ClientName),
                new("Car", j => j.CarName),
                new("Plate", j => j.CarRegistrationNumber),
                new("Job Type", j => j.TypeName),
                new("Status", j => j.Status),
                new("Start", j => j.StartTime, ExcelFormats.DateTime),
                new("End", j => j.EndTime, ExcelFormats.DateTime),
                new("Description", j => j.Description)
            });
        }

        public byte[] ExportParts(List<PartVM> parts)
        {
            return ExportSingleSheet("Parts Stock", parts, new ExcelColumn<PartVM>[]
            {
                new("Name", p => p.Name),
                new("Part Number", p => p.PartNumber),
                new("Current Qty", p => p.Quantity, ExcelFormats.Integer),
                new("Availability Balance", p => p.AvailabilityBalance, ExcelFormats.Integer),
                new("Parts to Client", p => p.PartsToSend, ExcelFormats.Integer),
                new("Min Qty", p => p.MinimumQuantity, ExcelFormats.Integer),
                new("Unit Price", p => p.Price, ExcelFormats.Currency)
            });
        }

        private record JobDetailsRow(
            string Client,
            string Car,
            string RegNumber,
            string JobType,
            string Mechanic,
            decimal LaborCost,
            DateTime? StartTime,
            DateTime? EndTime,
            string Description
        );

        private record PartUsedRow(
            string PartName,
            int PlannedQty,
            int SentQty,
            int UsedQty,
            int RequestedQty,
            decimal UnitPrice
        )
        {
            public decimal Total => UsedQty * UnitPrice;
        }

        public byte[] ExportJob(JobDetailsVM job)
        {
            using var wb = new XLWorkbook();

            var wsInfo = wb.Worksheets.Add("Job Details");
            var details = new[]
            {
                new JobDetailsRow(
                    job.ClientName,
                    job.CarName,
                    job.CarRegistrationNumber,
                    job.JobTypeName ?? "",
                    job.MechanicName ?? "",
                    job.LaborCost,
                    job.StartTime == default ? null : job.StartTime,
                    job.EndTime == default ? null : job.EndTime,
                    job.Description ?? ""
                )
            };

            WriteSheet(wsInfo, details, new ExcelColumn<JobDetailsRow>[]
            {
                new("Client", x => x.Client),
                new("Car", x => x.Car),
                new("Reg #", x => x.RegNumber),
                new("Job Type", x => x.JobType),
                new("Mechanic", x => x.Mechanic),
                new("Labor Cost", x => x.LaborCost, ExcelFormats.Currency),
                new("Start Time", x => x.StartTime, ExcelFormats.DateTime),
                new("End Time", x => x.EndTime, ExcelFormats.DateTime),
                new("Description", x => x.Description)
            });

            var wsParts = wb.Worksheets.Add("Parts Used");
            var parts = job.Parts.Select(p => new PartUsedRow(
                p.PartName,
                p.PlannedQuantity,
                p.SentQuantity,
                p.UsedQuantity,
                p.RequestedQuantity,
                p.Price
            ));

            WriteSheet(wsParts, parts, new ExcelColumn<PartUsedRow>[]
            {
                new("Part Name", x => x.PartName),
                new("Planned Qty", x => x.PlannedQty, ExcelFormats.Integer),
                new("Sent Qty", x => x.SentQty, ExcelFormats.Integer),
                new("Used Qty", x => x.UsedQty, ExcelFormats.Integer),
                new("Requested Qty", x => x.RequestedQty, ExcelFormats.Integer),
                new("Unit Price", x => x.UnitPrice, ExcelFormats.Currency),
                new("Total", x => x.Total, ExcelFormats.Currency)
            });

            return ToBytes(wb);
        }

        public byte[] ExportJobTypes(IEnumerable<JobTypeVM> jobTypes)
        {
            return ExportSingleSheet("Job Types", jobTypes, new ExcelColumn<JobTypeVM>[]
            {
                new("Name", jt => jt.Name),
                new("Description", jt => jt.Description ?? "")
            });
        }

        public byte[] ExportCars(IEnumerable<VehicleVM> cars)
        {
            return ExportSingleSheet("Cars", cars, new ExcelColumn<VehicleVM>[]
            {
                new("Make", c => c.MakeName ?? ""),
                new("Model", c => c.ModelName ?? ""),
                new("Registration #", c => c.RegistrationNumber),
                new("Kilometers", c => c.Kilometers, ExcelFormats.Integer),
                new("Owner", c => c.OwnerName ?? "")
            });
        }
    }
}
