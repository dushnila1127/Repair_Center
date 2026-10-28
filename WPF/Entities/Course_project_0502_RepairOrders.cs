








namespace RepairCenter.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class Course_project_0502_RepairOrders
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Course_project_0502_RepairOrders()
        {
            this.Course_project_0502_OrderAssignments = new HashSet<Course_project_0502_OrderAssignments>();
            this.Course_project_0502_OrderWorks = new HashSet<Course_project_0502_OrderWorks>();
            this.Course_project_0502_RepairOrderStatusHistory = new HashSet<Course_project_0502_RepairOrderStatusHistory>();
        }
    
        public int RepairOrderID { get; set; }
        public string OrderNumber { get; set; }
        public int EquipmentID { get; set; }
        public System.DateTime AcceptedAt { get; set; }
        public Nullable<System.DateTime> IssuedAt { get; set; }
        public string FaultDescription { get; set; }
        public decimal RepairCost { get; set; }
        public int OrderStatusID { get; set; }
        public bool IsCancelled { get; set; }
        public string CancellationNote { get; set; }
        public System.DateTime CreatedAt { get; set; }
    
        public virtual Course_project_0502_Equipment Course_project_0502_Equipment { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_project_0502_OrderAssignments> Course_project_0502_OrderAssignments { get; set; }
        public virtual Course_project_0502_OrderStatuses Course_project_0502_OrderStatuses { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_project_0502_OrderWorks> Course_project_0502_OrderWorks { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_project_0502_RepairOrderStatusHistory> Course_project_0502_RepairOrderStatusHistory { get; set; }
    }
}
