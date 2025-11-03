using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ManageLibrary.Models;

public partial class ManageLibraryContext : DbContext
{
    public ManageLibraryContext()
    {
    }

    public ManageLibraryContext(DbContextOptions<ManageLibraryContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<Category> Categories { get; set; }


    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<LoanDetail> LoanDetails { get; set; }

    public virtual DbSet<LoanSlip> LoanSlips { get; set; }

    public virtual DbSet<Publisher> Publishers { get; set; }

    public virtual DbSet<Reader> Readers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Let Program.cs (AddDbContext) configure the provider from appsettings.json.
        // Only apply a fallback when not configured (e.g., design-time tooling).
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=DESKTOP-MT8EFFJ;Database=ManageLibrary;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__Account__349DA5A6FF1C58C7");

            entity.ToTable("Account");

            entity.HasIndex(e => e.ReaderId, "UQ_Account_Reader").IsUnique();

            entity.HasIndex(e => e.Username, "UQ__Account__536C85E4470847FE").IsUnique();

            entity.Property(e => e.AccountId)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.EmployeeId)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.ReaderId)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Employee).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__Account__Employe__3D5E1FD2");

            entity.HasOne(d => d.Reader).WithOne(p => p.Account)
                .HasForeignKey<Account>(d => d.ReaderId)
                .HasConstraintName("FK__Account__ReaderI__3E52440B");
        });

        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(e => e.AuthorId).HasName("PK__Author__70DAFC342110CD87");

            entity.ToTable("Author");

            entity.Property(e => e.AuthorId)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.BookId).HasName("PK__Books__3DE0C20738E8E067");

            entity.Property(e => e.BookId)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.AuthorId)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CategoryId)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Cost).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Position).HasMaxLength(50);
            entity.Property(e => e.PublisherId)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Author).WithMany(p => p.Books)
                .HasForeignKey(d => d.AuthorId)
                .HasConstraintName("FK__Books__AuthorId__47DBAE45");

            entity.HasOne(d => d.Category).WithMany(p => p.Books)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Books__CategoryI__46E78A0C");

            entity.HasOne(d => d.Publisher).WithMany(p => p.Books)
                .HasForeignKey(d => d.PublisherId)
                .HasConstraintName("FK__Books__Publisher__48CFD27E");

            entity.HasMany(d => d.Authors).WithMany(p => p.BooksNavigation)
                .UsingEntity<Dictionary<string, object>>(
                    "BookAuthor",
                    r => r.HasOne<Author>().WithMany()
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__BookAutho__Autho__5629CD9C"),
                    l => l.HasOne<Book>().WithMany()
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__BookAutho__BookI__5535A963"),
                    j =>
                    {
                        j.HasKey("BookId", "AuthorId").HasName("PK__BookAuth__6AED6DC4713A4E4F");
                        j.ToTable("BookAuthor");
                        j.IndexerProperty<string>("BookId")
                            .HasMaxLength(20)
                            .IsUnicode(false);
                        j.IndexerProperty<string>("AuthorId")
                            .HasMaxLength(20)
                            .IsUnicode(false);
                    });
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Category__19093A0B747258D5");

            entity.ToTable("Category");

            entity.Property(e => e.CategoryId)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PK__Employee__7AD04F118312997E");

            entity.Property(e => e.EmployeeId)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.Telephone).HasMaxLength(20);
        });

        modelBuilder.Entity<LoanDetail>(entity =>
        {
            entity.HasKey(e => e.LoanDetailId).HasName("PK__LoanDeta__760C10C891495266");

            entity.ToTable("LoanDetail");

            entity.Property(e => e.LoanDetailId)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.BookId)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Fine)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IsLose).HasDefaultValue(false);
            entity.Property(e => e.LoanId)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.LoanStatus).HasMaxLength(50);
            entity.Property(e => e.ReturnStatus).HasMaxLength(50);

            entity.HasOne(d => d.Book).WithMany(p => p.LoanDetails)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LoanDetai__BookI__52593CB8");

            entity.HasOne(d => d.Loan).WithMany(p => p.LoanDetails)
                .HasForeignKey(d => d.LoanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LoanDetai__LoanI__5165187F");
        });

        modelBuilder.Entity<LoanSlip>(entity =>
        {
            entity.HasKey(e => e.LoanId).HasName("PK__LoanSlip__4F5AD45765F47DF0");

            entity.ToTable("LoanSlip");

            entity.Property(e => e.LoanId)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.EmployeeId)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ReaderId)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Employee).WithMany(p => p.LoanSlips)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LoanSlip__Employ__4CA06362");

            entity.HasOne(d => d.Reader).WithMany(p => p.LoanSlips)
                .HasForeignKey(d => d.ReaderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LoanSlip__Reader__4BAC3F29");
        });

        modelBuilder.Entity<Publisher>(entity =>
        {
            entity.HasKey(e => e.PublisherId).HasName("PK__Publishe__4C657FAB4C51E3AD");

            entity.ToTable("Publisher");

            entity.Property(e => e.PublisherId)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Telephone).HasMaxLength(20);
        });

        modelBuilder.Entity<Reader>(entity =>
        {
            entity.HasKey(e => e.ReaderId).HasName("PK__Readers__8E67A5E18E911D82");

            entity.Property(e => e.ReaderId)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.NationalId).HasMaxLength(20);
            entity.Property(e => e.Telephone).HasMaxLength(20);
            entity.Property(e => e.TypeOfReader).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
