using System;
using System.Collections.Generic;
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

        public MainWindow()
        {
            InitializeComponent();
            dataContext = new();
        }

        // количество сотрудников ИТ-отдела (как основных, так и совместителей)
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
    }
}
