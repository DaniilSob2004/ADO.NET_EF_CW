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
using System.Windows.Shapes;

namespace second_.Views
{
    public partial class CrudDepartmentWindow : Window
    {
        public Data.Entity.Department? Department { get; set; }
        public bool IsDeleted { get; set; }

        public CrudDepartmentWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            idTextBox.Text = Department?.Id.ToString() ?? "";
            nameTextBox.Text = Department?.Name.ToString() ?? "";
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            Department!.Name = nameTextBox.Text;
            DialogResult = true;
        }

        private void SoftDelBtn_Click(object sender, RoutedEventArgs e)
        {
            IsDeleted = true;  // пометка, что это лёгкое удаление
            DialogResult = true;
        }

        private void HardDelBtn_Click(object sender, RoutedEventArgs e)
        {
            Department = null;
            DialogResult = true;
        }
    }
}
