#set page(margin: 2cm)
#set text(font: "PT Sans", size: 11pt, lang: "ru")
#set heading(numbering: "1.1")
#set par(justify: true, leading: 0.8em)

#align(center)[
  #text(size: 24pt, weight: "bold")[FinanceApp]
  #v(0.3em)
  #text(size: 14pt, fill: rgb("#666"))[Code Overview — Обзор кода]
  #v(0.3em)
  #text(size: 11pt, fill: rgb("#999"))[C\# / .NET 8 / WPF / Entity Framework Core / SQLite]
  #v(1em)
  #line(length: 80%, stroke: 0.5pt + rgb("#ccc"))
]

#v(1em)

= Архитектура

Приложение построено по классической архитектуре WPF с разделением на слои:

#table(
  columns: (1fr, 2fr),
  stroke: 0.5pt + rgb("#ddd"),
  fill: (x, y) => if y == 0 { rgb("#4CAF50").lighten(80%) } else { none },
  [*Слой*], [*Описание*],
  [Models], [Модели данных — 5 классов (User, Expense, Income, Target, UserTarget)],
  [Data], [Контекст БД — Entity Framework Core + SQLite],
  [Windows], [UI — 4 окна WPF (Login, Register, Main, Admin)],
  [Helpers], [Утилиты — хеширование паролей, экспорт в Word],
)

= Структура проекта

```
FinanceApp/
├── FinanceApp.sln              — решение Visual Studio
├── FinanceApp/
│   ├── FinanceApp.csproj       — проект (.NET 8, WPF)
│   ├── App.xaml                — точка входа
│   ├── Models/
│   │   ├── User.cs             — пользователь (логин, пароль, роль)
│   │   ├── Expense.cs          — расход (название, сумма, дата)
│   │   ├── Income.cs           — доход (название, сумма, дата)
│   │   ├── Target.cs           — финансовая цель (сумма, сроки)
│   │   └── UserTarget.cs       — связь пользователь ↔ цель
│   ├── Data/
│   │   └── FinanceDbContext.cs  — контекст EF Core + seed-данные
│   ├── Windows/
│   │   ├── LoginWindow.xaml     — окно авторизации
│   │   ├── RegisterWindow.xaml  — окно регистрации
│   │   ├── MainWindow.xaml      — главное окно (5 вкладок)
│   │   └── AdminWindow.xaml     — панель администратора
│   └── Helpers/
│       ├── BCryptHelper.cs      — SHA-256 хеширование
│       └── WordExporter.cs      — экспорт в .docx (OpenXml)
```

= База данных

#text(size: 10pt)[5 таблиц, SQLite, автоматическое создание при первом запуске.]

#v(0.5em)

#table(
  columns: (1fr, 2fr, 1fr),
  stroke: 0.5pt + rgb("#ddd"),
  fill: (x, y) => if y == 0 { rgb("#2196F3").lighten(80%) } else { none },
  [*Таблица*], [*Назначение*], [*Связи*],
  [Users], [Пользователи (логин, хеш пароля, роль)], [→ Expenses, Incomes, UserTargets],
  [Expenses], [Расходы (название, сумма, дата)], [← Users (FK: UserID)],
  [Incomes], [Доходы (название, сумма, дата)], [← Users (FK: UserID)],
  [Targets], [Финансовые цели (сумма, сроки)], [→ UserTargets],
  [UserTargets], [Связь пользователь ↔ цель], [← Users, ← Targets],
)

= Ключевые модули

== Авторизация и безопасность

- Пароли хешируются алгоритмом *SHA-256* перед сохранением
- При входе введённый пароль хешируется и сравнивается с сохранённым хешем
- Роли: `0` — пользователь, `1` — администратор
- Начальные данные: admin/admin123 (админ), 123qwe/1234 (пользователь)

```csharp
public static class BCryptHelper
{
    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(
            Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    public static bool VerifyPassword(
        string password, string hash)
    {
        return HashPassword(password) == hash;
    }
}
```

== Модели данных

Каждая модель — класс C\# с атрибутами Entity Framework:

```csharp
[Table("Users")]
public class User
{
    [Key]
    public int UserID { get; set; }

    [Required, MaxLength(50)]
    public string Username { get; set; }

    [Required, MaxLength(255)]
    public string PasswordHash { get; set; }

    public string? Photo { get; set; }
    public int Role { get; set; } = 0;  // 0=user, 1=admin

    public ICollection<Expense> Expenses { get; set; }
    public ICollection<Income> Incomes { get; set; }
    public ICollection<UserTarget> UserTargets { get; set; }
}
```

== Контекст базы данных

```csharp
public class FinanceDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<Income> Incomes { get; set; }
    public DbSet<Target> Targets { get; set; }
    public DbSet<UserTarget> UserTargets { get; set; }

    protected override void OnConfiguring(
        DbContextOptionsBuilder optionsBuilder)
    {
        var dbPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "FinanceDB.db");
        optionsBuilder.UseSqlite(
            $"Data Source={dbPath}");
    }
}
```

== Экспорт в Word

Использует библиотеку *DocumentFormat.OpenXml* (не требует установки Microsoft Office):

```csharp
public static class WordExporter
{
    public static void Export(string filePath,
        string username,
        List<Income> incomes,
        List<Expense> expenses)
    {
        using var doc = WordprocessingDocument.Create(
            filePath,
            WordprocessingDocumentType.Document);

        // Заголовок, сводка, таблицы доходов/расходов
        // Times New Roman, таблицы с рамками
    }
}
```

== Пользовательский интерфейс (WPF)

#table(
  columns: (1fr, 2fr),
  stroke: 0.5pt + rgb("#ddd"),
  fill: (x, y) => if y == 0 { rgb("#FF9800").lighten(80%) } else { none },
  [*Окно*], [*Функционал*],
  [LoginWindow], [Ввод логина/пароля, проверка в БД, переход по роли],
  [RegisterWindow], [Создание аккаунта, валидация, проверка уникальности],
  [MainWindow], [5 вкладок: сводка, расходы, доходы, цели, экспорт],
  [AdminWindow], [Управление пользователями + статистика системы],
)

= Технологический стек

#table(
  columns: (1fr, 1fr, 2fr),
  stroke: 0.5pt + rgb("#ddd"),
  fill: (x, y) => if y == 0 { rgb("#9C27B0").lighten(80%) } else { none },
  [*Технология*], [*Версия*], [*Назначение*],
  [C\#], [12.0], [Язык программирования],
  [.NET], [8.0], [Платформа],
  [WPF], [—], [Десктопный UI-фреймворк],
  [Entity Framework Core], [8.0.0], [ORM для работы с БД],
  [SQLite], [—], [Файловая СУБД (без сервера)],
  [OpenXml SDK], [3.0.1], [Генерация .docx без Office],
  [SHA-256], [—], [Хеширование паролей],
)

= NuGet-зависимости

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore"
                  Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite"
                  Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools"
                  Version="8.0.0" />
<PackageReference Include="DocumentFormat.OpenXml"
                  Version="3.0.1" />
```

#v(2em)
#align(center)[
  #line(length: 60%, stroke: 0.5pt + rgb("#ccc"))
  #v(0.5em)
  #text(size: 9pt, fill: rgb("#999"))[
    FinanceApp — Курсовой проект по МДК 02.01 «Технология разработки ПО» \
    09.02.07 Информационные системы и программирование \
    СГТУ имени Гагарина Ю.А. — ППК — 2025
  ]
]
