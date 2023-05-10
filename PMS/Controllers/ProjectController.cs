using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using PMS.Data;
using PMS.Models;
using PMS.Pagging;
using PMS.ViewModels;
using System.ComponentModel;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace PMS.Controllers
{
    public class ProjectController : Controller
    {
        private readonly AppDbContext _db;

        public ProjectController(AppDbContext context)
        {

            _db = context;
        }
        public IActionResult Index(string? searchString, string sortOrder, int pg = 1)
        {
            var projectDetails = new List<ProjectVM>();
            projectDetails = (from p in _db.Projects
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

           
            if (searchString == null)
            {
                projectDetails = projectDetails;
            }
            else
            {
                ViewBag.SearchStr = searchString;
                projectDetails = projectDetails.Where(u => u.ProjectName.ToLower().Contains(searchString.ToLower()) ||
                                                      u.ProjectName.ToLower().Contains(searchString.ToLower())).ToList();
            }

            //Sorting
            ViewData["NameOrder"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            switch (sortOrder)
            {
                case "name_desc":
                    projectDetails = projectDetails.OrderByDescending(a => a.ProjectName).ToList();
                    break;
                default:
                    projectDetails = projectDetails.OrderBy(a => a.ProjectName).ToList();
                    break;
            }

            //Paging
            const int pageSize = 3;
            if (pg < 1)
                pg = 1;
            int recsCount = projectDetails.Count;
            var pager = new Pager(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = projectDetails.Skip(recSkip).Take(pager.PageSize).ToList();
            ViewBag.Pager = pager;

            return View(data);
        }

        // Add or Edit Projects details
        public IActionResult Upsert(int? id)
        {
            ProjectVM project = new()
            {
                Projects = new(),
                EmployeeList = _db.Employees.Where(u => u.EmployeeType == EmployeeType.ProjectManager).ToList().Select(
                i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString(),
                }),
                
            };
            if (id == null || id == 0)
            {
                //create product
                return View(project);
            }
            else
            {
                //update product
                project.Projects = _db.Projects.FirstOrDefault(u => u.Id == id);
                return View(project);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken] //prevents from cross-site forgery attack
        public IActionResult Upsert(ProjectVM obj)
        {
            if (obj.Id == 0)
            {
                _db.Projects.Add(obj.Projects);
                TempData["success"] = "Product Added successfully";
            }
            else
            {
                _db.Projects.Update(obj.Projects);
                TempData["success"] = "Product Updated successfully";
            }
            _db.SaveChanges();


            return RedirectToAction("Index");
        }


        public async Task<IActionResult> AssignEmployees(int id)
        {
            var project = await _db.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            ViewBag.Developers = _db.Employees.Where(u => u.EmployeeType == EmployeeType.Developer)
                                          .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.Name })
                                          .ToList();

            var viewModel = new AssignEmployeeVM
            {
                ProjectId = project.Id,
                ProjectName = project.ProjectName,
            };
            return View(viewModel);
        }


        [HttpPost]
        public IActionResult AssignEmployees(AssignEmployeeVM model)
        {
            // Get the project from the database
            var project = _db.Projects.Find(model.ProjectId);

            if (project == null)
            {
                return NotFound();
            }

            // Add the selected employee to the project
            if (model.Developers != null)
            {
                foreach (var employeeId in model.Developers)
                {
                    // Get the employee ID from the database
                    var employee = _db.Employees.FirstOrDefault(u => u.Id.ToString() == employeeId);
                    if (employee.EmployeeType == EmployeeType.Developer)
                    {
                        var projectEmployee = new ProjectEmployee
                        {
                            EmployeeId = employee.Id,
                            ProjectId = project.Id
                        };

                        _db.ProjectEmployees.Add(projectEmployee);
                    }
                }
            }
            else
            {
                TempData["error"] = "Select Employee From List!";
                return View(model);
            }
            _db.SaveChanges();

            return RedirectToAction("Index");
        }



        public IActionResult ListOfEmployee(int? id)
        {
            var empDetails = new List<AssignEmployeeVM>();
            empDetails = (from p in _db.Projects
                          join pe in _db.ProjectEmployees on p.Id equals pe.ProjectId
                          join e in _db.Employees on pe.EmployeeId equals e.Id
                          
                              select new AssignEmployeeVM
                              {
                                  ProjectId = p.Id,
                                  ProjectName = p.ProjectName,
                                  DeadlineDate = p.DeadlineDate.Date,
                                  EmployeeName = e.Name,
                                  EmployeeCode = e.EmployeeCode,
                                  EmployeeType = e.EmployeeType.ToString()
                              }).ToList();
            
            return View(empDetails);
        }

        // Delete Products
        #region API CALLS
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _db.Projects.FirstOrDefault(u => u.Id == id);

            _db.Projects.Remove(obj);
            _db.SaveChanges();

            return Json(new { success = true });
        }
        #endregion

        //public IActionResult ExportExcel(IFormFileCollection form)
        //{
        //    List<Project> project = new List<Project>();

        //    // Change filepath Accondingly
        //    var filePath = "C:\\Users\\Ashish.Kumar\\OneDrive - Pacific Global Solutions Ltd\\Desktop\\project.xlsx";
        //    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        //    using (var package = new ExcelPackage(new FileInfo(filePath)))
        //    {
        //        var worksheet = package.Workbook.Worksheets[1];
        //        var rowcount = worksheet.Dimension.Rows;
        //        var columncount = worksheet.Dimension.Columns;
        //        for (int row = 2; row <= rowcount; row++)
        //        {
        //            project.Add(new Project
        //            {
        //                ProjectName = worksheet.Cells[row, 1].Value.ToString(),
        //                ProjectDetail = worksheet.Cells[row, 2].Value.ToString(),
        //                DeadlineDate = DateTime.Now,
        //                ProjectManagerName = null
        //            });
        //        }
        //    }
        //    _db.Projects.AddRange(project);
        //    _db.SaveChanges();
        //    TempData["success"] = "Project Details Added Successfully.";
        //    return RedirectToAction("Index");

        //}
    }
}
