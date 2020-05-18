using BillingManagement.Business;
using BillingManagement.Models;
using BillingManagement.UI.ViewModels.Commands;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace BillingManagement.UI.ViewModels
{
    public class CustomerViewModel : BaseViewModel
    {
        readonly CustomersDataService customersDataService = new CustomersDataService();

        private ObservableCollection<Customer> customers = new ObservableCollection<Customer>();
        private Customer selectedCustomer = new Customer();
        private BillingManagementContext dbCustomerVM;

        public ObservableCollection<Customer> Customers
        {
            get => customers;
            set
            {
                customers = value;
                OnPropertyChanged();
            }
        }

        public Customer SelectedCustomer
        {
            get => selectedCustomer;
            set
            {
                selectedCustomer = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand<Customer> DeleteCustomerCommand { get; private set; }
        public RelayCommand<Customer> AddCustomerCommand { get; private set; }

        public CustomerViewModel(BillingManagementContext db, ObservableCollection<Customer> c)
        {
            DeleteCustomerCommand = new RelayCommand<Customer>(DeleteCustomer, CanDeleteCustomer);
            AddCustomerCommand = new RelayCommand<Customer>(AddCustomer, CanAddCustomer);

            dbCustomerVM = db;
            customers = c;
            InitValues();
            selectedCustomer = customers.First();
            
        }

        private void InitValues()
        {
            Customers = new ObservableCollection<Customer>(customersDataService.GetAll());
            Debug.WriteLine(Customers.Count);
        }

        private void DeleteCustomer(Customer c)
        {
            var currentIndex = Customers.IndexOf(c);

            if (currentIndex > 0) currentIndex--;

            SelectedCustomer = Customers[currentIndex];

            Customers.Remove(c);
        }

        private void AddCustomer(Customer c)
        {
            dbCustomerVM.Update(c);
            dbCustomerVM.SaveChanges();
            Customers = new ObservableCollection<Customer>(Customers.OrderBy(c => c.Info));
        }
        private bool CanDeleteCustomer(Customer c)
        {
            if (c == null) return false;

            
            return c.Invoices.Count == 0;
        }
        private bool CanAddCustomer(Customer c)
        {
            if (c == null) return false;

            
            return true;
        }





    }
}
