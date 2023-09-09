using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using second_.Data;

namespace second_.Views
{
    public partial class MainWindow : Window
    {
        private DataContext dataContext;
        public ObservableCollection<Pair> Pairs { get; set; }
        public ObservableCollection<Data.Entity.Department> DepartmentsView { get; set; } = null!;
        private ICollectionView departmentsListView = null!;

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

            // загружаем и соединяем dataContext.Departments с коллекцией ObservableCollection
            dataContext.Departments.Load();  // загрузка данных из таблицы 'Departments' в память
            DepartmentsView = dataContext.Departments.Local.ToObservableCollection();  // конвертирует коллекцию dataContext.Departments в ObservableCollection
            departmentsListUI.ItemsSource = DepartmentsView;  // привязываем элемент интерфейса к ObservableCollection

            // настраиваем объект коллекции ICollectionView, для лучшего обновления и фильтрации коллекции ObservableCollection
            departmentsListView = CollectionViewSource.GetDefaultView(DepartmentsView);  // привязываем DepartmentsView к ICollectionView
            departmentsListView.Filter = item => (item as Data.Entity.Department)?.DeleteDt == null;  // фильтр, выводим товары которые не удалены
            //departmentsListView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
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

        private void Btn5_HW_Click(object sender, RoutedEventArgs e)
        {
            // ФИО - второй отдел
            IEnumerable<Pair> query = dataContext.Managers.Join(
                    dataContext.Departments,
                    m => m.IdSecDep,
                    d => d.Id,
                    (m, d) => new Pair() { Key = $"{m.Surname} {m.Name[0]}.{m.Secname[0]}.", Value = d.Name }
                ).OrderBy(pair => pair.Value);

            UpdateCollection(query);
        }


        #region Генератор - увеличивается при каждом обращении
        private int _N;
        public int N { get => _N++; set => _N = value; }
        #endregion
        private void Btn6_Click(object sender, RoutedEventArgs e)
        {
            // порядковий номер отдела - название отдела
            // Неправильное решение (получаем только одинаковые числа для каждой записи)
            N = 1;  // генератор
            IQueryable query = dataContext.Departments
                .OrderBy(d => d.Name)
                .Select(  // ранее связывание
                    d => new Pair() { Key = N.ToString(), Value = d.Name }
                );  // до N имеется одноразовое обращение при построении SQL запроса, но не многоразовое обращение
            // Pair в Select для того чтобы просто указать что хотим получить на выходе

            UpdateCollection(query);

            /*  Как это работает?
                var query = ... задаёт правило, из чего будет построенный SQL
                Какой SQL будет в результате?
                   SELECT 1, d.Name FROM Departments d ORDER BY d.Name
                   при формировании запроса EF 'увидет', что в него входит переменная N,
                   он её вычеслит и добавит в SQL - в запросе будет значение N, т.е. один)
                Итерация - по результатам запроса
                   1 - Бухгалтерия
                   1 - Отдел кадров
                   ...
                Объекты Pair создаются из этих результатов, и имеют "1" в ключе

                Это ранее связывание переменной, при подготовке запроса и перед выполнением запроса
                Поэтому N - это 1 и остаётся.
                N(1)  -->  SQL  -->  результаты с (1), а не с N
            */
        }

        private void Btn7_Click(object sender, RoutedEventArgs e)
        {
            // порядковий номер отдела - название отдела
            // Правильное решение
            N = 1;  // инициализация счётчика(генератора)
            IEnumerable<Pair> query = dataContext.Departments
                .OrderBy(d => d.Name)
                .AsEnumerable()  // преобразователь, далее LINQ-Enumerable, также можно .ToList()
                .Select(         // ToList() - это создать List<> в памяти, а Enumerable это правило(генератор)
                    d => new Pair() {  // позднее связывание
                        Key = N.ToString(),  // в режиме LINQ-Enumerable это действительно
                        Value = d.Name       // повторяется и 'N' увеличивается
                    }
                );
            UpdateCollection(query);

            /*  Как это работает? Отличия от Button6
                Запрос SQL формируется инструкциями, которые передают .AsEnumerable()
                .Departments
                .OrderBy(d => d.Name)  ->  SELECT 1, d.Name FROM Departments d ORDER BY d.Name
                Запрос выполняется с результатами:
                   id - Бухгалтерия
                   id - Отдел кадров
                Итерация в цикле создаёт объекты Pair в которых есть обращение к N и с каждым
                обращением N увеличивается в геттере генератора N.
                Это позднее связывание

                (*)  -->  SQL  -->  результаты с N
             */
        }

