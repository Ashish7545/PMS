
using PMSAPI.Models.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMSAPI.Models.ViewModels
{
    public class AssignEmployeeVM
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public List<ProjectEmployee>? Developers { get; set; }

        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeType { get; set; }

    }
}
