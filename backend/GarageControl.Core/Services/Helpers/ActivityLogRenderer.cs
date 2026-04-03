using GarageControl.Core.Models;

namespace GarageControl.Core.Services.Helpers
{
    /// <summary>
    /// Converts a structured <see cref="ActivityLogData"/> payload into the same
    /// HTML strings that were previously hand-crafted at each call site.
    /// </summary>
    public static class ActivityLogRenderer
    {
        public static string BuildMessageHtml(string actorHtml, string logType, ActivityLogData data)
        {
            return logType switch
            {
                "Worker"   => BuildWorkerHtml(actorHtml, data),
                "Client"   => BuildClientHtml(actorHtml, data),
                "Vehicle"  => BuildVehicleHtml(actorHtml, data),
                "Make"     => BuildMakeHtml(actorHtml, data),
                "Model"    => BuildModelHtml(actorHtml, data),
                "JobType"  => BuildJobTypeHtml(actorHtml, data),
                "Order"    => BuildOrderHtml(actorHtml, data),
                "Job"      => BuildJobHtml(actorHtml, data),
                "Part"     => BuildPartHtml(actorHtml, data),
                "Folder"   => BuildFolderHtml(actorHtml, data),
                _          => $"{actorHtml} performed action on {logType}"
            };
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static string Bold(string? value) => $"<b>{value}</b>";

        private static string FormatChanges(List<ActivityPropertyChange>? changes)
        {
            if (changes == null || changes.Count == 0) return string.Empty;

            var formatted = changes.Select(c =>
            {
                string oldDisp = string.IsNullOrEmpty(c.OldValue) ? "[empty]" : c.OldValue;
                string newDisp = string.IsNullOrEmpty(c.NewValue) ? "[empty]" : c.NewValue;

                if (oldDisp.Length > 100 || newDisp.Length > 100)
                    return c.FieldName;

                return $"{c.FieldName} from {Bold(oldDisp)} to {Bold(newDisp)}";
            }).ToList();

            return string.Join(", ", formatted);
        }

        /// <summary>
        /// Applies the single-change / multi-field / detailed-update condensing logic
        /// that was repeated in each service.
        /// </summary>
        private static string BuildUpdatedHtml(
            string actorHtml,
            string entityLabel,
            string entityLink,
            List<ActivityPropertyChange>? changes)
        {
            if (changes == null || changes.Count == 0)
                return $"{actorHtml} updated {entityLabel} {entityLink}";

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
                return $"{actorHtml} changed {formatted[0]} of {entityLabel} {entityLink}";

            if (allSimple)
                return $"{actorHtml} updated details of {entityLabel} {entityLink}";

            return $"{actorHtml} updated {entityLabel} {entityLink}: {string.Join(", ", formatted)}";
        }

        // ── Per-entity builders ────────────────────────────────────────────────

        private static string BuildWorkerHtml(string actorHtml, ActivityLogData d)
        {
            switch (d.Action)
            {
                case "created":
                    return $"{actorHtml} created worker {Bold(d.EntityName)}";

                case "fired":
                    return $"{actorHtml} fired {Bold(d.EntityName)}";

                case "updated":
                {
                    string link = d.EntityId != null
                        ? $"<a href='/workers/{d.EntityId}?highlight=true' class='log-link target-link'>{d.EntityName}</a>"
                        : Bold(d.EntityName);

                    // Mix of relation changes (strings without from/to) and field changes
                    var allChanges = d.Changes ?? new List<ActivityPropertyChange>();
                    var allStrings = allChanges.Select(c =>
                    {
                        string oldDisp = string.IsNullOrEmpty(c.OldValue) ? "[empty]" : c.OldValue;
                        string newDisp = string.IsNullOrEmpty(c.NewValue) ? "[empty]" : c.NewValue;

                        // Relation changes are stored with empty NewValue as a flag
                        if (c.NewValue == null)
                            return c.FieldName; // pre-formatted relation string like "added access <b>X</b>"

                        if (oldDisp.Length > 100 || newDisp.Length > 100)
                            return c.FieldName;

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
                    return $"{actorHtml} {d.Action} worker {Bold(d.EntityName)}";
            }
        }

        private static string BuildClientHtml(string actorHtml, ActivityLogData d)
        {
            switch (d.Action)
            {
                case "created":
                    return $"{actorHtml} created client " +
                           $"<a href='/clients/{d.EntityId}?highlight=true' class='log-link target-link'>{d.EntityName}</a>";

                case "deleted":
                    return $"{actorHtml} deleted client {Bold(d.EntityName)}";

                case "updated":
                {
                    string link = $"<a href='/clients/{d.EntityId}?highlight=true' class='log-link target-link'>{d.EntityName}</a>";
                    return BuildUpdatedHtml(actorHtml, "client", link, d.Changes);
                }

                default:
                    return $"{actorHtml} {d.Action} client {Bold(d.EntityName)}";
            }
        }

        private static string BuildVehicleHtml(string actorHtml, ActivityLogData d)
        {
            switch (d.Action)
            {
                case "added":
                {
                    string carLink = $"<a href='/cars/{d.EntityId}?highlight=true' class='log-link target-link'>{d.EntityName}</a>";
                    string clientLink = d.SecondaryEntityId != null
                        ? $"<a href='/clients/{d.SecondaryEntityId}?highlight=true' class='log-link target-link'>{d.SecondaryEntityName}</a>"
                        : Bold(d.SecondaryEntityName);
                    return $"{actorHtml} added car {carLink} to client {clientLink}";
                }

                case "deleted":
                    return $"{actorHtml} deleted car {Bold(d.EntityName)}";

                case "updated":
                {
                    string carLink = $"<a href='/cars/{d.EntityId}?highlight=true' class='log-link target-link'>{d.EntityName}</a>";
                    var changes = d.Changes ?? new List<ActivityPropertyChange>();
                    var formatted = changes.Select(c =>
                    {
                        string oldDisp = string.IsNullOrEmpty(c.OldValue) ? "[empty]" : c.OldValue;
                        string newDisp = string.IsNullOrEmpty(c.NewValue) ? "[empty]" : c.NewValue;
                        return $"{c.FieldName} from {Bold(oldDisp)} to {Bold(newDisp)}";
                    }).ToList();

                    if (formatted.Count == 1)
                        return $"{actorHtml} changed {formatted[0]} of car {carLink}";

                    return $"{actorHtml} updated car {carLink}: {string.Join(", ", formatted)}";
                }

                default:
                    return $"{actorHtml} {d.Action} vehicle {Bold(d.EntityName)}";
            }
        }

        private static string BuildMakeHtml(string actorHtml, ActivityLogData d)
        {
            switch (d.Action)
            {
                case "created":
                    return $"{actorHtml} created custom make " +
                           $"<a href='/makes-and-models/{d.EntityId}?highlight=true' class='log-link target-link'>{d.EntityName}</a>";

                case "deleted":
                    return $"{actorHtml} deleted make {Bold(d.EntityName)}";

                case "renamed":
                    return $"{actorHtml} renamed make {Bold(d.EntityName)} to " +
                           $"<a href='/makes-and-models/{d.EntityId}?highlight=true' class='log-link target-link'>{d.SecondaryEntityName}</a>";

                case "merged":
                    return $"{actorHtml} merged custom make {Bold(d.EntityName)} into " +
                           $"<a href='/makes-and-models/{d.SecondaryEntityId}?highlight=true' class='log-link target-link'>{d.SecondaryEntityName}</a>";

                default:
                    return $"{actorHtml} {d.Action} make {Bold(d.EntityName)}";
            }
        }

        private static string BuildModelHtml(string actorHtml, ActivityLogData d)
        {
            switch (d.Action)
            {
                case "added":
                    return $"{actorHtml} added model " +
                           $"<a href='/makes-and-models/{d.SecondaryEntityId}/model/{d.EntityId}?highlight=true' class='log-link target-link'>{d.EntityName}</a> " +
                           $"to make <a href='/makes-and-models/{d.SecondaryEntityId}?highlight=true' class='log-link target-link'>{d.SecondaryEntityName}</a>";

                case "deleted":
                    return $"{actorHtml} deleted model {Bold(d.EntityName)} from make {Bold(d.SecondaryEntityName)}";

                case "renamed":
                    return $"{actorHtml} renamed model {Bold(d.EntityName)} to " +
                           $"<a href='/makes-and-models/{d.SecondaryEntityId}/model/{d.EntityId}?highlight=true' class='log-link target-link'>{d.SecondaryEntityName}</a> " +
                           $"(Make: <a href='/makes-and-models/{d.SecondaryEntityId}?highlight=true' class='log-link target-link'>{d.SecondaryEntityName}</a>)";

                case "merged":
                    return $"{actorHtml} merged custom model {Bold(d.EntityName)} into " +
                           $"<a href='/makes-and-models/{d.SecondaryEntityId}/model/{d.EntityId}?highlight=true' class='log-link target-link'>{d.SecondaryEntityName}</a>";

                default:
                    return $"{actorHtml} {d.Action} model {Bold(d.EntityName)}";
            }
        }

        private static string BuildJobTypeHtml(string actorHtml, ActivityLogData d)
        {
            string link = d.EntityId != null
                ? $"<a href='/job-types?highlightId={d.EntityId}' class='log-link target-link'>{d.EntityName}</a>"
                : Bold(d.EntityName);

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

        private static string BuildOrderHtml(string actorHtml, ActivityLogData d)
        {
            string orderLink = $"<a href='/orders/{d.EntityId}?highlight=true' class='log-link target-link'>order for {d.EntityName}</a>";

            switch (d.Action)
            {
                case "created":
                    return $"{actorHtml} created {orderLink}";

                case "updated":
                {
                    string html = $"{actorHtml} updated {orderLink}";
                    if (d.Changes != null && d.Changes.Any())
                    {
                        var formatted = d.Changes.Select(c => $"{c.FieldName} from {Bold(c.OldValue)} to {Bold(c.NewValue)}");
                        html += $": {string.Join(", ", formatted)}";
                    }
                    return html;
                }

                default:
                    return $"{actorHtml} {d.Action} {orderLink}";
            }
        }

        private static string BuildJobHtml(string actorHtml, ActivityLogData d)
        {
            // EntityId = jobId, EntityName = jobTypeName, SecondaryEntityId = orderId, SecondaryEntityName = carInfo
            string orderLink = d.SecondaryEntityId != null
                ? $"<a href='/orders/{d.SecondaryEntityId}?highlight=true' class='log-link target-link'>order for {d.SecondaryEntityName}</a>"
                : Bold(d.SecondaryEntityName);

            string jobLink = d.EntityId != null && d.SecondaryEntityId != null
                ? $"<a href='/orders/{d.SecondaryEntityId}?highlightJob={d.EntityId}' class='log-link target-link'>'{d.EntityName}'</a>"
                : Bold(d.EntityName);

            switch (d.Action)
            {
                case "created":
                {
                    string html = $"{actorHtml} created job {jobLink} for {orderLink}";
                    if (d.Changes != null && d.Changes.Any())
                    {
                        var partStrings = d.Changes.Select(c => c.FieldName); // part changes stored in FieldName
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
                                allChanges.Add(c.FieldName); // pre-formatted part change
                            else
                                allChanges.Add($"{c.FieldName} from {Bold(c.OldValue)} to {Bold(c.NewValue)}");
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

        private static string BuildPartHtml(string actorHtml, ActivityLogData d)
        {
            string partLink = d.EntityId != null
                ? $"<a href='/parts?partId={d.EntityId}' class='log-link target-link'>{d.EntityName}</a>"
                : Bold(d.EntityName);

            switch (d.Action)
            {
                case "created":
                    return $"{actorHtml} created part {partLink}";

                case "deleted":
                    return $"{actorHtml} deleted part {Bold(d.EntityName)}";

                case "moved":
                    return $"{actorHtml} moved part {partLink} from {Bold(d.SecondaryEntityName)} to {Bold(d.SecondaryEntityId)}";
                    // SecondaryEntityName = oldParent, SecondaryEntityId = newParent (repurposing field)

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
                        return $"{actorHtml} changed {formatted[0]} of part {partLink}";

                    if (allSimple)
                        return $"{actorHtml} updated details of part {partLink}";

                    return $"{actorHtml} updated part {partLink}: {string.Join(", ", formatted)}";
                }

                default:
                    return $"{actorHtml} {d.Action} part {partLink}";
            }
        }

        private static string BuildFolderHtml(string actorHtml, ActivityLogData d)
        {
            switch (d.Action)
            {
                case "created":
                    return $"{actorHtml} created group of parts {Bold(d.EntityName)}";

                case "deleted":
                    return $"{actorHtml} deleted group of parts {Bold(d.EntityName)}";

                case "renamed":
                    return $"{actorHtml} renamed group of parts {Bold(d.EntityName)} to {Bold(d.SecondaryEntityName)}";

                case "moved":
                    return $"{actorHtml} moved group of parts {Bold(d.EntityName)} from {Bold(d.SecondaryEntityName)} to {Bold(d.SecondaryEntityId)}";
                    // SecondaryEntityName = oldParent, SecondaryEntityId = newParent

                default:
                    return $"{actorHtml} {d.Action} group of parts {Bold(d.EntityName)}";
            }
        }
    }
}
