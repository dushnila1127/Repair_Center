








namespace RepairCenter.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class Course_project_0502_Specializations
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Course_project_0502_Specializations()
        {
            this.Course_project_0502_Employees = new HashSet<Course_project_0502_Employees>();
        }
    
        public int SpecializationID { get; set; }
        public string SpecializationName { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_project_0502_Employees> Course_project_0502_Employees { get; set; }
    }
}
