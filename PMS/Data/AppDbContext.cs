﻿using Microsoft.EntityFrameworkCore;
using PMS.Models;
using System;

namespace PMS.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Project> Projects { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<ProjectEmployee> ProjectEmployees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProjectEmployee>()
                .HasKey(pe => new { pe.ProjectId, pe.EmployeeId });

            modelBuilder.Entity<ProjectEmployee>()
                .HasOne(pe => pe.Project)
                .WithMany(p => p.ProjectEmployees)
                .HasForeignKey(pe => pe.ProjectId);

            modelBuilder.Entity<ProjectEmployee>()
                .HasOne(pe => pe.Employee)
                .WithMany(e => e.ProjectEmployees)
                .HasForeignKey(pe => pe.EmployeeId);
        }
    }
}
