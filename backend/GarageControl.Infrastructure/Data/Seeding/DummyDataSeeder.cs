using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GarageControl.Infrastructure.Data.Models;
using GarageControl.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GarageControl.Infrastructure.Data.Seeding
{
    public static class DummyDataSeeder
    {
        private static readonly Random Rnd = new Random(42); // Seeded for consistency

        private class WorkshopConfig
        {
            public string Name { get; set; } = null!;
            public List<string> Specializations { get; set; } = new();
        }

        private class NamesConfig
        {
            public List<string> FirstNames { get; set; } = new();
            public List<string> LastNames { get; set; } = new();
        }

        private class PartsConfig
        {
            public List<string> Folders { get; set; } = new();
            public List<string> Parts { get; set; } = new();
        }

        public static async Task SeedAsync(GarageControlDbContext context, IServiceProvider serviceProvider)
        {
            if (await context.Workshops.AnyAsync())
            {
                // Already seeded
                return;
            }

            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var baseDir = Path.Combine(AppContext.BaseDirectory, "Data", "Seeding", "DataSets");
            if (!Directory.Exists(baseDir))
            {
                // Fallback for development where the working directory might be the project root
                baseDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "GarageControl.Infrastructure", "Data", "Seeding", "DataSets");
            }
            
            var makesWithModels = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(await File.ReadAllTextAsync(Path.Combine(baseDir, "MakesModels.json")), opts)!;
            var workshopConfigs = JsonSerializer.Deserialize<List<WorkshopConfig>>(await File.ReadAllTextAsync(Path.Combine(baseDir, "Workshops.json")), opts)!;
            var namesConfig = JsonSerializer.Deserialize<NamesConfig>(await File.ReadAllTextAsync(Path.Combine(baseDir, "Names.json")), opts)!;
            var baseJobTypes = JsonSerializer.Deserialize<List<string>>(await File.ReadAllTextAsync(Path.Combine(baseDir, "JobTypes.json")), opts)!;
            var partsConfig = JsonSerializer.Deserialize<PartsConfig>(await File.ReadAllTextAsync(Path.Combine(baseDir, "Parts.json")), opts)!;

            var firstNames = namesConfig.FirstNames.ToArray();
            var lastNames = namesConfig.LastNames.ToArray();
            var baseFolders = partsConfig.Folders.ToArray();
            var baseParts = partsConfig.Parts.ToArray();

            var (dbMakes, dbModels) = await SeedMakesAndModelsAsync(context, makesWithModels);
            var dbWorkshops = await SeedWorkshopsAsync(context, workshopConfigs);

            await SeedWorkshopDependenciesAsync(context, serviceProvider, dbWorkshops, dbModels, firstNames, lastNames, baseJobTypes, baseFolders, baseParts);

            await context.SaveChangesAsync();
        }

        private static async Task<(List<CarMake>, List<CarModel>)> SeedMakesAndModelsAsync(GarageControlDbContext context, Dictionary<string, List<string>> makesWithModels)
        {
            var dbMakes = new List<CarMake>();
            var dbModels = new List<CarModel>();

            foreach (var kvp in makesWithModels)
            {
                var make = new CarMake { Name = kvp.Key };
                dbMakes.Add(make);
                
                foreach (var modelName in kvp.Value)
                {
                    dbModels.Add(new CarModel { Name = modelName, CarMakeId = make.Id });
                }
            }

            await context.CarMakes.AddRangeAsync(dbMakes);
            await context.CarModels.AddRangeAsync(dbModels);

            return (dbMakes, dbModels);
        }

        private static Task<List<Workshop>> SeedWorkshopsAsync(GarageControlDbContext context, List<WorkshopConfig> workshopConfigs)
        {
            var dbWorkshops = new List<Workshop>();
            
            foreach (var wC in workshopConfigs)
            {
                var w = new Workshop
                {
                    Name = wC.Name,
                    Address = $"{Rnd.Next(1, 999)} Main St.",
                    PhoneNumber = $"+1555{Rnd.Next(100000, 999999)}"
                };
                
                dbWorkshops.Add(w);
            }

            return Task.FromResult(dbWorkshops);
        }

        private static async Task SeedWorkshopDependenciesAsync(GarageControlDbContext context, IServiceProvider serviceProvider, List<Workshop> dbWorkshops, List<CarModel> dbModels, string[] firstNames, string[] lastNames, List<string> baseJobTypes, string[] baseFolders, string[] baseParts)
        {
            var userManager = serviceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<User>>();
            var allAccesses = await context.Accesses.ToListAsync();

            foreach (var w in dbWorkshops)
                {
                    // Create a Boss user for the workshop
                    var bossUser = new User { UserName = $"boss_{w.Name.Replace(" ", "").ToLower()}@example.com", Email = $"boss_{w.Name.Replace(" ", "").ToLower()}@example.com" };
                    await userManager.CreateAsync(bossUser, "Password123!");
                    w.BossId = bossUser.Id;
                    w.Boss = bossUser;
                    context.Workshops.Add(w);

                    // Workshop specific makes/models
                    var customMakes = new List<CarMake>();
                    for (int i = 0; i < Rnd.Next(1, 4); i++)
                    {
                        var cm = new CarMake { Name = $"Special {w.Name} {i + 1}", CreatorId = bossUser.Id, Creator = bossUser };
                        customMakes.Add(cm);
                        for (int j = 0; j < Rnd.Next(1, 4); j++)
                        {
                            var cmo = new CarModel { Name = $"Mod {j + 1}", CarMake = cm, CarMakeId = cm.Id, CreatorId = bossUser.Id, Creator = bossUser };
                            cm.CarModels.Add(cmo);
                        }
                    }
                    await context.CarMakes.AddRangeAsync(customMakes);

                    var workshopModels = dbModels.Concat(customMakes.SelectMany(m => m.CarModels)).ToList();

                    // Clients (~20 per workshop)
                    var dbClients = new List<Client>();
                    int clientCount = Rnd.Next(15, 25);
                    for (int i = 0; i < clientCount; i++)
                    {
                        string fn = firstNames[Rnd.Next(firstNames.Length)];
                        string ln = lastNames[Rnd.Next(lastNames.Length)];
                        dbClients.Add(new Client
                        {
                            Name = $"{fn} {ln}",
                            Email = $"{fn.ToLower()}.{ln.ToLower()}{RandomNumber(100)}@example.com",
                            PhoneNumber = $"+1555{Rnd.Next(1000000, 9999999)}",
                            WorkshopId = w.Id,
                            Workshop = w
                        });
                    }
                    await context.Clients.AddRangeAsync(dbClients);

                    // Cars (~1-2 per client)
                    var dbCars = new List<Car>();
                    foreach (var client in dbClients)
                    {
                        int carCount = Rnd.Next(1, 3);
                        for (int i = 0; i < carCount; i++)
                        {
                            var randomModel = workshopModels[Rnd.Next(workshopModels.Count)];
                            dbCars.Add(new Car
                            {
                                ModelId = randomModel.Id,
                                Model = randomModel,
                                VIN = GenerateRandomVin(),
                                RegistrationNumber = GenerateRandomLicensePlate(),
                                OwnerId = client.Id,
                                Owner = client,
                                Kilometers = Rnd.Next(5000, 150000),
                            });
                        }
                    }
                    await context.Cars.AddRangeAsync(dbCars);

                    // Workers
                    int workerCount = Rnd.Next(4, 11);
                    var workers = new List<Worker>();

                    string bossFn = firstNames[Rnd.Next(firstNames.Length)];
                    string bossLn = lastNames[Rnd.Next(lastNames.Length)];
                    var bossWorker = new Worker
                    {
                        Name = $"{bossFn} {bossLn} (Boss)",
                        UserId = bossUser.Id,
                        User = bossUser,
                        WorkshopId = w.Id,
                        Workshop = w,
                        HiredOn = DateTime.UtcNow.AddMonths(-100),
                        Accesses = allAccesses.ToList()
                    };

                    // Boss Schedule
                    for (int d = 1; d <= 5; d++)
                    {
                        bossWorker.Schedules.Add(new WorkerSchedule
                        {
                            WorkerId = bossWorker.Id,
                            Worker = bossWorker,
                            DayOfWeek = (DayOfWeek)d,
                            StartTime = new TimeOnly(9, 0),
                            EndTime = new TimeOnly(17, 0)
                        });
                    }

                    workers.Add(bossWorker);

                    var regularAccesses = allAccesses.Where(a => a.Name != "Workshop Details" && a.Name != "Workers").ToList();

                    for (int i = 0; i < workerCount; i++)
                    {
                        string fn = firstNames[Rnd.Next(firstNames.Length)];
                        string ln = lastNames[Rnd.Next(lastNames.Length)];
                        var workerUser = new User 
                        { 
                            UserName = $"{fn.ToLower()}.{ln.ToLower()}{RandomNumber(100)}@worker.com", 
                            Email = $"{fn.ToLower()}.{ln.ToLower()}{RandomNumber(100)}@worker.com",
                            LastLogin = DateTime.UtcNow.AddMinutes(-Rnd.Next(0, 20000)) // Random login in last 2 weeks
                        };
                        await userManager.CreateAsync(workerUser, "Password123!");

                        int numAccesses = Rnd.Next(2, regularAccesses.Count);
                        var subsetAccesses = regularAccesses.OrderBy(x => Rnd.Next()).Take(numAccesses).ToList();

                        var worker = new Worker
                        {
                            Name = $"{fn} {ln}",
                            UserId = workerUser.Id,
                            User = workerUser,
                            WorkshopId = w.Id,
                            Workshop = w,
                            HiredOn = DateTime.UtcNow.AddMonths(-Rnd.Next(1, 60)),
                            Accesses = subsetAccesses
                        };

                        // Regular Worker Schedule
                        for (int d = 1; d <= 5; d++)
                        {
                            worker.Schedules.Add(new WorkerSchedule
                            {
                                WorkerId = worker.Id,
                                Worker = worker,
                                DayOfWeek = (DayOfWeek)d,
                                StartTime = new TimeOnly(9, 0),
                                EndTime = new TimeOnly(17, 0)
                            });
                        }

                        workers.Add(worker);
                    }
                    await context.Workers.AddRangeAsync(workers);
                    var workerAvailability = workers.ToDictionary(w => w.Id, w => DateTime.UtcNow.Date.AddDays(-30).AddHours(9));

                    // Job Types
                    var jobTypesCount = Rnd.Next(8, 12);
                    var selectedJtNames = baseJobTypes.OrderBy(x => Rnd.Next()).Take(jobTypesCount).ToList();
                    var jobTypes = new List<JobType>();
                    foreach (var jtName in selectedJtNames)
                    {
                        jobTypes.Add(new JobType
                        {
                            Name = jtName,
                            Description = $"Standard procedure for {jtName}",
                            WorkshopId = w.Id,
                            Workshop = w
                        });
                    }
                    await context.JobTypes.AddRangeAsync(jobTypes);

                    foreach (var jt in jobTypes)
                    {
                        var randomWorker = workers[Rnd.Next(workers.Count)];
                        randomWorker.Activities.Add(jt);
                    }

                    foreach (var worker in workers)
                    {
                        var additional = jobTypes.OrderBy(x => Rnd.Next()).Take(Rnd.Next(1, 4)).ToList();
                        foreach(var a in additional)
                        {
                            if (!worker.Activities.Contains(a)) worker.Activities.Add(a);
                        }
                    }

                    // Folders
                    int folderCount = Rnd.Next(2, 4);
                    var selectedFolderNames = baseFolders.OrderBy(x => Rnd.Next()).Take(folderCount).ToList();
                    var folders = new List<PartsFolder>();
                    foreach (var fName in selectedFolderNames)
                    {
                        folders.Add(new PartsFolder
                        {
                            Name = fName,
                            WorkshopId = w.Id,
                            Workshop = w
                        });
                    }
                    await context.PartsFolders.AddRangeAsync(folders);

                    // Parts
                    var parts = new List<Part>();
                    int partsCount = Rnd.Next(18, 25);
                    for (int i = 0; i < partsCount; i++)
                    {
                        var pName = baseParts[Rnd.Next(baseParts.Length)];
                        var folder = folders[Rnd.Next(folders.Count)];
                        int initialQuantity = Rnd.Next(100, 200);
                        parts.Add(new Part
                        {
                            Name = $"{pName} {Rnd.Next(1, 100)}",
                            PartNumber = $"PN-{Rnd.Next(10000, 99999)}",
                            Price = (decimal)(Rnd.NextDouble() * 200 + 10),
                            Quantity = initialQuantity,
                            MinimumQuantity = Rnd.Next(2, 10),
                            AvailabilityBalance = initialQuantity,
                            ParentId = folder.Id,
                            Parent = folder,
                            WorkshopId = w.Id,
                            Workshop = w
                        });
                    }
                    await context.Parts.AddRangeAsync(parts);

                    // Orders
                    // Orders
                    var ordersToProcess = new List<(Order order, DateTime start)>();
                    for (int i = 0; i < 15; i++)
                    {
                        var randomCar = dbCars[Rnd.Next(dbCars.Count)];
                        var order = new Order
                        {
                            CarId = randomCar.Id,
                            Car = randomCar,
                            IsArchived = false,
                            Kilometers = Rnd.Next(5000, 150000)
                        };

                        // Pick a start date within the last 30 days or next 15 days
                        var daysOffset = Rnd.Next(-30, 15);
                        var carAvailability = DateTime.UtcNow.Date.AddDays(daysOffset).AddHours(9);
                        
                        // Ensure it's a weekday
                        while (carAvailability.DayOfWeek == DayOfWeek.Saturday || carAvailability.DayOfWeek == DayOfWeek.Sunday)
                        {
                            carAvailability = carAvailability.AddDays(1);
                        }
                        ordersToProcess.Add((order, carAvailability));
                    }

                    foreach (var (order, initialStart) in ordersToProcess.OrderBy(o => o.start))
                    {
                        var carAvailability = initialStart;
                        int jobCount = Rnd.Next(3, 7);
                        var jobs = new List<Job>();

                        for (int j = 0; j < jobCount; j++)
                        {
                            var randomJt = jobTypes[Rnd.Next(jobTypes.Count)];
                            var specializedWorkers = workers.Where(wrk => wrk.Activities.Any(act => act.Id == randomJt.Id)).ToList();
                            var randomWorker = specializedWorkers.Any() 
                                ? specializedWorkers[Rnd.Next(specializedWorkers.Count)] 
                                : workers[Rnd.Next(workers.Count)];

                            int duration = Rnd.Next(2, 5);
                            var jobStart = carAvailability > workerAvailability[randomWorker.Id] 
                                ? carAvailability 
                                : workerAvailability[randomWorker.Id];

                            // Working Hours & Shift End Check
                            while (jobStart.Hour + duration > 17 || jobStart.DayOfWeek == DayOfWeek.Saturday || jobStart.DayOfWeek == DayOfWeek.Sunday)
                            {
                                jobStart = jobStart.Date.AddDays(1).AddHours(9);
                            }

                            var jobEnd = jobStart.AddHours(duration);

                            var status = Rnd.Next(0, 3) switch
                            {
                                0 => JobStatus.Pending,
                                1 => JobStatus.InProgress,
                                _ => JobStatus.Done
                            };

                            // Ensure status matches the timeline
                            if (jobStart > DateTime.UtcNow)
                            {
                                status = JobStatus.Pending;
                            }
                            else if (jobEnd > DateTime.UtcNow && status == JobStatus.Done)
                            {
                                status = JobStatus.InProgress;
                            }

                            var job = new Job
                            {
                                Order = order,
                                OrderId = order.Id,
                                JobType = randomJt,
                                JobTypeId = randomJt.Id,
                                Worker = randomWorker,
                                WorkerId = randomWorker.Id,
                                Status = status,
                                Description = status == JobStatus.Done ? "Completed successfully." : "",
                                LaborCost = (decimal)(Rnd.NextDouble() * 100 + 20),
                                StartTime = jobStart,
                                EndTime = jobEnd
                            };

                            // Update markers
                            workerAvailability[randomWorker.Id] = jobEnd;
                            carAvailability = jobEnd;

                            int jpCount = Rnd.Next(1, 5);
                            var selectedParts = parts.OrderBy(x => Rnd.Next()).Take(jpCount).ToList();
                            foreach(var randomPart in selectedParts)
                            {
                                int planned = Rnd.Next(1, 6);
                                int requested = 0;
                                int sent = 0;
                                int used = 0;

                                if (status == JobStatus.Done)
                                {
                                    planned = Rnd.Next(1, 4);
                                    requested = planned;
                                    sent = planned;
                                    used = planned;
                                }
                                else if (status == JobStatus.InProgress)
                                {
                                    requested = Rnd.Next(0, planned + 1);
                                    sent = Rnd.Next(0, requested + 1);
                                    used = Rnd.Next(0, sent + 1);
                                }
                                else // Pending
                                {
                                    requested = Rnd.Next(0, 2) == 0 ? 0 : Rnd.Next(1, planned + 1);
                                }

                                randomPart.Quantity -= sent;
                                randomPart.AvailabilityBalance -= planned;
                                
                                job.JobParts.Add(new JobPart
                                {
                                    Job = job,
                                    Part = randomPart,
                                    PlannedQuantity = planned,
                                    RequestedQuantity = requested,
                                    SentQuantity = sent,
                                    UsedQuantity = used,
                                    Price = randomPart.Price
                                });
                            }

                            jobs.Add(job);
                        }
                        order.Jobs = jobs;
                        if (jobs.All(j => j.Status == JobStatus.Done))
                        {
                            order.IsArchived = true;
                        }
                        context.Orders.Add(order);
                    }
                }
        }

        private static int RandomNumber(int max) => Rnd.Next(1, max);

        private static string GenerateRandomVin()
        {
            const string chars = "ABCDEFGHJKLMNPRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 17).Select(s => s[Rnd.Next(s.Length)]).ToArray());
        }

        private static string GenerateRandomLicensePlate()
        {
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string numbers = "0123456789";
            
            var plateChars = new char[7];
            plateChars[0] = letters[Rnd.Next(letters.Length)];
            plateChars[1] = letters[Rnd.Next(letters.Length)];
            plateChars[2] = letters[Rnd.Next(letters.Length)];
            plateChars[3] = '-';
            plateChars[4] = numbers[Rnd.Next(numbers.Length)];
            plateChars[5] = numbers[Rnd.Next(numbers.Length)];
            plateChars[6] = numbers[Rnd.Next(numbers.Length)];
            
            return new string(plateChars);
        }
    }
}
