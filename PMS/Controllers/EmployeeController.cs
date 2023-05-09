﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml;
using PMS.Data;
using PMS.Models;
using PMS.Pagging;

namespace PMS.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly AppDbContext _db;

        public EmployeeController(AppDbContext context)
        {

            _db = context;
        }
        public IActionResult Index(string? searchString, string sortOrder, int pg = 1)
        {
            var projectDetails = new List<Employee>();
            if (searchString == null)
            {
                projectDetails = _db.Employees.ToList();
            }
            else
            {
                ViewBag.SearchStr = searchString;
                projectDetails = _db.Employees.Where(u => u.Name.ToLower().Contains(searchString.ToLower()) ||
                                                      u.Name.ToLower().Contains(searchString.ToLower())).ToList();
            }

            //Sorting
            ViewData["NameOrder"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            switch (sortOrder)
            {
                case "name_desc":
                    projectDetails = projectDetails.OrderByDescending(a => a.Name).ToList();
                    break;
                default:
                    projectDetails = projectDetails.OrderBy(a => a.Name).ToList();
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


        // Add or Edit Employee details
        public IActionResult Upsert(int? id)
        {
            Employee employee = new Employee();
            if (id == null || id == 0)
            {
                //create Employee
                ViewBag.EmpType = new SelectList(Enum.GetValues(typeof(EmployeeType)));
                return View(employee);
            }
            else
            {
                //update Employee
                ViewBag.EmpType = new SelectList(Enum.GetValues(typeof(EmployeeType)));
                employee = _db.Employees.FirstOrDefault(u => u.Id == id);
                return View(employee);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken] //prevents from cross-site forgery attack
        public IActionResult Upsert(Employee obj)
        {
            if (obj.Id == 0)
            {
                _db.Employees.Add(obj);
                TempData["success"] = "Product Added successfully";
            }
            else
            {
                _db.Employees.Update(obj);
                TempData["success"] = "Product Updated successfully";
            }
            _db.SaveChanges();


            return RedirectToAction("Index");
        }

        // Delete Products
        #region API CALLS
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _db.Employees.FirstOrDefault(u => u.Id == id);

            _db.Employees.Remove(obj);
            _db.SaveChanges();

            return Json(new { success = true });
        }
        #endregion

        public async Task<IActionResult> ExportExcel(IFormFileCollection form)
        {
            List<Employee> emp = new List<Employee>();



            // Change filepath Accondingly
            var filePath = "C:\\Users\\AshisH\\Desktop\\project.xlsx";
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var rowcount = worksheet.Dimension.Rows;
                var columncount = worksheet.Dimension.Columns;
                for (int row = 2; row <= rowcount; row++)
                {
                    emp.Add(new Employee
                    {
                        Name = worksheet.Cells[row, 1].Value.ToString(),
                        EmployeeCode = worksheet.Cells[row, 2].Value.ToString(),
                        JoiningDate = DateTime.Now,
                        EmployeeType = (EmployeeType)int.Parse(worksheet.Cells[row, 3].Value.ToString()),
                    });
                }
            }
            _db.Employees.AddRange(emp);
            _db.SaveChanges();
            TempData["success"] = "Employee Details Added Successfully.";
            return RedirectToAction("Index");
        }
    }
}