namespace PMSAPI.Models.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string EmployeeCode { get; set; }
        public DateTime JoiningDate { get; set; } = DateTime.Now;
        public EmployeeType EmployeeType { get; set; }
        public ICollection<ProjectEmployee> ProjectEmployees { get; set; }
    }
}
