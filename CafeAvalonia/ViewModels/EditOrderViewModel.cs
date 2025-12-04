using Avalonia.Controls;
using CafeAvalonia.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace CafeAvalonia.ViewModels
{
    public partial class EditOrderViewModel : ReactiveObject
    {
        private Order _order;
        private Window _window;

        public Order Order
        {
            get => _order;
            set => this.RaiseAndSetIfChanged(ref _order, value);
        }

        public List<OrderStatus> AvailableStatuses { get; } = new List<OrderStatus>
        {
            OrderStatus.принят,
            OrderStatus.готовится,
            OrderStatus.готов,
            OrderStatus.оплачен
        };

        private OrderStatus _selectedStatus;
        public OrderStatus SelectedStatus
        {
            get => _selectedStatus;
            set => this.RaiseAndSetIfChanged(ref _selectedStatus, value);
        }

        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        public EditOrderViewModel(Order order, Window window)
        {
            _order = order;
            _window = window;

            SelectedStatus = order.Status switch
            {
                "принят" => OrderStatus.принят,
                "готовится" => OrderStatus.готовится,
                "готов" => OrderStatus.готов,
                "оплачен" => OrderStatus.оплачен,
                _ => OrderStatus.принят
            };

            SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
            CancelCommand = ReactiveCommand.Create(() => _window.Close());
        }

        private async Task SaveAsync()
        {
            try
            {
                Order.Status = SelectedStatus.ToString();
                // Здесь сохранение данных, например через DbContext
                // await dbContext.SaveChangesAsync();

                _window.Close();
            }
            catch (Exception ex)
            {
                // Обработка ошибок
                Console.WriteLine($"Ошибка при сохранении: {ex.Message}");
            }
        }
    }
}
