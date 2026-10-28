








namespace RepairCenter.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class Course_project_0502_Equipment
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Course_project_0502_Equipment()
        {
            this.Course_project_0502_RepairOrders = new HashSet<Course_project_0502_RepairOrders>();
        }
    
        public int EquipmentID { get; set; }
        public int ClientID { get; set; }
        public int EquipmentModelID { get; set; }
        public string SerialNumber { get; set; }
        public string Notes { get; set; }
    
        public virtual Course_project_0502_Clients Course_project_0502_Clients { get; set; }
        public virtual Course_project_0502_EquipmentModels Course_project_0502_EquipmentModels { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_project_0502_RepairOrders> Course_project_0502_RepairOrders { get; set; }
    }
}
