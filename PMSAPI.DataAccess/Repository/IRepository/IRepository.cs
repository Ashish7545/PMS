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
        Task<Project> GetProjectByEId(int employeeId);

        Task<Project> AddProject(Project projectDetails);
        Task UpdateProject(Project project);

        Task DeleteProject(Project obj);


        // Employee

        public IEnumerable<Employee> GetAllEmployee();
        Task<Employee> GetEmployeeById(int employeeId);
        Task<Employee> GetDeveloper(int employeeId);
        Task<Employee> AddEmployee(Employee employee);
        Task UpdateEmployee(Employee employee);
        public IEnumerable<AssignEmployeeVM> ListOfEmployee(int id);
        Task DeleteEmployees(Employee obj);


        // Project_Employee

        Task<ProjectEmployee> GetDetailsByEId(int employeeId);
        Task<ProjectEmployee> GetDetailsByPId(int projectId);
        Task<ProjectEmployee> GetProjectEmployee(int employeeId, int projectId);
        Task<ProjectEmployee> AddProjectEmployee(ProjectEmployee proEmpDetails);
        Task UnAssignedEmployee(ProjectEmployee obj);

    }
}
