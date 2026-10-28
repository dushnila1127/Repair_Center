








namespace RepairCenter.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class Course_project_0502_Roles
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Course_project_0502_Roles()
        {
            this.Course_project_0502_Users = new HashSet<Course_project_0502_Users>();
        }
    
        public int RoleID { get; set; }
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_project_0502_Users> Course_project_0502_Users { get; set; }
    }
}
