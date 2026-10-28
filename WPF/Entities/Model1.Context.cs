using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace RepairCenter.Entities
{
    public partial class user24_dbEntities1 : DbContext
    {
        public user24_dbEntities1() : base("name=user24_dbEntities1") { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }

        public virtual DbSet<Course_project_0502_Clients> Course_project_0502_Clients { get; set; }
        public virtual DbSet<Course_project_0502_Employees> Course_project_0502_Employees { get; set; }
        public virtual DbSet<Course_project_0502_Equipment> Course_project_0502_Equipment { get; set; }
        public virtual DbSet<Course_project_0502_EquipmentBrands> Course_project_0502_EquipmentBrands { get; set; }
        public virtual DbSet<Course_project_0502_EquipmentModels> Course_project_0502_EquipmentModels { get; set; }
        public virtual DbSet<Course_project_0502_EquipmentTypes> Course_project_0502_EquipmentTypes { get; set; }
        public virtual DbSet<Course_project_0502_OrderAssignments> Course_project_0502_OrderAssignments { get; set; }
        public virtual DbSet<Course_project_0502_OrderStatuses> Course_project_0502_OrderStatuses { get; set; }
        public virtual DbSet<Course_project_0502_OrderWorks> Course_project_0502_OrderWorks { get; set; }
        public virtual DbSet<Course_project_0502_RepairOrders> Course_project_0502_RepairOrders { get; set; }
        public virtual DbSet<Course_project_0502_RepairOrderStatusHistory> Course_project_0502_RepairOrderStatusHistory { get; set; }
        public virtual DbSet<Course_project_0502_Roles> Course_project_0502_Roles { get; set; }
        public virtual DbSet<Course_project_0502_Specializations> Course_project_0502_Specializations { get; set; }
        public virtual DbSet<Course_project_0502_Users> Course_project_0502_Users { get; set; }
    }
}
