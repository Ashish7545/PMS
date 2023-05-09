namespace PMS.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string ProjectName { get; set; }
        public string ProjectDetail { get; set; }
        public DateTime DeadlineDate { get; set; } = DateTime.Now;
        public string? ProjectManagerName { get; set; }
        public ICollection<ProjectEmployee> ProjectEmployees { get; set; }
    }
}
