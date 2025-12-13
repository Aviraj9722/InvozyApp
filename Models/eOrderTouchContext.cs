using eOrderTouchApp.Models.ReportsModel;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace eOrderTouchApp.Models;

public partial class eOrderTouchContext : DbContext
{
    public eOrderTouchContext()
    {
    }

    public eOrderTouchContext(DbContextOptions<eOrderTouchContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TblBusiness> TblBusinesses { get; set; }

    public virtual DbSet<TblBusinessType> TblBusinessTypes { get; set; }

    public virtual DbSet<TblCategory> TblCategories { get; set; }

    public virtual DbSet<TblEnquiry> TblEnquiries { get; set; }

    public virtual DbSet<TblFeedback> TblFeedbacks { get; set; }

    public virtual DbSet<TblOrderDetail> TblOrderDetails { get; set; }

    public virtual DbSet<TblOrderMaster> TblOrderMasters { get; set; }

    public virtual DbSet<TblPrinterSize> TblPrinterSizes { get; set; }

    public virtual DbSet<TblProduct> TblProducts { get; set; }

    public virtual DbSet<TblUom> TblUoms { get; set; }

    public virtual DbSet<TblUser> TblUsers { get; set; }

    public virtual DbSet<TblUserLicense> TblUserLicenses { get; set; }

    public DbSet<TblDealer> TblDealer { get; set; }
    public DbSet<TblVendor> TblVendors { get; set; }
    public DbSet<TblPOMaster> TblPOMaster { get; set; }
    public DbSet<TblPODetails> TblPODetails { get; set; }
    public DbSet<TblReports> TblReports { get; set; }
   

    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //        => optionsBuilder.UseSqlServer("Server=DESKTOP-1TNHCLD\\SQLEXPRESS;Database=eOrderTouch;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TblBusiness>(entity =>
        {
            entity.ToTable("tblBusinesses");

            entity.Property(e => e.Address).HasMaxLength(250);
            entity.Property(e => e.BusinessName).HasMaxLength(150);
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.MobileNo).HasMaxLength(50);
            entity.Property(e => e.Gstin)
                .HasMaxLength(20)
                .HasColumnName("GSTIN");
            entity.Property(e => e.IsGstapplicable).HasColumnName("IsGSTApplicable");
            entity.Property(e => e.Logo).IsUnicode(false);
            entity.Property(e => e.OwnerName).HasMaxLength(100);
            entity.Property(e => e.Qrcode)
                .IsUnicode(false)
                .HasColumnName("QRCode");

            entity.HasOne(d => d.BusinessType).WithMany(p => p.TblBusinesses)
                .HasForeignKey(d => d.BusinessTypeId)
                .HasConstraintName("FK_tblBusinesses_tblBusinessType");

            entity.HasOne(d => d.PrinterSize).WithMany(p => p.TblBusinesses)
                .HasForeignKey(d => d.PrinterSizeId)
                .HasConstraintName("FK_tblBusinesses_tblPrinterSize");
        });

        modelBuilder.Entity<TblBusinessType>(entity =>
        {
            entity.ToTable("tblBusinessType");

            entity.Property(e => e.BusinessTypeName).HasMaxLength(100);
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblCategory>(entity =>
        {
            entity.ToTable("tblCategories");

            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasOne(d => d.Business).WithMany(p => p.TblCategories)
                .HasForeignKey(d => d.BusinessId)
                .HasConstraintName("FK_tblCategories_tblBusinesses");
        });

        modelBuilder.Entity<TblEnquiry>(entity =>
        {
            entity.ToTable("tblEnquiry");

            entity.Property(e => e.EmailId).HasMaxLength(100);
            entity.Property(e => e.FollowUpFour).HasMaxLength(50);
            entity.Property(e => e.FollowUpOne).HasMaxLength(50);
            entity.Property(e => e.FollowUpThree).HasMaxLength(50);
            entity.Property(e => e.FollowUpTwo).HasMaxLength(50);
            entity.Property(e => e.MobileNo).HasMaxLength(15);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<TblFeedback>(entity =>
        {
            entity.ToTable("tblFeedback");

            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.CustomerName).HasMaxLength(255);
            entity.Property(e => e.MobileNo).HasMaxLength(50);

            entity.HasOne(d => d.Buisness).WithMany(p => p.TblFeedbacks)
                .HasForeignKey(d => d.BuisnessId)
                .HasConstraintName("FK_tblFeedback_tblBusinesses");
        });

        modelBuilder.Entity<TblOrderDetail>(entity =>
        {
            entity.ToTable("tblOrderDetails");

            entity.Property(e => e.Gstamount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("GSTAmount");
            entity.Property(e => e.Gstpercentage)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("GSTPercentage");
            entity.Property(e => e.Oid).HasColumnName("OID");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Qty).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Total).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.OidNavigation).WithMany(p => p.TblOrderDetails)
                .HasForeignKey(d => d.Oid)
                .HasConstraintName("FK_tblOrderDetails_tblOrderMaster");

            entity.HasOne(d => d.Product).WithMany(p => p.TblOrderDetails)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_tblOrderDetails_tblProduct");
        });

        modelBuilder.Entity<TblOrderMaster>(entity =>
        {
            entity.ToTable("tblOrderMaster");

            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.CustomerMobNo).HasMaxLength(15);
            entity.Property(e => e.TableDetails).HasMaxLength(50);
            entity.Property(e => e.CustomerName).HasMaxLength(255);
            entity.Property(e => e.DateOfOrder).HasColumnType("datetime");
            entity.Property(e => e.GrandTotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Gsttotal)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("GSTTotal");
            entity.Property(e => e.PaymentMode).HasMaxLength(20);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Buisness).WithMany(p => p.TblOrderMasters)
                .HasForeignKey(d => d.BuisnessId)
                .HasConstraintName("FK_tblOrderMaster_tblBusinesses");

            entity.HasOne(d => d.User).WithMany(p => p.TblOrderMasters)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_tblOrderMaster_tblUser");
        });

        modelBuilder.Entity<TblPrinterSize>(entity =>
        {
            entity.ToTable("tblPrinterSize");

            entity.Property(e => e.PrinterSize).HasMaxLength(50);
        });

        modelBuilder.Entity<TblProduct>(entity =>
        {
            entity.ToTable("tblProduct");

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.Gstamount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("GSTAmount");
            entity.Property(e => e.Gstpercentage)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("GSTPercentage");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Photo).IsUnicode(false);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RegionalName).HasMaxLength(1000);
            entity.Property(e => e.UoMid).HasColumnName("UoMId");

            entity.HasOne(d => d.Business).WithMany(p => p.TblProducts)
                .HasForeignKey(d => d.BusinessId)
                .HasConstraintName("FK_tblProduct_tblBusinesses");

            entity.HasOne(d => d.Category).WithMany(p => p.TblProducts)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_tblProduct_tblCategories");

            entity.HasOne(d => d.User).WithMany(p => p.TblProducts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_tblProduct_tblUser");
        });

        modelBuilder.Entity<TblUom>(entity =>
        {
            entity.ToTable("tblUOM");

            entity.Property(e => e.UnitName).HasMaxLength(20);

            entity.HasOne(d => d.Business).WithMany(p => p.TblUoms)
                .HasForeignKey(d => d.BusinessId)
                .HasConstraintName("FK_tblUOM_tblBusinesses");
        });

        modelBuilder.Entity<TblUser>(entity =>
        {
            entity.ToTable("tblUser");

            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.EmailId).HasMaxLength(50);
            entity.Property(e => e.MobileNumber).HasMaxLength(15);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.UserName).HasMaxLength(50);

            entity.HasOne(d => d.Bussiness).WithMany(p => p.TblUsers)
                .HasForeignKey(d => d.BussinessId)
                .HasConstraintName("FK_tblUser_tblBusinesses");
        });

        modelBuilder.Entity<TblUserLicense>(entity =>
        {
            entity.ToTable("tblUserLicense");

            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.LicenseKey).HasMaxLength(100);
            entity.Property(e => e.StartDate).HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.TblUserLicenses)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_tblUserLicense_tblUser");
        });

        modelBuilder.Entity<TblDealer>(entity =>
        {
            entity.ToTable("tblDealer");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Address).HasMaxLength(250);
            entity.Property(e => e.MobileNo).HasMaxLength(15);
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.EmailId).HasMaxLength(100);
            entity.Property(e => e.GSTN).HasMaxLength(20);

            entity.Property(e => e.DealerCode)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblVendor>(entity =>
        {
            entity.ToTable("tblVendor");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.EmailId).HasMaxLength(100);
            entity.Property(e => e.MobileNo).HasMaxLength(15);
            entity.Property(e => e.GSTN).HasMaxLength(20);
            entity.Property(e => e.Address).HasMaxLength(250);
        });

        modelBuilder.Entity<TblPOMaster>(entity =>
        {
            entity.ToTable("tblPOMaster");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.DateOfPurchase).HasColumnType("datetime");
            entity.Property(e => e.GrandTotal).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Business)
                .WithMany(b => b.POMasters)
                .HasForeignKey(e => e.BusinessId)
                .HasConstraintName("FK_tblPOMaster_tblBusinesses");

            entity.HasOne(e => e.Vendor)
                .WithMany(v => v.POMasters)
                .HasForeignKey(e => e.VendorId)
                .HasConstraintName("FK_tblPOMaster_tblVendor");
        });

        modelBuilder.Entity<TblPODetails>(entity =>
        {
            entity.ToTable("tblPODetails");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Quantity).HasColumnType("int");

            entity.HasOne(e => e.POMaster)
                .WithMany(m => m.PODetails)
                .HasForeignKey(e => e.POMasterId)
                .HasConstraintName("FK_tblPODetails_tblPOMaster");

            entity.HasOne(e => e.Product)
                .WithMany(p => p.PODetails)
                .HasForeignKey(e => e.ProductId)
                .HasConstraintName("FK_tblPODetails_tblProduct");
        });

        modelBuilder.Entity<TblReports>(entity =>
        {
            entity.ToTable("tblReports");       

            entity.HasKey(e => e.Id);           

            entity.Property(e => e.Id)
                .HasColumnName("Id");

            entity.Property(e => e.Name)
                .HasColumnName("Name")
                .HasMaxLength(200)            
                .IsRequired(false);           
        });

        

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    
}
