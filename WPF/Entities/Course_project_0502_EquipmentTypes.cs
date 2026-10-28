








namespace RepairCenter.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class Course_project_0502_EquipmentTypes
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Course_project_0502_EquipmentTypes()
        {
            this.Course_project_0502_EquipmentModels = new HashSet<Course_project_0502_EquipmentModels>();
        }
    
        public int EquipmentTypeID { get; set; }
        public string TypeName { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_project_0502_EquipmentModels> Course_project_0502_EquipmentModels { get; set; }
    }
}
