using Avalonia.Threading;
using CafeAvalonia.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CafeAvalonia.ViewModels
{
    public class ShiftCreationDialogViewModel : ReactiveObject
    {
        public ObservableCollection<Employee> Waiters { get; } = new();
        public ObservableCollection<Employee> Cooks { get; } = new();

        public ReactiveCommand<Unit, Unit> CreateShiftsCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        private int _selectedWaitersCount = 2;
        private int _selectedCooksCount = 2;

        public int SelectedWaitersCount
        {
            get => _selectedWaitersCount;
            set => this.RaiseAndSetIfChanged(ref _selectedWaitersCount, value);
        }

        public int SelectedCooksCount
        {
            get => _selectedCooksCount;
            set => this.RaiseAndSetIfChanged(ref _selectedCooksCount, value);
        }

        public ShiftCreationDialogViewModel(BdcafeContext dbContext)
        {
            var context = dbContext ?? new BdcafeContext();

            // Загружаем официантов и поваров
            _ = Task.Run(async () =>
            {
                var waiters = await context.Employees
                    .Where(e => e.Speciality == "официант")
                    .ToListAsync();
                var cooks = await context.Employees
                    .Where(e => e.Speciality == "повар")
                    .ToListAsync();

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    foreach (var waiter in waiters) Waiters.Add(waiter);
                    foreach (var cook in cooks) Cooks.Add(cook);
                });
            });

            CreateShiftsCommand = ReactiveCommand.CreateFromTask(CreateShiftsAsync);
            CancelCommand = ReactiveCommand.CreateFromTask(async () => { });
        }

        private async Task CreateShiftsAsync()
        {
            if (SelectedWaitersCount + SelectedCooksCount < 4 || SelectedWaitersCount + SelectedCooksCount > 7)
                return; // Проверка требований

            var context = new BdcafeContext();
            var startDate = DateTime.Now.Date.AddDays(1);
            var waiters = Waiters.Take(SelectedWaitersCount).ToList();
            var cooks = Cooks.Take(SelectedCooksCount).ToList();

            for (int i = 0; i < 5; i++)
            {
                var shift = new Shift
                {
                    DateStart = startDate.AddDays(i),
                    DateFinis = startDate.AddDays(i).AddHours(10)
                };

                context.Shifts.Add(shift);
                await context.SaveChangesAsync();

                // Назначаем официантов
                foreach (var waiter in waiters)
                {
                    context.Shiftassignments.Add(new Shiftassignment
                    {
                        FkShiftsid = shift.Id,
                        FkEmployeeid = waiter.Id
                    });
                }

                // Назначаем поваров
                foreach (var cook in cooks)
                {
                    context.Shiftassignments.Add(new Shiftassignment
                    {
                        FkShiftsid = shift.Id,
                        FkEmployeeid = cook.Id
                    });
                }
                await context.SaveChangesAsync();
            }
        }
    }
}
