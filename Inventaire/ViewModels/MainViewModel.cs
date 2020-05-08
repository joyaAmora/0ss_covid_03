using BillingManagement.Business;
using BillingManagement.Models;
using BillingManagement.UI.ViewModels.Commands;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace BillingManagement.UI.ViewModels
{
	class MainViewModel : BaseViewModel
    {
		private BaseViewModel _vm;
		BillingManagementContext db = new BillingManagementContext();
		ObservableCollection<Customer> dbCustomers;
		ObservableCollection<Invoice> dbInvoices;
		readonly CustomersDataService customersDataService = new CustomersDataService();

		public BaseViewModel VM
		{
			get { return _vm; }
			set {
				_vm = value;
				OnPropertyChanged();
			}
		}

		private string searchCriteria;

		public string SearchCriteria
		{
			get { return searchCriteria; }
			set { 
				searchCriteria = value;
				OnPropertyChanged();
			}
		}


		CustomerViewModel customerViewModel;
		InvoiceViewModel invoiceViewModel;

		public ChangeViewCommand ChangeViewCommand { get; set; }

		public DelegateCommand<object> AddNewItemCommand { get; private set; }

		public DelegateCommand<Invoice> DisplayInvoiceCommand { get; private set; }
		public DelegateCommand<Customer> DisplayCustomerCommand { get; private set; }

		public DelegateCommand<Customer> AddInvoiceToCustomerCommand { get; private set; }
		public RelayCommand<Customer> SearchCommand { get; set; }

		public MainViewModel()
		{
			ChangeViewCommand = new ChangeViewCommand(ChangeView);
			DisplayInvoiceCommand = new DelegateCommand<Invoice>(DisplayInvoice);
			DisplayCustomerCommand = new DelegateCommand<Customer>(DisplayCustomer);

			AddNewItemCommand = new DelegateCommand<object>(AddNewItem, CanAddNewItem);
			AddInvoiceToCustomerCommand = new DelegateCommand<Customer>(AddInvoiceToCustomer);

			SearchCommand = new RelayCommand<Customer>(SearchCustomer, CanAddNewItem);

			customerViewModel = new CustomerViewModel();
			invoiceViewModel = new InvoiceViewModel(customerViewModel.Customers);

			dbCustomers = new ObservableCollection<Customer>();
			dbInvoices = new ObservableCollection<Invoice>();

			var sort = dbCustomers.OrderBy(x => x.LastName);
			var CustomersSorted = new ObservableCollection<Customer>(sort);
			

			VM = customerViewModel;

		}

		private void ChangeView(string vm)
		{
			switch (vm)
			{
				case "customers":
					VM = customerViewModel;
					break;
				case "invoices":
					VM = invoiceViewModel;
					break;
			}
		}

		private void DisplayInvoice(Invoice invoice)
		{
			invoiceViewModel.SelectedInvoice = invoice;
			VM = invoiceViewModel;
		}

		private void DisplayCustomer(Customer customer)
		{
			customerViewModel.SelectedCustomer = customer;
			VM = customerViewModel;
		}

		private void AddInvoiceToCustomer(Customer c)
		{
			var invoice = new Invoice(c);
			c.Invoices.Add(invoice);
			DisplayInvoice(invoice);
		}

		private void AddNewItem (object item)
		{
			if (VM == customerViewModel)
			{
				var c = new Customer();
				customerViewModel.Customers.Add(c);
				customerViewModel.SelectedCustomer = c;
			}
		}

		private bool CanAddNewItem(object o)
		{
			bool result = false;

			result = VM == customerViewModel;
			return result;
		}

		private void SearchCustomer(object parameter)
		{
			string input = searchCriteria as string;
			IEnumerable<Customer> Customers = customerViewModel.Customers.ToList<Customer>();
			IEnumerable<Customer> FoundCustomers = customerViewModel.Customers.ToList<Customer>();
			Customer SelectedCustomer = customerViewModel.SelectedCustomer;

			FoundCustomers = Customers.Where(c => c.Name.ToUpper().StartsWith(input.ToUpper()) || c.LastName.ToUpper().StartsWith(input.ToUpper()));


			int list = FoundCustomers.Count();
			if(list > 0)
			{
				customerViewModel.Customers = new ObservableCollection<Customer>(FoundCustomers);
				customerViewModel.SelectedCustomer = FoundCustomers.First();
			}
			else
			{
				customerViewModel.Customers = new ObservableCollection<Customer> (customersDataService.GetAll().ToList());
				customerViewModel.SelectedCustomer = Customers.First<Customer>();
				MessageBox.Show("Aucun contact trouvé, retour à la liste initiale");
				
			}
		}

	}
}
