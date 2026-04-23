using MediCare.API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MediCare.API.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<long>, long>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Visit> Visits { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Bed> Beds { get; set; }
        public DbSet<BedAssignment> BedAssignments { get; set; }
        public DbSet<Medication> Medications { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<DoctorSchedule> DoctorSchedules { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Đổi tên bảng Identity
            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
            modelBuilder.Entity<IdentityRole<long>>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<long>>().ToTable("UserRoles");

            // bảng Patient
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasIndex(p => p.UHID).IsUnique(); // Unique constraint on UHID
                entity.Property(p => p.PatientType).HasDefaultValue("Outpatient"); // Default value for PatientType
            });

            // bảng Doctor
            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.HasIndex(d => d.LicenseNumber).IsUnique(); // Unique constraint on LicenseNumber

                entity.Property(d => d.ConsultationFee)
                      .HasPrecision(18, 2);

                entity.HasOne(d => d.User)
                      .WithOne(u => u.Doctor)
                      .HasForeignKey<Doctor>(d => d.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Department)
                      .WithMany(dep => dep.Doctors)
                      .HasForeignKey(d => d.DepartmentId);
            });

            // bảng DoctorSchedule
            modelBuilder.Entity<DoctorSchedule>(entity =>
            {
                entity.HasOne(ds => ds.Doctor) // 1-1 với Doctor
                      .WithMany(d => d.Schedules) // 1 Doctor có nhiều Schedule
                      .HasForeignKey(ds => ds.Id) 
                      .OnDelete(DeleteBehavior.Cascade); // doctor bị xóa, xóa luôn schedule

                entity.Property(ds => ds.DayOfWeek)
                      .HasColumnType("TINYINT"); // enum DayOfWeek (0-6)
            });

            // bảng Appointment
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasIndex(a => new { a.DoctorId, a.AppointmentDate, a.StartTime }) //1 doctor không có 2 appointment trùng giờ
                      .IsUnique();

                entity.HasOne(a => a.Patient) // 1-1
                      .WithMany(p => p.Appointments) // 1-n
                      .HasForeignKey(a => a.PatientId)
                      .OnDelete(DeleteBehavior.Cascade); // bệnh nhân bị xóa, xóa luôn appointment

                entity.HasOne(a => a.Doctor)
                      .WithMany(d => d.Appointments) // 1-n
                      .HasForeignKey(a => a.DoctorId);
            });

            // bảng Visit
            modelBuilder.Entity<Visit>(entity =>
            {
                entity.HasOne(v => v.Appointment)
                      .WithMany(a => a.Visits)
                      .HasForeignKey(v => v.AppointmentId);
            });

            // bảng Prescription
            modelBuilder.Entity<Prescription>(entity =>
            {
                entity.HasOne(p => p.Visit)
                      .WithMany(v => v.Prescriptions)
                      .HasForeignKey(p => p.VisitId);
            });

            // bảng Bed
            modelBuilder.Entity<Bed>(entity =>
            {
                entity.HasIndex(b => new { b.RoomId, b.BedNumber })
                      .IsUnique();
            });
            // bảng BedAssignment
            modelBuilder.Entity<BedAssignment>(entity =>
            {
                entity.HasOne(b => b.Bed)
                      .WithMany(b => b.BedAssignments)
                      .HasForeignKey(b => b.BedId);

                entity.HasOne(b => b.Patient)
                      .WithMany(p => p.BedAssignments)
                      .HasForeignKey(b => b.PatientId);
            });

            // bảng Invoice và InvoiceItem lưu thông tin thanh toán cho bệnh nhân
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasIndex(i => i.InvoiceNumber).IsUnique();

                entity.Property(i => i.SubTotal).HasPrecision(18, 2); // SubTotal = sum(InvoiceItem.TotalPrice)
                entity.Property(i => i.DiscountAmount).HasPrecision(18, 2);// DiscountAmount = SubTotal * DiscountPercent
                entity.Property(i => i.TaxAmount).HasPrecision(18, 2);
                entity.Property(i => i.TotalAmount).HasPrecision(18, 2);
                entity.Property(i => i.PaidAmount).HasPrecision(18, 2);

                entity.HasMany(i => i.InvoiceItems)
                      .WithOne(ii => ii.Invoice)
                      .HasForeignKey(ii => ii.InvoiceId);
            });

            modelBuilder.Entity<InvoiceItem>(entity =>
            {
                entity.Property(i => i.Quantity).HasPrecision(10, 2);// Quantity có thể là số lẻ
                entity.Property(i => i.UnitPrice).HasPrecision(18, 2); // UnitPrice = TotalAmount / Quantity
                entity.Property(i => i.TotalPrice).HasPrecision(18, 2); // TotalPrice = Quantity * UnitPrice
            });

            // bảng RefreshToken
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasOne(rt => rt.User)
                      .WithMany()
                      .HasForeignKey(rt => rt.UserId)
                      .OnDelete(DeleteBehavior.Cascade); // user bị xóa, xóa luôn refresh token
            });
        }
    }
}