using Microsoft.EntityFrameworkCore;
using PMSAPI.DataAccess.Data;
using PMSAPI.Models.Models;
using PMSAPI.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMSAPI.DataAccess.Repository
{
    public class Repository : IRepository.IRepository
    {
        private readonly AppDbContext _db;
        public Repository(AppDbContext db)
        {
            _db = db;
        }

        //Project

        public IEnumerable<ProjectVM> GetAllProject()
        {
            var projects = new List<ProjectVM>();
            projects = (from p in _db.Projects
                        join e in _db.Employees on p.EmployeeID equals e.Id
                        where e.EmployeeType == EmployeeType.ProjectManager
                        select new ProjectVM
                        {
                            Id = p.Id,
                            ProjectName = p.ProjectName,
                            ProjectDetail = p.ProjectDetail,
                            DeadlineDate = p.DeadlineDate.Date,
                            NoOfEmployee = _db.ProjectEmployees.Where(u => u.ProjectId == p.Id).Count(),
                            ProjectManagerName = e.Name
                        }).ToList();
            return projects;
        }

        public async Task<Project> GetProjectById(int projectId)
        {
            return await _db.Projects.FirstOrDefaultAsync(x => x.Id == projectId);
        }
        public async Task<Project> GetProjectByEId(int employeeId)
        {
            return await _db.Projects.FirstOrDefaultAsync(x => x.EmployeeID == employeeId);
        }

        public async Task<Project> AddProject(Project projectDetails)
        {
            _db.Add(projectDetails);
            await _db.SaveChangesAsync();
            return projectDetails;
        }

        public async Task UpdateProject(Project project)
        {
            _db.Update(project);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteProject(Project obj)
        {
            _db.Remove(obj);
            await _db.SaveChangesAsync();
        }



        //Employee

        public IEnumerable<Employee> GetAllEmployee()
        {
            var employeeDetails = _db.Employees.ToList();
            return employeeDetails;
        }

        public async Task<Employee> GetEmployeeById(int employeeId)
        {
            return await _db.Employees.FirstOrDefaultAsync(x => x.Id == employeeId);
        }
        public async Task<Employee> GetDeveloper(int employeeId)
        {
            return await _db.Employees.FirstOrDefaultAsync(x => x.Id == employeeId && x.EmployeeType == EmployeeType.Developer);
        }

        public async Task<Employee> AddEmployee(Employee employee)
        {
            _db.Add(employee);
            await _db.SaveChangesAsync();
            return employee;
        }

        public async Task UpdateEmployee(Employee employee)
        {
            _db.Update(employee);
            await _db.SaveChangesAsync();
        }

        public IEnumerable<AssignEmployeeVM> ListOfEmployee(int id)
        {
            var empDetails = new List<AssignEmployeeVM>();
            empDetails = (from p in _db.Projects
                          join pe in _db.ProjectEmployees on p.Id equals pe.ProjectId
                          join e in _db.Employees on pe.EmployeeId equals e.Id
                          where p.Id == id
                          select new AssignEmployeeVM
                          {
                              ProjectId = pe.ProjectId,
                              EmployeeId = pe.EmployeeId,
                              ProjectName = p.ProjectName,
                              EmployeeName = e.Name,
                              EmployeeCode = e.EmployeeCode,
                              EmployeeType = e.EmployeeType.ToString()
                          }).ToList();

            return empDetails;
        }

        public async Task DeleteEmployees(Employee obj)
        {
            _db.Remove(obj);
            await _db.SaveChangesAsync();
        }


        // Project_Employee

        public async Task<ProjectEmployee> GetDetailsByEId(int employeeId)
        {
            var details = await _db.ProjectEmployees.FirstOrDefaultAsync(x => x.EmployeeId == employeeId);
            return details;
        }
        public async Task<ProjectEmployee> GetDetailsByPId(int projectId)
        {
            var details = await _db.ProjectEmployees.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            return details;
        }

        public async Task<ProjectEmployee> GetProjectEmployee(int projectId, int employeeId)
        {
            var details = await _db.ProjectEmployees.Where(u => u.EmployeeId == employeeId && u.ProjectId == projectId).FirstOrDefaultAsync();
            return details;
        }


        public async Task<ProjectEmployee> AddProjectEmployee(ProjectEmployee projectEmployee)
        {
            _db.Add(projectEmployee);
            await _db.SaveChangesAsync();
            return projectEmployee;
        }

        public async Task UnAssignedEmployee(ProjectEmployee obj)
        {
            _db.Remove(obj);
            await _db.SaveChangesAsync();
        }
    }
}
