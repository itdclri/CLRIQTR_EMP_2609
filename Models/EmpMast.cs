using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CLRIQTR_EMP.Models
{
    public class EmpMast
    {

        [Key]
        [StringLength(45)]
        public string EmpNo { get; set; }

        [StringLength(45)]
        public string EmpName { get; set; }

        public int LabCode { get; set; }

        [StringLength(45)]
        public string Gender { get; set; }

        [StringLength(45)]
        public string PayLvl { get; set; }

        public string Designation { get; set; }

        [StringLength(45)]
        public string DOB { get; set; }

        [StringLength(45)]
        public string DOJ { get; set; }

        public int? BasicPay { get; set; }

        [StringLength(45)]
        public string Category { get; set; }

        [StringLength(45)]
        public string DOP { get; set; }

        [StringLength(45)]
        public string DOR { get; set; }

        [StringLength(45)]
        public string PWD { get; set; }

        [StringLength(145)]
        public string Email { get; set; }

        [StringLength(145)]
        public string EmpGroup { get; set; }

        [StringLength(145)]
        public string Grade { get; set; }

        [StringLength(45)]
        public string Active { get; set; }

        [StringLength(45)]
        public string Phy { get; set; }

        [StringLength(45)]
        public string Checked { get; set; }

        [StringLength(45)]
        public string ChkDtte { get; set; }

       
        [StringLength(45)]
        public string MobileNumber { get; set; }

        [NotMapped]
        public string LabName { get; set; }

        [NotMapped]
        public string DesDesc { get; set; }

        public DateTime? EnteredDate { get; set; }
        public string EnteredIP { get; set; }

        //public string CatNew { get; set; }


        // ========== DateTime Properties for View ==========
     
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DOB_dt { get; set; }

     
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DOJ_dt { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DOP_dt { get; set; }

      
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DOR_dt { get; set; }

    }
}