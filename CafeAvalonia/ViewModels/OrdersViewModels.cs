using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CafeAvalonia;
using CafeAvalonia.Models;
using CafeAvalonia.ViewModels;
using CafeAvalonia.Views;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;

namespace CafeAvalonia.ViewModels
{
    public class OrdersViewModel : ReactiveObject
    {
        private readonly BdcafeContext _dbContext = new();
        private readonly User _currentUser;
        private Order? _selectedOrder;

        public ObservableCollection<Order> Orders { get; } = new();

        public Order? SelectedOrder
        {
            get => _selectedOrder;
            set => this.RaiseAndSetIfChanged(ref _selectedOrder, value);
        }

        public bool IsWaiter { get; }   // только официант
        public ReactiveCommand<Unit, Unit> AddOrderCommand { get; }
        public ReactiveCommand<Order, Unit> ShowOrderDetailsCommand { get; }
        public ReactiveCommand<Unit, Unit> RefreshOrdersCommand { get; }

        public OrdersViewModel(User currentUser)
        {
            _currentUser = currentUser;

            // определяем роль
            if (!Enum.TryParse<EmployeeSpeciality>(_currentUser.FkEmployee?.Speciality ?? "",
                    true, out var speciality))
                speciality = EmployeeSpeciality.официант;

            IsWaiter = speciality == EmployeeSpeciality.официант;

            ShowOrderDetailsCommand = ReactiveCommand.Create<Order>(ShowOrderDetails);
            AddOrderCommand = ReactiveCommand.CreateFromTask(AddOrderAsync,
                this.WhenAnyValue(vm => vm.IsWaiter)); 
            RefreshOrdersCommand = ReactiveCommand.CreateFromTask(RefreshOrdersAsync,
                this.WhenAnyValue(vm => vm.IsWaiter));

            _ = LoadOrdersAsync();
        }
        private async Task RefreshOrdersAsync()
        {
            await LoadOrdersAsync();
            Console.WriteLine("Список заказов обновлён");
        }

        private async Task AddOrderAsync()
        {
            var win = new AddOrderWindow();
            var vm = new AddOrderViewModel(_currentUser);
            win.DataContext = vm;

            Order? created = null;
            vm.CloseAction = o =>
            {
                created = o;
                win.Close();
            };

            // owner возьми из ApplicationLifetime, как ты делал ранее
            var lifetime = Avalonia.Application.Current?.ApplicationLifetime
                           as IClassicDesktopStyleApplicationLifetime;
            var owner = lifetime?.MainWindow;

            await win.ShowDialog(owner);

            if (created != null)
            {
                _dbContext.Orders.Add(created);
                await _dbContext.SaveChangesAsync();
                await Dispatcher.UIThread.InvokeAsync(() => Orders.Add(created));
            }
        }

        private async Task LoadOrdersAsync()
        {
            try
            {
                Console.WriteLine(" Загрузка заказов...");

                using var db = new BdcafeContext();
                await db.Database.EnsureCreatedAsync();

                var orders = await db.Orders
                    .Include(o => o.FkDishes)
                    .Include(o => o.FkEmployee)
                    .ToListAsync();

                Console.WriteLine($" Загружено заказов из БД: {orders.Count}");

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Orders.Clear();
                    foreach (var order in orders)
                    {
                        Orders.Add(order);
                        Console.WriteLine($" Добавлен заказ {order.Id}: {order.FkDishes?.Name}");
                        Console.WriteLine($" Orders.Count = {Orders.Count}");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Ошибка БД: {ex.Message}");

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Orders.Clear();
                    Orders.Add(new Order
                    {
                        Id = 999,
                        FkDishes = new Dish { Name = " Тест Шаурма" },
                        FkEmployee = new Employee { Surname = "Тестов" },
                        TableNumber = 42,
                        ClientsCount = 3,
                        Status = "принят",
                        Price = 999
                    });
                    Console.WriteLine(" Добавлен ТЕСТОВЫЙ заказ!");
                });
            }
        }


        private void ShowOrderDetails(Order order)
        {
            var detailsWindow = new OrderDetailsWindow();

            detailsWindow.DataContext = new OrderDetailsViewModel(order, _currentUser, detailsWindow);

            detailsWindow.Show();
        }
    }
}
