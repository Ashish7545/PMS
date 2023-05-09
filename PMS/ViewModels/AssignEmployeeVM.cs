using Microsoft.AspNetCore.Mvc.Rendering;
using PMS.Models;

namespace PMS.ViewModels
{
    public class AssignEmployeeVM
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        //public List<string> Developers { get; set; }
        public List<string> ProjectManager { get; set; }
    }
}
