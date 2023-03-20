using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FastReport.Data;
using FastReport.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using RiponR51.Data;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Linq;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;

namespace RiponR51.Controllers
{
    public class DoctorPatients : Controller
    {

        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _configuration;
        private ApplicationDbContext RR;
        private IHostingEnvironment HostEnvironment;
        public DoctorPatients(SignInManager<IdentityUser> signInManager, ApplicationDbContext _RR,IHostingEnvironment _HostEnvironment, IConfiguration configuration)
        {
            _signInManager = signInManager;
            RR = _RR;
            HostEnvironment = _HostEnvironment;
            _configuration = configuration;
        }



        [Route("myroute")]
        public IActionResult Index()
        {
            if (_signInManager.IsSignedIn(User)) //verify if it's logged
            {
                ViewBag.code = GenerateCode();
                ViewBag.code2 = GenerateCode2();
                return View();
            }
            else
                return LocalRedirect("~/");

        }

        public string GenerateCode()
        {
            string a1 = "";
            string b1 = "";

            try
            {
                var a = (from det in RR.Doctors select det.Doctor_Code.Substring(3)).Max();
                if (a == null)
                    a = "0";
                int b = int.Parse(a.ToString()) + 1;
                if (b < 10)
                {
                    b1 = "000" + b.ToString();
                }
                else if (b < 100)
                {
                    b1 = "00" + b.ToString();
                }
                else if (b < 1000)
                {
                    b1 = "0" + b.ToString();
                }
                else
                {
                    b1 = b.ToString();
                }
                a1 = "AC-" + b1.ToString();
            }
            catch (Exception ex)
            {
                a1 = "AC-0001";
            }
            return a1;
        }

        public string GenerateCode2()
        {
            string a1 = "";
            string b1 = "";

            try
            {
                var a = (from det in RR.Patients select det.Patient_Id.Substring(2)).Max();
                if (a == null)
                    a = "0";
                int b = int.Parse(a.ToString()) + 1;
                if (b < 10)
                {
                    b1 = "000" + b.ToString();
                }
                else if (b < 100)
                {
                    b1 = "00" + b.ToString();
                }
                else if (b < 1000)
                {
                    b1 = "0" + b.ToString();
                }
                else
                {
                    b1 = b.ToString();
                }
                a1 = "S-" + b1.ToString();
            }
            catch (Exception ex)
            {
                a1 = "S-0001";
            }
            return a1;
        }

        public record Doctors2
        {
            public string Doctor_Code { get; set; }
            public string DoctorName { get; set; }
            public string Gender { get; set; }
            public string Address { get; set; }
            public int Tel { get; set; }
            public string Designation { get; set; }
            public string Image { get; set; }
            public Patient[] Patients { get; set; }
        }
        [HttpPost]
        public JsonResult AddDoctorPatients([FromBody] Doctors2 JigJag)
        {
            using var transaction = RR.Database.BeginTransaction();
            try
            {
                Doctor m = new Doctor() { Doctor_Code = JigJag.Doctor_Code, DoctorName = JigJag.DoctorName, Gender = JigJag.Gender, Address = JigJag.Address, Tel=JigJag.Tel, Designation=JigJag.Designation, Image = JigJag.Image };
                RR.Doctors.Add(m);

                foreach (var c in JigJag.Patients)
                {
                    Patient d = new Patient() { Patient_Id = c.Patient_Id, PatientName = c.PatientName, Gender = c.Gender, Number = c.Number, Date = c.Date, Active = c.Active, Doctor_Code = JigJag.Doctor_Code };
                    RR.Patients.Add(d);
                }
                RR.SaveChanges();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return Json("Error");
                // Other steps for handling failures
            }
            return Json(JigJag);
        }




        public JsonResult GetDoctor(string doctor_code)
        {
            var d = (from dc in RR.Doctors where dc.Doctor_Code == doctor_code select new { dc.Doctor_Code, dc.DoctorName, dc.Gender, dc.Address, dc.Tel, dc.Designation, dc.Image });
            return Json(d);
        }


        public JsonResult GetPatient(string doctor_code)
        {
            var p = (from dc in RR.Patients where dc.Doctor_Code == doctor_code select new { dc.Patient_Id, dc.PatientName, dc.Gender, dc.Number, dc.Date, dc.Active });
            return Json(p);
        }

        public JsonResult InsertDoctor(Doctor dd)
        {
            Doctor d = new Doctor() { Doctor_Code = dd.Doctor_Code, DoctorName = dd.DoctorName, Gender = dd.Gender, Address = dd.Address, Tel = dd.Tel, Designation = dd.Designation, Image = dd.Image };
            RR.Doctors.Add(d);
            RR.SaveChanges();
            return Json(d);
        }


