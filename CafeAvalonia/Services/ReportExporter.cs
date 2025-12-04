using CafeAvalonia.ViewModels;
using ClosedXML.Excel;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CafeAvalonia.Services
{
    public static class ReportExporter
    {
        public static Task ExportToXlsxAsync(string path, ShiftReportViewModel vm)
        {
            using var wb = new XLWorkbook();
            var wsAll = wb.Worksheets.Add("Все заказы");
            var wsPaid = wb.Worksheets.Add("Оплаченные");

            wsAll.Cell(1, 1).Value = "ID";
            wsAll.Cell(1, 2).Value = "Блюдо";
            wsAll.Cell(1, 3).Value = "Сотрудник";
            wsAll.Cell(1, 4).Value = "Стол";
            wsAll.Cell(1, 5).Value = "Гостей";
            wsAll.Cell(1, 6).Value = "Статус";
            wsAll.Cell(1, 7).Value = "Цена";
            wsAll.Cell(1, 8).Value = "Создан";


            var row = 2;
            foreach (var o in vm.OrdersInShift)
            {
                wsAll.Cell(row, 1).Value = o.Id;
                wsAll.Cell(row, 2).Value = o.FkDishes?.Name;
                wsAll.Cell(row, 3).Value = o.FkEmployee?.Surname;
                wsAll.Cell(row, 4).Value = o.TableNumber;
                wsAll.Cell(row, 5).Value = o.ClientsCount;
                wsAll.Cell(row, 6).Value = o.Status;
                wsAll.Cell(row, 7).Value = o.Price;
                wsAll.Cell(row, 8).Value = o.CreatedAt;
                row++;
            }

            wsAll.Cell(row + 1, 6).Value = "Сумма всех:";
            wsAll.Cell(row + 1, 7).Value = vm.TotalRevenueAll;

         
            wsPaid.Cell(1, 1).Value = "ID";
            wsPaid.Cell(1, 2).Value = "Блюдо";
            wsPaid.Cell(1, 3).Value = "Сотрудник";
            wsPaid.Cell(1, 4).Value = "Стол";
            wsPaid.Cell(1, 5).Value = "Гостей";
            wsPaid.Cell(1, 6).Value = "Статус";
            wsPaid.Cell(1, 7).Value = "Цена";
            wsPaid.Cell(1, 8).Value = "Создан";

            row = 2;
            foreach (var o in vm.PaidOrdersInShift)
            {
                wsPaid.Cell(row, 1).Value = o.Id;
                wsPaid.Cell(row, 2).Value = o.FkDishes?.Name;
                wsPaid.Cell(row, 3).Value = o.FkEmployee?.Surname;
                wsPaid.Cell(row, 4).Value = o.TableNumber;
                wsPaid.Cell(row, 5).Value = o.ClientsCount;
                wsPaid.Cell(row, 6).Value = o.Status;
                wsPaid.Cell(row, 7).Value = o.Price;
                wsPaid.Cell(row, 8).Value = o.CreatedAt;
                row++;
            }

            wsPaid.Cell(row + 1, 6).Value = "Выручка:";
            wsPaid.Cell(row + 1, 7).Value = vm.TotalRevenuePaid;

            // Инфо по смене (можно отдельным листом)
            if (vm.CurrentShift is not null)
            {
                var wsInfo = wb.Worksheets.Add("Смена");
                wsInfo.Cell(1, 1).Value = "Смена ID";
                wsInfo.Cell(1, 2).Value = vm.CurrentShift.Id;
                wsInfo.Cell(2, 1).Value = "Начало";
                wsInfo.Cell(2, 2).Value = vm.CurrentShift.DateStart;
                wsInfo.Cell(3, 1).Value = "Конец";
                wsInfo.Cell(3, 2).Value = vm.CurrentShift.DateFinis;
            }

            wb.SaveAs(path);
            return Task.CompletedTask;
        }
        public static Task ExportToPdfAsync(string path, ShiftReportViewModel vm)
        {
            PdfConfig.Init();

            var doc = new PdfDocument();
            var page = doc.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            var titleFont = new XFont("MyFont", 16);
            var headerFont = new XFont("MyFont", 12);
            var textFont = new XFont("MyFont", 10);

            double left = 20;
            double top = 20;
            double y = top;

            // Заголовок
            gfx.DrawString("Отчет по смене", titleFont, XBrushes.Black, new XPoint(left, y));
            y += 25;

            // Инфо по смене
            if (vm.CurrentShift is not null)
            {
                gfx.DrawString($"Смена ID: {vm.CurrentShift.Id}", textFont, XBrushes.Black, new XPoint(left, y)); y += 15;
                gfx.DrawString($"Начало: {vm.CurrentShift.DateStart:dd.MM.yyyy HH:mm}", textFont, XBrushes.Black, new XPoint(left, y)); y += 15;
                gfx.DrawString($"Конец:  {vm.CurrentShift.DateFinis:dd.MM.yyyy HH:mm}", textFont, XBrushes.Black, new XPoint(left, y)); y += 20;
            }

            // Общие итоги
            gfx.DrawString($"Всего заказов: {vm.OrdersInShift.Count}", textFont, XBrushes.Black, new XPoint(left, y)); y += 15;
            gfx.DrawString($"Сумма всех: {vm.TotalRevenueAll}", textFont, XBrushes.Black, new XPoint(left, y)); y += 15;
            gfx.DrawString($"Оплачено заказов: {vm.PaidOrdersInShift.Count}", textFont, XBrushes.Black, new XPoint(left, y)); y += 15;
            gfx.DrawString($"Выручка: {vm.TotalRevenuePaid}", textFont, XBrushes.Black, new XPoint(left, y)); y += 25;

            // Таблица: Все заказы (как лист "Все заказы")
            gfx.DrawString("Все заказы", headerFont, XBrushes.Black, new XPoint(left, y));
            y += 18;

            // Заголовки колонок
            double xId = left;
            double xDish = xId + 30;
            double xEmp = xDish + 140;
            double xTable = xEmp + 120;
            double xGuests = xTable + 50;
            double xStatus = xGuests + 50;
            double xPrice = xStatus + 60;
            double xCreated = xPrice + 70;

            gfx.DrawString("ID", headerFont, XBrushes.Black, new XPoint(xId, y));
            gfx.DrawString("Блюдо", headerFont, XBrushes.Black, new XPoint(xDish, y));
            gfx.DrawString("Сотр.", headerFont, XBrushes.Black, new XPoint(xEmp, y));
            gfx.DrawString("Стол", headerFont, XBrushes.Black, new XPoint(xTable, y));
            gfx.DrawString("Гостей", headerFont, XBrushes.Black, new XPoint(xGuests, y));
            gfx.DrawString("Статус", headerFont, XBrushes.Black, new XPoint(xStatus, y));
            gfx.DrawString("Цена", headerFont, XBrushes.Black, new XPoint(xPrice, y));
            gfx.DrawString("Создан", headerFont, XBrushes.Black, new XPoint(xCreated, y));
            y += 14;

            foreach (var o in vm.OrdersInShift)
            {
                // новая страница при переполнении
                if (y > page.Height - 40)
                {
                    page = doc.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = top;
                }

                gfx.DrawString(o.Id.ToString(), textFont, XBrushes.Black, new XPoint(xId, y));
                gfx.DrawString(o.FkDishes?.Name ?? "", textFont, XBrushes.Black, new XPoint(xDish, y));
                gfx.DrawString(o.FkEmployee?.Surname ?? "", textFont, XBrushes.Black, new XPoint(xEmp, y));
                gfx.DrawString(o.TableNumber.ToString(), textFont, XBrushes.Black, new XPoint(xTable, y));
                gfx.DrawString(o.ClientsCount.ToString(), textFont, XBrushes.Black, new XPoint(xGuests, y));
                gfx.DrawString(o.Status ?? "", textFont, XBrushes.Black, new XPoint(xStatus, y));
                gfx.DrawString(o.Price.ToString("0.00"), textFont, XBrushes.Black, new XPoint(xPrice, y));
                gfx.DrawString(o.CreatedAt.ToString("dd.MM HH:mm"), textFont, XBrushes.Black, new XPoint(xCreated, y));

                y += 12;
            }

            y += 18;
            gfx.DrawString($"Сумма всех: {vm.TotalRevenueAll}", headerFont, XBrushes.Black, new XPoint(xStatus, y));
            y += 25;

            // Таблица: Оплаченные (как лист "Оплаченные")
            gfx.DrawString("Оплаченные заказы", headerFont, XBrushes.Black, new XPoint(left, y));
            y += 18;

            // Заголовки колонок те же
            gfx.DrawString("ID", headerFont, XBrushes.Black, new XPoint(xId, y));
            gfx.DrawString("Блюдо", headerFont, XBrushes.Black, new XPoint(xDish, y));
            gfx.DrawString("Сотр.", headerFont, XBrushes.Black, new XPoint(xEmp, y));
            gfx.DrawString("Стол", headerFont, XBrushes.Black, new XPoint(xTable, y));
            gfx.DrawString("Гостей", headerFont, XBrushes.Black, new XPoint(xGuests, y));
            gfx.DrawString("Статус", headerFont, XBrushes.Black, new XPoint(xStatus, y));
            gfx.DrawString("Цена", headerFont, XBrushes.Black, new XPoint(xPrice, y));
            gfx.DrawString("Создан", headerFont, XBrushes.Black, new XPoint(xCreated, y));
            y += 14;

            foreach (var o in vm.PaidOrdersInShift)
            {
                if (y > page.Height - 40)
                {
                    page = doc.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = top;
                }

                gfx.DrawString(o.Id.ToString(), textFont, XBrushes.Black, new XPoint(xId, y));
                gfx.DrawString(o.FkDishes?.Name ?? "", textFont, XBrushes.Black, new XPoint(xDish, y));
                gfx.DrawString(o.FkEmployee?.Surname ?? "", textFont, XBrushes.Black, new XPoint(xEmp, y));
                gfx.DrawString(o.TableNumber.ToString(), textFont, XBrushes.Black, new XPoint(xTable, y));
                gfx.DrawString(o.ClientsCount.ToString(), textFont, XBrushes.Black, new XPoint(xGuests, y));
                gfx.DrawString(o.Status ?? "", textFont, XBrushes.Black, new XPoint(xStatus, y));
                gfx.DrawString(o.Price.ToString("0.00"), textFont, XBrushes.Black, new XPoint(xPrice, y));
                gfx.DrawString(o.CreatedAt.ToString("dd.MM HH:mm"), textFont, XBrushes.Black, new XPoint(xCreated, y));

                y += 12;
            }

            y += 18;
            gfx.DrawString($"Выручка: {vm.TotalRevenuePaid}", headerFont, XBrushes.Black, new XPoint(xStatus, y));

            doc.Save(path);
            return Task.CompletedTask;
        }

    }

}