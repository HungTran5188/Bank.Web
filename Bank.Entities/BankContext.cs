using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bank.Entities
{
    public class BankContext : DbContext
    {
        public BankContext(DbContextOptions<BankContext> options)
          : base(options)
        {

        }
        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<AccountTransaction> AccountTransactions { get; set; }
        

    }

    public partial class Account
    {

        public Account()
        {
            AccountTransactions = new HashSet<AccountTransaction>();
        }
        [Key]
        public int AccountID { get; set; }

        [Required]
        [StringLength(16)]
        public string AccountNumber { get; set; }

        
        [StringLength(16)]
        public string AccountName { get; set; }

        [Required]
        [StringLength(250)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public decimal Balance { get; set; }

        public DateTime? CreatedDate { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
        public virtual ICollection<AccountTransaction> AccountTransactions { get; set; }
    }

    public partial class AccountTransaction
    {
        [Key]
        public int AccountTransactionID { get; set; }

        public string Description { get; set; }

        public DateTime? TranDate { get; set; }

        public int? AccountID { get; set; }

        public virtual Account Account { get; set; }
    }
}
