using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CafeAvalonia.Models;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace CafeAvalonia.ViewModels
{
    public partial class ShiftsViewModel : ReactiveObject
    {
        private readonly BdcafeContext _dbContext;
        private readonly User _currentUser;
        private Shift? _selectedShift;

        public ObservableCollection<Shift> Shifts { get; } = new();
        public ObservableCollection<Employee> AvailableEmployees { get; } = new();

        public Shift? SelectedShift
        {
            get => _selectedShift;
            set => this.RaiseAndSetIfChanged(ref _selectedShift, value);
        }

        public ReactiveCommand<Unit, Unit> CreateShift5DaysCommand { get; }
        public ReactiveCommand<Unit, Unit> RefreshShiftsCommand { get; }
        public ReactiveCommand<Employee, Unit> AssignEmployeeCommand { get; }
        public ReactiveCommand<Unit, Unit> AssignToTableCommand { get; }
        public ReactiveCommand<Shift, Unit> ViewShiftDetailsCommand { get; }

    
        public ShiftsViewModel(User currentUser, BdcafeContext dbContext)
        {
            _currentUser = currentUser;
            _dbContext = dbContext ?? new BdcafeContext();

           

            CreateShift5DaysCommand = ReactiveCommand.CreateFromTask(CreateShift5DaysAsync);
            RefreshShiftsCommand = ReactiveCommand.CreateFromTask(LoadShiftsAsync);
            AssignEmployeeCommand = ReactiveCommand.Create<Employee>(AssignEmployee);
            AssignToTableCommand = ReactiveCommand.CreateFromTask(AssignToTableAsync);
            ViewShiftDetailsCommand = ReactiveCommand.CreateFromTask<Shift>(ViewShiftDetailsAsync);

            _ = LoadShiftsAsync();
            _ = LoadAvailableEmployeesAsync();
        }
        private async Task ViewShiftDetailsAsync(Shift shift)
        {
            var shiftDetailsWindow = new ShiftDetailsWindow();
            shiftDetailsWindow.DataContext = new ShiftDetailsViewModel(shift, _dbContext);
            shiftDetailsWindow.Show(); 
        }

        private async Task LoadShiftsAsync()
        {
            try
            {
                var shifts = await _dbContext.Shifts
                    .Include(s => s.Shiftassignments)
                    .ThenInclude(sa => sa.FkEmployee)
                    .OrderBy(s => s.DateStart)
                    .ToListAsync();

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Shifts.Clear();
                    foreach (var shift in shifts)
                        Shifts.Add(shift);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки смен: {ex.Message}");
            }
        }

        private async Task CreateShift5DaysAsync()
        {
            var dialog = new ShiftCreationDialogWindow();
            dialog.DataContext = new ShiftCreationDialogViewModel();
            dialog.Show(); 
            await LoadShiftsAsync();
        }

        private async Task LoadAvailableEmployeesAsync()
        {
            var employees = await _dbContext.Employees
                .Where(e => e.Speciality == "официант")
                .ToListAsync();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                AvailableEmployees.Clear();
                foreach (var emp in employees)
                    AvailableEmployees.Add(emp);
            });
        }

        private void AssignEmployee(Employee employee)
        {
            if (SelectedShift != null)
            {
                var assignment = new Shiftassignment
                {
                    FkShiftsid = SelectedShift.Id,
                    FkEmployeeid = employee.Id
                };
                _dbContext.Shiftassignments.Add(assignment);
                _dbContext.SaveChanges();
                LoadShiftsAsync();
            }
        }

        private async Task AssignToTableAsync()
        {
            // Логика назначения официанта на столик
            // Пока заглушка
            await Task.CompletedTask;
        }
    }
}

