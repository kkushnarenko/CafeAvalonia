using Avalonia.Controls;
using Avalonia.Threading;
using CafeAvalonia.Models;
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
        private Order? _selectedOrder;
        private readonly User _currentUser;

        public ObservableCollection<Order> Orders { get; } = new();

        public Order? SelectedOrder
        {
            get => _selectedOrder;
            set => this.RaiseAndSetIfChanged(ref _selectedOrder, value);
        }

        public ReactiveCommand<Order, Unit> ShowOrderDetailsCommand { get; }

        public OrdersViewModel(User currentUser)
        {
            _currentUser = currentUser;
            ShowOrderDetailsCommand = ReactiveCommand.Create<Order>(ShowOrderDetails);

            Console.WriteLine(" OrdersViewModel создан"); 

            _ = LoadOrdersAsync();
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
