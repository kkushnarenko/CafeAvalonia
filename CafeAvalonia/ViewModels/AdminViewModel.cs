using Avalonia.Controls;
using CafeAvalonia.Models;
using CafeAvalonia.Views;
using Microsoft.EntityFrameworkCore;  
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace CafeAvalonia.ViewModels
{
    public class AdminViewModel : ReactiveObject
    {
        private readonly BdcafeContext _dbContext = new();

        private ObservableCollection<Employee> _employees = new();
        private Employee? _selectedEmployee;
        private Window? _parentWindow;
        private User currentUser;


        public Action<Employee>? OnEmployeeSaved { get; set; }

        public ObservableCollection<Employee> Employees
        {
            get => _employees;
            set => this.RaiseAndSetIfChanged(ref _employees, value);
        }

        public Employee? SelectedEmployee
        {
            get => _selectedEmployee;
            set => this.RaiseAndSetIfChanged(ref _selectedEmployee, value);
        }

        public ReactiveCommand<Unit, Unit> AddEmployeeCommand { get; }
        public ReactiveCommand<Employee, Unit> ShowDetailsCommand { get; }

        public ReactiveCommand<Unit, Unit> NavigateToShiftsCommand { get; }
        public ReactiveCommand<Unit, Unit> NavigateToOrdersCommand { get; }

        public ReactiveCommand<Unit, Unit> RefreshEmployeesCommand { get; }


        public AdminViewModel(Window? parentWindow = null)
        {
            _parentWindow = parentWindow;
            AddEmployeeCommand = ReactiveCommand.CreateFromTask(AddEmployeeAsync);
            ShowDetailsCommand = ReactiveCommand.Create<Employee>(ShowEmployeeDetails);

            RefreshEmployeesCommand = ReactiveCommand.CreateFromTask(RefreshEmployeesAsync);

            LoadEmployees();

            NavigateToShiftsCommand = ReactiveCommand.Create(NavigateToShifts);
            NavigateToOrdersCommand = ReactiveCommand.Create(NavigateToOrders);
        }

        private async Task RefreshEmployeesAsync()
        {
            await LoadEmployees(); // Используем существующий метод
        }
        private void NavigateToShifts()
        {
            // Открыть окно смен
            var shiftsWindow = new ShiftsWindow(currentUser);  // окно смен
            shiftsWindow.Show();
           
        }

        private void NavigateToOrders()
        {
            // Открыть окно заказов
            var ordersWindow = new OrdersWindow(currentUser); 
            ordersWindow.Show();
           
        }
        private void ShowEmployeeDetails(Employee employee)
        {
            var detailsWindow = new EmployeeDetailsWindow();
            var vm = new EmployeeDetailsViewModel(employee, detailsWindow);
            detailsWindow.DataContext = vm;
            detailsWindow.Show();
        }
       
        private async Task LoadEmployees()
        {
            try
            {
                using var db = new BdcafeContext();
                await db.Database.EnsureCreatedAsync();

                var employees = await db.Employees.ToListAsync();
                Console.WriteLine($"Загружено сотрудников: {employees.Count}"); 

                Dispatcher.UIThread.Post(() =>
                {
                    Employees.Clear();
                    foreach (var emp in employees)
                    {
                        Console.WriteLine($"Добавлен: {emp.Surname} {emp.Name}"); 
                        Employees.Add(emp);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                // Тестовые данные
                Dispatcher.UIThread.Post(() =>
                {
                    Employees.Clear();
                    Employees.Add(new Employee
                    {
                        Id = 1,
                        Surname = "Иванов",
                        Name = "Иван",
                        Patronymic = "Иванович",
                        Speciality = "официант",
                        Status = "Работает"
                    });
                });
            }
        }


        private async Task AddEmployeeAsync()
        {
            var addVm = new AddEmployeeViewModel();

           
            addVm.OnEmployeeSaved += employee => Employees.Add(employee);

            var dialog = new AddEmployeeWindow { DataContext = addVm };

            if (_parentWindow != null)
            {
                await dialog.ShowDialog(_parentWindow);
            }
            else
            {
                dialog.Show();
            }
        }
    }
}
