using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CafeAvalonia.Models;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CafeAvalonia.ViewModels;

public class AddEmployeeViewModel : ReactiveObject
{

    private string _surname = "";
    private string _name = "";
    private string _patronymic = "";
    private EmployeeStatus _status = EmployeeStatus.Работает;
    private string _photo = "";
    private string _scanContract = "";
    private string _login = "";
    private string _email = "";
    private string _password = "";
    private string _role = "официант"; // по умолчанию
    private string Speciality => Role;
    private Window? _window;

    // Свойства сотрудника
    public string Surname { get => _surname; set => this.RaiseAndSetIfChanged(ref _surname, value); }
    public string Name { get => _name; set => this.RaiseAndSetIfChanged(ref _name, value); }
    public string Patronymic { get => _patronymic; set => this.RaiseAndSetIfChanged(ref _patronymic, value); }
    public EmployeeStatus Status { get => _status; set => this.RaiseAndSetIfChanged(ref _status, value); }
    
    public string Photo { get => _photo; set => this.RaiseAndSetIfChanged(ref _photo, value); }
    public string ScanContract { get => _scanContract; set => this.RaiseAndSetIfChanged(ref _scanContract, value); }
  

    // Свойства пользователя
    public string Login { get => _login; set => this.RaiseAndSetIfChanged(ref _login, value); }
    public string Email { get => _email; set => this.RaiseAndSetIfChanged(ref _email, value); }
    public string Password { get => _password; set => this.RaiseAndSetIfChanged(ref _password, value); }
    public string[] Roles { get; } = { "повар", "официант" };
    public string Role { get => _role; set => this.RaiseAndSetIfChanged(ref _role, value); }

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    public ReactiveCommand<Unit, Unit> AddPhotoCommand { get; }
    public ReactiveCommand<Unit, Unit> AddContractCommand { get; }

    public Action<Employee>? OnEmployeeSaved { get; set; }


    public AddEmployeeViewModel()
    {
        SaveCommand = ReactiveCommand.CreateFromTask(SaveEmployeeAsync);
        CancelCommand = ReactiveCommand.Create(CloseWindow);
        AddPhotoCommand = ReactiveCommand.CreateFromTask(AddPhotoAsync);
        AddContractCommand = ReactiveCommand.CreateFromTask(AddContractAsync);

        SaveCommand.ThrownExceptions.Subscribe(ex =>
        {
            Console.WriteLine($"SaveCommand ошибка: {ex.Message}");
          
        });

        AddPhotoCommand.ThrownExceptions.Subscribe(ex =>
            Console.WriteLine($"AddPhotoCommand ошибка: {ex.Message}"));

        AddContractCommand.ThrownExceptions.Subscribe(ex =>
            Console.WriteLine($"AddContractCommand ошибка: {ex.Message}"));
    }

    public void SetWindow(Window window) => _window = window;

    private async Task AddPhotoAsync()
    {
        var files = await _window!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Выберите фото",
            AllowMultiple = false,
            FileTypeFilter = new[] { new FilePickerFileType("Images")
            {
                Patterns = new[] { "*.jpg", "*.jpeg", "*.png" },
                AppleUniformTypeIdentifiers = new[] { "public.image" },
                MimeTypes = new[] { "image/*" }
            } }
        });

        if (files.Any())
            Photo = files.First().Path.LocalPath;
    }

    private async Task AddContractAsync()
    {
        var files = await _window!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Выберите скан договора",
            AllowMultiple = false,
            FileTypeFilter = new[] { new FilePickerFileType("PDF")
            {
                Patterns = new[] { "*.pdf" },
                MimeTypes = new[] { "application/pdf" }
            } }
        });

        if (files.Any())
            ScanContract = files.First().Path.LocalPath;
    }

    private async Task SaveEmployeeAsync()
    {
        // Проверка обязательных полей
        try
        {
            // Проверка файлов
            if (string.IsNullOrWhiteSpace(Photo) || string.IsNullOrWhiteSpace(ScanContract) ||
                !File.Exists(Photo) || !File.Exists(ScanContract))
            {
                Console.WriteLine("Ошибка: файлы не выбраны");
                return;
            }

            using var dbContext = new BdcafeContext();


            string photoPath = CopyFileToAppFolder(Photo, "photos");
            string contractPath = CopyFileToAppFolder(ScanContract, "contracts");

            EmployeeSpeciality speciality;
            if (!Enum.TryParse<EmployeeSpeciality>(Role, true, out speciality))
            {
                speciality = EmployeeSpeciality.официант; // значение по умолчанию
            }

            


            // Создать сотрудника
            var employee = new Employee
            {
                Surname = Surname,
                Name = Name,
                Patronymic = Patronymic,
                Status = _status,
                Photo = photoPath,
                ScanContract = contractPath,
                Speciality = Role
            };


            await dbContext.Employees.AddAsync(employee);
            await dbContext.SaveChangesAsync();


            // Создать пользователя
            var user = new User
            {
                Login = Login,
                Email = Email,
                Password = Password, 
                FkEmployeeId = employee.Id // Связь с сотрудником
            };

            // Сохранить пользователя
            await dbContext.Users.AddAsync(user);
            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine("Ошибка сохранения в базе:");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                Console.WriteLine($"Entries count: {ex.Entries.Count}");

                foreach (var entry in ex.Entries)
                {
                    Console.WriteLine($"Entity type: {entry.Entity.GetType().Name}");
                    Console.WriteLine($"Entity state: {entry.State}");
                }
              
                throw;
            }

            OnEmployeeSaved?.Invoke(employee);
            CloseWindow();
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine("Ошибка сохранения в базе:");
            Console.WriteLine($"Message: {ex.Message}");
            Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
            Console.WriteLine($"Entries count: {ex.Entries.Count}");

            foreach (var entry in ex.Entries)
            {
                Console.WriteLine($"Entity type: {entry.Entity.GetType().Name}");
                Console.WriteLine($"Entity state: {entry.State}");
            }

            throw;
        }
    }
        
       
    

    private string CopyFileToAppFolder(string filePath, string folder)
    {
        // Копировать файл в папку приложения
        string targetFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folder);
        Directory.CreateDirectory(targetFolder);
        string fileName = Path.GetFileName(filePath);
        string targetPath = Path.Combine(targetFolder, fileName);
        File.Copy(filePath, targetPath, true);
        return targetPath;
    }


    private void CloseWindow() => _window?.Close();
}
