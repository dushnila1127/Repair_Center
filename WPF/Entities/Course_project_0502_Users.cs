








namespace RepairCenter.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class Course_project_0502_Users
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Course_project_0502_Users()
        {
            this.Course_project_0502_RepairOrderStatusHistory = new HashSet<Course_project_0502_RepairOrderStatusHistory>();
            this.Course_project_0502_Roles = new HashSet<Course_project_0502_Roles>();
        }
    
        public int UserID { get; set; }
        public string LoginName { get; set; }
        public string PasswordHash { get; set; }
        public string AccountType { get; set; }
        public Nullable<int> EmployeeID { get; set; }
        public Nullable<int> ClientID { get; set; }
        public bool IsActive { get; set; }
    
        public virtual Course_project_0502_Clients Course_project_0502_Clients { get; set; }
        public virtual Course_project_0502_Employees Course_project_0502_Employees { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_project_0502_RepairOrderStatusHistory> Course_project_0502_RepairOrderStatusHistory { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Course_project_0502_Roles> Course_project_0502_Roles { get; set; }
    }
}
