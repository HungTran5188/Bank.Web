
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Bank.Entities;
using System.Threading.Tasks;
using Bank.Infrastructures;
using Bank.Models;
using Bank.Dto;
using System.Threading;

namespace Bank.Cores
{
    public interface IAccountRepository
    {
        Task<List<Account>> GetAll();
        Task<Account> Get(int? id);
        Task<int> Add(Account entity);

        Task<int> Delete(int id);
        Task<int> DepositAmount(int id, byte[] rowVersion, AccountModel model);
        Task<int> WithdrawAmount(int id, byte[] rowVersion, AccountModel model);
        void TranferAmount(byte[] rowVersion, AccountModel model, Account senderEntity, Account receiverEntity);
        bool IsValidAccount(AccountModel model, out Account senderEntity, out Account receiverEntity);
    }
    public class AccountRepository : IAccountRepository
    {
        private readonly BankContext _context;
        private static readonly object _obj = new object();
        public AccountRepository(BankContext context)
        {
            _context = context;

        }

        public Task<int> Add(Account entity)
        {
            entity.Password = CommonFunction.CreatePasswordHash(entity.Password, Conts.PASSWORD_SALT);
            entity.CreatedDate = DateTime.Now;
            _context.Add(entity);
            _context.Entry(entity).State = EntityState.Added;
            return _context.SaveChangesAsync();
        }

        public Task<int> Delete(int id)
        {
            var account = _context.Accounts.SingleOrDefault(x => x.AccountID == id);
            _context.Accounts.Remove(account);
            return _context.SaveChangesAsync();

        }

        public Task<Account> Get(int? id)
        {

            return _context.Accounts.SingleOrDefaultAsync(x => x.AccountID == id);
        }

        public Task<List<Account>> GetAll()
        {
            return _context.Accounts.ToListAsync();
        }

        public Task<int> DepositAmount(int id, byte[] rowVersion, AccountModel model)
        {
            AccountTransaction trans = null;
            Account entity = null;


            entity = _context.Accounts.SingleOrDefault(x => x.AccountID == id);
            if (entity == null)
            {
                throw new BankException("Unable to deposit amount. Account Number was deleted by another user.");
            }

            _context.Entry(entity).Property("RowVersion").OriginalValue = rowVersion;
            BankAccount ac = new BankAccount(entity.Balance);
            ac.Deposit(model.Amount);
            entity.Balance = ac.Balance;

            trans = new AccountTransaction()
            {
                AccountID = entity.AccountID,
                Description = string.Format("Successfull deposit amount: {0}. Total balance: {1}. Account Number: {2}", model.Amount.ToString(), entity.Balance.ToString(), entity.AccountNumber),
                TranDate = DateTime.Now,

            };
            entity.AccountTransactions = new[] { trans };
            _context.Entry(trans).State = EntityState.Added;

            _context.Entry(entity).State = EntityState.Modified;

            return _context.SaveChangesAsync();

        }

        public Task<int> WithdrawAmount(int id, byte[] rowVersion, AccountModel model)
        {
            AccountTransaction trans = null;
            Account entity = null;
            Task<int> result;

            entity = _context.Accounts.SingleOrDefault(x => x.AccountID == id);
            if (entity == null)
            {
                throw new BankException("Unable to withdraw amount. Account Number was deleted by another user.");
            }


            _context.Entry(entity).Property("RowVersion").OriginalValue = rowVersion;

            trans = new AccountTransaction()
            {
                AccountID = entity.AccountID,

                TranDate = DateTime.Now,

            };

            var withdrawAccount = new BankAccount(entity.Balance);
            if (withdrawAccount.Withdraw(model.Amount))
            {
                entity.Balance = withdrawAccount.Balance;
                trans.Description = string.Format("Successfull Withdraw amount: {0}. Total balance: {1}. Account Number {2}", model.Amount.ToString(), entity.Balance.ToString(), entity.AccountNumber);
                entity.AccountTransactions = new[] { trans };
                _context.Entry(trans).State = EntityState.Added;
                _context.Entry(entity).State = EntityState.Modified;
                result = _context.SaveChangesAsync();
            }
            else
            {
                trans.Description = string.Format("Unable to withdraw amount: {0}. Insufficient balance. Account Number {1}", model.Amount.ToString(), entity.AccountNumber);
                _context.Entry(trans).State = EntityState.Added;
                result = _context.SaveChangesAsync();
                throw new BankException("Unable to withdraw amount. Insufficient balance.");
            }
            return result;


        }

        public void TranferAmount(byte[] rowVersion, AccountModel model, Account senderEntity, Account receiverEntity)
        {
            AccountTransaction trans = null;
            using (var transaction = _context.Database.BeginTransaction())
            {
            Monitor.Enter(_obj);
            try
            {
              
                var receiverAccount = new BankAccount(receiverEntity.Balance);
                receiverAccount.Deposit(model.Amount);
                receiverEntity.Balance = receiverAccount.Balance;
             
                _context.Entry(senderEntity).State = EntityState.Modified;

                _context.Entry(receiverEntity).State = EntityState.Modified;

                _context.Entry(senderEntity).Property("RowVersion").OriginalValue = rowVersion;
                trans = new AccountTransaction()
                {
                    AccountID = senderEntity.AccountID,
                    Description = string.Format("Successfull tranfer amount: {0} from {1} to {2}", model.Amount.ToString(), senderEntity.AccountNumber, receiverEntity.AccountNumber),
                    TranDate = DateTime.Now,

                };
                senderEntity.AccountTransactions = new[] { trans };
                _context.Entry(trans).State = EntityState.Added;
                _context.SaveChanges();
                 transaction.Commit();
            }

            catch (Exception ex)
            {
                transaction.Rollback();
               
                throw new BankException(ex.Message);
            }
            finally { Monitor.Exit(_obj); }

            }

        }

        public bool IsValidAccount(AccountModel model, out Account senderEntity, out Account receiverEntity)
        {

            senderEntity = _context.Accounts.SingleOrDefault(x => x.AccountID == model.AccountID);
            if (senderEntity == null)
            {
                throw new BankException("Unable to tranfer amount. Sender Account Number was deleted by another user.");
            }
            receiverEntity = _context.Accounts.SingleOrDefault(x => x.AccountNumber == model.TranferNumber.Trim() && x.AccountID != model.AccountID);
            if (receiverEntity == null)
            {
                throw new BankException("Not found tranfer number.");
            }

            var senderAccount = new BankAccount(senderEntity.Balance);
            if (!senderAccount.Withdraw(model.Amount))
            {
                throw new BankException("Unable to tranfer amount. Insufficient balance!");
            }
            else
            {
                senderEntity.Balance = senderAccount.Balance;
            }
            return true;
        }
    }
}
