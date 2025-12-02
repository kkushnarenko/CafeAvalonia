create table employee
(
	ID serial primary key,
	Surname Varchar(50) not null,
	Name Varchar(50) not null,
	Patronymic Varchar(50) not null,
	Status Varchar(50) not null check (Status = 'Уволен' or Status = 'Работает'),
	Photo Text not null,
	Scan_Contract text not null,
	Speciality text not null check (Speciality = 'повар' or Speciality = 'официант' OR Speciality = 'администратор')
); 
 

create table users
(
	ID serial primary key,
	Login varchar(100) NOT NULL,
	Email varchar(50) NOT NULL,
	Password varchar(50) NOT NULL,
	FK_Employee_ID int references employee(ID) 
);

INSERT INTO users 
(Login, Email, Password, FK_Employee_ID) Values
VALUES 
('admin', 'admin@ecample.com', '1234', 3);



INSERT INTO employee 
(Surname, Name, Patronymic, Status, Photo, Scan_Contract, Speciality)
VALUES 
('Иванов', 'Иван', 'Иванович', 'Работает', 'путь/к/фото.jpg', 'путь/к/договору.pdf', 'администратор');

CREATE TABLE Dishes(
	ID serial primary key,
	Name Varchar(50) not null,
	Incridients Text not null,
	Price money not null
);


INSERT INTO Dishes (Name, Incridients, Price) VALUES
('Цезарь', 'Салат, курица, пармезан, соус', 450.00),
('Пицца Маргарита', 'Тесто, томатный соус, моцарелла, базилик', 500.00),
('Борщ', 'Свекла, капуста, картофель, мясо, сметана', 300.00),
('Карбонара', 'Паста, бекон, яйцо, сыр, сливки', 550.00),
('Оливье', 'Картофель, морковь, яйца, колбаса, майонез', 350.00),
('Суши сет', 'Рис, рыба, нори, соевый соус', 800.00),
('Шашлык', 'Свинина, специи, лук', 700.00),
('Куриные крылья', 'Крылья, соус барбекю, специи', 400.00),
('Гречка с грибами', 'Гречневая крупа, грибы, лук', 250.00),
('Запечённый картофель', 'Картофель, сливочное масло, зелень', 200.00);


create table "Order"(
	ID serial primary key,
	FK_employeeID int references employee(ID),
	FK_dishesID int references dishes(ID),
	Clients_count int not null,
	Table_number int not null,
	Price money not null,
	Status Varchar(50) not null
);

INSERT INTO "Order" (FK_employeeID, FK_dishesID, Clients_count, Table_number, Price, Status) VALUES
(1, 1, 2, 5, 900.00, 'готов'),
(2, 3, 4, 3, 1200.00, 'готовиться'),
(3, 2, 3, 1, 1500.00, 'готов'),
(1, 5, 1, 6, 350.00, 'готовиться'),
(2, 7, 2, 4, 1400.00, 'готов'),
(3, 6, 5, 7, 4000.00, 'готовиться'),
(1, 4, 3, 2, 1650.00, 'готов'),
(2, 8, 4, 8, 1600.00, 'готовиться'),
(3, 9, 2, 9, 500.00, 'готов'),
(1, 10, 1, 9, 200.00, 'готовиться');


create table Shifts(
	ID serial primary key,
	Date_Start timestamp without time zone not null,
	Date_Finis timestamp without time zone not null
);

create table ShiftAssignment(
	ID serial primary key, 
	FK_ShiftsID int references Shifts(ID),
	FK_EmployeeID int references employee(ID)
);



