using Microsoft.EntityFrameworkCore;
using second_.Data.Entity;
using System;
using System.Linq;
using System.Text;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // настройка самой БД - отношений между данными их ограничение и уникальность,
            // и также засевание (от англ. seed - зерно) - заполнение начальными данными
            modelBuilder
                .Entity<Manager>()  // настройка навигационного свойства
                .HasOne(m => m.MainDep)  // название свойства
                .WithMany(d => d.MainManagers)  // указывается: тип отношения (1:M -> один ко многим)
                                                // в параметре указываем обратное навигационное свойство
                .HasForeignKey(m => m.IdMainDep)  // внешний ключ  \  равность которых
                .HasPrincipalKey(d => d.Id);  // управляющий ключ  /  нуждается
            // после настройки отношения нужно сделать и применить миграцию

            modelBuilder
                .Entity<Manager>()
                .HasOne(m => m.SecDep)
                .WithMany(d => d.SecManagers)
                .HasForeignKey(m => m.IdSecDep)
                .HasPrincipalKey(d => d.Id);
            // если в WithMany() не указать свойство, то навигационные свойства настраиваются
            // автоматически со следующими правилами:
            // - первичный ключ (название) - Id
            // - внешний ключ - [Entity]Id -> DepartmentId

            modelBuilder
                .Entity<Manager>()
                .HasOne(m => m.Chief)
                .WithMany(m => m.SubManagers)
                .HasForeignKey(m => m.IdChief)
                .HasPrincipalKey(m => m.Id);

            modelBuilder
                .Entity<Manager>()
                .HasIndex(m => m.Login)
                .IsUnique();

            //modelBuilder
            //    .Entity<Manager>()
            //    .Property(m => m.Name)
            //    .HasMaxLength(100);  // максимальная длина Manager.Name
        }
    }
}
