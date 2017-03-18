using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bank.Models
{

    public enum ActionType { Deposit = 1, Withdraw = 2, Tranfer = 3}
    public class AccountModel
    {
        public int AccountID { get; set; }

        [Required]
        [RegularExpression(@"[0-9]*\.?[0-9]+", ErrorMessage = "{0} must be a Number.")]
        [StringLength(16, MinimumLength = 4)]
        public string AccountNumber { get; set; }

        [Required]
      
        [StringLength(250, MinimumLength = 4)]
        public string AccountName { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:C0}")]
        [RegularExpression(@"[0-9]*\.?[0-9]+", ErrorMessage = "{0} must be a Number.")]
        public decimal Balance { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:C0}")]
        [RegularExpression(@"[0-9]*\.?[0-9]+", ErrorMessage = "{0} must be a Number.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a value bigger than {1}")]
        public decimal Amount { get; set; }

        public byte[] RowVersion { get; set; }

        public ActionType ActionType { get; set; }
        [Required]
        [RegularExpression(@"[0-9]*\.?[0-9]+", ErrorMessage = "{0} must be a Number.")]
        [StringLength(16, MinimumLength = 4)]
        public string TranferNumber { get; set; }
    }
    public class AccountCreateModel : AccountModel
    {
        [Required(ErrorMessage = "Password is required")]
        [StringLength(255, ErrorMessage = "Must be between 5 and 255 characters", MinimumLength = 5)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [StringLength(255, ErrorMessage = "Must be between 5 and 255 characters", MinimumLength = 5)]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}
