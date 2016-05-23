using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace ExpenseTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Tracker tracker = new Tracker();
        public ObservableCollection<DataRow> data = new ObservableCollection<DataRow>();

        public MainWindow()
        {
            InitializeComponent();
            dataGrid.DataContext = data;
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            #region validation
            //check for empty values
            if (string.IsNullOrEmpty(textBoxPerson.Text) || string.IsNullOrEmpty(textBoxService.Text) || string.IsNullOrEmpty(textBoxAmount.Text))
            {
                ShowError("Person, Service and Amount should not be empty!");
                return;
            }

            //replacing dot with comma for decimals
            var amountStr = textBoxAmount.Text.Replace(".", ",");
            decimal amount;
            //amount must be a positive decimal
            if (!decimal.TryParse(amountStr, out amount) || amount < 0)
            {
                ShowError("Amount should be positive decimal value");
                return;
            }
            #endregion

            tracker.AddNewExpense(textBoxPerson.Text, textBoxService.Text, amount);

            var dataRow = new DataRow { Person = textBoxPerson.Text, Service = textBoxService.Text, Amount = amount };
            data.Add(dataRow);

            textBoxTotal.Text = tracker.TotalAmount().ToString();
            textBoxAverage.Text = tracker.Avarage().ToString();
             
            var expenses =  tracker.GetExpenses();
            var result = new StringBuilder();

            foreach (var item in expenses)
            {
                result.Append(item.Key + ": ");
                result.Append(item.Value.Values.Sum());
                result.Append(Environment.NewLine);
            }
            textBoxExpenses.Text = result.ToString();
        }

        private void buttonCalculate_Click(object sender, RoutedEventArgs e)
        {
            var transactions = tracker.CalculateTransactions();
            var result = new StringBuilder();
            foreach (var tr in transactions)
            {
                result.Append(tr.from + " => ");
                result.Append(tr.to + " :");
                result.Append(tr.amount);
                result.Append(Environment.NewLine);
            }
            textBoxTransactions.Text = result.ToString();
        }

        private static void ShowError(string message)
        {
            MessageBox.Show("Error: " + message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public class DataRow
    {
        private string person;
        private string service;
        private decimal amount;

        public string Person
        {
            get { return person; }
            set { person = value; }
        }
        public string Service
        {
            get { return service; }
            set { service = value; }
        }
        public decimal Amount
        {
            get { return amount; }
            set { amount = value; }
        }
    }
}