        private void Btn8_Click(object sender, RoutedEventArgs e)
        {
            // название отдела - кол-во работников
            // GroupJoin - аналог GROUP BY

            IQueryable query = dataContext.Departments  // левая таблица
                .GroupJoin(
                    dataContext.Managers,  // правая таблица
                    d => d.Id,  // первичный ключ (с одним значениям)
                    m => m.IdMainDep,  // внешний ключ (с множеством значений)
                    (d, m) => new Pair { Key = d.Name, Value = m.Count().ToString() }
                    // группа: один отдел - коллекция работников
                    // к первому параметру обращаемся как к объекту (один объект)
                    // ко второму - как к коллекции (IEnumerable)
                ).OrderByDescending(p => Convert.ToInt32(p.Value));

            UpdateCollection(query);
        }

        private void Btn9_Click(object sender, RoutedEventArgs e)
        {
            // 1. ФИО - кол-во подчиненных
            // 2. ФИО - кол-подчиннёных (но убрать записи где подчинённых >= 2)
            IQueryable query = dataContext.Managers.GroupJoin(  // шеф
                    dataContext.Managers,  // подчинённый
                    chief => chief.Id,
                    m => m.IdChief,
                    (chief, m) => new Pair { Key = $"{chief.Surname} {chief.Name[0]}.{chief.Secname[0]}.", Value = m.Count().ToString() }
                ).Where(pair => Convert.ToInt32(pair.Value) >= 2);

            UpdateCollection(query);
        }

        private void Btn10_Click(object sender, RoutedEventArgs e)
        {
            // найти однофамильцев
            N = 1;
            IEnumerable<Pair> query = dataContext.Managers.GroupBy(
                    m => m.Surname
                )
                // group.Key -> m => m.Surname
                .Select(group => new Pair { Key = group.Key, Value = group.Count().ToString() })
                .Where(p => p.Value != "1");

            UpdateCollection(query);
        }

        private void Btn11_HW_Click(object sender, RoutedEventArgs e)
        {
            // название отдела - количество совместителей (SecDep)
            IQueryable query = dataContext.Departments.GroupJoin(
                    dataContext.Managers,
                    d => d.Id,
                    m => m.IdSecDep,
                    (d, m) => new Pair { Key = d.Name, Value = m.Count().ToString() }
                )
                .OrderByDescending(pair => Convert.ToInt32(pair.Value));

            UpdateCollection(query);
        }

        private void Btn12_HW_Click(object sender, RoutedEventArgs e)
        {
            // запрос с однофамильцами с нумерацией
            N = 1;  // генератор
            IEnumerable<Pair> query = dataContext.Managers
                .GroupBy(m => m.Surname)
                .AsEnumerable()
                .Where(m => m.Count() > 1)
                .Select(group => new Pair { Key = N.ToString(), Value = group.Key });

            UpdateCollection(query);
        }

        private void Btn13_HW_Click(object sender, RoutedEventArgs e)
        {
            // вывести трех сотрудников с наибольшим количеством подчиненных: кол-во подчиненных - Ф И.О.
            IQueryable query = dataContext.Managers.GroupJoin(
                    dataContext.Managers,
                    chief => chief.Id,
                    m => m.IdChief,
                    (chief, m) => new Pair { Key = m.Count().ToString(), Value = $"{chief.Surname} {chief.Name[0]}.{chief.Secname[0]}." }
                )
                .OrderByDescending(pair => Convert.ToInt32(pair.Key))
                .Take(3);

            UpdateCollection(query);
        }


        private void Btn14_Click(object sender, RoutedEventArgs e)
        {
            // работа с навигационными свойствами
            // работник - название отдела
            IQueryable query = dataContext.Managers
                .Include(m => m.MainDep)  // вкл. навигационной зависимости
                .Select(m => new Pair
                {
                    Key = m.Surname,
                    Value = m.MainDep.Name  // навигационная зависимость
                });

            UpdateCollection(query);
        }

