








namespace RepairCenter.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class Course_project_0502_OrderStatuses
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Course_project_0502_OrderStatuses()
        {
            this.Course_project_0502_RepairOrders = new HashSet<Course_project_0502_RepairOrders>();
            this.Course_project_0502_RepairOrderStatusHistory = new HashSet<Course_project_0502_RepairOrderStatusHistory>();
        }
    
        public int OrderStatusID { get; set; }
        public string StatusName { get; set; }
        public bool IsFinal { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_project_0502_RepairOrders> Course_project_0502_RepairOrders { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_project_0502_RepairOrderStatusHistory> Course_project_0502_RepairOrderStatusHistory { get; set; }
    }
}
