using BauFlow.Entities;
using BauFlow.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BauFlow.Data
{
    //✔ Kunden anlegen
    //✔ Projekte erstellen
    //✔ Angebote bauen
    //✔ PDF generieren
    //✔ Angebote annehmen
    //✔ Rechnung erzeugen
    //✔ Status tracken
    //✔ Multi-Tenant SaaS betreiben
    public class ApplicationDbContext
     : IdentityDbContext<ApplicationUser>
    {
        private readonly ITenantProvider _tenantProvider;

        public ApplicationDbContext(
     DbContextOptions<ApplicationDbContext> options,
     ITenantProvider tenantProvider)
     : base(options)
        {
            _tenantProvider = tenantProvider; // ❗ FEHLTE
        }
        public Guid? CurrentCompanyId => _tenantProvider.GetCompanyId();
        public DbSet<ApplicationUser> AspNetUsers { get; set; }

        public DbSet<Company> Companies { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<QuoteItem> QuoteItems { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<RunningNumber> RunningNumbers { get; set; }
        public DbSet<Payment> Payments { get; set; }

        public override int SaveChanges()
        {
            ApplyMultiTenantRules();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyMultiTenantRules();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyMultiTenantRules()
        {
            var companyId = CurrentCompanyId;

            if (companyId == null)
                return;

            var entries = ChangeTracker
                .Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CompanyId = companyId.Value;
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Property(nameof(BaseEntity.CompanyId)).IsModified = false;
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Customer>()
       .HasQueryFilter(e => CurrentCompanyId != null && e.CompanyId == CurrentCompanyId);

            builder.Entity<Project>()
                .HasQueryFilter(e => CurrentCompanyId != null && e.CompanyId == CurrentCompanyId);

            builder.Entity<Quote>()
                .HasQueryFilter(e => CurrentCompanyId != null && e.CompanyId == CurrentCompanyId);

            builder.Entity<Invoice>()
                .HasQueryFilter(e => CurrentCompanyId != null && e.CompanyId == CurrentCompanyId);
        }

    }

}
