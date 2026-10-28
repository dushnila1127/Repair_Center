








namespace RepairCenter.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class Course_project_0502_RepairOrderStatusHistory
    {
        public int StatusHistoryID { get; set; }
        public int RepairOrderID { get; set; }
        public int OrderStatusID { get; set; }
        public System.DateTime ChangedAt { get; set; }
        public Nullable<int> ChangedByUserID { get; set; }
        public string ChangeNote { get; set; }
    
        public virtual Course_project_0502_OrderStatuses Course_project_0502_OrderStatuses { get; set; }
        public virtual Course_project_0502_RepairOrders Course_project_0502_RepairOrders { get; set; }
        public virtual Course_project_0502_Users Course_project_0502_Users { get; set; }
    }
}
