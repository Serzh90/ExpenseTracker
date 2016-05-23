using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker
{
    /// <summary>
    /// Class for handling expenses and calculating transactions
    /// </summary>
    class Tracker
    {
        private static decimal Precision = Properties.Settings.Default.Precision;
        /// <summary>
        /// Expenses are stored as a dicionary where key is user name and value is another dictionary representing service and amount mapping
        /// UserName -> ServiceName -> Amount 
        /// </summary>
        private Dictionary<string,Dictionary<string,decimal>> Expenses = new Dictionary<string, Dictionary<string,decimal>>();
        private Dictionary<string, decimal> Balance;
        internal void AddNewExpense(string personStr, string serviceStr, decimal amount)
        {
            //new person  
            if (!Expenses.ContainsKey(personStr))
            {
                //creating and saving new expense
                var expense = new Dictionary<string, decimal> { { serviceStr, amount } };
                Expenses.Add(personStr, expense);
            }
            //person exists, adding expense
            else if (Expenses[personStr] != null)
                //new expense
                if (!Expenses[personStr].ContainsKey(serviceStr))
                    Expenses[personStr].Add(serviceStr, amount);
                //expense/service already exists, increasing amount  
                else
                    Expenses[personStr][serviceStr] += amount;
        }

        internal Dictionary<string, Dictionary<string, decimal>> GetExpenses()
        {
            return Expenses;
        }
        /// <summary>
        /// Method to calculate avarage payment made by person with rounding  
        /// </summary>
        internal decimal Avarage() => (Expenses.Count > 0) ? Math.Round(TotalAmount() / Expenses.Count, 2) : 0; 
        /// <summary>
        /// Method to calculate total amount of payments made by all persons  
        /// </summary>
        internal decimal TotalAmount() => Expenses.Select(x => x.Value.Values.Sum()).Sum();
        /// <summary>
        /// Main method to calculate transactions required between persons
        /// </summary>
        internal List<Transaction> CalculateTransactions()
        {
            //list on of transactions to be made, required for UI 
            var transactions = new List<Transaction>();
            //if only one payment made - no need to make any transactions
            if (Expenses.Count < 2)
                return transactions;
            //average payment per person
            var average = Avarage();
            Balance = new Dictionary<string, decimal>();
            //calculating balance per person as difference betwen average and paid amounts, if balance is 0 -> no transactions required for the person 
            foreach (var item in Expenses)
            {
                var balance = average - item.Value.Values.Sum();
                if (balance != 0)
                    Balance.Add(item.Key, average - item.Value.Values.Sum());
            }
            //at first we are getting transactions from pairs which can be simply resolved within single transaction   
            transactions.AddRange(GetSimpleTransactionsFromPairs());
            //continue with all the rest
            //if one or zero persons with balance left - calculation is completed  
            while (Balance.Count > 1) {
                //sorting to pair biggest and smallest balances  
                var sortedBalance = Balance.OrderBy(x => x.Value);
                decimal transactionAmount;
                //recipient it the one who has the "worst" balance, meaning he needs money the most
                var recipient = sortedBalance.First();
                //sender is the one who has the best balance, meaning he can make biggest payment within one transaction  
                var sender = sortedBalance.Last();
                var recipientShouldReceive = Math.Abs(recipient.Value);
                var senderShouldSend = Math.Abs(sender.Value);
                
                if (senderShouldSend > recipientShouldReceive)
                    transactionAmount = recipientShouldReceive;
                else
                    transactionAmount = senderShouldSend;
                var transaction = new Transaction{ from = sender.Key, to = recipient.Key, amount = decimal.Round(transactionAmount,2) };
                transactions.Add(transaction);
                Balance[sender.Key] -= transactionAmount;
                Balance[recipient.Key] += transactionAmount;
                if (Math.Abs(Balance[sender.Key]) <= Precision)
                    Balance.Remove(sender.Key);
                if (Math.Abs(Balance[recipient.Key]) <= Precision)
                    Balance.Remove(recipient.Key);
            }
            return transactions;
        }
        /// <summary>
        /// Method to find pairs of persons which can be resolved within one transaction, this allows to reduce overall amount of transactions
        /// </summary>
        private List<Transaction> GetSimpleTransactionsFromPairs()
        {
            var transactions = new List<Transaction>();
            var negative = Balance.Where(x => x.Value < 0).ToList();
            var positive = Balance.Where(x => x.Value > 0).ToList();
            //var combinations = negative.SelectMany(x => positive, (x, y) => Tuple.Create(x, y)).Where(tuple => tuple.Item1.Value + tuple.Item2.Value == 0);
            foreach (var neg in negative)
            {
                foreach (var pos in positive)
                {
                    //pair found
                    if (pos.Value + neg.Value == 0)
                    {
                        //safety check if these values were not paired before in same loop  
                        if (Balance.ContainsKey(pos.Key) && Balance.ContainsKey(neg.Key))
                        {
                            var transaction = new Transaction { from = neg.Key, to = pos.Key, amount = decimal.Round(pos.Value, 2) };
                            transactions.Add(transaction);
                            Balance.Remove(neg.Key);
                            Balance.Remove(pos.Key);
                            continue;
                        }
                    }
                }
            }
            return transactions;
        }

        public struct Transaction
        {
            public string from;
            public string to;
            public decimal amount;
        }
    }
}
