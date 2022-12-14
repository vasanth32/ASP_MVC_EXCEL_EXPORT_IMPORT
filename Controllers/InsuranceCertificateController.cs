using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2013.Excel;
using Excel_MVC.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Excel_MVC.Controllers
{
    public class InsuranceCertificateController : Controller
    {
        //SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DBCS"].ConnectionString);

        //OleDbConnection Econ;
        InsuranceSampleEntities db = new InsuranceSampleEntities();
        // GET: InsuranceCertificate  
        public ActionResult Index()
        {
            var insuranceCertificate = db.InsuranceCertificates.ToList();
            return View(insuranceCertificate);
        }
        public ActionResult Done()
        {            
            return View();
        }

        [HttpPost]
        public FileResult ExportToExcel()
        {
            DataTable dt = new DataTable("Grid");
            dt.Columns.AddRange(new DataColumn[11] { new DataColumn("SrNo"),
                                                     new DataColumn("Title"),
                                                     new DataColumn("FirstName"),
                                                     new DataColumn("LastName"),
                                                     new DataColumn("DateOfBirth"),
                                                     new DataColumn("Age"),
                                                     new DataColumn("Gender"),
                                                     new DataColumn("MaritalStatus"),
                                                     new DataColumn("EmployeeNumber"),
                                                     new DataColumn("NomineeName"),
                                                     new DataColumn("NomineeRelationship")});

            var insuranceCertificate = from InsuranceCertificate in db.InsuranceCertificates select InsuranceCertificate;

            foreach (var insurance in insuranceCertificate)
            {
                dt.Rows.Add(insurance.SrNo, insurance.Title, insurance.FirstName, insurance.LastName,
                    insurance.DateOfBirth, insurance.Age, insurance.Gender, insurance.MaritalStatus,
                    insurance.EmployeeNumber, insurance.NomineeName, insurance.NomineeRelationship);
            }

            using (XLWorkbook wb = new XLWorkbook()) //Install ClosedXml from Nuget for XLWorkbook  
            {
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream()) //using System.IO;  
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ExcelFile.xlsx");
                }
            }
        }

        public ActionResult Upload(FormCollection formCollection)
        {
            var usersList = new List<InsuranceCertificate>();
            if (Request != null)
            {
                HttpPostedFileBase file = Request.Files["UploadedFile"];
                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    string fileName = file.FileName;
                    string fileContentType = file.ContentType;
                    byte[] fileBytes = new byte[file.ContentLength];
                    var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                    using (var package = new ExcelPackage(file.InputStream))
                    {
                        var currentSheet = package.Workbook.Worksheets;
                        var workSheet = currentSheet.First();
                        var noOfCol = workSheet.Dimension.End.Column;
                        var noOfRow = workSheet.Dimension.End.Row;
                        for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                        {
                            var user = new InsuranceCertificate();
                            user.SrNo = Convert.ToInt32(workSheet.Cells[rowIterator, 1].Value);
                            user.Title = workSheet.Cells[rowIterator, 2].Value.ToString();
                            user.FirstName = workSheet.Cells[rowIterator, 3].Value.ToString();
                            user.LastName = workSheet.Cells[rowIterator, 4].Value.ToString();
                            user.DateOfBirth = DateTime.Now;
                            user.Age = 2;
                            user.Gender = workSheet.Cells[rowIterator, 6].Value.ToString();
                            user.MaritalStatus = workSheet.Cells[rowIterator, 7].Value.ToString();
                            user.EmployeeNumber = 322; 
                            user.NomineeName = workSheet.Cells[rowIterator, 9].Value.ToString();
                            user.NomineeRelationship = workSheet.Cells[rowIterator, 10].Value.ToString();
                            
                            usersList.Add(user);
                        }
                    }
                }
            }
            using (InsuranceSampleEntities ex = new InsuranceSampleEntities())
            {
                foreach (var item in usersList)
                {
                    ex.InsuranceCertificates.Add(item);
                }
                ex.SaveChanges();
            }
            return RedirectToAction("Done");
        }



    }
}