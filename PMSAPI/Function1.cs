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
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PMSAPI
{

    public class Function1
    {
        private readonly IRepository _repository;
        public Function1(IRepository repository)
        {
            _repository = repository;
        }

        //Project

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
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "UpdateProjects/{projectId}")] HttpRequest req,
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


        [FunctionName("AssignEmployees")]
        public async Task<IActionResult> AssignEmployees(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "AssignEmployees/{projectId}")] HttpRequest req,
            ILogger log, [FromRoute] int projectId)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var assignEmp = JsonConvert.DeserializeObject<AssignEmployeeVM>(requestBody);

            var projectExists = await _repository.GetProjectById(projectId);
            if(projectExists == null)
            {
                return new NotFoundResult();
            }
            if (assignEmp.Developers == null)
            {
                return new BadRequestObjectResult("Select Employee!");
            }
            else
            {
                foreach (var details in assignEmp.Developers)
                {

                    // Get the employee ID from the database
                    var employee = await _repository.GetDeveloper(details.EmployeeId);
                    var projectEmployee = new ProjectEmployee
                    {
                        EmployeeId = employee.Id,
                        ProjectId = projectExists.Id
                    };
                    var EmpId = await _repository.GetProjectEmployee(projectExists.Id, employee.Id);
                    if (EmpId != null)
                    {
                        return new BadRequestObjectResult("Employee Already Assigned to this project!");
                    }
                    else
                    {
                        await _repository.AddProjectEmployee(projectEmployee);
                        
                    }
                }
                return new OkObjectResult("Employee Assigned Successfully.");
            }
        }


        [FunctionName("DeleteProject")]
        public async Task<IActionResult> DeleteProject(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "DeleteProject/{projectId}")] HttpRequest req,
            ILogger log, [FromRoute] int projectId)
        {
            var obj = await _repository.GetProjectById(projectId);
            var obj1 = await _repository.GetDetailsByPId(projectId);
            if (obj1 != null)
            {
                return new BadRequestObjectResult("First Unassigned Employee, Then Delete the project!");
            }
            else
            {
                await _repository.DeleteProject(obj);
                return new OkObjectResult("Project Deleted Successfully!");
            }
        }


        // Employee

        [FunctionName("GetAllEmployees")]
        public async Task<IActionResult> GetAllEmployees(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "GetAllEmployees")] HttpRequest req,
            ILogger log)
        {
            var employee = _repository.GetAllEmployee();
            return new OkObjectResult(employee);
        }


        [FunctionName("AddEmployeee")]
        public async Task<IActionResult> AddEmployee(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route ="AddEmployee")] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var employeeDetails = JsonConvert.DeserializeObject<Employee>(requestBody);

            await _repository.AddEmployee(employeeDetails);
            return new OkObjectResult("Employee Added Successfully!");
        }


        [FunctionName("UpdateEmployee")]
        public async Task<IActionResult> UpdateEmployee(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "UpdateEmployee/{employeeId}")] HttpRequest req,
            ILogger log, [FromRoute] int employeeId)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var employeeDetails = JsonConvert.DeserializeObject<Employee>(requestBody);

            var employee = await _repository.GetEmployeeById(employeeId);
            if (employee == null)
            {
                return new NotFoundResult();
            }
            else
            {
                //Project_Employee table Details
                var empAssigned = await _repository.GetDetailsByEId(employeeId);
                if (empAssigned != null)
                {
                    return new BadRequestObjectResult("Assigned Employee Can't be modified!");
                }
                else
                {
                    employee.Id = employeeDetails.Id;
                    employee.Name = employeeDetails.Name;
                    employee.EmployeeCode= employeeDetails.EmployeeCode;
                    employee.JoiningDate= employeeDetails.JoiningDate;
                    employee.EmployeeType = employeeDetails.EmployeeType;
                    await _repository.UpdateEmployee(employee);
                    return new OkObjectResult("Employee Details Updated Successfully!");
                }
            }
            
        }

        [FunctionName("ListOfEmployee")]
        public async Task<IActionResult> ListOfEmployee(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "ListOfEmployee/{projectId}")] HttpRequest req, 
            ILogger log, [FromRoute] int projectId)
        {
            var employeeList = _repository.ListOfEmployee(projectId);
            return new OkObjectResult(employeeList);
        }


        [FunctionName("DeleteEmployees")]
        public async Task<IActionResult> DeleteEmployees(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "DeleteEmployees/{employeeId}")] HttpRequest req,
            ILogger log, [FromRoute] int employeeId)
        {
            var obj = await _repository.GetEmployeeById(employeeId);
            var obj1 = await _repository.GetProjectByEId(employeeId);
            if (obj1 != null)
            {
                return new BadRequestObjectResult("Employee is Assigned As Project Manager. Can't be deleted!");
            }
            else
            {
                await _repository.DeleteEmployees(obj);
                return new OkObjectResult("Employee Deleted Successfully!");
            }
        }

        // Project_Employee

        [FunctionName("UnAssignedEmployees")]
        public async Task<IActionResult> UnAssignedEmployees(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "UnAssignedEmployees/{projectId}/{employeeId}")] HttpRequest req,
            ILogger log, [FromRoute] int projectId, [FromRoute] int employeeId)
        {
            var obj = await _repository.GetProjectEmployee(projectId, employeeId);
            if (obj == null)
            {
                return new BadRequestObjectResult("Details doesn't exists!");
            }
            else
            {
                await _repository.UnAssignedEmployee(obj);
                return new OkObjectResult("Employee UnAssigned Successfully!");
            }
        }
    }
}
