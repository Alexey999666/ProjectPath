using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ProjectPath.ModelsDB;

public partial class ProjectNewPartsContext : DbContext
{
    public ProjectNewPartsContext()
    {
    }

    public ProjectNewPartsContext(DbContextOptions<ProjectNewPartsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Nomenclature> Nomenclatures { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<ProjectAction> ProjectActions { get; set; }

    public virtual DbSet<ProjectComposition> ProjectCompositions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<StockBalance> StockBalances { get; set; }

    public virtual DbSet<VwActiveProject> VwActiveProjects { get; set; }

    public virtual DbSet<VwPartsLocation> VwPartsLocations { get; set; }

    public virtual DbSet<VwProjectCompositionDetail> VwProjectCompositionDetails { get; set; }

    public virtual DbSet<VwWarehouseStock> VwWarehouseStocks { get; set; }

    public virtual DbSet<Warehouse> Warehouses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost\\SQLExpress; database=ProjectNewParts; Trusted_Connection = True; Encrypt = false");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customer");

            entity.Property(e => e.CustomerId).ValueGeneratedNever();
            entity.Property(e => e.Adress).HasMaxLength(50);
            entity.Property(e => e.ContactPerson).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.NameOrganization).HasMaxLength(50);
            entity.Property(e => e.Phone).HasMaxLength(50);
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("Department");

            entity.Property(e => e.DepartmentId).ValueGeneratedNever();
            entity.Property(e => e.Location).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Type).HasMaxLength(50);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PK_User");

            entity.ToTable("Employee");

            entity.Property(e => e.EmployeeId).ValueGeneratedNever();
            entity.Property(e => e.EmployeeLogin).HasMaxLength(50);
            entity.Property(e => e.EmployeePassword).HasMaxLength(50);
            entity.Property(e => e.EmployeeRoleId).HasColumnName("EmployeeRoleID");
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Position).HasMaxLength(50);
            entity.Property(e => e.Salary).HasColumnType("money");
            entity.Property(e => e.Surname).HasMaxLength(50);

            entity.HasOne(d => d.Department).WithMany(p => p.Employees)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Employee_Department");

            entity.HasOne(d => d.EmployeeRole).WithMany(p => p.Employees)
                .HasForeignKey(d => d.EmployeeRoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Employee_Role");
        });

        modelBuilder.Entity<Nomenclature>(entity =>
        {
            entity.ToTable("Nomenclature");

            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Type).HasMaxLength(50);
            entity.Property(e => e.UnitMeasure).HasMaxLength(50);
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("Project", tb => tb.HasTrigger("trg_ProjectStatusChange"));

            entity.Property(e => e.ActualCompletionDate).HasColumnType("datetime");
            entity.Property(e => e.EstimatedDesignCost).HasColumnType("money");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.PlannedCompletionDate).HasColumnType("datetime");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Customer).WithMany(p => p.Projects)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Project_Customer");

            entity.HasOne(d => d.Department).WithMany(p => p.Projects)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Project_Department");

            entity.HasOne(d => d.SupervisorNavigation).WithMany(p => p.Projects)
                .HasForeignKey(d => d.Supervisor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Project_Employee");
        });

        modelBuilder.Entity<ProjectAction>(entity =>
        {
            entity.HasKey(e => e.ProjectActionsId);

            entity.Property(e => e.DateExecution).HasColumnType("datetime");
            entity.Property(e => e.TypeOperation).HasMaxLength(50);

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectActions)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProjectActions_Project");
        });

        modelBuilder.Entity<ProjectComposition>(entity =>
        {
            entity.ToTable("ProjectComposition", tb => tb.HasTrigger("trg_CheckPartAvailability"));

            entity.Property(e => e.Quantity).HasColumnType("decimal(12, 1)");

            entity.HasOne(d => d.Nomenclature).WithMany(p => p.ProjectCompositions)
                .HasForeignKey(d => d.NomenclatureId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProjectComposition_Nomenclature");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectCompositions)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProjectComposition_Project");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.UserRoleId);

            entity.ToTable("Role");

            entity.Property(e => e.UserRoleId)
                .ValueGeneratedNever()
                .HasColumnName("UserRoleID");
            entity.Property(e => e.Role1)
                .HasMaxLength(50)
                .HasColumnName("Role");
        });

        modelBuilder.Entity<StockBalance>(entity =>
        {
            entity.ToTable("StockBalance");

            entity.Property(e => e.StockBalanceId).ValueGeneratedNever();
            entity.Property(e => e.Quantity).HasColumnType("decimal(12, 1)");

            entity.HasOne(d => d.Nomenclature).WithMany(p => p.StockBalances)
                .HasForeignKey(d => d.NomenclatureId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockBalance_Nomenclature");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.StockBalances)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockBalance_Warehouse");
        });

        modelBuilder.Entity<VwActiveProject>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_ActiveProjects");

            entity.Property(e => e.Customer).HasMaxLength(50);
            entity.Property(e => e.PlannedCompletionDate).HasColumnType("datetime");
            entity.Property(e => e.ProjectId).HasColumnName("ProjectID");
            entity.Property(e => e.ProjectName).HasMaxLength(50);
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Supervisor).HasMaxLength(152);
        });

        modelBuilder.Entity<VwPartsLocation>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_PartsLocation");

            entity.Property(e => e.AvailableQuantity).HasColumnType("decimal(12, 1)");
            entity.Property(e => e.CurrentLocation)
                .HasMaxLength(11)
                .IsUnicode(false);
            entity.Property(e => e.LocationName).HasMaxLength(50);
            entity.Property(e => e.PartName).HasMaxLength(50);
            entity.Property(e => e.PartType).HasMaxLength(50);
            entity.Property(e => e.ResponsiblePerson).HasMaxLength(101);
            entity.Property(e => e.UnitMeasure).HasMaxLength(50);
        });

        modelBuilder.Entity<VwProjectCompositionDetail>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_ProjectCompositionDetails");

            entity.Property(e => e.PartName).HasMaxLength(50);
            entity.Property(e => e.PartType).HasMaxLength(50);
            entity.Property(e => e.ProjectName).HasMaxLength(50);
            entity.Property(e => e.Quantity).HasColumnType("decimal(12, 1)");
            entity.Property(e => e.ResponsiblePerson).HasMaxLength(152);
            entity.Property(e => e.UnitMeasure).HasMaxLength(50);
        });

        modelBuilder.Entity<VwWarehouseStock>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_WarehouseStock");

            entity.Property(e => e.ConditionalPosition).HasMaxLength(50);
            entity.Property(e => e.MaterialName).HasMaxLength(50);
            entity.Property(e => e.MaterialType).HasMaxLength(50);
            entity.Property(e => e.Quantity).HasColumnType("decimal(12, 1)");
            entity.Property(e => e.ResponsiblePerson).HasMaxLength(152);
            entity.Property(e => e.UnitMeasure).HasMaxLength(50);
            entity.Property(e => e.WarehouseType).HasMaxLength(50);
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.ToTable("Warehouse");

            entity.Property(e => e.WarehouseId).ValueGeneratedNever();
            entity.Property(e => e.ConditionalPosition).HasMaxLength(50);
            entity.Property(e => e.Type).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
