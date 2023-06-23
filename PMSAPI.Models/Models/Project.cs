
namespace PMSAPI.Models.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string ProjectName { get; set; }
        public string ProjectDetail { get; set; }
        public DateTime DeadlineDate { get; set; } = DateTime.Now;
        
        public int? EmployeeID { get; set; }
        public Employee Employee { get; set; }
        public ICollection<ProjectEmployee> ProjectEmployees { get; set; }

        
    }
}
