namespace ProjectEstimationApp.Models
{
    public class ProjectData
    {
        public List<Resource> Resources { get; set; } = new List<Resource>();
        public string? ProjectStartDate { get; set; }
        public string? ProjectEndDate { get; set; }
        public List<AdditionalCost> AdditionalCosts { get; set; } = new List<AdditionalCost>();
    }

    public class Resource
    {
        public string Name { get; set; } = string.Empty; 
        public float Cost { get; set; }
        public int NumberOfResources { get; set; }
        public float Total => Cost * NumberOfResources;
    }

    public class AdditionalCost
    {
        public string Name { get; set; } = string.Empty;
        public float Cost { get; set; }
        public int NumberOfResources { get; set; }
        public float Total => Cost * NumberOfResources;
    }
}