using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CLRIQTR_EMP.Models
{
    public class NewApplicationModel 
    {
        [Required(ErrorMessage = "Please select an option")]
        [Display(Name = "Applying for Quarters Type")]
        public bool ApplyingForQuarters { get; set; }

        [Required(ErrorMessage = "Please select an option")]
        [Display(Name = "Applying for Scientist Quarters")]
        public bool ApplyingForScientistQuarters { get; set; }

        public string SpouseWorking { get; set; }
        public string SpouseOfficeName { get; set; }
        public string QuarterType { get; set; }

        public string PayLevel { get; set; }

        // Flag to indicate if the first form is submitted
        public bool FirstFormSubmitted { get; set; } = false;

        
        // Additional Fields (Second Form)

        [Display(Name = "Date of Application")]
        [DataType(DataType.Date)]
        public DateTime? DateOfApplication { get; set; }

        [Display(Name = "Type Eligible")]
        public string TypeEligible { get; set; } // e.g. "V", "IV"

        [Display(Name = "Present Place of Residence")]
        public string PresentResidence { get; set; }

        [Display(Name = "Interested For Lower Type ")]
        public string InterestedForLowerType { get; set; } // Interested / Not Interested

        [Display(Name = "Own House in Municipal Limits")]
        public string OwnHouse { get; set; } // Yes/No

        [Display(Name = "Owner's Name")]
        public string OwnerName { get; set; }

        [Display(Name = "Owner's Address")]
        public string OwnerAddress { get; set; }

        [Display(Name = "Is the House Let Out")]
        public string HouseLetOut { get; set; } // Yes/No

        [Display(Name = "Monthly Rent Received")]
        public string MonthlyRent { get; set; }

        [Display(Name = "Details of Family Members (Name, Relation, Age)")]
        public string FamilyDetails { get; set; }

        [Display(Name = "Permanent or Temporary")]
        public string PermanentOrTemporary { get; set; } // Permanent / Temporary

        [Display(Name = "Surety Name")]
        public string SuretyName { get; set; }

        [Display(Name = "Surety Designation")]
        public string SuretyDesignation { get; set; }

        [Display(Name = "Surety Permanent Post")]
        public string SuretyPermanentPost { get; set; }

        [Display(Name = "Services Declared Essential")]
        public string ServicesEssential { get; set; } // Yes/No

        [Display(Name = "Are you a Common Cadre Officer")]
        public string IsCommonCadreOfficer { get; set; } // Yes/No

        // ✅ New properties for dynamic lower type selection
        public string LowerType { get; set; }  // immediate lower entitlement
        public List<string> LowerTypesAvailable { get; set; } // all lower entitlements

        
        public string IsHouseEightKm { get; set; }

 
        public string EmpMobileNumber { get; set; }

     
        public string AppStatus { get; set; }

      
        public string LabCode { get; set; }

        public string QtrAppNo { get; set; }

        public string NatureOfDisability { get; set; }

        public bool ShowDisabilityFields { get; set; }

        public string SaQtrAppNo { get; set; }


        // Constructor to initialize strings to empty to avoid null reference issues
        public NewApplicationModel()
        {
            ApplyingForQuarters = false;
            ApplyingForScientistQuarters = false;
            QuarterType = "";
            PayLevel = "";
            TypeEligible = "";
            PresentResidence = "";
            InterestedForLowerType = "";
            OwnHouse = "";
            OwnerName = "";
            OwnerAddress = "";
            HouseLetOut = "";
            MonthlyRent = "";
            FamilyDetails = "";
            PermanentOrTemporary = "";
            SuretyName = "";
            SuretyDesignation = "";
            SuretyPermanentPost = "";
            ServicesEssential = "";
            IsCommonCadreOfficer = "";
            LowerType = "";
            LowerTypesAvailable = new List<string>();
                IsHouseEightKm = "";
            EmpMobileNumber = "";
            AppStatus = "";
            LabCode = "";
        }


   

    }

    public class EqtrApply
    {
        [Key]
        [Required]
        public string QtrAppNo { get; set; }

        public string SpouseWorking { get; set; } // To store "Y" or "N"
        public string SpouseOffice { get; set; }  // To store the office name

        [Required]
        public string EmpNo { get; set; }

        [Required]
        public string OwnHouse { get; set; }

        [Required]
        public string OwnName { get; set; }

        [Required]
        public string OwnAdd { get; set; }

        [Required]
        public string IsRent { get; set; }

        [Required]
        public string OwnRent { get; set; }

        [Required]
        public string IsHouseEightKm { get; set; }

        [Required]
        public string NewOrCor { get; set; }

        [Required]
        public string CpAccom { get; set; }

        [Required]
        public string LowerTypeSel { get; set; }

        [Required]
        public string Saint { get; set; }

        [Required]
        public string Doa { get; set; }

        [Required]
        public string Toe { get; set; }

        [Required]
        public string QtrRes { get; set; }

        [Required]
        public string EmpMobNo { get; set; }

        [Required]
        public string AppStatus { get; set; }

        [Required]
        public string PermTemp { get; set; }

        [Required]
        public string Surname { get; set; }

        [Required]
        public string SurPost { get; set; }

        [Required]
        public string SurDesig { get; set; }

        [Required]
        public string LabCode { get; set; }

        public string EqtrTypeSel { get; set; }

        public string Ess { get; set; }

        public string Cco { get; set; }

        public string DisDesc { get; set; }

    


    }

    public class SaEqtrApply
    {
        [Key]
        [Required]
        public string SaQtrAppNo { get; set; }

        [Required]
        public string EmpNo { get; set; }

        public string SpouseWorking { get; set; } // To store "Y" or "N"
        public string SpouseOffice { get; set; }  // To store the office name

        [Required]
        public string OwnHouse { get; set; }

        [Required]
        public string OwnName { get; set; }

        [Required]
        public string OwnAdd { get; set; }

        [Required]
        public string IsRent { get; set; }

        [Required]
        public string OwnRent { get; set; }

        [Required]
        public string IsHouseEightKm { get; set; }

        [Required]
        public string NewOrCor { get; set; }

        [Required]
        public string CpAccom { get; set; }

        [Required]
        public string LowerTypeSel { get; set; }

        [Required]
        public string Saint { get; set; }

        [Required]
        public string Doa { get; set; }

        [Required]
        public string Toe { get; set; }

        [Required]
        public string QtrRes { get; set; }

        [Required]
        public string EmpMobNo { get; set; }

        [Required]
        public string AppStatus { get; set; }

        [Required]
        public string PermTemp { get; set; }

        [Required]
        public string Surname { get; set; }

        [Required]
        public string SurPost { get; set; }

        [Required]
        public string SurDesig { get; set; }

        [Required]
        public string LabCode { get; set; }
    }
}
