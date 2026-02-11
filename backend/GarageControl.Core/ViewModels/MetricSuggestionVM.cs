namespace GarageControl.Core.ViewModels
{
    public class MetricSuggestionVM
    {
        public string Name { get; set; } = null!;
        public int Count { get; set; }
        public bool IsExisting { get; set; }
    }
}
