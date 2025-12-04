using Avalonia.Controls;
using Avalonia.Threading;
using CafeAvalonia.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CafeAvalonia.Views;

namespace CafeAvalonia.ViewModels;

public partial class OrderDetailsViewModel : ReactiveObject
{
    private readonly BdcafeContext _dbContext = new();
    private readonly User _currentUser;
    private Order _order;
    private Window? _window;

    private bool _isAdmin;
    public bool IsAdmin
    {
        get => _isAdmin;
        set => this.RaiseAndSetIfChanged(ref _isAdmin, value);
    }
    private bool _canEditOrder;
    public bool CanEditOrder
    {
        get => _canEditOrder;
        set => this.RaiseAndSetIfChanged(ref _canEditOrder, value);
    }

    public Order Order
    {
        get => _order;
        set => this.RaiseAndSetIfChanged(ref _order, value);
    }

    public List<OrderStatus> AvailableStatuses { get; }

    private OrderStatus _selectedStatus;
    public OrderStatus SelectedStatus
    {
        get => _selectedStatus;
        set => this.RaiseAndSetIfChanged(ref _selectedStatus, value);
    }

    public ReactiveCommand<Unit, Unit> UpdateStatusCommand { get; }

    public OrderDetailsViewModel(Order order, User currentUser, Window? window = null)
    {
        _order = order;
        _currentUser = currentUser;
        _window = window;

       
        if (!Enum.TryParse<OrderStatus>(order.Status ?? "", true, out var parsedStatus))
            parsedStatus = OrderStatus.принят;
        SelectedStatus = parsedStatus;

    
        var specialityStr = _currentUser.FkEmployee?.Speciality ?? "официант";
        IsAdmin = specialityStr.Contains("администратор", StringComparison.OrdinalIgnoreCase);

        CanEditOrder = IsAdmin && order.Status != "оплачен";

        AvailableStatuses = IsAdmin && order.Status != "оплачен"
            ? new List<OrderStatus> { OrderStatus.принят, OrderStatus.готовится, OrderStatus.готов, OrderStatus.оплачен }
            : specialityStr switch
            {
                var s when s.Contains("официант") => new List<OrderStatus> { OrderStatus.принят, OrderStatus.оплачен },
                var s when s.Contains("повар") => new List<OrderStatus> { OrderStatus.готовится, OrderStatus.готов },
                _ => new List<OrderStatus>()
            };

      
        UpdateStatusCommand = ReactiveCommand.CreateFromTask(UpdateStatusAsync,
            this.WhenAnyValue(vm => vm.SelectedStatus)
                .Select(status => AvailableStatuses.Contains(status)));

        EditOrderCommand = ReactiveCommand.Create(OnEditOrder,
            this.WhenAnyValue(x => x.IsAdmin));
    }

 
    public ReactiveCommand<Unit, Unit> EditOrderCommand { get; }

    private void OnEditOrder()
    {
        var editWindow = new EditOrderWindow();
        var editVm = new EditOrderViewModel(_order, editWindow);
        editWindow.DataContext = editVm;
        editWindow.Show();
    }



    private async Task UpdateStatusAsync()
    {
        try
        {
            _order.Status = SelectedStatus.ToString();
            _dbContext.Orders.Update(_order);
            await _dbContext.SaveChangesAsync();

            Console.WriteLine($"Статус заказа {_order.Id} изменён на: {SelectedStatus}");

            // Закрываем окно через UI поток
            Dispatcher.UIThread.Post(() => _window?.Close());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка обновления статуса: {ex.Message}");
        }
    }
}
