using Avalonia.Threading;
using CafeAvalonia.Models;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CafeAvalonia.Views;

namespace CafeAvalonia.ViewModels;

public partial class MainWindowViewModel : ReactiveObject
{
    private readonly BdcafeContext _dbContext;
    private User _currentUser;

    public string Login
    {
        get => _login;
        set => this.RaiseAndSetIfChanged(ref _login, value);
    }
    private string _login = "";

    public string Password
    {
        get => _password;
        set => this.RaiseAndSetIfChanged(ref _password, value);
    }
    private string _password = "";

    public ReactiveCommand<Unit, Unit> LoginCommand { get; }

    public MainWindowViewModel(BdcafeContext dbContext)
    {
        _dbContext = dbContext ?? new BdcafeContext();

        var canLogin = this.WhenAnyValue(
            x => x.Login,
            x => x.Password,
            (login, password) => !string.IsNullOrWhiteSpace(login) && !string.IsNullOrWhiteSpace(password));

        LoginCommand = ReactiveCommand.CreateFromTask(LoginAsync, canLogin);
    }

    private async Task LoginAsync()
    {
        var (isValid, user) = await CheckUserCredentialsAsync(Login, Password);
        if (!isValid) return;

        _currentUser = user;

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            switch (user.FkEmployee?.Speciality?.ToLower())
            {
                case "администратор":
                    var adminWindow = new AdminWindow(); 
                    adminWindow.Show();
                    break;
                case "официант":
                case "повар":
                    var ordersWindow = new OrdersWindow(_currentUser); 
                    ordersWindow.Show();
                    break;
            }
        });
    }


    private async Task<(bool IsValid, User User)> CheckUserCredentialsAsync(string login, string password)
    {
        var user = await _dbContext.Users
            .Include(u => u.FkEmployee)
            .FirstOrDefaultAsync(u => u.Login == login);

        if (user == null || user.Password != password)
            return (false, null);

        return (true, user);
    }

    // Публичные свойства для проверки прав в других VM
    public bool IsAdmin => _currentUser?.FkEmployee?.Speciality?.ToLower() == "администратор";
    public bool IsWaiter => _currentUser?.FkEmployee?.Speciality?.ToLower() == "официант";
    public bool IsCook => _currentUser?.FkEmployee?.Speciality?.ToLower() == "повар";

    public User CurrentUser => _currentUser;
}
