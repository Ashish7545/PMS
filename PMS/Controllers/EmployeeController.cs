using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml;
using PMS.Data;
using PMS.Models;
using PMS.Pagging;
using System.Data;
using System.Reflection;

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
                projectDetails = _db.Employees.Where(u => u.Name.ToLower().Contains(searchString.ToLower())).ToList();
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
                TempData["success"] = "Employee Added successfully";
            }
            else
            {
                var empAssigned = _db.ProjectEmployees.FirstOrDefault(u => u.EmployeeId== obj.Id);
                if(empAssigned != null)
                {
                    ViewBag.EmpType = new SelectList(Enum.GetValues(typeof(EmployeeType)));
                    TempData["error"] = "Assigned User can not be Modified!";
                    return View(obj);
                }
                else
                {
                    _db.Employees.Update(obj);
                    TempData["success"] = "Employee details updated successfully";
                }
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
            var obj1 = _db.Projects.Where(u => u.EmployeeID == id).FirstOrDefault();
            if(obj1 != null)
            {
                TempData["error"] = "Employee is Assigned As Project Manager. Can't be deleted!";
                return RedirectToAction("Index");
            }
            else
            {
                _db.Employees.Remove(obj);
                _db.SaveChanges();
            }
            return Json(new { success = true });
        }
        #endregion

        //Import Data to Excel
        public async Task<IActionResult> ImportExcel(IFormFileCollection form)
        {
            List<Employee> emp = new List<Employee>();

            // Change filepath Accondingly
            var filePath = "C:\\Users\\Ashish.Kumar\\Downloads\\Report\\project.xlsx";
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

        //Export data to excel
        public IActionResult ExportExcel()
        {
            try
            {
                var data = _db.Employees.ToList();
                if (data != null && data.Count > 0)
                {
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        wb.Worksheets.Add(ToConvertDataTable(data.ToList()));
                        using (MemoryStream ms = new MemoryStream())
                        {
                            wb.SaveAs(ms);
                            string fileName = $"Employees_{DateTime.Now.ToString("dd/MM/yyyy")}.xlsx";
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
            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in properties)
            {
                dt.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[properties.Length];
                for (int i = 0; i < properties.Length; i++)
                {
                    values[i] = properties[i].GetValue(item, null);
                }
                dt.Rows.Add(values);
            }
            return dt;
        }
    }
}
