using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PMSAPI.DataAccess.Repository.IRepository;
using PMSAPI.Models.Models;
using PMSAPI.Models.ViewModels;

namespace PMSAPI
{

    public class Function1
    {
        private readonly IRepository _repository;
        public Function1(IRepository repository)
        {
            _repository = repository;
        }

        [FunctionName("GetAllProject")]
        public async Task<IActionResult> GetAllProject(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "GetAllProject")] HttpRequest req,
            ILogger log)
        {
            var projects = _repository.GetAllProject();
            return new OkObjectResult(projects);
        }

        [FunctionName("AddProjects")]
        public async Task<IActionResult> AddProjects(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "AddProjects")] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var projectDetails = JsonConvert.DeserializeObject<Project>(requestBody);

            await _repository.AddProject(projectDetails);
            return new OkObjectResult("Project Added Successfully!");
        }

        [FunctionName("UpdateProjects")]
        public async Task<IActionResult> UpdateProjects(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "AddProjects/{projectId}")] HttpRequest req,
            ILogger log, [FromRoute] int projectId)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var projectDetails = JsonConvert.DeserializeObject<Project>(requestBody);

            var project = await _repository.GetProjectById(projectId);
            if (project == null)
            {
                return new NotFoundResult();
            }

            project.Id = projectDetails.Id;
            project.ProjectName = projectDetails.ProjectName;
            project.ProjectDetail = projectDetails.ProjectDetail;
            project.DeadlineDate = projectDetails.DeadlineDate;
            project.EmployeeID = projectDetails.EmployeeID;

            await _repository.UpdateProject(project);
            return new OkObjectResult("Project Details Updated Successfully!");
        }

        //[FunctionName("AssignEmployees")]
        //public async Task<IActionResult> AssignEmployees(
        //    [HttpTrigger(AuthorizationLevel.Function, "post", Route = "AssignEmployees/{empId}")] HttpRequest req,
        //    ILogger log, [FromRoute] int empId)
        //{
        //    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        //    var assignEmp = JsonConvert.DeserializeObject<AssignEmployeeVM>(requestBody);
        //}
    }
}
