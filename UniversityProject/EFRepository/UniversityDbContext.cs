using System;
using System.Collections.Generic;
using EFRepository.EntityClass;
using Microsoft.EntityFrameworkCore;
using UCore;

namespace EFRepository;

public partial class UniversityDbContext : DbContext
{
    public UniversityDbContext() { }
    public UniversityDbContext(DbContextOptions<UniversityDbContext> options)
        : base(options)
    {
    }
    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<MillitaryClass> IdMilitaries { get; set; }
    public virtual DbSet<DegreesStudyClass> DegreesStudy { get; set; }

    public virtual DbSet<Passport> Passports { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddEntityAddress();
        // modelBuilder.AddEntityForDegreesStudy();
        modelBuilder.AddEntityForMillitaryClass();
        modelBuilder.AddEntityForPassport();
        modelBuilder.AddEntityForStudent();
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    private string _getConnectionString;
}
