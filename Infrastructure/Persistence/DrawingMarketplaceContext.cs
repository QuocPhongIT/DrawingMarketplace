using System;
using System.Collections.Generic;
using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DrawingMarketplace.Infrastructure.Persistence;

public partial class DrawingMarketplaceContext : DbContext
{
    public DrawingMarketplaceContext()
    {
    }

    public DrawingMarketplaceContext(DbContextOptions<DrawingMarketplaceContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Banner> Banners { get; set; } = null!;
    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Collaborator> Collaborators { get; set; }

    public virtual DbSet<CollaboratorBank> CollaboratorBanks { get; set; }

    public virtual DbSet<CollaboratorRequest> CollaboratorRequests { get; set; }

    public virtual DbSet<Content> Contents { get; set; }

    public virtual DbSet<ContentStat> ContentStats { get; set; }

    public virtual DbSet<CopyrightReport> CopyrightReports { get; set; }

    public virtual DbSet<Coupon> Coupons { get; set; }

    public virtual DbSet<Download> Downloads { get; set; }

    public virtual DbSet<MediaFile> Files { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderCoupon> OrderCoupons { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Otp> Otps { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentTransaction> PaymentTransactions { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<Wallet> Wallets { get; set; }

    public virtual DbSet<WalletTransaction> WalletTransactions { get; set; }

    public virtual DbSet<Withdrawal> Withdrawals { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //modelBuilder
        //    .HasPostgresEnum("collaborator_status", new[] { "pending", "approved", "rejected", "suspended" })
        //    .HasPostgresEnum("content_status", new[] { "draft", "published", "archived" })
        //    .HasPostgresEnum("coupon_type", new[] { "percent", "fixed" })
        //    .HasPostgresEnum("order_status", new[] { "pending", "paid", "cancelled", "failed" })
        //    .HasPostgresEnum("otp_type", new[] { "verify_account", "reset_password" })
        //    .HasPostgresEnum("payment_status", new[] { "pending", "success", "failed" })
        //    .HasPostgresEnum("report_status", new[] { "pending", "resolved", "rejected" })
        //    .HasPostgresEnum("user_status", new[] { "active", "inactive", "banned" })
        //    .HasPostgresEnum("wallet_owner_type", new[] { "user", "collaborator" })
        //    .HasPostgresEnum("wallet_tx_type", new[] { "credit", "debit", "commission", "withdrawal" })
        //    .HasPostgresEnum("withdrawal_status", new[] { "pending", "approved", "rejected", "paid" })
        //.HasPostgresExtension("pgcrypto");
        modelBuilder.Entity<Banner>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("banners_pkey");
            entity.ToTable("banners");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");

            entity.Property(e => e.Title)
                .IsRequired()
                .HasColumnName("title");

            entity.Property(e => e.Subtitle)
                .HasColumnName("subtitle");

            entity.Property(e => e.ImageUrl)
                .IsRequired()
                .HasColumnName("image_url");

            entity.Property(e => e.Button1Text)
                .HasColumnName("button1_text");

            entity.Property(e => e.Button1Link)
                .HasColumnName("button1_link");

            entity.Property(e => e.Button2Text)
                .HasColumnName("button2_text");

            entity.Property(e => e.Button2Link)
                .HasColumnName("button2_link");

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");

            entity.Property(e => e.DisplayOrder)
                .HasDefaultValue(0)
                .HasColumnName("display_order");

            entity.Property(e => e.CreatedBy)
                .HasColumnName("created_by");

            entity.Property(e => e.UpdatedBy)
                .HasColumnName("updated_by");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            entity.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");

            entity.HasOne(d => d.CreatedByNavigation)
                .WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("banners_created_by_fkey");

            entity.HasOne(d => d.UpdatedByNavigation)
                .WithMany()
                .HasForeignKey(d => d.UpdatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("banners_updated_by_fkey");

            entity.HasIndex(e => new { e.DisplayOrder, e.CreatedAt })
                .HasFilter("\"is_active\" = TRUE AND \"deleted_at\" IS NULL");

            entity.HasIndex(e => e.DeletedAt);
            entity.HasIndex(e => e.CreatedBy);
            entity.HasIndex(e => e.UpdatedBy);
        });
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("carts_pkey");

            entity.ToTable("carts");

            entity.HasIndex(e => e.UserId, "carts_user_id_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithOne(p => p.Cart)
                .HasForeignKey<Cart>(d => d.UserId)
                .HasConstraintName("carts_user_id_fkey");
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => new { e.CartId, e.ContentId }).HasName("cart_items_pkey");

            entity.ToTable("cart_items");

            entity.Property(e => e.CartId).HasColumnName("cart_id");
            entity.Property(e => e.ContentId).HasColumnName("content_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Price)
                .HasPrecision(18, 2)
                .HasColumnName("price");

            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.CartId)
                .HasConstraintName("cart_items_cart_id_fkey");

            entity.HasOne(d => d.Content).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.ContentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cart_items_content_id_fkey");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("categories_pkey");

            entity.ToTable("categories");

            entity.HasIndex(e => new { e.Name, e.ParentId }, "idx_categories_unique").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("categories_parent_id_fkey");
        });
        modelBuilder.HasPostgresEnum<CollaboratorActivityStatus>("collaborator_activity_status");

        modelBuilder.Entity<Collaborator>(entity =>
        {
            entity.ToTable("collaborators");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasColumnName("id")
                  .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.UserId)
                  .HasColumnName("user_id");

            entity.Property(e => e.Status)
                  .HasColumnName("status")
                  .HasColumnType("collaborator_activity_status")
                  .HasDefaultValueSql("'approved'::collaborator_activity_status");

            entity.Property(e => e.CommissionRate)
                  .HasColumnName("commission_rate")
                  .HasPrecision(5, 2)
                  .HasDefaultValue(0m);

            entity.Property(e => e.CreatedAt)
                  .HasColumnName("created_at")
                  .HasDefaultValueSql("now()");

            entity.Property(e => e.UpdatedAt)
                  .HasColumnName("updated_at");

            entity.Property(e => e.DeletedAt)
                  .HasColumnName("deleted_at");

            entity.HasOne(d => d.User)
                  .WithOne(p => p.Collaborator)
                  .HasForeignKey<Collaborator>(d => d.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.HasPostgresEnum<CollaboratorRequestStatus>("collaborator_request_status");

        modelBuilder.Entity<CollaboratorRequest>(entity =>
        {
            entity.ToTable("collaborator_requests");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasColumnName("id")
                  .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.UserId)
                  .HasColumnName("user_id");

            entity.Property(e => e.ApprovedBy)
                  .HasColumnName("approved_by");

            entity.Property(e => e.Status)
                  .HasColumnName("status")
                  .HasColumnType("collaborator_request_status")
                  .HasDefaultValueSql("'pending'::collaborator_request_status");

            entity.Property(e => e.CreatedAt)
                  .HasColumnName("created_at")
                  .HasDefaultValueSql("now()");

            entity.Property(e => e.ApprovedAt)
                  .HasColumnName("approved_at");

            entity.HasOne(d => d.User)
                  .WithMany(p => p.CollaboratorRequestUsers)
                  .HasForeignKey(d => d.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.ApprovedByNavigation)
                  .WithMany(p => p.CollaboratorRequestApprovedByNavigations)
                  .HasForeignKey(d => d.ApprovedBy)
                  .OnDelete(DeleteBehavior.Restrict);
        });


        modelBuilder.Entity<CollaboratorBank>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("collaborator_banks_pkey");

            entity.ToTable("collaborator_banks");

            entity.HasIndex(e => new { e.CollaboratorId, e.BankAccount }, "collaborator_banks_collaborator_id_bank_account_key").IsUnique();

            entity.HasIndex(e => e.CollaboratorId, "idx_collaborator_banks_collab");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.BankAccount).HasColumnName("bank_account");
            entity.Property(e => e.BankName).HasColumnName("bank_name");
            entity.Property(e => e.CollaboratorId).HasColumnName("collaborator_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.IsDefault)
                .HasDefaultValue(false)
                .HasColumnName("is_default");
            entity.Property(e => e.OwnerName).HasColumnName("owner_name");

            entity.HasOne(d => d.Collaborator).WithMany(p => p.CollaboratorBanks)
                .HasForeignKey(d => d.CollaboratorId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("collaborator_banks_collaborator_id_fkey");
        });


        modelBuilder.HasPostgresEnum<ContentStatus>("content_status");
        modelBuilder.Entity<Content>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("contents_pkey");

            entity.ToTable("contents");

            entity.HasIndex(e => e.CategoryId, "idx_contents_category");

            entity.HasIndex(e => e.CollaboratorId, "idx_contents_collaborator");

            entity.Property(e => e.Status)
            .HasColumnName("status")
            .HasColumnType("content_status")
            .HasDefaultValueSql("'draft'::content_status");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CollaboratorId).HasColumnName("collaborator_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Price)
                .HasPrecision(18, 2)
                .HasColumnName("price");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Category).WithMany(p => p.Contents)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("contents_category_id_fkey");

            entity.HasOne(d => d.Collaborator).WithMany(p => p.Contents)
                .HasForeignKey(d => d.CollaboratorId)
                .HasConstraintName("contents_collaborator_id_fkey");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Contents)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("contents_created_by_fkey");
        });

        modelBuilder.Entity<ContentStat>(entity =>
        {
            entity.HasKey(e => e.ContentId).HasName("content_stats_pkey");

            entity.ToTable("content_stats");

            entity.Property(e => e.ContentId)
                .ValueGeneratedNever()
                .HasColumnName("content_id");
            entity.Property(e => e.Downloads)
                .HasDefaultValue(0)
                .HasColumnName("downloads");
            entity.Property(e => e.Purchases)
                .HasDefaultValue(0)
                .HasColumnName("purchases");
            entity.Property(e => e.Views)
                .HasDefaultValue(0)
                .HasColumnName("views");

            entity.HasOne(d => d.Content).WithOne(p => p.ContentStat)
                .HasForeignKey<ContentStat>(d => d.ContentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("content_stats_content_id_fkey");
        });

        modelBuilder.Entity<CopyrightReport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("copyright_reports_pkey");

            entity.ToTable("copyright_reports");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ContentId).HasColumnName("content_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.ReporterId).HasColumnName("reporter_id");

            entity.HasOne(d => d.Content).WithMany(p => p.CopyrightReports)
                .HasForeignKey(d => d.ContentId)
                .HasConstraintName("copyright_reports_content_id_fkey");

            entity.HasOne(d => d.Reporter).WithMany(p => p.CopyrightReports)
                .HasForeignKey(d => d.ReporterId)
                .HasConstraintName("copyright_reports_reporter_id_fkey");
        });

        modelBuilder.HasPostgresEnum<CouponType>("coupon_type");

        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.ToTable("coupons");

            entity.HasKey(e => e.Id).HasName("coupons_pkey");

            entity.HasIndex(e => e.Code, "coupons_code_key").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.Code)
                .HasColumnName("code");
            entity.Property(e => e.Type)
                .HasColumnName("type")
                .HasColumnType("coupon_type")
                .HasDefaultValueSql("'percent'");

            entity.Property(e => e.Value)
                .HasPrecision(10, 2)
                .HasColumnName("value");

            entity.Property(e => e.MaxDiscount)
                .HasPrecision(10, 2)
                .HasColumnName("max_discount");

            entity.Property(e => e.MinOrderAmount)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("min_order_amount");

            entity.Property(e => e.UsageLimit)
                .HasColumnName("usage_limit");

            entity.Property(e => e.UsedCount)
                .HasDefaultValue(0)
                .HasColumnName("used_count");

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");

            entity.Property(e => e.ValidFrom)
                .HasColumnName("valid_from");

            entity.Property(e => e.ValidTo)
                .HasColumnName("valid_to");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<Download>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("downloads_pkey");

            entity.ToTable("downloads");

            entity.HasIndex(e => new { e.UserId, e.ContentId }, "downloads_user_id_content_id_key").IsUnique();

            entity.HasIndex(e => e.ContentId, "idx_downloads_content");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ContentId).HasColumnName("content_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Content).WithMany(p => p.Downloads)
                .HasForeignKey(d => d.ContentId)
                .HasConstraintName("downloads_content_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Downloads)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("downloads_user_id_fkey");
        });

        modelBuilder.HasPostgresEnum<FilePurpose>("file_purpose");
        modelBuilder.Entity<MediaFile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("files_pkey");

            entity.ToTable("files");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ContentId).HasColumnName("content_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.FileName).HasColumnName("file_name");
            entity.Property(e => e.FileType).HasColumnName("file_type");
            entity.Property(e => e.FileUrl).HasColumnName("file_url");
            entity.Property(e => e.Size).HasColumnName("size");
            entity.Property(e => e.Purpose)
                .IsRequired()
                .HasColumnName("purpose")
                .HasColumnType("file_purpose")
                .HasDefaultValueSql("'downloadable'::file_purpose");
            entity.Property(e => e.DisplayOrder)
                .HasColumnName("display_order")
                .HasDefaultValue(0);

            entity.HasOne(d => d.Content).WithMany(p => p.Files)
                .HasForeignKey(d => d.ContentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("files_content_id_fkey");
        });
        modelBuilder.HasPostgresEnum<OrderStatus>("order_status");
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("orders_pkey");

            entity.ToTable("orders");

            entity.HasIndex(e => e.UserId, "idx_orders_user");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Currency)
                .HasDefaultValueSql("'VND'::text")
                .HasColumnName("currency");
            entity.Property(e => e.Status)
               .HasColumnName("status")
               .HasColumnType("order_status")
               .HasDefaultValueSql("'pending'");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(18, 2)
                .HasColumnName("total_amount");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("orders_user_id_fkey");
        });

        modelBuilder.Entity<OrderCoupon>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("order_coupons_pkey");

            entity.ToTable("order_coupons");

            entity.Property(e => e.OrderId)
                .ValueGeneratedNever()
                .HasColumnName("order_id");
            entity.Property(e => e.AppliedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("applied_at");
            entity.Property(e => e.CouponId).HasColumnName("coupon_id");
            entity.Property(e => e.DiscountAmount)
                .HasPrecision(18, 2)
                .HasColumnName("discount_amount");

            entity.HasOne(d => d.Coupon).WithMany(p => p.OrderCoupons)
                .HasForeignKey(d => d.CouponId)
                .HasConstraintName("order_coupons_coupon_id_fkey");

            entity.HasOne(d => d.Order).WithOne(p => p.OrderCoupon)
                .HasForeignKey<OrderCoupon>(d => d.OrderId)
                .HasConstraintName("order_coupons_order_id_fkey");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => new { e.OrderId, e.ContentId }).HasName("order_items_pkey");

            entity.ToTable("order_items");

            entity.HasIndex(e => e.CollaboratorId, "idx_order_items_collaborator");

            entity.HasIndex(e => e.OrderId, "idx_order_items_order");

            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ContentId).HasColumnName("content_id");
            entity.Property(e => e.CollaboratorId).HasColumnName("collaborator_id");
            entity.Property(e => e.Price)
                .HasPrecision(18, 2)
                .HasColumnName("price");

            entity.HasOne(d => d.Collaborator).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.CollaboratorId)
                .HasConstraintName("order_items_collaborator_id_fkey");

            entity.HasOne(d => d.Content).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ContentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("order_items_content_id_fkey");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("order_items_order_id_fkey");
        });

        modelBuilder.HasPostgresEnum<OtpType>("otp_type");

        modelBuilder.Entity<Otp>(entity =>
        {
            entity.ToTable("otps");

            entity.HasKey(e => e.Id).HasName("otps_pkey");

            entity.Property(e => e.Type)
                .HasColumnName("type")
                .HasColumnType("otp_type")
                .IsRequired();

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.AttemptCount)
                .HasColumnName("attempt_count")
                .HasDefaultValue(0);

            entity.Property(e => e.CodeHash)
                .HasColumnName("code_hash");

            entity.Property(e => e.IsUsed)
                .HasColumnName("is_used")
                .HasDefaultValue(false);

            entity.Property(e => e.Email)
                .HasColumnName("email");

            entity.Property(e => e.ExpiredAt)
                .HasColumnName("expired_at");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()");

            entity.Property(e => e.UserId)
                .HasColumnName("user_id");

            entity.HasIndex(e => e.ExpiredAt, "idx_otps_expired");

            entity.HasOne(d => d.User)
                .WithMany(p => p.Otps)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("otps_user_id_fkey");
        });

        modelBuilder.HasPostgresEnum<PaymentStatus>("payment_status");
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("payments_pkey");

            entity.ToTable("payments");

            entity.HasIndex(e => e.OrderId, "uq_payments_order").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("amount");
            entity.Property(e => e.Status)
               .HasColumnName("status")
               .HasColumnType("payment_status")
               .HasDefaultValueSql("'pending'");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PaymentMethod).HasColumnName("payment_method");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Order).WithOne(p => p.Payment)
                .HasForeignKey<Payment>(d => d.OrderId)
                .HasConstraintName("payments_order_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Payments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("payments_user_id_fkey");
        });

        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("payment_transactions_pkey");

            entity.ToTable("payment_transactions");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.Provider).HasColumnName("provider");
            entity.Property(e => e.RawResponse)
                .HasColumnType("jsonb")
                .HasColumnName("raw_response");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.PaymentUrl)
                .HasColumnName("payment_url")
                .HasMaxLength(1000)
                .IsUnicode(true)
                .IsRequired(false);
            entity.HasOne(d => d.Payment).WithMany(p => p.PaymentTransactions)
                .HasForeignKey(d => d.PaymentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("payment_transactions_payment_id_fkey");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id)
                  .HasName("refresh_tokens_pkey");

            entity.ToTable("refresh_tokens");

            entity.HasIndex(e => e.UserId, "idx_refresh_tokens_user");

            entity.HasIndex(e => e.TokenHash)
                  .IsUnique()
                  .HasDatabaseName("refresh_tokens_token_hash_key");

            entity.Property(e => e.Id)
                  .HasColumnName("id")
                  .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.UserId)
                  .HasColumnName("user_id");

            entity.Property(e => e.TokenHash)
                  .HasColumnName("token_hash")
                  .IsRequired();

            entity.Property(e => e.ExpiredAt)
                  .HasColumnName("expired_at")
                  .HasColumnType("timestamptz");

            entity.Property(e => e.CreatedAt)
                  .HasColumnName("created_at")
                  .HasColumnType("timestamptz")
                  .HasDefaultValueSql("now()");
            entity.Property(e => e.RevokedAt)
                  .HasColumnName("revoked_at")
                  .HasColumnType("timestamptz");

            entity.Property(e => e.ReplacedByToken)
                  .HasColumnName("replaced_by_token");

            entity.Property(e => e.IpAddress)
                  .HasColumnName("ip_address");

            entity.Property(e => e.Device)
                  .HasColumnName("device");

            entity.HasOne(d => d.User)
                  .WithMany(p => p.RefreshTokens)
                  .HasForeignKey(d => d.UserId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("refresh_tokens_user_id_fkey");
        });


        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("reviews_pkey");

            entity.ToTable("reviews");

            entity.HasIndex(e => e.ContentId, "idx_reviews_content");

            entity.HasIndex(e => new { e.UserId, e.ContentId }, "reviews_user_id_content_id_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.ContentId).HasColumnName("content_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Content).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.ContentId)
                .HasConstraintName("reviews_content_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("reviews_user_id_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.HasIndex(e => e.Name, "roles_name_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Name).HasColumnName("name");
        });
        modelBuilder.HasPostgresEnum<UserStatus>("user_status");

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.DeletedAt);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.Username)
                .HasColumnName("username");

            entity.Property(e => e.Email)
                .HasColumnName("email");

            entity.Property(e => e.PasswordHash)
                .HasColumnName("password_hash");

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasColumnType("user_status")
                .HasDefaultValueSql("'active'");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            entity.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");
        });


        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId }).HasName("user_roles_pkey");

            entity.ToTable("user_roles");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("user_roles_role_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("user_roles_user_id_fkey");
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("wallets_pkey");

            entity.ToTable("wallets");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Balance)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("balance");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<WalletTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("wallet_transactions_pkey");

            entity.ToTable("wallet_transactions");

            entity.HasIndex(e => e.CreatedAt, "idx_wallet_tx_created_at");

            entity.HasIndex(e => e.WalletId, "idx_wallet_tx_wallet");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
            entity.Property(e => e.WalletId).HasColumnName("wallet_id");

            entity.HasOne(d => d.Wallet).WithMany(p => p.WalletTransactions)
                .HasForeignKey(d => d.WalletId)
                .HasConstraintName("wallet_transactions_wallet_id_fkey");
        });

        modelBuilder.Entity<Withdrawal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("withdrawals_pkey");

            entity.ToTable("withdrawals");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("amount");
            entity.Property(e => e.BankId).HasColumnName("bank_id");
            entity.Property(e => e.CollaboratorId).HasColumnName("collaborator_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.ProcessedAt).HasColumnName("processed_at");
            entity.Property(e => e.ProcessedBy).HasColumnName("processed_by");

            entity.HasOne(d => d.Bank).WithMany(p => p.Withdrawals)
                .HasForeignKey(d => d.BankId)
                .HasConstraintName("withdrawals_bank_id_fkey");

            entity.HasOne(d => d.Collaborator).WithMany(p => p.Withdrawals)
                .HasForeignKey(d => d.CollaboratorId)
                .HasConstraintName("withdrawals_collaborator_id_fkey");

            entity.HasOne(d => d.ProcessedByNavigation).WithMany(p => p.Withdrawals)
                .HasForeignKey(d => d.ProcessedBy)
                .HasConstraintName("withdrawals_processed_by_fkey");
        });
        modelBuilder.HasPostgresExtension("pgcrypto");
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
