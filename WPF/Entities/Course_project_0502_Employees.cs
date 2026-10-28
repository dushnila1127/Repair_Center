








namespace RepairCenter.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class Course_project_0502_Employees
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Course_project_0502_Employees()
        {
            this.Course_project_0502_OrderAssignments = new HashSet<Course_project_0502_OrderAssignments>();
            this.Course_project_0502_OrderWorks = new HashSet<Course_project_0502_OrderWorks>();
            this.Course_project_0502_Users = new HashSet<Course_project_0502_Users>();
            this.Course_project_0502_Specializations = new HashSet<Course_project_0502_Specializations>();
        }
    
        public int EmployeeID { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public bool IsActive { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_project_0502_OrderAssignments> Course_project_0502_OrderAssignments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_project_0502_OrderWorks> Course_project_0502_OrderWorks { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_project_0502_Users> Course_project_0502_Users { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_project_0502_Specializations> Course_project_0502_Specializations { get; set; }
    }
}
