using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CLRIQTR_EMP.Models
{
    public class EmpLogin
    {
        [Required(ErrorMessage = "Employee Number is required")]
        public string empno { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "Password length should be 8 to 20 characters")]
        [CustomPasswordValidation]
        public string pwd { get; set; }

        [Required(ErrorMessage = "Lab selection is required")]
        public string lab { get; set; }
    }

    public class CustomPasswordValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string password = value as string;

            if (password == null)
                return new ValidationResult("Password is required.");

            if (password.Length < 8 || password.Length > 20)
                return new ValidationResult("Password length must be between 8 and 20 characters.");

            // At least one uppercase letter
            if (!Regex.IsMatch(password, @"[A-Z]"))
                return new ValidationResult("Password must contain at least one uppercase letter.");

            // At least one lowercase letter
            if (!Regex.IsMatch(password, @"[a-z]"))
                return new ValidationResult("Password must contain at least one lowercase letter.");

            // At least two digits
            if (Regex.Matches(password, @"\d").Count < 2)
                return new ValidationResult("Password must contain at least two digits.");

            // At least one special character
            if (!Regex.IsMatch(password, @"[\W_]"))
                return new ValidationResult("Password must contain at least one special character (e.g., !@#$%^&*).");

            return ValidationResult.Success;
        }
    }

}
