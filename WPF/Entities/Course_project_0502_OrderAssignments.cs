








namespace RepairCenter.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class Course_project_0502_OrderAssignments
    {
        public int RepairOrderID { get; set; }
        public int EmployeeID { get; set; }
        public System.DateTime AssignedAt { get; set; }
        public string AssignmentNote { get; set; }
    
        public virtual Course_project_0502_Employees Course_project_0502_Employees { get; set; }
        public virtual Course_project_0502_RepairOrders Course_project_0502_RepairOrders { get; set; }
    }
}
