using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LibraryBQ.Model;

public partial class LibraryBQContext : DbContext
{
    public LibraryBQContext()
    {
    }

    public LibraryBQContext(DbContextOptions<LibraryBQContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<BookCopy> BookCopies { get; set; }

    public virtual DbSet<LoanHistory> LoanHistories { get; set; }

    public virtual DbSet<LoanStatus> LoanStatuses { get; set; }

    public virtual DbSet<ReservationHistory> ReservationHistories { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) // DI 컨테이너 DbContext 폴링 적용, 더이상 미사용
    {
        // Json 설정 파일(appsetting.json) - 구성 객체(IConfiguration)를 이용한 연결문자열 사용, DB 연동 실행.
        string? connStr = App.Config.GetConnectionString("MSSQLConnection");
        optionsBuilder.UseSqlServer(connStr);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Book__3214EC27127AE2C4");

            entity.ToTable("Book");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Author)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Title)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<BookCopy>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BookCopy__3214EC2730B22215");

            entity.ToTable("BookCopy", tb => tb.HasTrigger("trg_BookCopy_Insert"));

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.BookId).HasColumnName("BookID");
            entity.Property(e => e.LoanStatusId).HasColumnName("LoanStatusID");

            entity.HasOne(d => d.Book).WithMany(p => p.BookCopies)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BookCopy__BookID__3E52440B");

            entity.HasOne(d => d.LoanStatus).WithMany(p => p.BookCopies)
                .HasForeignKey(d => d.LoanStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BookCopy__LoanSt__3F466844");
        });

        modelBuilder.Entity<LoanHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LoanHist__3214EC27364A2E3C");

            entity.ToTable("LoanHistory");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.BookCopyId).HasColumnName("BookCopyID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.BookCopy).WithMany(p => p.LoanHistories)
                .HasForeignKey(d => d.BookCopyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LoanHisto__BookC__46E78A0C");

            entity.HasOne(d => d.User).WithMany(p => p.LoanHistories)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LoanHisto__UserI__47DBAE45");
        });

        modelBuilder.Entity<LoanStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LoanStat__3214EC27213D4B7E");

            entity.ToTable("LoanStatus");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.StatusName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ReservationHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Reservat__3214EC2793D829AA");

            entity.ToTable("ReservationHistory");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.BookCopyId).HasColumnName("BookCopyID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.BookCopy).WithMany(p => p.ReservationHistories)
                .HasForeignKey(d => d.BookCopyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reservati__BookC__5629CD9C");

            entity.HasOne(d => d.User).WithMany(p => p.ReservationHistories)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reservati__UserI__571DF1D5");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3214EC274D4F3B34");

            entity.ToTable("User");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.MaxLoanNum).HasDefaultValue((byte)10);
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UserNo)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
