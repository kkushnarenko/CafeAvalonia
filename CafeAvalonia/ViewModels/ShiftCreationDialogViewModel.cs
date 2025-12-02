using Avalonia.Threading;
using CafeAvalonia.Models;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CafeAvalonia.ViewModels
{
    public class ShiftCreationDialogViewModel : ReactiveObject
    {
        public ReactiveCommand<Unit, Unit> CreateShiftCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        private DateTimeOffset? _shiftDate;
        public DateTimeOffset? ShiftDate
        {
            get => _shiftDate;
            set => this.RaiseAndSetIfChanged(ref _shiftDate, value);
        }

        private TimeSpan? _shiftStartTime = TimeSpan.FromHours(10);
        public TimeSpan? ShiftStartTime
        {
            get => _shiftStartTime;
            set => this.RaiseAndSetIfChanged(ref _shiftStartTime, value);
        }

        private int _shiftDurationHours = 10;
        public int ShiftDurationHours
        {
            get => _shiftDurationHours;
            set => this.RaiseAndSetIfChanged(ref _shiftDurationHours, value);
        }

   
        private string _dateValidationMessage = "";
        public string DateValidationMessage
        {
            get => _dateValidationMessage;
            set => this.RaiseAndSetIfChanged(ref _dateValidationMessage, value);
        }

        public ShiftCreationDialogViewModel()
        {
          
            ShiftDate = DateTimeOffset.Now.Date.AddDays(1);

      
            this.WhenAnyValue(x => x.ShiftDate)
                .Subscribe(_ => ValidateDate());

            CreateShiftCommand = ReactiveCommand.CreateFromTask(CreateShiftAsync);
            CancelCommand = ReactiveCommand.CreateFromTask(async () => { });
        }

     
        private void ValidateDate()
        {
            var today = DateTimeOffset.Now.Date;
            var maxDate = today.AddDays(5);

            if (ShiftDate == null)
            {
                DateValidationMessage = "";
                return;
            }

            if (ShiftDate.Value.Date < today)
            {
                DateValidationMessage = "Дата не может быть раньше сегодняшнего дня";
                return;
            }

            if (ShiftDate.Value.Date > maxDate)
            {
                DateValidationMessage = $" Дата смены только на 5 дней вперед! Максимум: {maxDate:dd.MM.yyyy}";
                return;
            }

            // ✅ Корректная дата
            DateValidationMessage = " Дата корректна";
        }

        private async Task CreateShiftAsync()
        {
            var today = DateTimeOffset.Now.Date;
            var maxDate = today.AddDays(5);

            if (ShiftDate == null || ShiftDate.Value.Date < today || ShiftDate.Value.Date > maxDate)
            {
                DateValidationMessage = " Выберите корректную дату!";
                return;
            }

            try
            {
                using var context = new BdcafeContext();
                DateTime startDateTime = ShiftDate.Value.DateTime + (ShiftStartTime ?? TimeSpan.Zero);
                DateTime endDateTime = startDateTime.AddHours(ShiftDurationHours);

                var shift = new Shift
                {
                    DateStart = startDateTime,
                    DateFinis = endDateTime
                };

                context.Shifts.Add(shift);
                await context.SaveChangesAsync();

                DateValidationMessage = " Смена создана успешно!";
            }
            catch (Exception ex)
            {
                DateValidationMessage = $" Ошибка: {ex.Message}";
            }
        }
    }
}
