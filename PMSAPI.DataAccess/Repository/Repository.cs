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


        //Employee

        public async Task<Employee> GetEmployeeById(int employeeId)
        {
            return await _db.Employees.FirstOrDefaultAsync(x => x.Id == employeeId);
        }
    }
}
