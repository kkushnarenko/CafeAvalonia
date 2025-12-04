using Avalonia.Controls;
using CafeAvalonia.Services;
using CafeAvalonia.Models;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace CafeAvalonia.ViewModels;
public class ShiftReportViewModel : ReactiveObject
{
    private readonly Window? _parent;
    // 1) Все заказы смены
    private ObservableCollection<Order> _ordersInShift = new();
    public ObservableCollection<Order> OrdersInShift
    {
        get => _ordersInShift;
        set => this.RaiseAndSetIfChanged(ref _ordersInShift, value);
    }

    // 2) Только оплаченные заказы смены
    private ObservableCollection<Order> _paidOrdersInShift = new();
    public ObservableCollection<Order> PaidOrdersInShift
    {
        get => _paidOrdersInShift;
        set => this.RaiseAndSetIfChanged(ref _paidOrdersInShift, value);
    }

    // Отдельные суммы
    private decimal _totalRevenueAll;
    public decimal TotalRevenueAll
    {
        get => _totalRevenueAll;
        set => this.RaiseAndSetIfChanged(ref _totalRevenueAll, value);
    }

    private decimal _totalRevenuePaid;
    public decimal TotalRevenuePaid
    {
        get => _totalRevenuePaid;
        set => this.RaiseAndSetIfChanged(ref _totalRevenuePaid, value);
    }


    private Shift? _currentShift;
    public Shift? CurrentShift
    {
        get => _currentShift;
        set => this.RaiseAndSetIfChanged(ref _currentShift, value);
    }

    public ReactiveCommand<Unit, Unit> LoadCurrentShiftReportCommand { get; }
    public ReactiveCommand<Unit, Unit> RefreshReportCommand { get; }
    public ReactiveCommand<Unit, Unit> ExportPdfCommand { get; }
    public ReactiveCommand<Unit, Unit> ExportXlsxCommand { get; }


    public ShiftReportViewModel(Window? parentWindow = null)
    {
        _parent = parentWindow;

        LoadCurrentShiftReportCommand = ReactiveCommand.CreateFromTask(LoadCurrentShiftReportAsync);
        RefreshReportCommand = ReactiveCommand.CreateFromTask(RefreshReportAsync);

        ExportPdfCommand = ReactiveCommand.CreateFromTask(ExportPdfAsync);
        ExportXlsxCommand = ReactiveCommand.CreateFromTask(ExportXlsxAsync);

        _ = LoadCurrentShiftReportAsync();
    }
    private async Task LoadCurrentShiftReportAsync()
    {
        Console.WriteLine("Загрузка отчета по смене...");

        try
        {
            using var db = new BdcafeContext();

            // 1. Активная смена
            CurrentShift = await db.Shifts
                .FirstOrDefaultAsync(s => s.DateStart <= DateTime.Now && s.DateFinis > DateTime.Now);

            Console.WriteLine(CurrentShift is null
                ? " Нет активной смены"
                : $" Смена {CurrentShift.Id}: {CurrentShift.DateStart} - {CurrentShift.DateFinis}");

            if (CurrentShift == null)
            {
                ClearAll();
                return;
            }

            // 2. Сотрудники смены
            var shiftEmployeeIds = await db.Shiftassignments
                .Where(sa => sa.FkShiftsid == CurrentShift.Id)
                .Select(sa => sa.FkEmployeeid)
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .ToListAsync();

            Console.WriteLine($"Сотрудники смены: [{string.Join(", ", shiftEmployeeIds)}]");

            if (!shiftEmployeeIds.Any())
            {
                Console.WriteLine(" Нет сотрудников в смене");
                ClearAll();
                return;
            }

            // 3. Все заказы смены
            Console.WriteLine(
                $"Фильтр времени: {CurrentShift.DateStart:dd.MM HH:mm:ss} .. {CurrentShift.DateFinis:dd.MM HH:mm:ss}");

            var shiftOrders = await db.Orders
                .Include(o => o.FkEmployee)
                .Include(o => o.FkDishes)
                .Where(o => o.FkEmployeeid.HasValue
                         && shiftEmployeeIds.Contains(o.FkEmployeeid.Value)
                         && o.CreatedAt >= CurrentShift.DateStart
                         && o.CreatedAt <= CurrentShift.DateFinis)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            Console.WriteLine($"Найдено заказов смены: {shiftOrders.Count}");
            foreach (var o in shiftOrders)
                Console.WriteLine($"  Заказ {o.Id}: {o.CreatedAt} | emp={o.FkEmployeeid} | {o.Status}");

            OrdersInShift = new ObservableCollection<Order>(shiftOrders);
            TotalRevenueAll = shiftOrders.Sum(o => o.Price);

            var paidOrders = shiftOrders.Where(o => o.Status == "оплачен").ToList();
            PaidOrdersInShift = new ObservableCollection<Order>(paidOrders);
            TotalRevenuePaid = paidOrders.Sum(o => o.Price);

            Console.WriteLine($"Оплаченных заказов: {paidOrders.Count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Ошибка: {ex}");
            ClearAll();
        }
    }

    private async Task<string?> AskSavePathAsync(string defaultName, string extension, string filterName)
    {
        if (_parent is null)
            return null;

        var sfd = new SaveFileDialog
        {
            InitialFileName = defaultName,
            Filters =
            {
                new FileDialogFilter
                {
                    Name = filterName,
                    Extensions = { extension.TrimStart('.') }
                }
            }
        };

        return await sfd.ShowAsync(_parent);
    }


    private async Task ExportPdfAsync()
    {
        var path = await AskSavePathAsync("Отчет_по_смене.pdf", ".pdf", "PDF");
        if (string.IsNullOrWhiteSpace(path))
            return;

        // здесь вызов генерации PDF
        await ReportExporter.ExportToPdfAsync(path, this);
    }

    private async Task ExportXlsxAsync()
    {
        var path = await AskSavePathAsync("Отчет_по_смене.xlsx", ".xlsx", "Excel");
        if (string.IsNullOrWhiteSpace(path))
            return;

        // вызов генерации Excel
        await ReportExporter.ExportToXlsxAsync(path, this);
    }


    private void ClearAll()
    {
        OrdersInShift.Clear();
        PaidOrdersInShift.Clear();
        TotalRevenueAll = 0;
        TotalRevenuePaid = 0;
    }


    private Task RefreshReportAsync() => LoadCurrentShiftReportAsync();
}

