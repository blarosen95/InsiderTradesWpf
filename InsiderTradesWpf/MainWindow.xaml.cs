using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace InsiderTradesWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public TransactionList TransactionList = new TransactionList();
        public List<string> Cells = new List<string>();
        public ObservableCollection<Transaction> oc;
        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new DataObject();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            //Clear the transaction list so that it doesn't contain older data.
            TransactionList.Clear();
            Edgar edgar = new Edgar();
            try
            {
                Cells = await edgar.GetInfo(SymbolBox.Text);
                var subCells = Cells.ChunkBy(12);

                ParallelOptions options = new ParallelOptions {MaxDegreeOfParallelism = 3};
                //Skip the first item in the list (should just be column headers).
                Parallel.For(1, subCells.Count, options, async i =>
                {
                    var transaction = new Transaction(i, subCells[i][0], subCells[i][1], subCells[i][2],
                        subCells[i][3], subCells[i][4], subCells[i][5], subCells[i][6], subCells[i][7],
                        subCells[i][8],
                        subCells[i][9], subCells[i][10], subCells[i][11]);

                    TransactionList.Push(transaction);
                });
                oc = new ObservableCollection<Transaction>(TransactionList.AsParallel()
                    .OrderBy(transaction => transaction.SortingKey));
                OnPropertyChanged("Transactions");

                //TODO: Remove following line when alternative is realized
                DataGrid1.ItemsSource = oc;

                //Switch to ListPage if ready
                //TODO: ListPage not implemented yet
            }
            catch (HttpRequestException err)
            {
                MessageBox.Show(
                    $"Error: {err.Message}\nEither your internet is down (most likely), or the website/service needed is down (least likely).");
                //TODO: refine the above error message by testing user's internet connection.
            }
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}