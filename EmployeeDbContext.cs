using Microsoft.EntityFrameworkCore;

namespace OracleTestContainersExample;

public class EmployeeDbContext : DbContext
{
    public EmployeeDbContext(DbContextOptions<EmployeeDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("EMPLOYEES");

            entity.HasKey(e => e.Id);

            entity
                .Property(e => e.Id)
                .HasColumnName("ID");

            entity
                .Property(e => e.Name)
                .HasColumnName("NAME")
                .HasMaxLength(100)
                .IsRequired();

            entity
                .Property(e => e.Email)
                .HasColumnName("EMAIL")
                .HasMaxLength(100)
                .IsRequired();

            entity
                .Property(e => e.HireDate)
                .HasColumnName("HIRE_DATE");
        });
    }
}
