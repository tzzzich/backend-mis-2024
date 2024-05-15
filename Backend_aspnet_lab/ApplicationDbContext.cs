using Backend_aspnet_lab;
using Backend_aspnet_lab.Models;
using Backend_aspnet_lab.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Configuration;

public class ApplicationDbContext : IdentityDbContext<Doctor, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
    {
    }

    public DbSet<Comment> Comments { get; set; }

    public DbSet<Consultation> Consultations { get; set; }

    public DbSet<Diagnosis> Diagnoses { get; set; }

    public DbSet<Doctor> Doctors { get; set; }

    public DbSet<Inspection> Inspections { get; set; }

    public DbSet<Patient> Patients { get; set; }

    public DbSet<Speciality> Specialities { get; set; }

    public DbSet<Icd10Record> Icd10Records { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

	}
}