using Microsoft.EntityFrameworkCore;
using UCore;

namespace EFRepository.EntityClass;

public static class EntityForPassport
{
    public static void AddEntityForPassport(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Passport>(entity =>
        {
            entity.ToTable("Passport");
            entity.HasKey(e => e.PassportId);   
            entity.Property(e => e.PassportId).HasColumnName("Id").ValueGeneratedOnAdd();
            entity.HasIndex(e => new { e.Serial, e.Number }, "IndexSerialNumber").IsUnique();
            entity.Property(e => e.FirstName).HasMaxLength(255);
            entity.Property(e => e.LastName).HasMaxLength(255);
            entity.Property(e => e.MiddleName).HasMaxLength(255);
            entity.Property(e => e.Number).HasMaxLength(6);
            entity.Property(e => e.PlaceReceipt).HasMaxLength(255);
            entity.Property(e => e.Serial).HasMaxLength(4);
            entity.HasOne(d => d.Address).WithMany(p => p.Passports)
                .HasForeignKey(d => d.AddressId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Passport_AddressId_Address_Id");
        });
    }
}