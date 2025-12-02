using CafeAvalonia.Models;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace CafeAvalonia.ViewModels
{

    public class AddOrderViewModel : ReactiveObject
    {
        private readonly User _currentUser;

        // количество клиентов
        private int _clientsCount = 1;
        public int ClientsCount
        {
            get => _clientsCount;
            set => this.RaiseAndSetIfChanged(ref _clientsCount, value);
        }

        // столик
        private TableNumber _selectedTable = TableNumber.Стол1;
        public TableNumber SelectedTable
        {
            get => _selectedTable;
            set => this.RaiseAndSetIfChanged(ref _selectedTable, value);
        }
        public IEnumerable<TableNumber> Tables { get; } =
            (TableNumber[])Enum.GetValues(typeof(TableNumber));

        // выбор блюда
        private DishType _selectedDish = DishType.Цезарь;
        public DishType SelectedDish
        {
            get => _selectedDish;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedDish, value);
                Price = GetPriceByDish(value); // автоподстановка цены по блюду
            }
        }
        public IEnumerable<DishType> Dishes { get; } =
            (DishType[])Enum.GetValues(typeof(DishType));

        // цена
        private decimal _price;
        public decimal Price
        {
            get => _price;
            set => this.RaiseAndSetIfChanged(ref _price, value);
        }

        private decimal GetPriceByDish(DishType dish) => dish switch
        {
            DishType.Цезарь => 450m,
            DishType.ПиццаМаргарита => 500m,
            DishType.Борщ => 300m,
            DishType.Карбонара => 550m,
            DishType.Оливье => 350m,
            DishType.СушиСет => 800m,
            DishType.Шашлык => 700m,
            DishType.КуриныеКрылья => 400m,
            DishType.ГречкаСГрибами => 250m,
            DishType.ЗапечённыйКартофель => 200m,
            _ => 0m
        };

        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }
        public Action<Order?>? CloseAction { get; set; }

        public AddOrderViewModel(User currentUser)
        {
            _currentUser = currentUser;

            Price = GetPriceByDish(_selectedDish);

            SaveCommand = ReactiveCommand.Create(Save);

            // Подписка на ошибки команды — чтобы не было незамеченных исключений
            SaveCommand.ThrownExceptions.Subscribe(ex =>
            {
                Console.WriteLine("Ошибка SaveCommand: " + ex);
                // Здесь можно добавить уведомление пользователя, например с помощью MessageBox
                // или задать свойство ошибки, чтобы отобразить в UI
            });

            CancelCommand = ReactiveCommand.Create(() => CloseAction?.Invoke(null));
        }

        private void Save()
        {
            try
            {
                if (_currentUser.FkEmployee?.Id == null)
                    throw new InvalidOperationException("У пользователя нет привязанного сотрудника");

                var order = new Order
                {
                    // НЕ заполняй Id - EF сам сгенерирует!
                    FkEmployeeid = _currentUser.FkEmployee.Id,  // убери ? если уверен что не null
                    FkDishesid = (int)SelectedDish,
                    ClientsCount = ClientsCount,
                    TableNumber = (int)SelectedTable,
                    Price = Price,
                    Status = "принят"
                };

                CloseAction?.Invoke(order);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Детали: {ex.InnerException.Message}");
            }
        }

    }
}
