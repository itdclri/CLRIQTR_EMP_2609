using CLRIQTR_EMP.Data.Repositories.Implementations;
using CLRIQTR_EMP.Models;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using NLog;


namespace CLRIQTR_EMP.Controllers
{
    public class LoginController : Controller
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private LoginRepository _loginRepository = new LoginRepository();


        private string SanitizeEmpNo(string rawEmpNo)
        {
            if (string.IsNullOrWhiteSpace(rawEmpNo))
                return null;

            var clean = rawEmpNo.Trim();

            // allow only letters + digits
            if (!Regex.IsMatch(clean, @"^[A-Za-z0-9]+$"))
                return null;

            if (clean.Length > 10)
                clean = clean.Substring(0, 10);

            return clean;
        }

        // 🔐 Password sanitizer/validator
        private string SanitizePassword(string rawPassword)
        {
            if (string.IsNullOrWhiteSpace(rawPassword))
                return null;

            // Remove leading/trailing spaces only
            string pwd = rawPassword.Trim();

            // Reject control characters (newlines, tabs, etc.)
            foreach (char c in pwd)
            {
                if (char.IsControl(c))
                    return null;
            }

            // Enforce length limit to prevent abuse (you can change 128)
            if (pwd.Length > 128)
                pwd = pwd.Substring(0, 128);

            return pwd;
        }


        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string username, string password)
        {

            string empNo = SanitizeEmpNo(username);
            string pwd = SanitizePassword(password);

            if (string.IsNullOrEmpty(empNo) || string.IsNullOrEmpty(pwd))
            {
                logger.Warn("Login failed – invalid format. EmpNoRaw: {0}", username);
                ViewBag.Error = "Invalid login. Please check your Employee Number and Password.";
                return View();
            }

            logger.Info("Login attempt for EmpNo: {0}", empNo);

            EmpLogin user = _loginRepository.GetUser(username, password);

            if (user != null)
            {
                Session["Lab"] = user.lab;
                Session["EmpNo"] = user.empno;

                logger.Info("Login success for EmpNo: {0}, Lab: {1}", user.empno, user.lab);
                return RedirectToAction("Index", "Employee");
            }

            logger.Warn("Login failed – wrong credentials for EmpNo: {0}", empNo);
            ViewBag.Error = "Invalid login. Please check your Employee Number and Password.";
            return View();
        }

        // GET: Registration form
        [HttpGet]
        public ActionResult Registration()
        {
            return View(new EmpLogin());
        }

        // Inside your LoginController.cs

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration(EmpLogin model)
        {

            model.empno = SanitizeEmpNo(model.empno);
            model.pwd = SanitizePassword(model.pwd);

            if (string.IsNullOrEmpty(model.empno))
                ModelState.AddModelError("empno", "Invalid Employee Number format.");

            if (string.IsNullOrEmpty(model.pwd))
                ModelState.AddModelError("pwd", "Invalid Password format.");


            if (!ModelState.IsValid)
            {

                logger.Warn("Registration validation failed for EmpNoRaw: {0}", model.empno);
                return View(model);
            }

            // --- THIS IS THE CORRECTED LOGIC ---
            // 1. Check if user exists using ONLY the employee number
            if (_loginRepository.UserExists(model.empno))
            {
                // 2. Add the error to the 'empno' field so it shows under the textbox
                logger.Warn("Registration attempt for already registered EmpNo: {0}", model.empno);
                ModelState.AddModelError("empno", "This Employee Number is already registered.");
                return View(model);
            }
            // --- END OF CORRECTION ---

            string labCode = null;
            if (model.lab == "CLRI")
                labCode = "100";
            else if (model.lab == "SERC")
                labCode = "101";
            else if (model.lab == "CMC")
                labCode = "102";


            if (labCode == null)
            {
                ModelState.AddModelError("lab", "Invalid Lab selected.");
                return View(model);
            }

            // This line will now only run if the empno is NOT a duplicate
            bool success = _loginRepository.InsertUser(model.empno, model.pwd, labCode);

            if (!success)
            {
                logger.Error("Registration DB insert failed for EmpNo: {0}", model.empno);
                ModelState.AddModelError("", "Error during registration. Please try again.");
                return View(model);
            }

            logger.Info("Registration successful for EmpNo: {0}, LabCode: {1}", model.empno, labCode);
            TempData["Success"] = "Registration successful! Please login.";
            return RedirectToAction("Registration", "Login");
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index", "Login");
        }


        
       

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SendPasswordRecoveryEmail(string employeeNumber)
        {

            string empNo = SanitizeEmpNo(employeeNumber);

            if (string.IsNullOrEmpty(empNo))
            {
                logger.Warn("ForgotPassword – invalid emp number format: {0}", employeeNumber);
                ViewBag.Message = "Invalid employee number format.";
                return View("ForgotPassword");
            }

            logger.Info("ForgotPassword – recovery requested for EmpNo: {0}", empNo);
            // Use the service to send recovery email and get the response message
            string resultMessage = _loginRepository.SendPasswordRecoveryEmail(employeeNumber);
            logger.Info("ForgotPassword – result for EmpNo {0}: {1}", empNo, resultMessage);

            //string resultMessage = employeeDataAccess.SendMail(employeeNumber);

            // Display the result message to the user
            ViewBag.Message = resultMessage;

            return View("ForgotPassword");
        }


    }
}
