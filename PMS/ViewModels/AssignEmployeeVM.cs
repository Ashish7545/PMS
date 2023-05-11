using Microsoft.AspNetCore.Mvc.Rendering;
using PMS.Models;

namespace PMS.ViewModels
{
    public class AssignEmployeeVM
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public List<string>? Developers { get; set; }

        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeType { get; set; }

    }
}
