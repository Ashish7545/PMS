using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using PMSAPI.Models.Models;

namespace PMSAPI.Models.ViewModels
{
    public class ProjectVM
    {
        public int Id { get; set; }
        public string ProjectName { get; set; }
        public string ProjectDetail { get; set; }
        public DateTime DeadlineDate { get; set; } = DateTime.Now;
        public string ProjectManagerName { get; set; }

        public int? NoOfEmployee { get; set; }

     
        //public Project Projects { get; set; }
        //[ValidateNever]
        //public IEnumerable<SelectListItem> EmployeeList { get; set; }
    }
}
