using System;
using System.Collections.Generic;
using GarageControl.Core.Models;
using GarageControl.Core.Enums;
using GarageControl.Core.Services.Helpers;

var link = ActivityLogRenderer.GetEntityLink(LogEntityType.Part, "123", "Brake Pad");
var change = new ActivityPropertyChange("Removed part " + link + " (qty: 2)", "", "");
var log = new ActivityLogData(LogAction.Updated, "job123", "Job for Car", Changes: new List<ActivityPropertyChange> { change }) { ActorId = "w1", ActorName = "Worker 1" };
        
var result = ActivityLogRenderer.Render(LogEntityType.Job, log);
Console.WriteLine("Header: " + result.Header);
foreach(var detail in result.Details) {
    Console.WriteLine("Detail: " + detail);
}
