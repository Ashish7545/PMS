using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using PMS.Data;
using PMS.Models;
using PMS.Pagging;
using PMS.ViewModels;
using System.ComponentModel;
using System.Composition;
using System.Data;
using System.Reflection;
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
                projectDetails = projectDetails.Where(u => u.ProjectName.ToLower().Contains(searchString.ToLower())).ToList();
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
            ViewBag.ProjectName = project.ProjectName;

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
                        var EmpId = _db.ProjectEmployees.Where(u => u.EmployeeId == employee.Id && u.ProjectId == project.Id).FirstOrDefault();
                        if(EmpId != null)
                        {
                            TempData["error"] = "Employee Already Assigned to this project!";
                            ViewBag.Developers = _db.Employees.Where(u => u.EmployeeType == EmployeeType.Developer)
                                          .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.Name })
                                          .ToList();
                            return View(model);
                        }
                        else
                        {
                            _db.ProjectEmployees.Add(projectEmployee);
                        }
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

    


        public IActionResult ListOfEmployee(int? id, string? searchString, int pg = 1)
        {
            var empDetails = new List<AssignEmployeeVM>();
            empDetails = (from p in _db.Projects
                          join pe in _db.ProjectEmployees on p.Id equals pe.ProjectId
                          join e in _db.Employees on pe.EmployeeId equals e.Id
                          where p.Id == id
                              select new AssignEmployeeVM
                              {
                                  ProjectId = pe.ProjectId,
                                  EmployeeId= pe.EmployeeId,
                                  ProjectName = p.ProjectName,
                                  EmployeeName = e.Name,
                                  EmployeeCode = e.EmployeeCode,
                                  EmployeeType = e.EmployeeType.ToString()
                              }).ToList();
            
            ViewBag.ProjectName = _db.Projects.First(u => u.Id == id).ProjectName;

            if (searchString == null)
            {
                empDetails = empDetails;
            }
            else
            {
                ViewBag.SearchStr = searchString;
                empDetails = empDetails.Where(u => u.EmployeeName.ToLower().Contains(searchString.ToLower())).ToList();
            }
            //Paging
            const int pageSize = 3;
            if (pg < 1)
                pg = 1;
            int recsCount = empDetails.Count;
            var pager = new Pager(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = empDetails.Skip(recSkip).Take(pager.PageSize).ToList();
            ViewBag.Pager = pager;

            return View(data);
        }

        // Delete Products
        #region API CALLS
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _db.Projects.FirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            _db.Projects.Remove(obj);
            _db.SaveChanges();

            return Json(new { success = true });
        }



        [HttpDelete]
        public IActionResult UnAssignEmployee(int id, int eId)
        {
            var obj = _db.ProjectEmployees.Where(u => u.ProjectId == id && u.EmployeeId == eId).ToList();
            if(obj != null)
            {
                var obj1 = _db.ProjectEmployees.FirstOrDefault(u => u.ProjectId == id);
                _db.ProjectEmployees.Remove(obj1);
            }
            else
            {
                return NotFound();
            }
            _db.SaveChanges();

            return Json(new { success = true });
        }
        #endregion

        public IActionResult ImportExcel(IFormFileCollection form)
        {
            List<Project> project = new List<Project>();

        // Change filepath Accondingly
      
        var filePath = "C:\\Users\\Ashish.Kumar\\Downloads\\Report\\project.xlsx";
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
                        EmployeeID = int.Parse(worksheet.Cells[row, 3].Value.ToString())
                    });
                }
            }
            _db.Projects.AddRange(project);
            _db.SaveChanges();
            TempData["success"] = "Project Details Added Successfully.";
            return RedirectToAction("Index");

        }

        public IActionResult ExportExcel()
        {
            try
            {
                var data = _db.Projects.ToList();
                if(data != null && data.Count > 0)
                {
                    using(XLWorkbook wb = new XLWorkbook())
                    {
                        wb.Worksheets.Add(ToConvertDataTable(data.ToList()));
                        using(MemoryStream ms = new MemoryStream())
                        {
                            wb.SaveAs(ms);
                            string fileName = $"Project_{DateTime.Now.ToString("dd/MM/yyyy")}.xlsx";
                            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocuments.spreadsheetml.sheet", fileName);
                        }
                    }
                }
                TempData["error"] = "Data not Found!";
            }
            catch (Exception ex) 
            { }

            return RedirectToAction("Index");
        }


        public DataTable ToConvertDataTable<T>(List<T> items)
        {
            DataTable dt = new DataTable(typeof(T).Name);
            PropertyInfo[] properties= typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach(PropertyInfo prop in properties)
            {
                dt.Columns.Add(prop.Name);
            }
            foreach(T item in items)
            {
                var values = new object[properties.Length];
                for(int i = 0; i< properties.Length; i++)
                {
                    values[i] = properties[i].GetValue(item, null); 
                }
                dt.Rows.Add(values);
            }
            return dt;
        }
    }
}
