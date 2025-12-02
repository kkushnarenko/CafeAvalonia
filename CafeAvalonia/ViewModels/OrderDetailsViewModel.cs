using Avalonia.Controls;
using Avalonia.Threading;
using CafeAvalonia.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;

namespace CafeAvalonia.ViewModels;

public partial class OrderDetailsViewModel : ReactiveObject
{
    private readonly BdcafeContext _dbContext = new();
    private readonly User _currentUser;
    private Order _order;
    private Window? _window;

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

        // Попытка распарсить статус заказа из строки в enum
        if (!Enum.TryParse<OrderStatus>(order.Status ?? "", true, out var parsedStatus))
        {
            parsedStatus = OrderStatus.принят;
        }
        SelectedStatus = parsedStatus;

        // Определяем роль пользователя через enum
        if (!Enum.TryParse<EmployeeSpeciality>(_currentUser.FkEmployee?.Speciality ?? "", true, out var speciality))
        {
            speciality = EmployeeSpeciality.официант;
        }

        // Статусы согласно роли
        AvailableStatuses = speciality switch
        {
            EmployeeSpeciality.официант => new List<OrderStatus> { OrderStatus.принят, OrderStatus.оплачен },
            EmployeeSpeciality.повар => new List<OrderStatus> { OrderStatus.готовится, OrderStatus.готов },
            _ => new List<OrderStatus>()
        };

        UpdateStatusCommand = ReactiveCommand.CreateFromTask(UpdateStatusAsync,
            this.WhenAnyValue(vm => vm.SelectedStatus, status => AvailableStatuses.Contains(status)));
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
