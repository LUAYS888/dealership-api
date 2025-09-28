using Dealership.Api.Domain;                  //gateway to my DB.          This line refer to the folder Domain.
using Microsoft.EntityFrameworkCore;

namespace Dealership.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options) //The bridge between C# code and database.
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<OtpCode> Otps => Set<OtpCode>();   // <-- add this

    public DbSet<PurchaseRequest> PurchaseRequests => Set<PurchaseRequest>();
    public DbSet<Sale> Sales => Set<Sale>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        b.Entity<Vehicle>()
            .HasIndex(v => v.Vin)                //Vehicle Identification Number must be unique.
            .IsUnique();

        b.Entity<OtpCode>()
            .HasIndex(x => new { x.Email, x.Purpose, x.ConsumedUtc })
            .HasDatabaseName("IX_Otp_EmailPurpose"); // lookups for register

        b.Entity<OtpCode>()
            .HasIndex(x => new { x.UserId, x.Purpose, x.ConsumedUtc })
            .HasDatabaseName("IX_Otp_UserPurpose"); // lookups for login / others

        // simple required fields
        b.Entity<User>().Property(u => u.Email).IsRequired();
        b.Entity<User>().Property(u => u.PasswordHash).IsRequired();

        b.Entity<Vehicle>().Property(v => v.Vin).IsRequired();
        b.Entity<Vehicle>().Property(v => v.Make).IsRequired();
        b.Entity<Vehicle>().Property(v => v.Model).IsRequired();
        b.Entity<Vehicle>().Property(v => v.Year).IsRequired();
        b.Entity<Vehicle>().Property(v => v.Price).HasColumnType("decimal(18,2)");

        b.Entity<PurchaseRequest>()
        .HasIndex(x => new { x.UserId, x.VehicleId, x.Status });

        b.Entity<PurchaseRequest>()
            .HasOne(pr => pr.User)
            .WithMany()
            .HasForeignKey(pr => pr.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<PurchaseRequest>()
            .HasOne(pr => pr.Vehicle)
            .WithMany()
            .HasForeignKey(pr => pr.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Sale>()
            .Property(s => s.FinalPrice)
            .HasColumnType("decimal(18,2)");

        b.Entity<Sale>()
            .HasOne(s => s.Vehicle)
            .WithMany()
            .HasForeignKey(s => s.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Sale>()
            .HasOne(s => s.Customer)
            .WithMany()
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
