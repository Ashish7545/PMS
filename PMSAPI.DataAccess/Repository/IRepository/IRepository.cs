using PMSAPI.Models.Models;
using PMSAPI.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMSAPI.DataAccess.Repository.IRepository
{
    public interface IRepository
    {
        // Projects

        public IEnumerable<ProjectVM> GetAllProject();

        Task<Project> GetProjectById(int projectId);

        Task<Project> AddProject(Project projectDetails);
        Task UpdateProject(Project project);


        // Employee

        Task <Employee> GetEmployeeById(int employeeId);

    }
}
