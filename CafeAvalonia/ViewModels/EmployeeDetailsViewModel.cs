using Avalonia.Controls;
using Avalonia.Media.Imaging; 
using Avalonia.Platform;
using CafeAvalonia.Models;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using System;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;

namespace CafeAvalonia.ViewModels
{
    public class EmployeeDetailsViewModel : ReactiveObject
    {
        public Employee Employee { get; }

        public string FullName => $"{Employee.Surname} {Employee.Name} {Employee.Patronymic}";
        public string PhotoPath => Employee.Photo;

        // Добавляем недостающие свойства
        public string Speciality => Employee.Speciality.ToString();
        public EmployeeStatus[] Statuses { get; } =
        {
            EmployeeStatus.Работает,
            EmployeeStatus.Уволен
        };
        public EmployeeStatus Status
        {
            get => _status;
            set
            {
                this.RaiseAndSetIfChanged(ref _status, value);
                _ = SaveStatusAsync(value);
            }
        }
        private EmployeeStatus _status = EmployeeStatus.Работает;
        public string StatusDisplay => Employee.Status.ToString();  


        public string ScanContract => Employee.ScanContract;

        public ReactiveCommand<Unit, Unit> CloseCommand { get; }

        private readonly Window _window;

        private Bitmap? _photoBitmap;
        public Bitmap? PhotoBitmap
        {
            get => _photoBitmap;
            set => this.RaiseAndSetIfChanged(ref _photoBitmap, value);
        }

        public EmployeeDetailsViewModel(Employee employee, Window window)
        {
            Employee = employee;
            _window = window;


            CloseCommand = ReactiveCommand.Create(() => _window.Close());
            LoadPhotoAsync();
        }
        private async Task SaveStatusAsync(EmployeeStatus newStatus)
        {
            try
            {
                // 1) обновляем сущность в памяти
                Employee.Status = newStatus;

                // 2) сохраняем в БД
                await using var db = new BdcafeContext();

                // Подгружаем нужного сотрудника и обновляем статус
                var emp = await db.Employees.FirstOrDefaultAsync(e => e.Id == Employee.Id);
                if (emp is null)
                    return;

                emp.Status = newStatus;
                await db.SaveChangesAsync();

                Console.WriteLine($"Статус сотрудника {emp.Id} сохранен: {newStatus}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения статуса: {ex.Message}");
            }
        }

        private async void LoadPhotoAsync()
        {
            try
            {
                if (!string.IsNullOrEmpty(Employee.Photo) && File.Exists(Employee.Photo))
                {
                    await using var stream = File.OpenRead(Employee.Photo);
                    PhotoBitmap = new Bitmap(stream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки фото: {ex.Message}");
            }
        }

    }
}
