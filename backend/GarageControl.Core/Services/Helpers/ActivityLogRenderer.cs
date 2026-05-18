using GarageControl.Core.Models;
using GarageControl.Core.Enums;
using System;
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

    public static class ActivityLogRenderer
    {
        public static ActivityLogRendererResult Render(LogEntityType logType, ActivityLogData data, Func<LogEntityType, string, bool>? existsChecker = null)
        {
            string actorMarkup = BuildActorMarkup(data, existsChecker);

            return BuildGenericMarkup(actorMarkup, data, logType, existsChecker);
        }

        public static string BuildMessageMarkup(LogEntityType logType, ActivityLogData data)
        {
            return Render(logType, data).Header;
        }

        public static IEnumerable<(LogEntityType Type, string Id)> GetReferencedEntities(LogEntityType logType, ActivityLogData data)
        {
            var result = new List<(LogEntityType Type, string Id)>();

            if (!string.IsNullOrEmpty(data.ActorId))
                result.Add((LogEntityType.Worker, data.ActorId));

            if (!string.IsNullOrEmpty(data.EntityId) && logType != LogEntityType.Workshop)
            {
                // Promote to root entity type for checks
                var checkType = logType switch
                {
                    LogEntityType.ArchivedOrder => LogEntityType.Order,
                    LogEntityType.ArchivedJob   => LogEntityType.Job,
                    _                           => logType
                };
                result.Add((checkType, data.EntityId));
            }

            if (data.Changes != null)
            {
                foreach (var c in data.Changes)
                {
                    if (c.TargetEntityType != null)
                    {
                        if (!string.IsNullOrEmpty(c.IdOld)) result.Add((c.TargetEntityType.Value, c.IdOld));
                        if (!string.IsNullOrEmpty(c.IdNew)) result.Add((c.TargetEntityType.Value, c.IdNew));
                    }
                }
            }

            return result.Distinct();
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static string EscapeMarkup(string? text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Replace("\\", "\\\\")
                       .Replace("*", "\\*")
                       .Replace("[", "\\[")
                       .Replace("]", "\\]");
        }

        private static string Bold(string? value) => $"**{EscapeMarkup(value) ?? "[Unknown]"}**";
        private static string Link(string? text, string? url) => string.IsNullOrEmpty(url) ? Bold(text) : $"[{EscapeMarkup(text) ?? "[Unknown]"}]({url})";

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

        public static string GetEntityLink(
            LogEntityType entityType,
            string? id,
            string? label,
            Func<LogEntityType, string, bool>? existsChecker = null)
        {
            if (string.IsNullOrEmpty(id)) return Bold(label);

            if (existsChecker != null && !existsChecker(entityType, id))
                return Bold(label);

            string url = GetUrlTemplate(entityType, id);
            if (string.IsNullOrEmpty(url)) return Bold(label);

            return Link(label, url);
        }

        private static string GetUrlTemplate(LogEntityType entityType, string id)
        {
            var escapedId = Uri.EscapeDataString(id);

            return entityType switch
            {
                LogEntityType.Worker        => $"/workers/{escapedId}?highlight=true",
                LogEntityType.Client        => $"/clients/{escapedId}?highlight=true",
                LogEntityType.Vehicle       => $"/cars/{escapedId}?highlight=true",
                LogEntityType.Make          => $"/makes-and-models/{escapedId}?highlight=true",
                LogEntityType.Model         => $"/makes-and-models?highlightModel={escapedId}",
                LogEntityType.JobType       => $"/job-types?highlightId={escapedId}",
                LogEntityType.Part          => $"/parts?partId={escapedId}",
                LogEntityType.Folder        => $"/parts?folderId={escapedId}",
                
                LogEntityType.Order         => $"/orders/{escapedId}?highlight=true",
                LogEntityType.ArchivedOrder => $"/archived-orders/{escapedId}?highlight=true",
                
                LogEntityType.Job           => $"/orders?highlightJob={escapedId}",
                LogEntityType.ArchivedJob   => $"/archived-orders?highlightJob={escapedId}",
                
                _                           => ""
            };
        }

        private static string BuildActorMarkup(ActivityLogData d, Func<LogEntityType, string, bool>? existsChecker)
        {
            return GetEntityLink(LogEntityType.Worker, d.ActorId, d.ActorName, existsChecker);
        }

        private static ActivityLogRendererResult BuildUpdatedMarkup(
            string actorMarkup,
            string entityLabel,
            string link,
            List<ActivityPropertyChange>? changes,
            Func<LogEntityType, string, bool>? existsChecker)
        {
            var res = new ActivityLogRendererResult { Header = $"{actorMarkup} updated {entityLabel} {link}" };
            if (changes == null || changes.Count == 0)
                return res;

            res.Details = changes.Select(c =>
            {
                string oldDisp = string.IsNullOrEmpty(c.OldValue) ? "[empty]" : EscapeMarkup(c.OldValue);
                string newDisp = string.IsNullOrEmpty(c.NewValue) ? "[empty]" : EscapeMarkup(c.NewValue);

                string fieldName = Humanize(c.FieldName);
                
                if (fieldName.ToLower().Contains("password")) 
                    return "Updated Password";

                if (oldDisp == "[empty]" && newDisp == "[empty]")
                    return fieldName.Contains(" ") ? Capitalize(c.FieldName) : $"Updated {Capitalize(c.FieldName)}";

                string FormatValue(string? val, string? id, string fName, LogEntityType? targetType)
                {
                    string displayVal = val != null && val.Length > 50 ? val.Substring(0, 47) + "..." : val;
                    if (!string.IsNullOrEmpty(id) && targetType != null)
                    {
                        return GetEntityLink(targetType.Value, id, displayVal, existsChecker);
                    }
                    if (fName.ToLower().Contains("status")) return Bold(Humanize(displayVal));
                    return Bold(displayVal);
                }

                if (oldDisp == "[empty]" && newDisp != "[empty]")
                    return $"{Capitalize(fieldName)}: {FormatValue(newDisp, c.IdNew, fieldName, c.TargetEntityType)}";

                if (oldDisp != "[empty]" && newDisp == "[empty]")
                    return $"Removed {fieldName.ToLower()}";

                return $"{Capitalize(fieldName)} changed from {FormatValue(oldDisp, c.IdOld, fieldName, c.TargetEntityType)} to {FormatValue(newDisp, c.IdNew, fieldName, c.TargetEntityType)}";
            }).ToList();

            return res;
        }

        private static List<string> RenderChanges(List<ActivityPropertyChange>? changes, Func<LogEntityType, string, bool>? existsChecker)
        {
            if (changes == null) return new();
            return changes.Select(c => $"{Capitalize(Humanize(c.FieldName))} changed from {Bold(c.OldValue)} to {Bold(c.NewValue)}").ToList();
        }

        private static string GetEntityTypeName(LogEntityType type)
        {
            return type switch
            {
                LogEntityType.JobType => "job type",
                LogEntityType.ArchivedOrder => "order",
                LogEntityType.ArchivedJob => "job",
                LogEntityType.Folder => "group of parts",
                _ => type.ToString().ToLower()
            };
        }

        private static ActivityLogRendererResult BuildGenericMarkup(string actorMarkup, ActivityLogData d, LogEntityType type, Func<LogEntityType, string, bool>? existsChecker)
        {
            string typeName = GetEntityTypeName(type);
            
            if (type == LogEntityType.Workshop)
            {
                var wRes = new ActivityLogRendererResult { Header = $"{actorMarkup} {d.Action.ToString().ToLower()} workshop {Bold(d.EntityName)}" };
                if (d.Action == LogAction.Updated && d.Changes != null && d.Changes.Any())
                {
                    wRes.Header = $"{actorMarkup} updated workshop details";
                    wRes.Details = d.Changes.Select(c => $"{Capitalize(Humanize(c.FieldName))} from {Bold(c.OldValue)} to {Bold(c.NewValue)}").ToList();
                }
                return wRes;
            }

            string linkLabel = (type == LogEntityType.Order || type == LogEntityType.ArchivedOrder) ? $"order for {d.EntityName}" : d.EntityName;
            string link = GetEntityLink(type, d.EntityId, linkLabel, existsChecker);

            switch (d.Action)
            {
                case LogAction.Created:
                    var cr = new ActivityLogRendererResult { Header = $"{actorMarkup} " + (type == LogEntityType.Job ? "added" : "created") + $" {typeName} {link}" };
                    if (d.Changes?.Any() == true) cr.Details = RenderChanges(d.Changes, existsChecker);
                    return cr;
                    
                case LogAction.Deleted:
                    return new ActivityLogRendererResult { Header = $"{actorMarkup} deleted {typeName} {Bold(linkLabel)}" };
                    
                case LogAction.Fired:
                    return new ActivityLogRendererResult { Header = $"{actorMarkup} fired {Bold(d.EntityName)}" };
                    
                case LogAction.Archived:
                    var arch = BuildUpdatedMarkup(actorMarkup, typeName, link, d.Changes, existsChecker);
                    arch.Header = $"{actorMarkup} archived {link}";
                    return arch;
                    
                case LogAction.Updated:
                case LogAction.Renamed:
                    var upd = BuildUpdatedMarkup(actorMarkup, typeName, link, d.Changes, existsChecker);
                    if (type == LogEntityType.Part && upd.Details != null)
                        upd.Details = upd.Details.Select(l => l.Replace("Quantity", "Stockpile")).ToList();
                    return upd;
                    
                case LogAction.Merged:
                    return new ActivityLogRendererResult { Header = $"{actorMarkup} merged custom {typeName} {Bold(d.Changes?.FirstOrDefault()?.NewValue)} into {link}" };
                    
                case LogAction.Moved:
                    return new ActivityLogRendererResult { Header = $"{actorMarkup} moved {typeName} {link}", Details = RenderChanges(d.Changes, existsChecker) };
                    
                default:
                    return new ActivityLogRendererResult { Header = $"{actorMarkup} {d.Action.ToString().ToLower()} {typeName} {link}" };
            }
        }
    }
}
