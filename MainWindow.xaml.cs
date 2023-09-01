using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using second_.Data;

namespace second_
{
    public partial class MainWindow : Window
    {
        private DataContext dataContext;
        public ObservableCollection<Pair> Pairs { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Pairs = new ObservableCollection<Pair>();
            this.DataContext = this;
            dataContext = new();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            departmentsCountLabel.Content = dataContext.Departments.Count().ToString();
            managersCountLabel.Content = dataContext.Managers.Count().ToString();
            topChiefCountLabel.Content = dataContext.Managers.Where(manager => manager.IdChief == null)  // predicate - ф-ция, которая возвращает bool
                                         .Count().ToString();       // Для каждого элемента выполняется сравнение?
                                                                    // Нет! С анализа предиката создаётся SQL запрос
            smallChiefCountLabel.Content = dataContext.Managers.Where(manager => manager.IdChief != null).Count().ToString();

            // узнаём Id - 'IT отдела'
            Guid itGuid = Guid.Parse(dataContext.Departments.Where(department => department.Name == "IT відділ")
                                     .Select(department => department.Id).First().ToString());
            itDepartCountLabel.Content = dataContext.Managers.Where(manager => manager.IdMainDep == itGuid || manager.IdSecDep == itGuid).Count().ToString();

            twoDepartCountLabel.Content = dataContext.Managers.Where(manager => manager.IdMainDep != null && manager.IdSecDep != null).Count().ToString();
        }

        private void UpdateCollection(IQueryable pairs)
        {
            Pairs.Clear();
            foreach (Pair pair in pairs)  // цикл-итератор запускает выполнение запроса
            {                             // с этого момента идёт запрос к БД
                Pairs.Add(pair);
            }
        }

        private void UpdateCollection(IEnumerable<Pair> pairs)
        {
            Pairs.Clear();
            foreach (Pair pair in pairs)
            {
                Pairs.Add(pair);
            }
        }


        private void Btn1_Click(object sender, RoutedEventArgs e)
        {
            // ФИО тех, кто работает в 'Бухгалтерии'
            IQueryable query = dataContext.Managers.Where(m => m.IdMainDep == Guid.Parse("131ef84b-f06e-494b-848f-bb4bc0604266"))
            .Select(m => new Pair { Key = m.Surname, Value = $"{m.Name[0]}. {m.Secname[0]}." });
            // Select - правило преобразования, на входе элемент предыдущей коллекции (m - manager),
            // а на выходе - результат лямбды
            // query - "правило" постройки запроса. Сам запрос не отправленый, и не получено результата

            // цикл-итератор foreach запускает выполнение запроса
            // с этого момента идёт запрос к БД
            UpdateCollection(query);

            // Особенности:
            // - LINQ запрос можно сохранить в переменной, сам запрос это "правило" и не
            //   инициализирует обращение к БД
            // - LINQ-to-Entity использует присоединённый режим, т.е. каждый запрос отправляется к БД,
            //   а не к "скаченой" коллекции
            // - Выполнение запроса выполняется шагами:
            //   1. вызов агрегатора (.Count(), .Max() и тд.)
            //   2. вызов явного преобразования (.ToList(), .ToArray(), и тд.)
            //   3. запуск цикла по итераванному запросу (foreach)

            // - Фильтрация (.Where) лучше задействовать с индексованными полями, и первичным ключом
            //   (который автоматически индексируется)
            // - Инструкция .Select это преобразователь, а не запуск запроса
        }

        private void Btn2_Click(object sender, RoutedEventArgs e)
        {
            // ФИО - отдел в котором работает, пропустить первые 3 и вывести первые 8
            IQueryable query = dataContext.Managers.Join(  // запрос с соединением таблиц
                    dataContext.Departments,
                    m => m.IdMainDep,  // внешний ключ левой таблицы
                    d => d.Id,  // первичный ключ правой таблицы
                    (m, d) => new Pair { Key = $"{m.Surname} {m.Name[0]}.{m.Secname[0]}.", Value = d.Name }
                    // selector - правило преобразования пары сущностей для которых зарегистрировано соединение (JOIN)
                )
                .Skip(3)   // пропустить первые 3 записи выборки
                .Take(8);  // получить первые 8 записей из выборки
            // Managers - левая таблица, Departments - правая таблица

            UpdateCollection(query);
        }

        private void Btn3_Click(object sender, RoutedEventArgs e)
        {
            // ФИО - ФИО шефа, отсортировать по ФИО работника
            IEnumerable<Pair> query = dataContext.Managers.Join(
                    dataContext.Managers,
                    m => m.IdChief,
                    chief => chief.Id,
                    (m, chief) => new Pair() { Key = $"{m.Surname} {m.Name[0]}.{m.Secname[0]}.", Value = $"{chief.Surname} {chief.Name[0]}.{chief.Secname[0]}." }
                )
                .ToList()  // запускает запрос и преобразовывает результат в коллекцию List<Pair>
                .OrderBy(pair => pair.Key);  // выполняется после Select, значит работает с Pair
                                             // но в данном случае, другой LINQ, который действует на коллекцию, а не на запрос SQL
            UpdateCollection(query);
        }

        private void Btn4_Click(object sender, RoutedEventArgs e)
        {
            // Дата создания записи - ФИО, первые 7 из последних по дате
            IQueryable query = dataContext.Managers
                .OrderByDescending(m => m.CreateDt)
                .Select(m => new Pair { Key = $"{m.CreateDt}", Value = $"{m.Surname} {m.Name[0]}.{m.Secname[0]}." })
                .Take(7);

            UpdateCollection(query);
        }

        private void Btn5_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<Pair> query = dataContext.Managers.Join(
                    dataContext.Departments,
                    m => m.IdSecDep,
                    d => d.Id,
                    (m, d) => new Pair() { Key = $"{m.Surname} {m.Name[0]}.{m.Secname[0]}.", Value = d.Name }
                ).OrderBy(pair => pair.Value);

            UpdateCollection(query);
        }
    }

    public class Pair
    {
        public string Key { get; set; } = null!;
        public string? Value { get; set; }
    }
}
