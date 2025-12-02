using CafeAvalonia.Models;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace CafeAvalonia.ViewModels
{
    public partial class ShiftDetailsViewModel : ReactiveObject
    {
        public Shift CurrentShift { get; }
        public string ShiftTime => $"{CurrentShift.DateStart:dd.MM HH:mm} - {CurrentShift.DateFinis:dd.MM HH:mm}";

     
        public ObservableCollection<Employee> AllEmployees { get; } = new();
        public ObservableCollection<Employee> SelectedEmployees { get; } = new();

        public ObservableCollection<Employee> CookEmployees =>
            new(AllEmployees.Where(e => e.Speciality == "повар"));
        public ObservableCollection<Employee> WaiterEmployees =>
            new(AllEmployees.Where(e => e.Speciality == "официант"));

        public ReactiveCommand<Unit, Unit> SaveAssignmentsCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        private string _employeeCountMessage = "";
        public string EmployeeCountMessage
        {
            get => _employeeCountMessage;
            set => this.RaiseAndSetIfChanged(ref _employeeCountMessage, value);
        }

        public int EmployeeCount => SelectedEmployees.Count;

        private readonly BdcafeContext _dbContext;

        public ShiftDetailsViewModel(Shift shift, BdcafeContext dbContext)
        {
            CurrentShift = shift ?? throw new ArgumentNullException(nameof(shift));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

            LoadEmployeesAsync();
            SaveAssignmentsCommand = ReactiveCommand.CreateFromTask(SaveAssignmentsAsync);
            CancelCommand = ReactiveCommand.CreateFromTask(async () => { });

            this.WhenAnyValue(x => x.EmployeeCount)
                .Subscribe(_ => ValidateEmployeeCount());

            SelectedEmployees.CollectionChanged += OnSelectedEmployeesChanged;
        }

        private async Task LoadEmployeesAsync()
        {
           
            var employees = await _dbContext.Employees
                .Where(e => e.Speciality != "администратор")
                .ToListAsync();

            AllEmployees.Clear();
            foreach (var emp in employees)
                AllEmployees.Add(emp);

            // Загрузить уже назначенных (только поваров/официантов)
            var assignedEmployeeIds = CurrentShift.Shiftassignments?.Select(sa => sa.FkEmployeeid).ToHashSet() ?? new();
            var selected = AllEmployees.Where(e => assignedEmployeeIds.Contains(e.Id)).ToList();
            SelectedEmployees.Clear();
            foreach (var emp in selected)
                SelectedEmployees.Add(emp);
        }

        private void OnSelectedEmployeesChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            ValidateEmployeeCount();
        }

        private void ValidateEmployeeCount()
        {
            if (EmployeeCount == 0)
            {
                EmployeeCountMessage = "";
                return;
            }

            if (EmployeeCount < 4)
            {
                EmployeeCountMessage = $" Минимум 4 сотрудника! Выбрано: {EmployeeCount}";
                return;
            }

            if (EmployeeCount > 7)
            {
                EmployeeCountMessage = $" Максимум 7 сотрудников! Выбрано: {EmployeeCount}";
                return;
            }

            EmployeeCountMessage = $" Выбрано {EmployeeCount} сотрудников (4-7)";
        }

        private async Task SaveAssignmentsAsync()
        {
            if (EmployeeCount < 4 || EmployeeCount > 7)
            {
                EmployeeCountMessage = " Выберите от 4 до 7 сотрудников!";
                return;
            }

            try
            {
                var oldAssignments = _dbContext.Shiftassignments.Where(sa => sa.FkShiftsid == CurrentShift.Id);
                _dbContext.Shiftassignments.RemoveRange(oldAssignments);

                foreach (var emp in SelectedEmployees)
                {
                    _dbContext.Shiftassignments.Add(new Shiftassignment
                    {
                        FkShiftsid = CurrentShift.Id,
                        FkEmployeeid = emp.Id
                    });
                }

                await _dbContext.SaveChangesAsync();
                EmployeeCountMessage = $" Назначения сохранены! ({EmployeeCount} сотрудников)";
            }
            catch (Exception ex)
            {
                EmployeeCountMessage = $" Ошибка: {ex.Message}";
            }
        }
    }
}
