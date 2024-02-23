using Microsoft.EntityFrameworkCore;

namespace EncryptLocker.Database;
public class EncryptLockerContext : DbContext
{
    #region Properties

    public DbSet<CypherValue> CypherValues { get; set; }
    public DbSet<Locker> Lockers { get; set; }
    public DbSet<LockerAccessRight> LockerAccessRights { get; set; }
    public DbSet<PasswordReadLog> PasswordReadLogs { get; set; }
    public DbSet<RegisteredUser> RegisteredUsers { get; set; }
    public DbSet<SafeBase> SafeBases { get; set; }

    #endregion

    #region Constructors

    public EncryptLockerContext()
    {

    }

    public EncryptLockerContext(DbContextOptions options) : base(options)
    {

    }

    #endregion

    #region Methods

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured == false)
        {
            optionsBuilder.UseSqlServer("Server=localhost;Database=EncryptLocker;Trusted_Connection=True;Encrypt=False");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CypherValue>(e =>
        {
            e.HasKey(e => e.Id);
            e.Property(e => e.IV).HasMaxLength(256);
            e.Property(e => e.Tag).HasMaxLength(256);
            e.Property(e => e.Cypher).HasMaxLength(2048);
        });
        modelBuilder.Entity<Locker>(e =>
        {
            e.HasKey(e => e.Id);
            e.Property(e => e.KeyHash).HasMaxLength(256);
            e.Property(e => e.Name).IsUnicode().HasMaxLength(256);
        });
        modelBuilder.Entity<LockerAccessRight>(e =>
        {
            e.HasKey(e => e.Id);
            e.Property(e => e.AccessType).HasConversion(v => (int)v, v => (AccessType)v);
            e.HasOne(e => e.Locker).WithMany(d => d.LockerAccessRights).HasForeignKey(e => e.LockerId);
            e.HasOne(e => e.RegisteredUser).WithMany(d => d.LockerAccessRights).HasForeignKey(e => e.RegisteredUserId);
        });
        modelBuilder.Entity<PasswordReadLog>(e =>
        {
            e.HasKey(e => e.Id);
            e.HasOne(e => e.RegisteredUser).WithMany(d => d.PasswordReadLogs).HasForeignKey(e => e.RegisteredUserId);
            e.HasOne(e => e.SafeEntry).WithMany(d => d.PasswordReadLogs).HasForeignKey(e => e.SafeEntryId);
        });
        modelBuilder.Entity<RegisteredUser>(e =>
        {
            e.HasKey(e => e.Id);
        });
        modelBuilder.Entity<SafeEntry>(e =>
        {
            e.HasOne(e => e.Login).WithMany().HasForeignKey(e => e.LoginId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(e => e.Password).WithMany().HasForeignKey(e => e.PasswordId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(e => e.Url).WithMany().HasForeignKey(e => e.UrlId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(e => e.Note).WithMany().HasForeignKey(e => e.NoteId).OnDelete(DeleteBehavior.NoAction);
        });
        modelBuilder.Entity<SafeGroup>(e =>
        {

        });
        modelBuilder.Entity<SafeBase>(e =>
        {
            e.HasKey(e => e.Id);
            e.HasDiscriminator(e => e.Discriminator)
                .HasValue<SafeGroup>(SafeBase.SAFE_GROUP_DISCRIMINATOR)
                .HasValue<SafeEntry>(SafeBase.SAFE_ENTRY_DISCRIMINATOR);

            e.HasOne(e => e.Locker).WithMany(d => d.SafeEntries).HasForeignKey(e => e.LockerId);
            e.HasOne(e => e.Parent).WithMany(d => d.Children).HasForeignKey(e => e.ParentId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(e => e.Title).WithMany().HasForeignKey(e => e.TitleId).OnDelete(DeleteBehavior.NoAction);
        });
    }

    #endregion
}
