using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace second_.Data
{
    public class DataContext : DbContext
    {
        public DbSet<Entity.Department> Departments { get; set; } = null!;
        public DbSet<Entity.Manager> Managers { get; set; } = null!;

        public DataContext() : base() {}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(  // настройка подключения к БД из пакета SqlServer - драйверы MS SQL
                @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=ado-ef-p12;Integrated Security=True"
            );                            // строка для подключения - к несуществующей(или пустой) БД
        }
    }
}
