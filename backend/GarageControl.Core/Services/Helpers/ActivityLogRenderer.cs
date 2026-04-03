using GarageControl.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace GarageControl.Core.Services.Helpers
{
    /// <summary>
    /// Converts a structured <see cref="ActivityLogData"/> payload into HTML strings.
    /// Supports dynamic link generation based on entity existence and live names.
    /// </summary>
    public static class ActivityLogRenderer
    {
        public static string BuildMessageHtml(string logType, ActivityLogData data, IDictionary<string, string>? liveNames = null)
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
                _          => $"{actorHtml} performed action on {logType}"
            };
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static string Bold(string? value) => $"<b>{value}</b>";

        private static string GetLink(string? id, string? defaultName, string urlTemplate, IDictionary<string, string>? liveNames)
        {
            if (string.IsNullOrEmpty(id)) return Bold(defaultName);
            
            bool exists = liveNames != null && liveNames.ContainsKey(id);
            if (!exists) return Bold(defaultName);

            string displayName = liveNames![id] ?? defaultName ?? "Unknown";
            // Registration number based links (Vehicle) don't use ID in URL usually, but we check existence by ID
            string url = urlTemplate.Replace("{id}", id).Replace("{name}", displayName);
            
            return $"<a href='{url}' class='log-link target-link'>{displayName}</a>";
        }

        private static string BuildActorHtml(ActivityLogData d, IDictionary<string, string>? liveNames)
        {
            if (string.IsNullOrEmpty(d.ActorId)) 
                return $"<span class='actor-name'>{d.ActorName ?? "[Unknown]"}</span>";
            
            // Special case for Owner (non-worker)
            if (d.ActorName == "Owner")
                return $"<span class='actor-name'>Owner</span>";

            bool exists = liveNames != null && liveNames.ContainsKey(d.ActorId);
            if (!exists) return $"<span class='actor-name'>{d.ActorName ?? "Unknown"}</span>";

            string displayName = liveNames![d.ActorId] ?? d.ActorName ?? "Unknown";
            string url = $"/workers/{d.ActorId}?highlight=true";
            
            return $"<a href='{url}' class='log-link actor-link'>{displayName}</a>";
        }

        private static string BuildUpdatedHtml(
            string actorHtml,
            string entityLabel,
            string link,
            List<ActivityPropertyChange>? changes)
        {
            if (changes == null || changes.Count == 0)
                return $"{actorHtml} updated {entityLabel} {link}";

            var formatted = changes.Select(c =>
            {
                string oldDisp = string.IsNullOrEmpty(c.OldValue) ? "[empty]" : c.OldValue;
                string newDisp = string.IsNullOrEmpty(c.NewValue) ? "[empty]" : c.NewValue;

                if (oldDisp.Length > 100 || newDisp.Length > 100)
                    return c.FieldName;

                return $"{c.FieldName} from {Bold(oldDisp)} to {Bold(newDisp)}";
            }).ToList();

            bool allSimple = formatted.All(f => !f.Contains(" from "));

            if (formatted.Count == 1 && !allSimple)
                return $"{actorHtml} changed {formatted[0]} of {entityLabel} {link}";

            if (allSimple)
                return $"{actorHtml} updated details of {entityLabel} {link}";

            return $"{actorHtml} updated {entityLabel} {link}: {string.Join(", ", formatted)}";
        }

        // ── Per-entity builders ────────────────────────────────────────────────

        private static string BuildWorkerHtml(string actorHtml, ActivityLogData d, IDictionary<string, string>? liveNames)
        {
            string link = GetLink(d.EntityId, d.EntityName, "/workers/{id}?highlight=true", liveNames);

            switch (d.Action)
            {
                case "created":
                {
                    string html = $"{actorHtml} created worker {link}";
                    var allChanges = d.Changes ?? new List<ActivityPropertyChange>();
                    if (allChanges.Any())
                    {
                        var allStrings = allChanges.Select(c =>
                        {
                            if (c.NewValue == null) return c.FieldName;
                            return $"{c.FieldName} from {Bold(c.OldValue)} to {Bold(c.NewValue)}";
                        });
                        html += $": {string.Join(", ", allStrings)}";
                    }
                    return html;
                }

                case "fired":
                    return $"{actorHtml} fired {Bold(d.EntityName)}";

                case "updated":
                {
                    var allChanges = d.Changes ?? new List<ActivityPropertyChange>();
                    var allStrings = allChanges.Select(c =>
                    {
                        string oldDisp = string.IsNullOrEmpty(c.OldValue) ? "[empty]" : c.OldValue;
                        string newDisp = string.IsNullOrEmpty(c.NewValue) ? "[empty]" : c.NewValue;
                        if (c.NewValue == null) return c.FieldName; 
                        if (oldDisp.Length > 100 || newDisp.Length > 100) return c.FieldName;
                        return $"{c.FieldName} from {Bold(oldDisp)} to {Bold(newDisp)}";
                    }).ToList();

                    bool allSimple = allStrings.All(s => !s.Contains(" from ") && !s.Contains("added ") && !s.Contains("removed ") && !s.Contains("Updated ") && !s.Contains("deleted "));

                    if (allStrings.Count == 1 && (allStrings[0].Contains(" from ") || allStrings[0].Contains("Updated ")))
                        return $"{actorHtml} changed {allStrings[0]} of worker {link}";

                    if (allSimple)
                        return $"{actorHtml} updated details of worker {link}";

                    return $"{actorHtml} updated worker {link}: {string.Join(", ", allStrings)}";
                }

                default:
                    return $"{actorHtml} {d.Action} worker {link}";
            }
        }

        private static string BuildClientHtml(string actorHtml, ActivityLogData d, IDictionary<string, string>? liveNames)
        {
            string link = GetLink(d.EntityId, d.EntityName, "/clients?highlightId={id}", liveNames);

            switch (d.Action)
            {
                case "created":
                    return $"{actorHtml} created client {link}";

                case "deleted":
                    return $"{actorHtml} deleted client {Bold(d.EntityName)}";

                case "updated":
                    return BuildUpdatedHtml(actorHtml, "client", link, d.Changes);

                default:
                    return $"{actorHtml} {d.Action} client {link}";
            }
        }

        private static string BuildVehicleHtml(string actorHtml, ActivityLogData d, IDictionary<string, string>? liveNames)
        {
            // Registration based highlights
            string link = GetLink(d.EntityId, d.EntityName, "/vehicles?registrationNumber={name}", liveNames);

            switch (d.Action)
            {
                case "created":
                    return $"{actorHtml} created vehicle {link}";

                case "deleted":
                    return $"{actorHtml} deleted vehicle {Bold(d.EntityName)}";

                case "updated":
                    return BuildUpdatedHtml(actorHtml, "vehicle", link, d.Changes);

                default:
                    return $"{actorHtml} {d.Action} vehicle {link}";
            }
        }

        private static string BuildMakeHtml(string actorHtml, ActivityLogData d, IDictionary<string, string>? liveNames)
        {
            string link = GetLink(d.EntityId, d.EntityName, "/models?highlightId={id}", liveNames);

            switch (d.Action)
            {
                case "created":
                    return $"{actorHtml} created make {link}";

                case "deleted":
                    return $"{actorHtml} deleted make {Bold(d.EntityName)}";

                default:
                    return $"{actorHtml} {d.Action} make {link}";
            }
        }

        private static string BuildModelHtml(string actorHtml, ActivityLogData d, IDictionary<string, string>? liveNames)
        {
            string modelLink = GetLink(d.EntityId, d.EntityName, "/models?highlightModelId={id}", liveNames);
            string secMakeLink = GetLink(d.SecondaryEntityId, d.SecondaryEntityName, "/models?highlightMakeId={id}", liveNames);

            switch (d.Action)
            {
                case "created":
                    return $"{actorHtml} created model {modelLink} for make {secMakeLink}";

                case "deleted":
                    return $"{actorHtml} deleted model {Bold(d.EntityName)} from make {Bold(d.SecondaryEntityName)}";

                case "renamed":
                    return $"{actorHtml} renamed model {Bold(d.EntityName)} to {modelLink} (Make: {secMakeLink})";

                case "merged":
                    return $"{actorHtml} merged custom model {Bold(d.EntityName)} into {modelLink}";

                default:
                    return $"{actorHtml} {d.Action} model {Bold(d.EntityName)}";
            }
        }

        private static string BuildJobTypeHtml(string actorHtml, ActivityLogData d, IDictionary<string, string>? liveNames)
        {
            string link = GetLink(d.EntityId, d.EntityName, "/job-types?highlightId={id}", liveNames);

            switch (d.Action)
            {
                case "created":
                    return $"{actorHtml} created Job Type {link}";

                case "deleted":
                    return $"{actorHtml} deleted Job Type {Bold(d.EntityName)}";

                case "updated":
                {
                    var changes = d.Changes ?? new List<ActivityPropertyChange>();
                    var formatted = changes.Select(c =>
                    {
                        string oldDisp = string.IsNullOrEmpty(c.OldValue) ? "[empty]" : c.OldValue;
                        string newDisp = string.IsNullOrEmpty(c.NewValue) ? "[empty]" : c.NewValue;
                        if (oldDisp.Length > 100 || newDisp.Length > 100) return c.FieldName;
                        return $"{c.FieldName} from {Bold(oldDisp)} to {Bold(newDisp)}";
                    }).ToList();

                    bool allSimple = formatted.All(f => !f.Contains(" from "));

                    if (formatted.Count == 1 && !allSimple)
                        return $"{actorHtml} changed {formatted[0]} of Job Type {link}";

                    if (allSimple)
                        return $"{actorHtml} updated details of Job Type {link}";

                    return $"{actorHtml} updated Job Type {link}: {string.Join(", ", formatted)}";
                }

                default:
                    return $"{actorHtml} {d.Action} Job Type {link}";
            }
        }

        private static string BuildOrderHtml(string actorHtml, ActivityLogData d, IDictionary<string, string>? liveNames)
        {
            string orderLink = GetLink(d.EntityId, d.EntityName, "/orders/{id}?highlight=true", liveNames);
            if (orderLink == Bold(d.EntityName)) orderLink = $"order for {Bold(d.EntityName)}";

            switch (d.Action)
            {
                case "created":
                    return $"{actorHtml} created {orderLink}";

                case "updated":
                {
                    string html = $"{actorHtml} updated {orderLink}";
                    if (d.Changes != null && d.Changes.Any())
                    {
                        var formatted = d.Changes.Select(c =>
                        {
                            if (string.IsNullOrEmpty(c.OldValue) && string.IsNullOrEmpty(c.NewValue))
                                return c.FieldName;
                            return $"{c.FieldName} from {Bold(c.OldValue)} to {Bold(c.NewValue)}";
                        });
                        html += $": {string.Join(", ", formatted)}";
                    }
                    return html;
                }

                default:
                    return $"{actorHtml} {d.Action} {orderLink}";
            }
        }

        private static string BuildJobHtml(string actorHtml, ActivityLogData d, IDictionary<string, string>? liveNames)
        {
            string orderLink = GetLink(d.SecondaryEntityId, d.SecondaryEntityName, "/orders/{id}?highlight=true", liveNames);
            if (orderLink == Bold(d.SecondaryEntityName)) orderLink = $"order for {Bold(d.SecondaryEntityName)}";

            string jobLink = GetLink(d.EntityId, d.EntityName, "/orders/" + d.SecondaryEntityId + "?highlightJob={id}", liveNames);

            switch (d.Action)
            {
                case "created":
                {
                    string html = $"{actorHtml} created job {jobLink} for {orderLink}";
                    if (d.Changes != null && d.Changes.Any())
                    {
                        var partStrings = d.Changes.Select(c => c.FieldName); 
                        html += $": {string.Join(", ", partStrings)}";
                    }
                    return html;
                }

                case "updated":
                {
                    var allChanges = new List<string>();
                    if (d.Changes != null)
                    {
                        foreach (var c in d.Changes)
                        {
                            if (string.IsNullOrEmpty(c.OldValue) && string.IsNullOrEmpty(c.NewValue))
                            {
                                allChanges.Add(c.FieldName);
                            }
                            else if (c.FieldName == "mechanic")
                            {
                                var parts = c.NewValue.Split('|');
                                if (parts.Length == 2)
                                {
                                    string workerName = parts[0];
                                    string workerId = parts[1];
                                    string workerLink = GetLink(workerId, workerName, "/workers/{id}?highlight=true", liveNames);
                                    allChanges.Add($"mechanic from {Bold(c.OldValue)} to {workerLink}");
                                }
                                else
                                {
                                    allChanges.Add($"mechanic from {Bold(c.OldValue)} to {Bold(c.NewValue)}");
                                }
                            }
                            else
                            {
                                allChanges.Add($"{c.FieldName} from {Bold(c.OldValue)} to {Bold(c.NewValue)}");
                            }
                        }
                    }
                    if (!allChanges.Any()) return string.Empty;
                    return $"{actorHtml} updated job {jobLink} for {orderLink}: {string.Join(", ", allChanges)}";
                }

                case "deleted":
                    return $"{actorHtml} deleted job '{d.EntityName}' for {orderLink}";

                default:
                    return $"{actorHtml} {d.Action} job {jobLink} for {orderLink}";
            }
        }

        private static string BuildPartHtml(string actorHtml, ActivityLogData d, IDictionary<string, string>? liveNames)
        {
            string link = GetLink(d.EntityId, d.EntityName, "/parts?partId={id}", liveNames);

            switch (d.Action)
            {
                case "created":
                    return $"{actorHtml} created part {link}";

                case "deleted":
                    return $"{actorHtml} deleted part {Bold(d.EntityName)}";

                case "moved":
                    return $"{actorHtml} moved part {link} from {Bold(d.SecondaryEntityName)} to {Bold(d.SecondaryEntityId)}";

                case "updated":
                {
                    if (d.Changes == null || !d.Changes.Any()) return string.Empty;

                    var formatted = d.Changes.Select(c =>
                    {
                        string oldDisp = string.IsNullOrEmpty(c.OldValue) ? "[empty]" : c.OldValue;
                        string newDisp = string.IsNullOrEmpty(c.NewValue) ? "[empty]" : c.NewValue;
                        if (oldDisp.Length > 100 || newDisp.Length > 100) return c.FieldName;
                        return $"{c.FieldName} from {Bold(oldDisp)} to {Bold(newDisp)}";
                    }).ToList();

                    bool allSimple = formatted.All(f => !f.Contains(" from "));

                    if (formatted.Count == 1 && !allSimple)
                        return $"{actorHtml} changed {formatted[0]} of part {link}";

                    if (allSimple)
                        return $"{actorHtml} updated details of part {link}";

                    return $"{actorHtml} updated part {link}: {string.Join(", ", formatted)}";
                }

                default:
                    return $"{actorHtml} {d.Action} part {link}";
            }
        }

        private static string BuildFolderHtml(string actorHtml, ActivityLogData d, IDictionary<string, string>? liveNames)
        {
            string link = GetLink(d.EntityId, d.EntityName, "/parts?folderId={id}", liveNames);

            switch (d.Action)
            {
                case "created":
                    return $"{actorHtml} created group of parts {link}";

                case "deleted":
                    return $"{actorHtml} deleted group of parts {Bold(d.EntityName)}";

                case "renamed":
                    return $"{actorHtml} renamed group of parts {Bold(d.EntityName)} to {Bold(d.SecondaryEntityName)}";

                case "moved":
                    return $"{actorHtml} moved group of parts {link} from {Bold(d.SecondaryEntityName)} to {Bold(d.SecondaryEntityId)}";

                default:
                    return $"{actorHtml} {d.Action} group of parts {link}";
            }
        }

        private static string BuildWorkshopHtml(string actorHtml, ActivityLogData d)
        {
            string name = Bold(d.EntityName); 

            switch (d.Action)
            {
                case "updated":
                {
                    if (d.Changes == null || !d.Changes.Any()) return $"{actorHtml} updated workshop details";
                    var formatted = d.Changes.Select(c => $"{c.FieldName} from {Bold(c.OldValue)} to {Bold(c.NewValue)}");
                    return $"{actorHtml} updated workshop details: {string.Join(", ", formatted)}";
                }
                default:
                    return $"{actorHtml} {d.Action} workshop {name}";
            }
        }
    }
}
