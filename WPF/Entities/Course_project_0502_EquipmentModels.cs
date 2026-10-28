








namespace RepairCenter.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class Course_project_0502_EquipmentModels
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Course_project_0502_EquipmentModels()
        {
            this.Course_project_0502_Equipment = new HashSet<Course_project_0502_Equipment>();
        }
    
        public int EquipmentModelID { get; set; }
        public int EquipmentBrandID { get; set; }
        public int EquipmentTypeID { get; set; }
        public string ModelName { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_project_0502_Equipment> Course_project_0502_Equipment { get; set; }
        public virtual Course_project_0502_EquipmentBrands Course_project_0502_EquipmentBrands { get; set; }
        public virtual Course_project_0502_EquipmentTypes Course_project_0502_EquipmentTypes { get; set; }
    }
}
