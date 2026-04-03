namespace GarageControl.Core.Models
{
    /// <summary>
    /// Structured payload serialised to <c>ActivityLog.LogData</c>.
    /// All string fields are plain text (no HTML); the renderer builds the final HTML.
    /// </summary>
    public record ActivityLogData(
        /// <summary>
        /// Verb describing what happened, e.g. "created", "updated", "deleted",
        /// "renamed", "merged", "fired", "added", "moved".
        /// </summary>
        string Action,

        /// <summary>Primary entity's database ID (nullable for deletions where the entity is gone).</summary>
        string? EntityId,

        /// <summary>Primary entity's display name.</summary>
        string? EntityName,

        /// <summary>Secondary entity's database ID (e.g. the make when logging a model action).</summary>
        string? SecondaryEntityId = null,

        /// <summary>Secondary entity's display name.</summary>
        string? SecondaryEntityName = null,

        /// <summary>Field-level changes, if any.</summary>
        List<ActivityPropertyChange>? Changes = null
    );
}
