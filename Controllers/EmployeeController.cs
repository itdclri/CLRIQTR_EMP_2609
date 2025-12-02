using CLRIQTR_EMP.Data.Repositories.Implementations;
using CLRIQTR_EMP.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace CLRIQTR_EMP.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly EmployeeRepository _employeeRepo = new EmployeeRepository();

        public ActionResult Index()
        {
            try
            {
                var empNo = Session["EmpNo"]?.ToString();
                if (string.IsNullOrEmpty(empNo))
                {
                    TempData["ErrorMessage"] = "Please login to view employee details.";
                    return RedirectToAction("Index", "Login");
                }

                if (!_employeeRepo.CanApplyForNewQuarter(empNo))
                {
                    TempData["WarningMessage"] = "You are already a resident of eligible  quarter accomodation.";
                    return RedirectToAction("Index", "Home");
                }

                // If draft exists, go straight to drafts page
                if (_employeeRepo.HasDraftApplication(empNo))
                {
                    return RedirectToAction("ViewDrafts", "Employee");
                }

                // Original Flow: Get employee details
                var employee = _employeeRepo.GetEmployeeByEmpNo(empNo);

                if (employee == null)
                {
                    TempData["ErrorMessage"] = $"Employee with ID {empNo} not found . Contact your E-IV SO of your  Lab.";
                    return RedirectToAction("Index", "Login");
                }

                return View(employee);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while loading employee details. Please try again.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: New Application Step 1
        [HttpGet]
        public ActionResult NewApplication(string payLevel, string designation)
        {
            var empNo = Session["EmpNo"]?.ToString();
            if (string.IsNullOrEmpty(empNo))
            {
                TempData["ErrorMessage"] = "Please login to start a new application.";
                return RedirectToAction("Login", "Account");
            }

            if (!string.IsNullOrEmpty(payLevel))
            {
                Session["PayLevel"] = payLevel;
            }

            if (!string.IsNullOrEmpty(designation))
            {
                Session["Designation"] = designation;
            }

            var model = Session["NewApplicationModel"] as NewApplicationModel ?? new NewApplicationModel();

            var quarterType = _employeeRepo.GetQuarterTypeByEmpNo(empNo);
            model.QuarterType = quarterType;

            // SA eligibility & occupancy
            bool isSaEligible = _employeeRepo.IsScientistQuarterEligible(empNo);
            bool isSaOccupied = _employeeRepo.IsScientistQuarterAlreadyOccupied(empNo);

            ViewBag.IsSaEligible = isSaEligible;
            ViewBag.IsSaOccupied = isSaOccupied;
            ViewBag.ShowScientistOption = isSaEligible && !isSaOccupied;

            // does this emp already occupy their entitled type?
            bool entitledTypeOccupied = _employeeRepo.IsEntitledQuarterAlreadyOccupied(empNo);
            ViewBag.EntitledTypeOccupied = entitledTypeOccupied;

            return View(model);
        }

        // POST: New Application Step 1 (Next)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NewApplication(NewApplicationModel model)
        {
            Session["ApplyingForQuarters"] = model.ApplyingForQuarters;
            Session["ApplyingForScientistQuarters"] = model.ApplyingForScientistQuarters;
            // Store pay level if needed again
            var payLevel = Session["PayLevel"];
            Session["NewApplicationModel"] = model;

            return RedirectToAction("SubmitApplication");
        }

        // GET: SubmitApplication (Only Save Draft)
        [HttpGet]
        public ActionResult SubmitApplication(string draftId = null)
        {
            var empNo = Session["EmpNo"]?.ToString();
            if (string.IsNullOrEmpty(empNo))
                return RedirectToAction("Login", "Account");

            NewApplicationModel model ;

           

            if (!string.IsNullOrEmpty(draftId))
            {
                // Existing draft loading code...
                var tokens = draftId.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var firstDraftId = tokens.FirstOrDefault();

                if (!string.IsNullOrEmpty(firstDraftId))
                {
                    var draft = _employeeRepo.GetDraftByAppNo(firstDraftId);
                    if (draft != null)
                        model = MapEqtrApplyToModel(draft);
                    else
                        return RedirectToAction("ViewDrafts");
                }
                else
                {
                    return RedirectToAction("ViewDrafts");
                }
            }
            else
            {
                model = Session["NewApplicationModel"] as NewApplicationModel ?? new NewApplicationModel();
            }

            // Populate FamilyDetails here
            model.FamilyDetails = _employeeRepo.GetFamilyDetailsByEmpNo(empNo);

            // Existing quarter type & disability flags
            var quarterType = _employeeRepo.GetQuarterTypeByEmpNo(empNo);
            model.QuarterType = quarterType;

            bool hasPhysicalDisability = _employeeRepo.HasPhysicalDisability(empNo);
            model.ShowDisabilityFields = hasPhysicalDisability;

            var employee = _employeeRepo.GetEmployeeByEmpNo(empNo);
            model.DesignationCode = employee.Designation;
            model.DateOfJoining = employee.DOJ_dt;

            if (!string.IsNullOrEmpty(empNo))
            {
                var status = _employeeRepo.GetQtrStatus(empNo); // returns "IQ" or "OQ"

                if (status == "IQ")
                {
                    model.PresentResidence = _employeeRepo.GetQtrNo(empNo); // sets qtrno
                    model.IsQtrReadonly = true;
                }
                else
                {
                    model.IsQtrReadonly = false;
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitApplication(NewApplicationModel model, string action)
        {
            var empNo = Session["EmpNo"]?.ToString();
            if (string.IsNullOrEmpty(empNo))
                return RedirectToAction("Login", "Account");

            var quarterType = _employeeRepo.GetQuarterTypeByEmpNo(empNo);
            model.QuarterType = quarterType;

            // Check for physical disability
            bool hasPhysicalDisability = _employeeRepo.HasPhysicalDisability(empNo);
            model.ShowDisabilityFields = hasPhysicalDisability;

            string appStatus = "D"; // Default status: Draft
            if (action == "Submit")
                appStatus = "C"; // Submitted

            // Get applying preferences from session or DB
            bool ApplyingForQuarters = Session["ApplyingForQuarters"] != null
                ? Convert.ToBoolean(Session["ApplyingForQuarters"])
                : _employeeRepo.GetApplyingForQuartersFromDb(empNo);

            bool ApplyingForScientistQuarters = Session["ApplyingForScientistQuarters"] != null
                ? Convert.ToBoolean(Session["ApplyingForScientistQuarters"])
                : _employeeRepo.GetApplyingForScientistQuartersFromDb(empNo);

            // SA eligibility
            bool isSaEligible = _employeeRepo.IsScientistQuarterEligible(empNo);
            bool isSaOccupied = _employeeRepo.IsScientistQuarterAlreadyOccupied(empNo);
            bool scientistOptionAvailable = isSaEligible && !isSaOccupied;

            bool savedeqtr = false;
            bool savedsaqtr = false;

            Debug.WriteLine(model.SaQtrAppNo);
            Debug.WriteLine(model.QtrAppNo);

            // Handle EQTR Apply
            if (ApplyingForQuarters)
            {
                var entity = MapToEqtrApply(model);
                entity.EmpNo = empNo;
                entity.AppStatus = appStatus;
                entity.EqtrTypeSel = "Y";

                // Saint logic:
                // - If option not available => NA
                // - Else Yes => SI, No => SNI
                if (!scientistOptionAvailable)
                {
                    entity.Saint = "NA";
                }
                else
                {
                    entity.Saint = ApplyingForScientistQuarters ? "SI" : "SNI";
                }

                // Ensure it has its own QtrAppNo
                if (string.IsNullOrEmpty(model.QtrAppNo))
                {
                    entity.QtrAppNo = _employeeRepo.GenerateNewEqtrAppNo(empNo); // New method
                    model.QtrAppNo = entity.QtrAppNo; // Save back to model for linking
                    savedeqtr = _employeeRepo.InsertEqtrApply(entity);
                }
                else
                {
                    savedeqtr = _employeeRepo.UpdateEqtrApply(entity);
                }
            }

            // SAQTR only if eligible AND user selected Yes
            if (scientistOptionAvailable && ApplyingForScientistQuarters)
            {
                var entity = MapToSaEqtrApply(model);
                entity.EmpNo = empNo;
                entity.AppStatus = appStatus;
                entity.Saint = "SI";

                if (_employeeRepo.GetApplyingForScientistQuartersFromDb(empNo))
                {
                    savedsaqtr = _employeeRepo.UpdateSaEqtrApply(entity);

                    Debug.WriteLine(model);
                    Debug.WriteLine("Line2");
                }
                else
                {
                    entity.SaQtrAppNo = _employeeRepo.GenerateNewSaEqtrAppNo(empNo); // New method
                    model.SaQtrAppNo = entity.SaQtrAppNo;
                    savedsaqtr = _employeeRepo.InsertSaEqtrApply(entity);

                    Debug.WriteLine(model);
                    Debug.WriteLine("Line1");
                }
            }

            if (savedeqtr || savedsaqtr)
            {
                TempData["Message"] = appStatus == "D"
                    ? "Draft saved successfully!"
                    : "Application submitted successfully!";
                return RedirectToAction("ViewDrafts");
            }

            ModelState.AddModelError("", "Error saving application.");
            return View(model);
        }

        // GET: ViewDrafts
        public ActionResult ViewDrafts()
        {
            var empNo = Session["EmpNo"]?.ToString();
            if (string.IsNullOrEmpty(empNo))
                return RedirectToAction("Login", "Account");

            var drafts = _employeeRepo.GetDraftsByEmpNo(empNo)
                                      .Where(d => d.AppStatus == "D" || d.AppStatus == "C")
                                      .ToList();

            if (!drafts.Any())
            {
                // now safe to go to Index; HasDraftApplication(empNo) will be false
                return RedirectToAction("Index");
            }

            return View(drafts);
        }

        // POST: SubmitDraft (Final Submission)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitDraft(string qtrAppNo)
        {
            if (string.IsNullOrEmpty(qtrAppNo))
            {
                TempData["Error"] = "Invalid application number.";
                return RedirectToAction("ViewDrafts");
            }

            var draft = _employeeRepo.GetDraftByAppNo(qtrAppNo);
            if (draft == null)
            {
                TempData["Error"] = "Draft not found.";
                return RedirectToAction("ViewDrafts");
            }

            // (Final submission logic can be added here if needed)

            return RedirectToAction("ViewDrafts");
        }

        // --- Mapping Methods ---

        private EqtrApply MapToEqtrApply(NewApplicationModel model)
        {
            // Decide lower type flag based on quarter type
            string lowerTypeSel;
            if (model.QuarterType == "V")
            {
                // Only for Type V – yes/no matters
                lowerTypeSel = (model.InterestedForLowerType?.Trim().ToLower() == "interested")
                    ? "I"
                    : "NI";
            }
            else
            {
                // For all other types – NA
                lowerTypeSel = "NA";
            }

            return new EqtrApply
            {
                QtrAppNo = model.QtrAppNo,
                EmpNo = Session["EmpNo"]?.ToString() ?? "NA",
                OwnHouse = model.OwnHouse == "Yes" ? "O" : "NO",
                OwnName = model.OwnerName ?? "NA",
                OwnAdd = model.OwnerAddress ?? "NA",
                IsRent = model.HouseLetOut == "Yes" ? "R" : "NR",
                OwnRent = model.MonthlyRent ?? "0",
                IsHouseEightKm = "NA",
                NewOrCor = "NA",
                CpAccom = model.PresentResidence ?? "NA",
                LowerTypeSel = lowerTypeSel,
                Doa = model.DateOfApplication?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy"),
                Toe = model.QuarterType ?? "NA",
                QtrRes = "NA",
                EmpMobNo = model.EmpMobileNumber ?? "NA",
                AppStatus = model.AppStatus ?? "D",
                PermTemp = model.PermanentOrTemporary == "Permanent" ? "P" : "T",
                Surname = model.SuretyName ?? "NA",
                SurPost = model.SuretyPermanentPost ?? "NA",
                SurDesig = model.SuretyDesignation ?? "NA",
                LabCode = Session["Lab"]?.ToString() ?? "NA",

                // Saint will be set in SubmitApplication based on eligibility
                // Saint = ...

                EqtrTypeSel = Session["ApplyingForQuarters"] is bool applyingQuarters && applyingQuarters ? "Y" : "N",
                Ess = model.ServicesEssential == "Yes" ? "Y" : "N",
                Cco = model.IsCommonCadreOfficer == "Yes" ? "Y" : "N",
                DisDesc = model.NatureOfDisability ?? "NA",
                SpouseWorking = model.SpouseWorking == "Yes" ? "Y" : "N",
                SpouseOffice = model.SpouseOfficeName ?? "NA",
            };
        }

        private SaEqtrApply MapToSaEqtrApply(NewApplicationModel model)
        {
            // ✅ Same lower-type logic for SA table
            string saLowerTypeSel;
            if (model.QuarterType == "V")
            {
                saLowerTypeSel = (model.InterestedForLowerType?.Trim().ToLower() == "interested")
                    ? "I"
                    : "NI";
            }
            else
            {
                saLowerTypeSel = "NA";
            }

            return new SaEqtrApply
            {
                SaQtrAppNo = null,
                EmpNo = Session["EmpNo"]?.ToString() ?? "NA",
                OwnHouse = model.OwnHouse == "Yes" ? "O" : "NO",
                OwnName = model.OwnerName ?? "NA",
                OwnAdd = model.OwnerAddress ?? "NA",
                IsRent = model.HouseLetOut == "Yes" ? "R" : "NR",
                OwnRent = model.MonthlyRent ?? "0",
                IsHouseEightKm = "NA",
                NewOrCor = "NA",
                CpAccom = model.PresentResidence ?? "NA",
                LowerTypeSel = saLowerTypeSel,      // ✅ now I/NI only for type V, else NA
                Saint = "SI",
                Doa = model.DateOfApplication?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy"),
                Toe = model.QuarterType ?? "NA",
                QtrRes = "NA",
                EmpMobNo = model.EmpMobileNumber ?? "NA",
                AppStatus = model.AppStatus ?? "D",
                PermTemp = model.PermanentOrTemporary == "Permanent" ? "P" : "T",
                Surname = model.SuretyName ?? "NA",
                SurPost = model.SuretyPermanentPost ?? "NA",
                SurDesig = model.SuretyDesignation ?? "NA",
                LabCode = Session["Lab"]?.ToString() ?? "NA",
                SpouseWorking = model.SpouseWorking == "Yes" ? "Y" : "N",
                SpouseOffice = model.SpouseOfficeName ?? "NA",
            };
        }


        private NewApplicationModel MapEqtrApplyToModel(EqtrApply entity)
        {
            string lowerTypeValue;
            if (entity.LowerTypeSel == "I")
                lowerTypeValue = "Interested";
            else if (entity.LowerTypeSel == "NI")
                lowerTypeValue = "Not Interested";
            else
                lowerTypeValue = null; // For "NA" or anything else

            return new NewApplicationModel
            {
                QtrAppNo = entity.QtrAppNo,
                PresentResidence = entity.CpAccom,

                InterestedForLowerType = lowerTypeValue,
                OwnHouse = entity.OwnHouse == "O" ? "Yes" : "No",
                OwnerName = entity.OwnName,
                OwnerAddress = entity.OwnAdd,
                HouseLetOut = entity.IsRent == "R" ? "Yes" : "No",
                MonthlyRent = entity.OwnRent,
                PermanentOrTemporary = entity.PermTemp == "P" ? "Permanent" : "Temporary",
                SuretyName = entity.Surname,
                SuretyDesignation = entity.SurDesig,
                SuretyPermanentPost = entity.SurPost,
                ServicesEssential = entity.Ess == "Y" ? "Yes" : "No",
                IsCommonCadreOfficer = entity.Cco == "Y" ? "Yes" : "No",
                FamilyDetails = null,

                ApplyingForQuarters = entity.EqtrTypeSel == "Y",
                ApplyingForScientistQuarters = entity.Saint == "SI",

                TypeEligible = entity.Toe,
                EmpMobileNumber = entity.EmpMobNo,

                DateOfApplication = DateTime.TryParseExact(
                    entity.Doa,
                    "dd/MM/yyyy",
                    null,
                    System.Globalization.DateTimeStyles.None,
                    out var date
                ) ? date : (DateTime?)null,

                LabCode = entity.LabCode,
                AppStatus = entity.AppStatus,
                NatureOfDisability = entity.DisDesc,
                SpouseWorking = entity.SpouseWorking == "Y" ? "Yes" : "No",
                SpouseOfficeName = entity.SpouseOffice,
            };
        }

        // PDF Download
        [HttpGet]
        public ActionResult DownloadPdf(string qtrAppNo)
        {
            if (string.IsNullOrWhiteSpace(qtrAppNo))
                return HttpNotFound();

            // Clean and split input
            var tokens = qtrAppNo
                .Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .ToList();

            if (tokens.Count == 0)
                return HttpNotFound();

            var pdfStreams = new List<byte[]>();

            foreach (var token in tokens)
            {
                object application = null;
                string viewName = "Pdf"; // Default view

                if (token.Contains("/SA/"))
                {
                    application = _employeeRepo.GetSAQuarterApplicationDetails(token);
                    viewName = "SAPdf"; // Set view to SAPdf.cshtml for 'SA' applications
                }
                else
                {
                    application = _employeeRepo.GetQuarterApplicationDetails(token);
                }

                if (application == null)
                    continue;

                // Render each PDF using the selected view
                var pdfResult = new Rotativa.ViewAsPdf(viewName, application)
                {
                    PageSize = Rotativa.Options.Size.A4,
                    PageMargins = new Rotativa.Options.Margins(20, 15, 15, 15)
                };

                var pdfBytes = pdfResult.BuildPdf(ControllerContext);
                pdfStreams.Add(pdfBytes);
            }

            if (!pdfStreams.Any())
                return HttpNotFound();

            // Merge PDFs if there are more than one
            var mergedPdf = MergePdfFiles(pdfStreams);
            var empNo = Session["EmpNo"]?.ToString();

            // Send the merged PDF to the client
            return File(mergedPdf, "application/pdf", empNo + ".pdf");
        }

        private byte[] MergePdfFiles(List<byte[]> pdfFiles)
        {
            using (var outStream = new MemoryStream())
            {
                var outputDocument = new PdfSharp.Pdf.PdfDocument();

                foreach (var pdfBytes in pdfFiles)
                {
                    using (var inputStream = new MemoryStream(pdfBytes))
                    {
                        var inputDocument = PdfSharp.Pdf.IO.PdfReader.Open(inputStream, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import);

                        for (int i = 0; i < inputDocument.PageCount; i++)
                        {
                            outputDocument.AddPage(inputDocument.Pages[i]);
                        }
                    }
                }

                outputDocument.Save(outStream);
                return outStream.ToArray();
            }
        }
    }
}
