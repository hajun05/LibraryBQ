using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LibraryBQ.Model;

// EF Core DbContext 클래스: 데이터베이스와의 연결 및 엔티티 매핑을 담당.
// 도서관 시스템 내 도서, 대출, 예약, 사용자 정보 등을 관리.
public partial class LibraryBQContext : DbContext
{
    // 기본 생성자. DI 없이 수동으로 컨텍스트를 생성할 때 사용.
    public LibraryBQContext()
    {
    }

    // DI 컨테이너를 통한 DbContextOptions 주입용 생성자.
    // 보통 애플리케이션에서 이 생성자를 사용해서 컨텍스트를 생성.
    public LibraryBQContext(DbContextOptions<LibraryBQContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Book> Books { get; set; } // 도서(Book) 엔티티의 테이블과 연결된 DbSet.

    public virtual DbSet<BookCopy> BookCopies { get; set; } // 도서 복사본(BookCopy) 엔티티에 대한 DbSet.

    public virtual DbSet<LoanHistory> LoanHistories { get; set; } // 대출 기록(LoanHistory)을 저장하는 DbSet.

    public virtual DbSet<LoanStatus> LoanStatuses { get; set; } // 대출 상태(LoanStatus)를 나타내는 DbSet.

    public virtual DbSet<ReservationHistory> ReservationHistories { get; set; } // 예약 기록(ReservationHistory) 엔티티 컬렉션.

    public virtual DbSet<User> Users { get; set; } // 사용자(User) 정보를 담는 DbSet.

    // 데이터베이스 연결 옵션 구성 메서드.
    // DI 컨테이너에서 DbContext를 주입 받으면 이 메서드는 호출되지 않음 (미사용 주석 참고).
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Json 설정 파일(appsetting.json) - 구성 객체(IConfiguration)를 이용한 연결문자열 사용, DB 연동 실행.
        string? connStr = App.Config.GetConnectionString("MSSQLConnection");
        optionsBuilder.UseSqlServer(connStr);
    }

    // EF Core 모델 생성 및 테이블-엔티티 매핑 설정.
    // 각 엔티티의 기본키, 테이블명, 컬럼명, 제약조건, 관계(외래키 등) 정의.
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

    // 추가 매핑용 부분 메서드(스캐폴딩이 partial 메서드로 생성해 둠).
    // 필요시에 별도의 partial 클래스에서 구현 가능.
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
