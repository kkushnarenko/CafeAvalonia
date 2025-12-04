using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CafeAvalonia.Models;

public partial class BdcafeContext : DbContext
{
    public BdcafeContext()
    {
    }

    public BdcafeContext(DbContextOptions<BdcafeContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Dish> Dishes { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Shift> Shifts { get; set; }

    public virtual DbSet<Shiftassignment> Shiftassignments { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=BDCafe;Username=postgres;Password=JeyKe65");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Dish>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("dishes_pkey");

            entity.ToTable("dishes");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Incridients).HasColumnName("incridients");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasColumnType("money")
                .HasColumnName("price");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("employee_pkey");

            entity.ToTable("employee");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Patronymic)
                .HasMaxLength(50)
                .HasColumnName("patronymic");
            entity.Property(e => e.Photo).HasColumnName("photo");
            entity.Property(e => e.ScanContract).HasColumnName("scan_contract");
            entity.Property(e => e.Speciality).HasColumnName("speciality");
            entity.Property(e => e.Status)
           .HasColumnName("status")
           .HasConversion(
               v => v.ToString(),
               v => (EmployeeStatus)Enum.Parse(typeof(EmployeeStatus), v)
           ).HasMaxLength(50); ;
            entity.Property(e => e.Surname)
                .HasMaxLength(50)
                .HasColumnName("surname");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Order_pkey");

            entity.ToTable("Order");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClientsCount).HasColumnName("clients_count");
            entity.Property(e => e.FkDishesid).HasColumnName("fk_dishesid");
            entity.Property(e => e.FkEmployeeid).HasColumnName("fk_employeeid");
            entity.Property(e => e.Price)
                .HasColumnType("money")
                .HasColumnName("price");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.TableNumber).HasColumnName("table_number");

            entity.HasOne(d => d.FkDishes).WithMany(p => p.Orders)
                .HasForeignKey(d => d.FkDishesid)
                .HasConstraintName("Order_fk_dishesid_fkey");

            entity.HasOne(d => d.FkEmployee).WithMany(p => p.Orders)
                .HasForeignKey(d => d.FkEmployeeid)
                .HasConstraintName("Order_fk_employeeid_fkey");
            entity.Property(e => e.CreatedAt)
                .HasColumnName("createdat")  
                .HasColumnType("timestamp without time zone")
                .HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<Shift>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("shifts_pkey");

            entity.ToTable("shifts");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DateFinis)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_finis");
            entity.Property(e => e.DateStart)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_start");
        });

        modelBuilder.Entity<Shiftassignment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("shiftassignment_pkey");

            entity.ToTable("shiftassignment");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FkEmployeeid).HasColumnName("fk_employeeid");
            entity.Property(e => e.FkShiftsid).HasColumnName("fk_shiftsid");

            entity.HasOne(d => d.FkEmployee).WithMany(p => p.Shiftassignments)
                .HasForeignKey(d => d.FkEmployeeid)
                .HasConstraintName("shiftassignment_fk_employeeid_fkey");

            entity.HasOne(d => d.FkShifts).WithMany(p => p.Shiftassignments)
                .HasForeignKey(d => d.FkShiftsid)
                .HasConstraintName("shiftassignment_fk_shiftsid_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasColumnName("email");
            entity.Property(e => e.FkEmployeeId).HasColumnName("fk_employee_id");
            entity.Property(e => e.Login)
                .HasMaxLength(100)
                .HasColumnName("login");
            entity.Property(e => e.Password)
                .HasMaxLength(50)
                .HasColumnName("password");

            entity.HasOne(d => d.FkEmployee).WithMany(p => p.Users)
                .HasForeignKey(d => d.FkEmployeeId)
                .HasConstraintName("users_fk_employee_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
