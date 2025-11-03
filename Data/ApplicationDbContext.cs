using Employees_Attendence.Models;
using Microsoft.EntityFrameworkCore;

namespace Employees_Attendence.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Worker> Workers => Set<Worker>();
        public DbSet<WeeklyPayrollRecord> WeeklyPayrollRecords => Set<WeeklyPayrollRecord>();
        public DbSet<MonthlyPayrollRecord> MonthlyPayrollRecords => Set<MonthlyPayrollRecord>();
        public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            
        }
    }
}
