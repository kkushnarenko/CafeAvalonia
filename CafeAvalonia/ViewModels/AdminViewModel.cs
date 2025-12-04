using Avalonia.Controls;
using Avalonia.Threading;
using CafeAvalonia.Models;
using CafeAvalonia.Views;
using Microsoft.EntityFrameworkCore;  
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;


namespace CafeAvalonia.ViewModels
{
    public class AdminViewModel : ReactiveObject
    {
        private readonly BdcafeContext _dbContext = new();

        private ObservableCollection<Employee> _employees = new();
        private Employee? _selectedEmployee;
        private Window? _parentWindow;
        private readonly User currentUser;


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

        public ReactiveCommand<Unit, Unit> CreateShiftReportCommand { get; }



        public AdminViewModel(Window? parentWindow = null)
        {
            _parentWindow = parentWindow;
            currentUser = LoadCurrentAdmin();
            AddEmployeeCommand = ReactiveCommand.CreateFromTask(AddEmployeeAsync);
            ShowDetailsCommand = ReactiveCommand.Create<Employee>(ShowEmployeeDetails);

            RefreshEmployeesCommand = ReactiveCommand.CreateFromTask(RefreshEmployeesAsync);

            LoadEmployees();

            NavigateToShiftsCommand = ReactiveCommand.Create(NavigateToShifts);
            NavigateToOrdersCommand = ReactiveCommand.Create(NavigateToOrders);
            CreateShiftReportCommand = ReactiveCommand.Create(OpenShiftReport);
        }
        private void OpenShiftReport()
        {
            var reportWindow = new ShiftReportWindow();
            reportWindow.Show();
        }


        private User LoadCurrentAdmin()
        {
            try
            {
                using var db = new BdcafeContext();

                // Берем ПЕРВОГО админа из users (ваш ID=1)
                var adminUser = db.Users
                    .Include(u => u.FkEmployee)
                    .FirstOrDefault(u => u.FkEmployee.Speciality == "администратор");

                if (adminUser != null)
                {
                    Console.WriteLine($"Загружен админ: {adminUser.FkEmployee.Surname} {adminUser.Login}");
                    return adminUser;
                }

                // Если админов нет - берем первого пользователя из БД
                var firstUser = db.Users
                    .Include(u => u.FkEmployee)
                    .FirstOrDefault();

                if (firstUser != null)
                {
                    Console.WriteLine($"Нет админа, взят первый юзер: {firstUser.Login}");
                    return firstUser;
                }

                throw new Exception("В БД нет пользователей!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                // Fallback для теста
                return new User
                {
                    FkEmployee = new Employee { Speciality = "администратор" }
                };
            }
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
