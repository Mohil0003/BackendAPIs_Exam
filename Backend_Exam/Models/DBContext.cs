using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Backend_Exam.Models;

public partial class DBContext : DbContext
{
    public DBContext()
    {
    }

    public DBContext(DbContextOptions<DBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<TicketComment> TicketComments { get; set; }

    public virtual DbSet<TicketStatusLog> TicketStatusLogs { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=MOHIL\\SQLEXPRESS;Database=Support_Ticket_Mgm;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasOne(d => d.AssignedToNavigation).WithMany(p => p.TicketAssignedToNavigations).HasConstraintName("FK_tickets_users");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TicketCreatedByNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tickets_users1");
        });

        modelBuilder.Entity<TicketComment>(entity =>
        {
            entity.HasOne(d => d.Ticket).WithMany(p => p.TicketComments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ticket_comments_tickets");

            entity.HasOne(d => d.User).WithMany(p => p.TicketComments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ticket_comments_users");
        });

        modelBuilder.Entity<TicketStatusLog>(entity =>
        {
            entity.HasOne(d => d.ChangedByNavigation).WithMany(p => p.TicketStatusLogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ticket_status_logs_users");

            entity.HasOne(d => d.Ticket).WithMany(p => p.TicketStatusLogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ticket_status_logs_tickets");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_users_roles");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
