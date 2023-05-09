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
            var projectDetails = new List<Project>();
            if (searchString == null)
            {
                projectDetails = _db.Projects.ToList();
            }
            else
            {
                ViewBag.SearchStr = searchString;
                projectDetails = _db.Projects.Where(u => u.ProjectName.ToLower().Contains(searchString.ToLower()) ||
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
            Project project = new Project();
            if (id == null || id == 0)
            {
                //create product
                return View(project);
            }
            else
            {
                //update product
                project = _db.Projects.FirstOrDefault(u => u.Id == id);
                return View(project);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken] //prevents from cross-site forgery attack
        public IActionResult Upsert(Project obj)
        {
            if (obj.Id == 0)
            {
                _db.Projects.Add(obj);
                TempData["success"] = "Product Added successfully";
            }
            else
            {
                _db.Projects.Update(obj);
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
            //ViewBag.Developers = _db.Employees.Where(u => u.EmployeeType == EmployeeType.Developer)
            //                              .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.Name })
            //                              .ToList();

            ViewBag.ProjectManager = _db.Employees.Where(u => u.EmployeeType == EmployeeType.ProjectManager)
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
        public async Task<IActionResult> AssignEmployees(AssignEmployeeVM model)
        {
            return View(model);
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

        public IActionResult ExportExcel(IFormFileCollection form)
        {
            List<Project> project = new List<Project>();

        // Change filepath Accondingly
        var filePath = "C:\\Users\\AshisH\\Desktop\\project.xlsx";
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[1];
                var rowcount = worksheet.Dimension.Rows;
                var columncount = worksheet.Dimension.Columns;
                for (int row = 2; row <= rowcount; row++)
                {
                    project.Add(new Project
                    {
                        ProjectName = worksheet.Cells[row, 1].Value.ToString(),
                        ProjectDetail = worksheet.Cells[row, 2].Value.ToString(),
                        DeadlineDate = DateTime.Now,
                        ProjectManagerName = null
                    });
                }
            }
            _db.Projects.AddRange(project);
            _db.SaveChanges();
            TempData["success"] = "Project Details Added Successfully.";
            return RedirectToAction("Index");

        }
    }
}
