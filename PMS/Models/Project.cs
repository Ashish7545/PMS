﻿using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.Models
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
