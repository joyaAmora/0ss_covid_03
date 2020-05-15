using BillingManagement.Business;
using BillingManagement.Models;
using BillingManagement.UI.ViewModels.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Windows;

namespace BillingManagement.UI.ViewModels
{
	class MainViewModel : BaseViewModel
    {
		private BaseViewModel _vm;
		private string searchCriteria;
		private BillingManagementContext db;
		private ObservableCollection<Customer> customers;
		private ObservableCollection<Invoice> invoices;
		readonly CustomersDataService customersDataService = new CustomersDataService();
		CustomerViewModel customerViewModel;
		InvoiceViewModel invoiceViewModel;

		public BaseViewModel VM
		{
			get { return _vm; }
			set {
				_vm = value;
				OnPropertyChanged();
			}
		}

		public string SearchCriteria
		{
			get { return searchCriteria; }
			set { 
				searchCriteria = value;
				OnPropertyChanged();
			}
		}

		public BillingManagementContext Db 
		{
			get => db;
			set
			{
				db = value;
				OnPropertyChanged();
			} 
		}

		public ObservableCollection<Customer> Customers
		{
			get => customers;
			set 
			{ 
				customers = value;
				OnPropertyChanged();
			}
		}

		public ObservableCollection<Invoice> Invoices
		{
			get => invoices;
			set
			{
				invoices = value;
				OnPropertyChanged();
			}
		}


		public ChangeViewCommand ChangeViewCommand { get; set; }
		public DelegateCommand<object> AddNewItemCommand { get; private set; }
		public DelegateCommand<Invoice> DisplayInvoiceCommand { get; private set; }
		public DelegateCommand<Customer> DisplayCustomerCommand { get; private set; }
		public DelegateCommand<Customer> AddInvoiceToCustomerCommand { get; private set; }
		public RelayCommand<Customer> SearchCommand { get; set; }


		public MainViewModel()
		{
			db = new BillingManagementContext();

			fillDB();

			ChangeViewCommand = new ChangeViewCommand(ChangeView);
			DisplayInvoiceCommand = new DelegateCommand<Invoice>(DisplayInvoice);
			DisplayCustomerCommand = new DelegateCommand<Customer>(DisplayCustomer);

			AddNewItemCommand = new DelegateCommand<object>(AddNewItem, CanAddNewItem);
			AddInvoiceToCustomerCommand = new DelegateCommand<Customer>(AddInvoiceToCustomer);

			SearchCommand = new RelayCommand<Customer>(SearchCustomer, CanAddNewItem);

			customerViewModel = new CustomerViewModel();
			invoiceViewModel = new InvoiceViewModel(customerViewModel.Customers);

			customers = new ObservableCollection<Customer>();
			invoices = new ObservableCollection<Invoice>();
			

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
			int output;
			List<Customer> FoundCustomers = new List<Customer>();
			Customer SelectedCustomer = new Customer();
			if (Int32.TryParse(input,out output))
			{
				Customers.Clear();
				customerViewModel.SelectedCustomer = db.Customers.Find(output);
				if (customerViewModel.SelectedCustomer != null)
				{
					Customers.Add(customerViewModel.SelectedCustomer);
				}
				else
				{
					MessageBox.Show("Aucun client avec cet ID");
				}
			}
			else
			{
				FoundCustomers = db.Customers.Where(c => c.Name.ToUpper().StartsWith(input.ToUpper()) || c.LastName.ToUpper().StartsWith(input.ToUpper())).ToList();
				Customers.Clear();

				if (FoundCustomers.Count > 0)
				{
					foreach (Customer c in FoundCustomers)
					{
						Customers.Add(c);
					}
					Customers = new ObservableCollection<Customer>(Customers.OrderBy(c => c.LastName));
					customerViewModel.Customers = Customers;
					customerViewModel.SelectedCustomer = Customers.First();
				}
				else
				{
					customerViewModel.Customers = new ObservableCollection<Customer>(customersDataService.GetAll().ToList());
					customerViewModel.SelectedCustomer = Customers.First<Customer>();
					MessageBox.Show("Aucun contact trouvé, retour à la liste initiale");
				}

			}
		}
		
		private void fillDB()
		{
			if (db.Customers.Count() <= 0)
			{
				List<Customer> Customers = new CustomersDataService().GetAll().ToList();
				List<Invoice> Invoices = new InvoicesDataService(Customers).GetAll().ToList();

				foreach (Customer c in Customers)
				{
					db.Customers.Add(c);
					db.SaveChanges();
				}

			}

		}

		public void GetDataDB(object item)
		{
			List<Customer> CustomersList = db.Customers.ToList();
			Customers.Clear();
			foreach (Customer c in CustomersList)
				Customers.Add(c);

			Customers = new ObservableCollection<Customer>(Customers.OrderBy(c => c.LastName));

			if (customerViewModel != null) customerViewModel.Customers = Customers;
			if (customerViewModel != null) customerViewModel.SelectedCustomer = Customers.First();



			//Invoices
			List<Invoice> InvoicesList = db.Invoices.ToList();
			Invoices.Clear();
			foreach (Invoice i in InvoicesList)
				Invoices.Add(i);
			if (invoiceViewModel != null) invoiceViewModel.SelectedInvoice = Invoices.First();
		}
	}

}



