using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CLRIQTR_EMP.Models
{
    public class ApplicationModel
    {
        public string QtrAppNo { get; set; }
        public DateTime Doa { get; set; }
        public string LabName { get; set; }
        public string EmpNo { get; set; }
        public string EmpName { get; set; }
        public string Designation { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public string Category { get; set; }
        public string PhysicallyHandicapped { get; set; }
        public string DisabilityDetails { get; set; }
        public string PayLevel { get; set; }
        public DateTime DateOfPromotion { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime DateOfJoining { get; set; }
        public decimal BasicPay { get; set; }
        public string AccommodationDetails { get; set; }
        public string FamilyDetails { get; set; }
        public string EntitledType { get; set; }
        public string OwnHouse { get; set; }
        public string OwnerName { get; set; }
        public string OwnerAddress { get; set; }
        public string IsHouseLetOut { get; set; }
        public string RentReceived { get; set; }
        public string PermanentOrTemporary { get; set; }
        public string SuretyName { get; set; }
        public string SuretyDesignation { get; set; }
        public string SuretyPost { get; set; }
        public string ServicesEssential { get; set; }
    }
}