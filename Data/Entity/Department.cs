using System;
using System.Collections.Generic;

namespace second_.Data.Entity
{
    public class Department
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime? DeleteDt { get; set; }

        //////////////////// Inverse Navigation props ////////////////////

        // MainManagers - обратная к Manager.MainDep зависимость
        public IEnumerable<Manager> MainManagers { get; set; } = null!;
        public List<Manager> SecManagers { get; set; } = null!;
    }
}
