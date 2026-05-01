using GarageControl.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GarageControl.Core.Services.Helpers
{
    public class ActivityLogRendererResult
    {
        public string Header { get; set; } = "";
        public List<string> Details { get; set; } = new();
    }

    /// <summary>
    /// Converts a structured <see cref="ActivityLogData"/> payload into markup strings.
    /// Supports dynamic link generation based on captured entity data.
    /// </summary>
    public static class ActivityLogRenderer
    {
        public static ActivityLogRendererResult Render(string logType, ActivityLogData data)
        {
            string actorMarkup = BuildActorMarkup(data);

            return logType switch
            {
                "Worker"   => BuildWorkerMarkup(actorMarkup, data),
                "Client"   => BuildClientMarkup(actorMarkup, data),
                "Vehicle"  => BuildVehicleMarkup(actorMarkup, data),
                "Make"     => BuildMakeMarkup(actorMarkup, data),
                "Model"    => BuildModelMarkup(actorMarkup, data),
                "JobType"  => BuildJobTypeMarkup(actorMarkup, data),
                "Order"    => BuildOrderMarkup(actorMarkup, data),
                "Job"      => BuildJobMarkup(actorMarkup, data),
                "Part"     => BuildPartMarkup(actorMarkup, data),
                "Folder"   => BuildFolderMarkup(actorMarkup, data),
                "Workshop" => BuildWorkshopMarkup(actorMarkup, data),
                _          => new ActivityLogRendererResult { Header = $"{actorMarkup} performed action on {logType}" }
            };
        }

        public static string BuildMessageMarkup(string logType, ActivityLogData data)
        {
            return Render(logType, data).Header;
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static string Bold(string? value) => $"**{value ?? "[Unknown]"}**";
        private static string Link(string? text, string? url) => string.IsNullOrEmpty(url) ? Bold(text) : $"[{text ?? "[Unknown]"}]({url})";

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

        private static string GetLink(string? id, string? label, string urlTemplate)
        {
            if (string.IsNullOrEmpty(id)) return Bold(label);
            
            string url = urlTemplate.Replace("{id}", System.Uri.EscapeDataString(id))
                                    .Replace("{name}", System.Uri.EscapeDataString(label ?? "[Unknown]"));
            return Link(label, url);
        }

        private static string BuildActorMarkup(ActivityLogData d)
        {
            return Link(d.ActorName, $"/workers/{d.ActorId}?highlight=true");
        }

        private static ActivityLogRendererResult BuildUpdatedMarkup(
            string actorMarkup,
            string entityLabel,
            string link,
            List<ActivityPropertyChange>? changes)
        {
            var res = new ActivityLogRendererResult { Header = $"{actorMarkup} updated {entityLabel} {link}" };
            if (changes == null || changes.Count == 0)
                return res;

            res.Details = changes.Select(c =>
            {
                string oldDisp = string.IsNullOrEmpty(c.OldValue) ? "[empty]" : c.OldValue;
                string newDisp = string.IsNullOrEmpty(c.NewValue) ? "[empty]" : c.NewValue;

                if (oldDisp == "[empty]" && newDisp == "[empty]")
                    return Capitalize(c.FieldName);

                string fieldName = Humanize(c.FieldName);
                if (fieldName.ToLower().Contains("password")) return "Password changed";

                string FormatValue(string? val, string? id, string fName)
                {
                    if (!string.IsNullOrEmpty(id))
                    {
                        string template = "/";
                        if (fName.ToLower().Contains("worker") || fName.ToLower().Contains("mechanic")) template = "/workers/{id}?highlight=true";
                        else if (fName.ToLower().Contains("job type") || fName.ToLower().Contains("type")) template = "/job-types?highlightId={id}";
                        
                        return GetLink(id, val, template);
                    }
                    if (fName.ToLower().Contains("status")) return Bold(Humanize(val));
                    return Bold(val);
                }

                if (fieldName.ToLower().StartsWith("added ") || fieldName.ToLower().StartsWith("removed "))
                {
                    if (newDisp != "[empty]") return $"{Capitalize(fieldName)} {FormatValue(newDisp, c.NewId, fieldName)}";
                    if (oldDisp != "[empty]") return $"{Capitalize(fieldName)} {FormatValue(oldDisp, c.OldId, fieldName)}";
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
                    return $"{Capitalize(fieldName)}: {FormatValue(newDisp, c.NewId, fieldName)}";

                if (oldDisp != "[empty]" && newDisp == "[empty]")
                    return $"Removed {fieldName.ToLower()}";

                return $"{Capitalize(fieldName)} from {FormatValue(oldDisp, c.OldId, fieldName)} to {FormatValue(newDisp, c.NewId, fieldName)}";
            }).ToList();

            return res;
        }

        // ── Per-entity builders ────────────────────────────────────────────────

        private static ActivityLogRendererResult BuildWorkerMarkup(string actorMarkup, ActivityLogData d)
        {
            string link = GetLink(d.EntityId, d.EntityName, "/workers/{id}?highlight=true");
            ActivityLogRendererResult res;
            switch (d.Action)
            {
                case "created":
                    res = BuildUpdatedMarkup(actorMarkup, "worker", link, d.Changes);
                    res.Header = $"{actorMarkup} created worker {link}";
                    return res;
                
                case "fired":
                    res = new ActivityLogRendererResult { Header = $"{actorMarkup} fired {Bold(d.EntityName)}" };
                    break;

                case "updated":
                    return BuildUpdatedMarkup(actorMarkup, "worker", link, d.Changes);

                default:
                    res = new ActivityLogRendererResult { Header = $"{actorMarkup} {d.Action} worker {link}" };
                    break;
            }

            return res;
        }

        private static ActivityLogRendererResult BuildClientMarkup(string actorMarkup, ActivityLogData d)
        {
            string link = GetLink(d.EntityId, d.EntityName, "/clients?highlightId={id}");

            switch (d.Action)
            {
                case "created":
                    return new ActivityLogRendererResult { Header = $"{actorMarkup} created client {link}" };
                case "deleted":
                    return new ActivityLogRendererResult { Header = $"{actorMarkup} deleted client {Bold(d.EntityName)}" };
                case "updated":
                    return BuildUpdatedMarkup(actorMarkup, "client", link, d.Changes);
                default:
                    return new ActivityLogRendererResult { Header = $"{actorMarkup} {d.Action} client {link}" };
            }
        }

        private static ActivityLogRendererResult BuildVehicleMarkup(string actorMarkup, ActivityLogData d)
        {
            string link = GetLink(d.EntityId, d.EntityName, "/vehicles?registrationNumber={name}");

            switch (d.Action)
            {
                case "created":
                    return new ActivityLogRendererResult { Header = $"{actorMarkup} created vehicle {link}" };
                case "deleted":
                    return new ActivityLogRendererResult { Header = $"{actorMarkup} deleted vehicle {Bold(d.EntityName)}" };
                case "updated":
                    return BuildUpdatedMarkup(actorMarkup, "vehicle", link, d.Changes);
                default:
                    return new ActivityLogRendererResult { Header = $"{actorMarkup} {d.Action} vehicle {link}" };
            }
        }

        private static ActivityLogRendererResult BuildMakeMarkup(string actorMarkup, ActivityLogData d)
        {
            string link = GetLink(d.EntityId, d.EntityName, "/models?highlightId={id}");

            switch (d.Action)
            {
                case "created":
                    return new ActivityLogRendererResult { Header = $"{actorMarkup} created make {link}" };
                case "deleted":
                    return new ActivityLogRendererResult { Header = $"{actorMarkup} deleted make {Bold(d.EntityName)}" };
                default:
                    return new ActivityLogRendererResult { Header = $"{actorMarkup} {d.Action} make {link}" };
            }
        }

        private static ActivityLogRendererResult BuildModelMarkup(string actorMarkup, ActivityLogData d)
        {
            string modelLink = GetLink(d.EntityId, d.EntityName, "/models?highlightModelId={id}");
            string secMakeLink = GetLink(d.SecondaryEntityId, d.SecondaryEntityName, "/models?highlightMakeId={id}");

            switch (d.Action)
            {
                case "created":
                    return new ActivityLogRendererResult { Header = $"{actorMarkup} created model {modelLink} for make {secMakeLink}" };
                case "deleted":
                    return new ActivityLogRendererResult { Header = $"{actorMarkup} deleted model {Bold(d.EntityName)} from make {Bold(d.SecondaryEntityName)}" };
                case "renamed":
                    return new ActivityLogRendererResult { Header = $"{actorMarkup} renamed model {Bold(d.EntityName)} to {modelLink} (Make: {secMakeLink})" };
                case "merged":
                    return new ActivityLogRendererResult { Header = $"{actorMarkup} merged custom model {Bold(d.EntityName)} into {modelLink}" };
                default:
                    return new ActivityLogRendererResult { Header = $"{actorMarkup} {d.Action} model {Bold(d.EntityName)}" };
            }
        }

        private static ActivityLogRendererResult BuildJobTypeMarkup(string actorMarkup, ActivityLogData d)
        {
            string link = GetLink(d.EntityId, d.EntityName, "/job-types?highlightId={id}");

            switch (d.Action)
            {
                case "created":
                    return new ActivityLogRendererResult { Header = $"{actorMarkup} created Job Type {link}" };
                case "deleted":
                    return new ActivityLogRendererResult { Header = $"{actorMarkup} deleted Job Type {Bold(d.EntityName)}" };
                case "updated":
                    return BuildUpdatedMarkup(actorMarkup, "Job Type", link, d.Changes);
                default:
                    return new ActivityLogRendererResult { Header = $"{actorMarkup} {d.Action} Job Type {link}" };
            }
        }

        private static ActivityLogRendererResult BuildOrderMarkup(string actorMarkup, ActivityLogData d)
        {
            string orderLink = GetLink(d.EntityId, $"order for {d.EntityName}", "/orders/{id}?highlight=true");
            var res = BuildUpdatedMarkup(actorMarkup, "order", orderLink, d.Changes);
            
            res.Header = $"{actorMarkup} {d.Action} {orderLink}";
            if (d.Action == "deleted")
                res.Header = $"{actorMarkup} deleted {Bold(d.EntityName)}";

            return res;
        }

        private static ActivityLogRendererResult BuildJobMarkup(string actorMarkup, ActivityLogData d)
        {
            string orderLink = GetLink(d.SecondaryEntityId, $"order for {d.SecondaryEntityName}", "/orders/{id}?highlight=true");
            string jobLink = GetLink(d.EntityId, d.EntityName, "/orders/" + d.SecondaryEntityId + "?highlightJob={id}");
            
            var res = BuildUpdatedMarkup(actorMarkup, "job", jobLink, d.Changes);
            res.Header = $"{actorMarkup} {d.Action} job {jobLink} for {orderLink}";

            if (d.Action == "deleted")
            {
                res.Header = $"{actorMarkup} deleted job '{Bold(d.EntityName)}' for {orderLink}";
            }

            return res;
        }

        private static ActivityLogRendererResult BuildPartMarkup(string actorMarkup, ActivityLogData d)
        {
            string link = GetLink(d.EntityId, d.EntityName, "/parts?partId={id}");
            var res = new ActivityLogRendererResult { Header = $"{actorMarkup} {d.Action} part {link}" };

            switch (d.Action)
            {
                case "created":
                    res.Header = $"{actorMarkup} created part {link}";
                    break;
                case "deleted":
                    res.Header = $"{actorMarkup} deleted part {Bold(d.EntityName)}";
                    break;
                case "moved":
                    res.Header = $"{actorMarkup} moved part {link} from {Bold(d.SecondaryEntityName)} to {Bold(d.SecondaryEntityId)}";
                    break;
                case "updated":
                    var upd = BuildUpdatedMarkup(actorMarkup, "part", link, d.Changes);
                    upd.Details = upd.Details.Select(l => l.Replace("Quantity", "Stockpile")).ToList();
                    return upd;
            }
            return res;
        }

        private static ActivityLogRendererResult BuildFolderMarkup(string actorMarkup, ActivityLogData d)
        {
            string link = GetLink(d.EntityId, d.EntityName, "/parts?folderId={id}");
            var res = new ActivityLogRendererResult { Header = $"{actorMarkup} {d.Action} group of parts {link}" };

            switch (d.Action)
            {
                case "created":
                    res.Header = $"{actorMarkup} created group of parts {link}";
                    break;
                case "deleted":
                    res.Header = $"{actorMarkup} deleted group of parts {Bold(d.EntityName)}";
                    break;
                case "renamed":
                    res.Header = $"{actorMarkup} renamed group of parts {Bold(d.EntityName)} to {Bold(d.SecondaryEntityName)}";
                    break;
                case "moved":
                    res.Header = $"{actorMarkup} moved group of parts {link} from {Bold(d.SecondaryEntityName)} to {Bold(d.SecondaryEntityId)}";
                    break;
            }
            return res;
        }

        private static ActivityLogRendererResult BuildWorkshopMarkup(string actorMarkup, ActivityLogData d)
        {
            string name = Bold(d.EntityName);
            var res = new ActivityLogRendererResult { Header = $"{actorMarkup} {d.Action} workshop {name}" };

            if (d.Action == "updated" && d.Changes != null && d.Changes.Any())
            {
                res.Header = $"{actorMarkup} updated workshop details";
                res.Details = d.Changes.Select(c => $"{Capitalize(Humanize(c.FieldName))} from {Bold(c.OldValue)} to {Bold(c.NewValue)}").ToList();
            }
            return res;
        }
    }
}
