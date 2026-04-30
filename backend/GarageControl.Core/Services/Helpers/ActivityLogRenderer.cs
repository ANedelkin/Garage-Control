using GarageControl.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GarageControl.Core.Services.Helpers
{
    public class ActivityLogRendererResult
    {
        public string HeaderHtml { get; set; } = "";
        public List<string> DetailsHtml { get; set; } = new();
    }

    /// <summary>
    /// Converts a structured <see cref="ActivityLogData"/> payload into HTML strings.
    /// Supports dynamic link generation based on entity existence and live names.
    /// </summary>
    public static class ActivityLogRenderer
    {
        public static ActivityLogRendererResult Render(string logType, ActivityLogData data, IDictionary<string, string>? liveNames = null)
        {
            string actorHtml = BuildActorHtml(data, liveNames);

            return logType switch
            {
                "Worker"   => BuildWorkerHtml(actorHtml, data, liveNames),
                "Client"   => BuildClientHtml(actorHtml, data, liveNames),
                "Vehicle"  => BuildVehicleHtml(actorHtml, data, liveNames),
                "Make"     => BuildMakeHtml(actorHtml, data, liveNames),
                "Model"    => BuildModelHtml(actorHtml, data, liveNames),
                "JobType"  => BuildJobTypeHtml(actorHtml, data, liveNames),
                "Order"    => BuildOrderHtml(actorHtml, data, liveNames),
                "Job"      => BuildJobHtml(actorHtml, data, liveNames),
                "Part"     => BuildPartHtml(actorHtml, data, liveNames),
                "Folder"   => BuildFolderHtml(actorHtml, data, liveNames),
                "Workshop" => BuildWorkshopHtml(actorHtml, data),
                _          => new ActivityLogRendererResult { HeaderHtml = $"{actorHtml} performed action on {logType}" }
            };
        }

        public static string BuildMessageHtml(string logType, ActivityLogData data, IDictionary<string, string>? liveNames = null)
        {
            return Render(logType, data, liveNames).HeaderHtml;
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static string Bold(string? value) => $"<b>{value}</b>";

        private static string Humanize(string? value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            // Split camelCase or PascalCase
            var result = Regex.Replace(value, "([a-z])([A-Z])", "$1 $2");
            // Also handle underscores if any
            result = result.Replace("_", " ");
            return result;
        }

        private static string Capitalize(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return char.ToUpper(value[0]) + value.Substring(1);
        }

        private static string GetLink(string? id, string? defaultName, string urlTemplate, IDictionary<string, string>? liveNames, string prefix = "", bool forceLink = false)
        {
            if (string.IsNullOrEmpty(id)) return prefix + Bold(defaultName);
            
            bool exists = forceLink || (liveNames != null && liveNames.ContainsKey(id));
            if (!exists) return prefix + Bold(defaultName);

            string displayName = (liveNames != null && liveNames.ContainsKey(id)) ? (liveNames[id] ?? defaultName) : defaultName;
            displayName ??= "Unknown";

            string url = urlTemplate.Replace("{id}", id).Replace("{name}", displayName);
            
            return $"<a href='{url}' class='log-link target-link'>{prefix}<b>{displayName}</b></a>";
        }

        private static string BuildActorHtml(ActivityLogData d, IDictionary<string, string>? liveNames)
        {
            if (string.IsNullOrEmpty(d.ActorId)) 
                return $"<span class='actor-name'>{d.ActorName ?? "[Unknown]"}</span>";
            

            bool exists = liveNames != null && liveNames.ContainsKey(d.ActorId);
            if (!exists) return $"<span class='actor-name'>{d.ActorName ?? "Unknown"}</span>";

            string displayName = liveNames![d.ActorId] ?? d.ActorName ?? "Unknown";
            string url = $"/workers/{d.ActorId}?highlight=true";
            
            return $"<a href='{url}' class='log-link actor-link'>{displayName}</a>";
        }

        private static ActivityLogRendererResult BuildUpdatedHtml(
            string actorHtml,
            string entityLabel,
            string link,
            List<ActivityPropertyChange>? changes)
        {
            var res = new ActivityLogRendererResult { HeaderHtml = $"{actorHtml} updated {entityLabel} {link}" };
            if (changes == null || changes.Count == 0)
                return res;

            res.DetailsHtml = changes.Select(c =>
            {
                string fieldName = Humanize(c.FieldName);
                if (fieldName.ToLower().Contains("password")) return "Password changed";
                
                string oldDisp = string.IsNullOrEmpty(c.OldValue) ? "[empty]" : c.OldValue;
                string newDisp = string.IsNullOrEmpty(c.NewValue) ? "[empty]" : c.NewValue;

                // Handle Name|Id structured values
                string FormatValue(string val, string fName)
                {
                    if (val.Contains("|"))
                    {
                        var parts = val.Split('|');
                        if (parts.Length == 2)
                        {
                            string name = parts[0];
                            string id = parts[1];
                            string template = "/";
                            if (fName.ToLower().Contains("worker") || fName.ToLower().Contains("mechanic")) template = "/workers/{id}?highlight=true";
                            else if (fName.ToLower().Contains("job type") || fName.ToLower().Contains("type")) template = "/job-types?highlightId={id}";
                            
                            return GetLink(id, name, template, null, forceLink: true);
                        }
                    }
                    if (fName.ToLower().Contains("status")) return Bold(Humanize(val));
                    return Bold(val);
                }

                if (oldDisp == "[empty]" && newDisp == "[empty]")
                    return Capitalize(fieldName);

                if (fieldName.ToLower().StartsWith("added ") || fieldName.ToLower().StartsWith("removed "))
                {
                    if (newDisp != "[empty]") return $"{Capitalize(fieldName)} {FormatValue(newDisp, fieldName)}";
                    return Capitalize(fieldName);
                }

                bool isDescription = fieldName.ToLower().Contains("description");
                if (isDescription)
                {
                    string truncate(string s) => s.Length > 50 ? s.Substring(0, 47) + "..." : s;
                    if (oldDisp == "[empty]") return $"Added description: {Bold(truncate(newDisp))}";
                    if (newDisp == "[empty]") return "Removed description";
                    return $"Description changed from {Bold(truncate(oldDisp))} to {Bold(truncate(newDisp))}";
                }

                if (oldDisp.Length > 100 || newDisp.Length > 100)
                    return Capitalize(fieldName);

                if (oldDisp == "[empty]" && newDisp != "[empty]")
                    return $"{Capitalize(fieldName)}: {FormatValue(newDisp, fieldName)}";

                if (oldDisp != "[empty]" && newDisp == "[empty]")
                    return $"Removed {fieldName.ToLower()}";

                return $"{Capitalize(fieldName)} from {FormatValue(oldDisp, fieldName)} to {FormatValue(newDisp, fieldName)}";
            }).ToList();

            return res;
        }

        // ── Per-entity builders ────────────────────────────────────────────────

        private static ActivityLogRendererResult BuildWorkerHtml(string actorHtml, ActivityLogData d, IDictionary<string, string>? liveNames)
        {
            string link = GetLink(d.EntityId, d.EntityName, "/workers/{id}?highlight=true", liveNames);
            var res = new ActivityLogRendererResult { HeaderHtml = $"{actorHtml} {d.Action} worker {link}" };

            switch (d.Action)
            {
                case "created":
                    var created = BuildUpdatedHtml(actorHtml, "worker", link, d.Changes);
                    created.HeaderHtml = $"{actorHtml} created worker {link}";
                    return created;
                
                case "fired":
                    res.HeaderHtml = $"{actorHtml} fired {Bold(d.EntityName)}";
                    break;

                case "updated":
                    var upd = BuildUpdatedHtml(actorHtml, "worker", link, d.Changes);
                    return upd;
            }
            return res;
        }

        private static ActivityLogRendererResult BuildClientHtml(string actorHtml, ActivityLogData d, IDictionary<string, string>? liveNames)
        {
            string link = GetLink(d.EntityId, d.EntityName, "/clients?highlightId={id}", liveNames);

            switch (d.Action)
            {
                case "created":
                    return new ActivityLogRendererResult { HeaderHtml = $"{actorHtml} created client {link}" };
                case "deleted":
                    return new ActivityLogRendererResult { HeaderHtml = $"{actorHtml} deleted client {Bold(d.EntityName)}" };
                case "updated":
                    return BuildUpdatedHtml(actorHtml, "client", link, d.Changes);
                default:
                    return new ActivityLogRendererResult { HeaderHtml = $"{actorHtml} {d.Action} client {link}" };
            }
        }

        private static ActivityLogRendererResult BuildVehicleHtml(string actorHtml, ActivityLogData d, IDictionary<string, string>? liveNames)
        {
            string link = GetLink(d.EntityId, d.EntityName, "/vehicles?registrationNumber={name}", liveNames);

            switch (d.Action)
            {
                case "created":
                    return new ActivityLogRendererResult { HeaderHtml = $"{actorHtml} created vehicle {link}" };
                case "deleted":
                    return new ActivityLogRendererResult { HeaderHtml = $"{actorHtml} deleted vehicle {Bold(d.EntityName)}" };
                case "updated":
                    return BuildUpdatedHtml(actorHtml, "vehicle", link, d.Changes);
                default:
                    return new ActivityLogRendererResult { HeaderHtml = $"{actorHtml} {d.Action} vehicle {link}" };
            }
        }

        private static ActivityLogRendererResult BuildMakeHtml(string actorHtml, ActivityLogData d, IDictionary<string, string>? liveNames)
        {
            string link = GetLink(d.EntityId, d.EntityName, "/models?highlightId={id}", liveNames);

            switch (d.Action)
            {
                case "created":
                    return new ActivityLogRendererResult { HeaderHtml = $"{actorHtml} created make {link}" };
                case "deleted":
                    return new ActivityLogRendererResult { HeaderHtml = $"{actorHtml} deleted make {Bold(d.EntityName)}" };
                default:
                    return new ActivityLogRendererResult { HeaderHtml = $"{actorHtml} {d.Action} make {link}" };
            }
        }

        private static ActivityLogRendererResult BuildModelHtml(string actorHtml, ActivityLogData d, IDictionary<string, string>? liveNames)
        {
            string modelLink = GetLink(d.EntityId, d.EntityName, "/models?highlightModelId={id}", liveNames);
            string secMakeLink = GetLink(d.SecondaryEntityId, d.SecondaryEntityName, "/models?highlightMakeId={id}", liveNames);

            switch (d.Action)
            {
                case "created":
                    return new ActivityLogRendererResult { HeaderHtml = $"{actorHtml} created model {modelLink} for make {secMakeLink}" };
                case "deleted":
                    return new ActivityLogRendererResult { HeaderHtml = $"{actorHtml} deleted model {Bold(d.EntityName)} from make {Bold(d.SecondaryEntityName)}" };
                case "renamed":
                    return new ActivityLogRendererResult { HeaderHtml = $"{actorHtml} renamed model {Bold(d.EntityName)} to {modelLink} (Make: {secMakeLink})" };
                case "merged":
                    return new ActivityLogRendererResult { HeaderHtml = $"{actorHtml} merged custom model {Bold(d.EntityName)} into {modelLink}" };
                default:
                    return new ActivityLogRendererResult { HeaderHtml = $"{actorHtml} {d.Action} model {Bold(d.EntityName)}" };
            }
        }

        private static ActivityLogRendererResult BuildJobTypeHtml(string actorHtml, ActivityLogData d, IDictionary<string, string>? liveNames)
        {
            string link = GetLink(d.EntityId, d.EntityName, "/job-types?highlightId={id}", liveNames);

            switch (d.Action)
            {
                case "created":
                    return new ActivityLogRendererResult { HeaderHtml = $"{actorHtml} created Job Type {link}" };
                case "deleted":
                    return new ActivityLogRendererResult { HeaderHtml = $"{actorHtml} deleted Job Type {Bold(d.EntityName)}" };
                case "updated":
                    return BuildUpdatedHtml(actorHtml, "Job Type", link, d.Changes);
                default:
                    return new ActivityLogRendererResult { HeaderHtml = $"{actorHtml} {d.Action} Job Type {link}" };
            }
        }

        private static ActivityLogRendererResult BuildOrderHtml(string actorHtml, ActivityLogData d, IDictionary<string, string>? liveNames)
        {
            string orderLink = GetLink(d.EntityId, d.EntityName, "/orders/{id}?highlight=true", liveNames, prefix: "order for ");
            var res = BuildUpdatedHtml(actorHtml, "order", orderLink, d.Changes);
            
            res.HeaderHtml = $"{actorHtml} {d.Action} {orderLink}";
            if (d.Action == "deleted")
                res.HeaderHtml = $"{actorHtml} deleted {Bold(d.EntityName)}";

            return res;
        }

        private static ActivityLogRendererResult BuildJobHtml(string actorHtml, ActivityLogData d, IDictionary<string, string>? liveNames)
        {
            string orderLink = GetLink(d.SecondaryEntityId, d.SecondaryEntityName, "/orders/{id}?highlight=true", liveNames, prefix: "order for ");
            string jobLink = GetLink(d.EntityId, d.EntityName, "/orders/" + d.SecondaryEntityId + "?highlightJob={id}", liveNames);
            
            var res = BuildUpdatedHtml(actorHtml, "job", jobLink, d.Changes);
            res.HeaderHtml = $"{actorHtml} {d.Action} job {jobLink} for {orderLink}";

            if (d.Action == "deleted")
            {
                res.HeaderHtml = $"{actorHtml} deleted job '{Bold(d.EntityName)}' for {orderLink}";
            }

            return res;
        }

        private static ActivityLogRendererResult BuildPartHtml(string actorHtml, ActivityLogData d, IDictionary<string, string>? liveNames)
        {
            string link = GetLink(d.EntityId, d.EntityName, "/parts?partId={id}", liveNames);
            var res = new ActivityLogRendererResult { HeaderHtml = $"{actorHtml} {d.Action} part {link}" };

            switch (d.Action)
            {
                case "created":
                    res.HeaderHtml = $"{actorHtml} created part {link}";
                    break;
                case "deleted":
                    res.HeaderHtml = $"{actorHtml} deleted part {Bold(d.EntityName)}";
                    break;
                case "moved":
                    res.HeaderHtml = $"{actorHtml} moved part {link} from {Bold(d.SecondaryEntityName)} to {Bold(d.SecondaryEntityId)}";
                    break;
                case "updated":
                    var upd = BuildUpdatedHtml(actorHtml, "part", link, d.Changes);
                    upd.DetailsHtml = upd.DetailsHtml.Select(l => l.Replace("Quantity", "Stockpile")).ToList();
                    return upd;
            }
            return res;
        }

        private static ActivityLogRendererResult BuildFolderHtml(string actorHtml, ActivityLogData d, IDictionary<string, string>? liveNames)
        {
            string link = GetLink(d.EntityId, d.EntityName, "/parts?folderId={id}", liveNames);
            var res = new ActivityLogRendererResult { HeaderHtml = $"{actorHtml} {d.Action} group of parts {link}" };

            switch (d.Action)
            {
                case "created":
                    res.HeaderHtml = $"{actorHtml} created group of parts {link}";
                    break;
                case "deleted":
                    res.HeaderHtml = $"{actorHtml} deleted group of parts {Bold(d.EntityName)}";
                    break;
                case "renamed":
                    res.HeaderHtml = $"{actorHtml} renamed group of parts {Bold(d.EntityName)} to {Bold(d.SecondaryEntityName)}";
                    break;
                case "moved":
                    res.HeaderHtml = $"{actorHtml} moved group of parts {link} from {Bold(d.SecondaryEntityName)} to {Bold(d.SecondaryEntityId)}";
                    break;
            }
            return res;
        }

        private static ActivityLogRendererResult BuildWorkshopHtml(string actorHtml, ActivityLogData d)
        {
            string name = Bold(d.EntityName);
            var res = new ActivityLogRendererResult { HeaderHtml = $"{actorHtml} {d.Action} workshop {name}" };

            if (d.Action == "updated" && d.Changes != null && d.Changes.Any())
            {
                res.HeaderHtml = $"{actorHtml} updated workshop details";
                res.DetailsHtml = d.Changes.Select(c => $"{Capitalize(Humanize(c.FieldName))} from {Bold(c.OldValue)} to {Bold(c.NewValue)}").ToList();
            }
            return res;
        }
    }
}
