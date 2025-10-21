using CLRIQTR_EMP.Data.Repositories.Implementations;
using CLRIQTR_EMP.Models;
using System.Web.Mvc;

namespace CLRIQTR_EMP.Controllers
{
    public class LoginController : Controller
    {
        private LoginRepository _loginRepository = new LoginRepository();

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string username, string password)
        {
            EmpLogin user = _loginRepository.GetUser(username, password);

            if (user != null)
            {
                Session["Lab"] = user.lab;
                Session["EmpNo"] = user.empno;

                return RedirectToAction("Index", "Employee");
            }

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
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // --- THIS IS THE CORRECTED LOGIC ---
            // 1. Check if user exists using ONLY the employee number
            if (_loginRepository.UserExists(model.empno))
            {
                // 2. Add the error to the 'empno' field so it shows under the textbox
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
                ModelState.AddModelError("", "Error during registration. Please try again.");
                return View(model);
            }

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
            // Use the service to send recovery email and get the response message
            string resultMessage = _loginRepository.SendPasswordRecoveryEmail(employeeNumber);

            //string resultMessage = employeeDataAccess.SendMail(employeeNumber);

            // Display the result message to the user
            ViewBag.Message = resultMessage;

            return View("ForgotPassword");
        }


    }
}
