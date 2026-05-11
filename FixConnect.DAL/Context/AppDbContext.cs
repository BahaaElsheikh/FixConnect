using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FixConnect.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace FixConnect.DAL.Context
{
    public class AppDbContext : DbContext
    {
        // ✅ Dependency Injection: Constructor receives DbContextOptions from DI container
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Worker> Workers { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<WorksAt> WorksAt { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<Proposal> Proposals { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<PortfolioItem> PortfolioItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<WorkerVerification> WorkerVerifications { get; set; }

        public DbSet<Specialty> Specialties { get; set; }

        public DbSet<RequestImage> RequestImages { get; set; }


        public DbSet<JobInvoiceItem> JobInvoiceItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================
            // TPT (Table Per Type)
            // ============================
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Customer>().ToTable("Customers");
            modelBuilder.Entity<Worker>().ToTable("Workers");
            modelBuilder.Entity<Admin>().ToTable("Admins");

            // ============================
            // User
            // ============================
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserId);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(u => u.Phone).HasMaxLength(11);
                entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            // ============================
            // Customer (TPT child)
            // ============================
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(c => c.UserId);
                entity.Property(c => c.TotalRequests).HasDefaultValue(0);

                entity.HasOne(c => c.User)
                      .WithOne(u => u.Customer)
                      .HasForeignKey<Customer>(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ============================
            // Worker (TPT child)
            // ============================
            modelBuilder.Entity<Worker>(entity =>
            {
                entity.HasKey(w => w.UserId);
                entity.Property(w => w.IsVerified).HasDefaultValue(false);
                entity.Property(w => w.AvgRating).HasColumnType("decimal(3,2)").HasDefaultValue(0);

                entity.HasOne(w => w.User)
                      .WithOne(u => u.Worker)
                      .HasForeignKey<Worker>(w => w.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ============================
            // Admin (TPT child)
            // ============================
            modelBuilder.Entity<Admin>(entity =>
            {
                entity.HasKey(a => a.UserId);

                entity.HasOne(a => a.User)
                      .WithOne(u => u.Admin)
                      .HasForeignKey<Admin>(a => a.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ============================
            // WorksAt (Composite PK)
            // ============================
            modelBuilder.Entity<WorksAt>(entity =>
            {
                entity.HasKey(wa => new { wa.UserId, wa.RegionId });

                entity.HasOne(wa => wa.Worker)
                      .WithMany(w => w.WorksAt)
                      .HasForeignKey(wa => wa.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(wa => wa.Region)
                      .WithMany(r => r.WorksAt)
                      .HasForeignKey(wa => wa.RegionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ============================
            // Request
            // ============================
            modelBuilder.Entity<Request>(entity =>
            {
                entity.HasKey(r => r.RequestId);
                entity.Property(r => r.CreatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(r => r.Customer)
                      .WithMany(c => c.Requests)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Region)
                      .WithMany(reg => reg.Requests)
                      .HasForeignKey(r => r.RegionId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(r => r.TargetWorker)
                      .WithMany(w => w.TargetedRequests)
                      .HasForeignKey(r => r.TargetWorkerId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // ============================
            // Proposal
            // ============================
            modelBuilder.Entity<Proposal>(entity =>
            {
                entity.HasKey(p => p.ProposalId);

                entity.HasOne(p => p.Customer)
                      .WithMany(c => c.Proposals)
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(p => p.Request)
                      .WithMany(r => r.Proposals)
                      .HasForeignKey(p => p.RequestId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Worker)
                      .WithMany(w => w.Proposals)
                      .HasForeignKey(p => p.WorkerId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // ============================
            // Job
            // ============================
            modelBuilder.Entity<Job>(entity =>
            {
                entity.HasKey(j => j.JobId);
                entity.Property(j => j.LiveInvoiceTotal).HasColumnType("decimal(10,2)");

                entity.HasOne(j => j.Proposal)
                      .WithOne(p => p.Job)
                      .HasForeignKey<Job>(j => j.ProposalId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ============================
            // PortfolioItem
            // ============================
            modelBuilder.Entity<PortfolioItem>(entity =>
            {
                entity.HasKey(p => p.ItemId);

                entity.HasOne(p => p.Worker)
                      .WithMany(w => w.PortfolioItems)
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ============================
            // Review
            // ============================
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(r => r.ReviewId);
                entity.Property(r => r.RatingValue)
                      .IsRequired()
                      .HasAnnotation("Range", new[] { 1, 5 });

                entity.HasIndex(r => r.JobId).IsUnique();

                entity.HasOne(r => r.Customer)
                      .WithMany(c => c.Reviews)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(r => r.Job)
                      .WithOne(j => j.Review)
                      .HasForeignKey<Review>(r => r.JobId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(r => r.Worker)
                      .WithMany(w => w.Reviews)
                      .HasForeignKey(r => r.WorkerId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // ============================
            // Wallet
            // ============================
            modelBuilder.Entity<Wallet>(entity =>
            {
                entity.HasKey(w => w.WalletId);
                entity.Property(w => w.Balance).HasColumnType("decimal(10,2)").HasDefaultValue(0);
                entity.HasIndex(w => w.WorkerId).IsUnique();

                entity.HasOne(w => w.Worker)
                      .WithOne(wk => wk.Wallet)
                      .HasForeignKey<Wallet>(w => w.WorkerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ============================
            // Transaction
            // ============================
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(t => t.TransactionId);
                entity.Property(t => t.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(t => t.Amount).HasColumnType("decimal(10,2)");

                entity.HasOne(t => t.Wallet)
                      .WithMany(w => w.Transactions)
                      .HasForeignKey(t => t.WalletId)
                      .OnDelete(DeleteBehavior.NoAction);
            });


            modelBuilder.Entity<WorkerVerification>(entity =>
             {
                 entity.HasKey(v => v.VerificationId);
                 entity.Property(v => v.Status).HasDefaultValue("Pending");
                 entity.Property(v => v.SubmittedAt).HasDefaultValueSql("GETDATE()");

                 entity.HasOne(v => v.Worker)
                     .WithOne(w => w.Verification)
                     .HasForeignKey<WorkerVerification>(v => v.WorkerId)
                     .OnDelete(DeleteBehavior.Cascade);
             });


            // ============================
            // Specialty
            // ============================
            modelBuilder.Entity<Specialty>(entity =>
            {
                entity.HasKey(s => s.SpecialtyId);
                entity.Property(s => s.SpecialtyName).IsRequired().HasMaxLength(100);
                entity.HasIndex(s => s.SpecialtyName).IsUnique();
            });

            // Worker → Specialty
            modelBuilder.Entity<Worker>(entity =>
            {
                entity.HasOne(w => w.Specialty)
                      .WithMany(s => s.Workers)
                      .HasForeignKey(w => w.SpecialtyId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Request → Specialty
            modelBuilder.Entity<Request>(entity =>
            {
                entity.HasOne(r => r.Specialty)
                      .WithMany(s => s.Requests)
                      .HasForeignKey(r => r.SpecialtyId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull);
            });


        //Request Image 
            modelBuilder.Entity<RequestImage>(entity =>
            {
                entity.HasKey(r => r.ImageId);
                entity.Property(r => r.UploadedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(r => r.Request)
                      .WithMany(req => req.Images)
                      .HasForeignKey(r => r.RequestId)
                      .OnDelete(DeleteBehavior.Cascade);
            });



            // JobInvoiceItem
            modelBuilder.Entity<JobInvoiceItem>(entity =>
            {
                entity.HasKey(j => j.ItemId);
                entity.Property(j => j.Cost).HasColumnType("decimal(10,2)");
                entity.Property(j => j.AddedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(j => j.Job)
                      .WithMany(job => job.InvoiceItems)
                      .HasForeignKey(j => j.JobId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

        }
    }
}
