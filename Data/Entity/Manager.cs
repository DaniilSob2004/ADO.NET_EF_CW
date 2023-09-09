using System;
using System.Collections.Generic;

namespace second_.Data.Entity
{
    public class Manager
    {
        public Guid Id { get; set; }
        public String Surname { get; set; } = null!;
        public String Name { get; set; } = null!;
        public String Secname { get; set; } = null!;
        public String Login { get; set; } = null!;
        public String PassSalt { get; set; } = null!;  // стандарт rfc2898
        public String PassDk { get; set; } = null!;  // стандарт rfc2898
        public Guid IdMainDep { get; set; }  // отдел в котором работает
        public Guid? IdSecDep { get; set; }  // доп. отдел
        public Guid? IdChief { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime? DeleteDt { get; set; }
        public String Email { get; set; } = null!;
        public String? Avatar { get; set; }


        //////////////////// Navigation props ////////////////////
        public Department MainDep { get; set; } = null!;  // навигационное свойство
        public Department? SecDep { get; set; }  // опционально навигационное свойство (?)
        public Manager? Chief { get; set; }  // опционально навигационное свойство (?)

        //////////////////// Inverse Navigation props ////////////////////
        public List<Manager> SubManagers { get; set; } = null!;
    }
}
