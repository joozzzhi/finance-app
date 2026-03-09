using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using FinanceApp.Models;

namespace FinanceApp.Helpers
{
    public static class WordExporter
    {
        public static void Export(string filePath, string username,
            List<Income> incomes, List<Expense> expenses)
        {
            using var doc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document);

            var mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            // Заголовок
            AddParagraph(body, $"Финансовый отчёт — {username}", true, "28");
            AddParagraph(body, $"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}", false, "20");
            AddParagraph(body, "", false, "20");

            // Сводка
            var totalIncome = incomes.Sum(i => i.IncomeAmount);
            var totalExpense = expenses.Sum(e => e.ExpenseAmount);
            var balance = totalIncome - totalExpense;

            AddParagraph(body, $"Общий доход: {totalIncome:N2} ₽", true, "24");
            AddParagraph(body, $"Общий расход: {totalExpense:N2} ₽", true, "24");
            AddParagraph(body, $"Баланс: {balance:N2} ₽", true, "24");
            AddParagraph(body, "", false, "20");

            // Таблица доходов
            AddParagraph(body, "Доходы", true, "24");

            if (incomes.Count > 0)
            {
                var incomeTable = CreateTable(body);
                AddTableRow(incomeTable, new[] { "Название", "Сумма", "Дата" }, true);
                foreach (var inc in incomes.OrderByDescending(i => i.IncomeDate))
                {
                    AddTableRow(incomeTable, new[]
                    {
                        inc.IncomeName,
                        $"{inc.IncomeAmount:N2} ₽",
                        inc.IncomeDate.ToString("dd.MM.yyyy")
                    }, false);
                }
            }
            else
            {
                AddParagraph(body, "Нет данных о доходах", false, "20");
            }

            AddParagraph(body, "", false, "20");

            // Таблица расходов
            AddParagraph(body, "Расходы", true, "24");

            if (expenses.Count > 0)
            {
                var expenseTable = CreateTable(body);
                AddTableRow(expenseTable, new[] { "Название", "Сумма", "Дата" }, true);
                foreach (var exp in expenses.OrderByDescending(e => e.ExpenseDate))
                {
                    AddTableRow(expenseTable, new[]
                    {
                        exp.ExpenseName,
                        $"{exp.ExpenseAmount:N2} ₽",
                        exp.ExpenseDate.ToString("dd.MM.yyyy")
                    }, false);
                }
            }
            else
            {
                AddParagraph(body, "Нет данных о расходах", false, "20");
            }

            mainPart.Document.Save();
        }

        private static void AddParagraph(Body body, string text, bool bold, string fontSize)
        {
            var para = body.AppendChild(new Paragraph());
            var run = para.AppendChild(new Run());

            var runProps = run.AppendChild(new RunProperties());
            runProps.AppendChild(new FontSize { Val = fontSize });
            runProps.AppendChild(new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" });

            if (bold)
                runProps.AppendChild(new Bold());

            run.AppendChild(new Text(text));
        }

        private static Table CreateTable(Body body)
        {
            var table = body.AppendChild(new Table());

            var tblProps = table.AppendChild(new TableProperties());
            tblProps.AppendChild(new TableBorders(
                new TopBorder { Val = BorderValues.Single, Size = 4 },
                new BottomBorder { Val = BorderValues.Single, Size = 4 },
                new LeftBorder { Val = BorderValues.Single, Size = 4 },
                new RightBorder { Val = BorderValues.Single, Size = 4 },
                new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 },
                new InsideVerticalBorder { Val = BorderValues.Single, Size = 4 }
            ));
            tblProps.AppendChild(new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct });

            return table;
        }

        private static void AddTableRow(Table table, string[] cells, bool isHeader)
        {
            var row = table.AppendChild(new TableRow());

            foreach (var cellText in cells)
            {
                var cell = row.AppendChild(new TableCell());
                var para = cell.AppendChild(new Paragraph());
                var run = para.AppendChild(new Run());

                var runProps = run.AppendChild(new RunProperties());
                runProps.AppendChild(new FontSize { Val = "20" });
                runProps.AppendChild(new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" });

                if (isHeader)
                    runProps.AppendChild(new Bold());

                run.AppendChild(new Text(cellText));
            }
        }
    }
}