        public JsonResult InsertPatient(Patient pp)
        {
            Patient p = new Patient() { Patient_Id = pp.Patient_Id, PatientName = pp.PatientName, Gender = pp.Gender, Number = pp.Number, Date = pp.Date, Active = pp.Active, Doctor_Code = pp.Doctor_Code };
            RR.Patients.Add(p);
            RR.SaveChanges();
            return Json(p);
        }



        public JsonResult DeleteAll(string id)
        {
            using var transaction = RR.Database.BeginTransaction();
            try
            {
                List<Patient> p2 = RR.Patients.Where(Riyaz => Riyaz.Doctor_Code == id).ToList();
                RR.Patients.RemoveRange(p2);

                Doctor d2 = RR.Doctors.Find(id);
                if (d2 != null)
                {
                    RR.Doctors.Remove(d2);
                }
                RR.SaveChanges();
                transaction.Commit();
            }
            catch(Exception ex)
            {
                transaction.Rollback();
                return Json("Error");

            }
            return Json("OK");
        }


        [HttpPost]
        public ActionResult UploadFiles()
        {
            if (Request.Form.Files.Count > 0)
            {
                try
                {
                    var files = Request.Form.Files;
                    for (int i = 0; i < files.Count; i++)
                    {
                        IFormFile file = files[i];
                        string fname;
                        fname = file.FileName;
                        string webRootPath = HostEnvironment.WebRootPath;
                        string fname1 = Path.Combine(webRootPath, "Uploads/" + fname);
                        file.CopyTo(new FileStream(fname1, FileMode.Create));
                    }
                    return Json("File Uploaded Successfully!");
                }
                catch (Exception ex)
                {
                    return Json("Error occured. Error details: " + ex.Message);
                }
            }
            else
            {
                return Json("No files selected.");
            }
        }


        public string Patient_Exists(string id)
        {
            var a = (from p in RR.Patients where p.Patient_Id == id select new { p.Patient_Id, }).FirstOrDefault();
            if (a != null)
                return "1";
            else
                return "0";
        }










        public IActionResult Report()
        {
            if (!_signInManager.IsSignedIn(User)) //verify if it's logged
            {
                return LocalRedirect("~/");
            }
            var webReport = new WebReport();
            var mssqlDataConnection = new MsSqlDataConnection();
            mssqlDataConnection.ConnectionString = _configuration.GetConnectionString("ApplicationDbContext");
            webReport.Report.Dictionary.Connections.Add(mssqlDataConnection);
            webReport.Report.Load(Path.Combine(HostEnvironment.ContentRootPath, "reports", "Patients.frx"));
            var Doctor2 = GetTable<Doctor>(RR.Doctors, "Doctor");
            webReport.Report.RegisterData(Doctor2, "Doctor");
            return View(webReport);
        }

        static DataTable GetTable<TEntity>(IEnumerable<TEntity> table, string name) where TEntity : class
        {
            var offset = 78;
            DataTable result = new DataTable(name);
            PropertyInfo[] infos = typeof(TEntity).GetProperties();
            foreach (PropertyInfo info in infos)
            {
                if (info.PropertyType.IsGenericType
                && info.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    result.Columns.Add(new DataColumn(info.Name, Nullable.GetUnderlyingType(info.PropertyType)));
                }
                else
                {
                    result.Columns.Add(new DataColumn(info.Name, info.PropertyType));
                }
            }
            foreach (var el in table)
            {
                DataRow row = result.NewRow();
                foreach (PropertyInfo info in infos)
                {
                    if (info.PropertyType.IsGenericType &&
                    info.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        object t = info.GetValue(el);
                        if (t == null)
                        {
                            t = Activator.CreateInstance(Nullable.GetUnderlyingType(info.PropertyType));
                        }

                        row[info.Name] = t;
                    }
                    else
                    {
                        if (info.PropertyType == typeof(byte[]))
                        {
                            //Fix for Image issue.
                            var imageData = (byte[])info.GetValue(el);
                            var bytes = new byte[imageData.Length - offset];
                            Array.Copy(imageData, offset, bytes, 0, bytes.Length);
                            row[info.Name] = bytes;
                        }
                        else
                        {
                            row[info.Name] = info.GetValue(el);
                        }
                    }
                }
                result.Rows.Add(row);
            }

            return result;
        }




    }
}
