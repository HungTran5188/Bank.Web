using System;

namespace Bank.Dto
{
    public abstract class AccountDto
    {
        public abstract bool Deposit(decimal amount);
        public virtual bool Withdraw(decimal amount) { return false; }
        public abstract decimal Balance { get; }
    }
    public class BankAccount : AccountDto
    {
        private decimal _balance;
        private static readonly object _obj = new object();
        public BankAccount(decimal balance)
        {
            _balance = balance;
        }
      
        public override bool Deposit(decimal amount)
        {
            lock (_obj)
            {
                _balance += amount;
                return true;

            }
            }
        public override bool Withdraw(decimal amount)
        {
            lock (_obj)
            {
                if (_balance < amount)
                {

                    return false;
                }
                else
                {
                    _balance -= amount;
                    return true;
                }
            }
        }
        public override decimal Balance
        {
            get
            {
                return _balance;
            }
        }
    }
}
