








namespace RepairCenter.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class Course_project_0502_OrderWorks
    {
        public int OrderWorkID { get; set; }
        public int RepairOrderID { get; set; }
        public int EmployeeID { get; set; }
        public string WorkName { get; set; }
        public string WorkDescription { get; set; }
        public decimal WorkCost { get; set; }
        public short DurationMinutes { get; set; }
        public Nullable<System.DateTime> CompletedAt { get; set; }
    
        public virtual Course_project_0502_Employees Course_project_0502_Employees { get; set; }
        public virtual Course_project_0502_RepairOrders Course_project_0502_RepairOrders { get; set; }
    }
}