        private void Btn15_Click(object sender, RoutedEventArgs e)
        {
            // работа с навигационными свойствами
            // работник - название отдела (secDep)
            IQueryable query = dataContext.Managers
                .Include(m => m.SecDep)
                .Where(m => m.SecDep!.Name != null)
                .Select(m => new Pair
                {
                    Key = m.Surname,
                    Value = m.SecDep!.Name
                });

            UpdateCollection(query);
        }

        private void Btn16_Click(object sender, RoutedEventArgs e)
        {
            // работа с навигационными свойствами
            // работник - шеф
            IQueryable query = dataContext.Managers
                .Include(m => m.Chief)
                .Select(m => new Pair{
                    Key = $"{m.Surname} {m.Name[0]}.{m.Secname[0]}.",
                    Value = (m.Chief!.Surname == null) ? "--" : $"{m.Chief.Surname} {m.Chief.Name[0]}.{m.Chief.Secname[0]}."
                });

            UpdateCollection(query);
        }

        private void Btn17_Click(object sender, RoutedEventArgs e)
        {
            // обратные навигационные свойства
            // отдел - кол-во работников
            IQueryable query = dataContext.Departments
                //.Include(d => d.MainManagers)  // необязательно
                .Where(d => d.DeleteDt == null)
                .OrderByDescending(d => Convert.ToInt32(d.MainManagers.Count()))
                .Select(d => new Pair
                {
                    Key = d.Name,
                    Value = (d.MainManagers.Count() != 0) ? d.MainManagers.Count().ToString() : "закрытый"
                });

            UpdateCollection(query);
        }

        private void Btn18_Click(object sender, RoutedEventArgs e)
        {
            // обратные навигационные свойства
            // отдел(SecDep) - кол-во работников
            IQueryable query = dataContext.Departments
                .Include(d => d.SecManagers)  // необязательно
                .Where(d => d.DeleteDt == null)
                .OrderByDescending(d => Convert.ToInt32(d.SecManagers.Count()))
                .Select(d => new Pair
                {
                    Key = d.Name,
                    Value = (d.SecManagers.Count() != 0) ? d.SecManagers.Count().ToString() : "закрытый"
                });

            UpdateCollection(query);
        }

        private void Btn19_Click(object sender, RoutedEventArgs e)
        {
            IQueryable query = dataContext.Managers
                .Include(m => m.SubManagers)  // необязательно
                .Where(m => m.SubManagers.Count() > 0)
                .Select(m => new Pair
                {
                    Key = m.Surname,
                    Value = m.SubManagers.Count().ToString()
                });

            UpdateCollection(query);
        }


        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem item)
            {
                if (item.Content is Data.Entity.Department department)  // department - это отдел, по которому кликнули
                {
                    CrudDepartmentWindow dialog = new() { Department = department };
                    if (dialog.ShowDialog() ?? false)
                    {
                        if (dialog.Department is not null)  // not null - это значит dep изменили 
                        {
                            var dep = dataContext.Departments.Find(department.Id);  // находим в БД dep по указанному Id
                            if (dep is not null)  // если найдено
                            {
                                if (dialog.IsDeleted)  // лёгкое удаление
                                {
                                    dep.DeleteDt = DateTime.Now;
                                }
                                else
                                {
                                    dep.Name = department.Name;
                                }
                            }
                        }
                        else  // null, значит dep удалено
                        {
                            dataContext.Departments.Remove(department!);
                        }
                        dataContext.SaveChanges();
                        departmentsListView.Refresh();
                    }
                }
            }
        }

        private void AddDepartmentBtn_Click(object sender, RoutedEventArgs e)
        {
            Data.Entity.Department newDep = new() { Id = Guid.NewGuid(), Name = "" };
            CrudDepartmentWindow dialog = new() { Department = newDep };
            if (dialog.ShowDialog() ?? false)  // save btn
            {
                dataContext.Departments.Add(newDep);  // добавляем отдел в БД
                dataContext.SaveChanges();  // сохраняем изменения в БД
                departmentsListView.Refresh();
            }
        }
    }

    public class Pair
    {
        public string Key { get; set; } = null!;
        public string? Value { get; set; }
    }
}
