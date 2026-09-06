using Microsoft.EntityFrameworkCore;
using UCore;

namespace EFRepository.EntityClass;

public static class EntityForAddress
{
    public static void AddEntityAddress(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Address>(entity =>
        {
            entity.ToTable("Address");
            entity.HasKey(e => e.AddressId);
            entity.Property(e => e.AddressId).HasColumnName("Id").ValueGeneratedOnAdd();
            entity.Property(e => e.AddressString).HasMaxLength(255);
            entity.Property(e => e.City).HasMaxLength(255);
            entity.Property(e => e.Country).HasMaxLength(255);
            entity.Property(e => e.HouseNumber).HasMaxLength(255);
            entity.Property(e => e.Street).HasMaxLength(255);
        });
    }
}   