using HouseKeeper.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseKeeper.Contexts;
public class ApplicationContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Dataset> Datasets { get; set; }
    public DbSet<Dimension> Dimensions { get; set; }
    public DbSet<ObservationAttribute> ObservationAttributes { get; set; }
    public DbSet<Element> Elements { get; set; }
    public DbSet<Timeseries> Timeseries { get; set; }
    public DbSet<Observation> Observations { get; set; }
    public DbSet<ObservationAttributeValue> ObservationAttributeValues { get; set; }

    public ApplicationContext()
    {
        //Database.EnsureDeleted();
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=example");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        CreateUser(modelBuilder);
        CreateDataset(modelBuilder);
        CreateDimension(modelBuilder);
        CreateObservationAttribute(modelBuilder);
        CreateElement(modelBuilder);
        CreateTimseries(modelBuilder);
        CreateObservation(modelBuilder);
        CreateObservationAttributeValue(modelBuilder);
    }

    private static void CreateBaseEntity<T>(ModelBuilder modelBuilder, Action<EntityTypeBuilder<T>> action = null)
        where T : BaseEntity
    {
        modelBuilder.Entity<T>(entity =>
        {
            entity.HasKey(x => x.Id);
            action?.Invoke(entity);
        });
    }

    private static void CreateNamedEntity<T>(ModelBuilder modelBuilder, Action<EntityTypeBuilder<T>> action = null)
        where T : NamedEntity
    { 
        CreateBaseEntity<T>(modelBuilder, entity => 
        {
            entity.Property(x => x.Name);
            action?.Invoke(entity);
        });
    }

    private static void CreateUser(ModelBuilder modelBuilder)
    {
        CreateBaseEntity<User>(modelBuilder, entity => 
        {
            entity.HasMany(x => x.Datasets)
                .WithOne(x => x.Owner)
                .HasForeignKey(x => x.OwnerId);
        });
    }

    private static void CreateDataset(ModelBuilder modelBuilder)
    {
        CreateNamedEntity<Dataset>(modelBuilder, entity => 
        {
            entity.Property(x => x.OwnerId);
            entity.HasOne(x => x.Owner)
                .WithMany(x => x.Datasets)
                .HasForeignKey(x => x.OwnerId)
                .IsRequired();
            entity.HasMany(x => x.Dimensions)
                .WithOne(x => x.Dataset)
                .HasForeignKey(x => x.DatasetId)
                .IsRequired();
            entity.HasMany(x => x.ObservationAttributes)
                .WithOne(x => x.Dataset)
                .HasForeignKey(x => x.DatasetId)
                .IsRequired();
            entity.HasMany(x => x.Timeseries)
                .WithOne(x => x.Dataset)
                .HasForeignKey(x => x.DatasetId)
                .IsRequired();
        });
    }

    private static void CreateDimension(ModelBuilder modelBuilder)
    {
        CreateNamedEntity<Dimension>(modelBuilder, entity =>
        {
            entity.Property(x => x.DatasetId);
            entity.HasOne(x => x.Dataset)
                .WithMany(x => x.Dimensions)
                .HasForeignKey(x => x.DatasetId)
                .IsRequired();
            entity.HasMany(x => x.Elements)
                .WithOne(x => x.Dimension)
                .HasForeignKey(x => x.DimensionId)
                .IsRequired();
        });
    }

    private static void CreateObservationAttribute(ModelBuilder modelBuilder)
    {
        CreateNamedEntity<ObservationAttribute>(modelBuilder, entity =>
        {
            entity.Property(x => x.DatasetId);
            entity.HasOne(x => x.Dataset)
                .WithMany(x => x.ObservationAttributes)
                .HasForeignKey(x => x.DatasetId)
                .IsRequired();
            entity.HasMany(x => x.ObservationAttributeValues)
                .WithOne(x => x.Attribute)
                .HasForeignKey(x => x.AttributeId)
                .IsRequired();
        });
    }

    private static void CreateElement(ModelBuilder modelBuilder)
    {
        CreateNamedEntity<Element>(modelBuilder, entity =>
        {
            entity.Property(x => x.DimensionId);
            entity.HasOne(x => x.Dimension)
                .WithMany(x => x.Elements)
                .HasForeignKey(x => x.DimensionId)
                .IsRequired();
            entity.HasMany(x => x.TimeSeries)
                .WithMany(x => x.Elements)
                .UsingEntity(x => x.ToTable("TimeseriesCoordinates"));
        });
    }

    private static void CreateTimseries(ModelBuilder modelBuilder)
    {
        CreateBaseEntity<Timeseries>(modelBuilder, entity => 
        {
            entity.Property(x => x.DatasetId);
            entity.HasOne(x => x.Dataset)
                .WithMany(x => x.Timeseries)
                .HasForeignKey(x => x.DatasetId)
                .IsRequired();
            entity.Property(x => x.Mnemonics);
            entity.HasIndex(x => x.Mnemonics);
            entity.HasMany(x => x.Elements)
                .WithMany(x => x.TimeSeries)
                .UsingEntity(x => x.ToTable("TimeseriesCoordinates"));
            entity.HasMany(x => x.Observations)
                .WithOne(x => x.Timeseries)
                .HasForeignKey(x => x.TimeseriesId)
                .IsRequired();
        });
    }

    private static void CreateObservation(ModelBuilder modelBuilder)
    {
        CreateBaseEntity<Observation>(modelBuilder, entity => 
        {
            entity.Property(x => x.Timestamp);
            entity.HasOne(x => x.Timeseries)
                .WithMany(x => x.Observations)
                .HasForeignKey(x => x.TimeseriesId);
            entity.HasMany(x => x.Values)
                .WithOne(x => x.Observation)
                .HasForeignKey(x => x.ObservationId)
                .IsRequired();
        });
    }

    private static void CreateObservationAttributeValue(ModelBuilder modelBuilder)
    {
        CreateBaseEntity<ObservationAttributeValue>(modelBuilder, entity => 
        {
            entity.Property(x => x.Value);
            entity.HasOne(x => x.Observation)
                .WithMany(x => x.Values)
                .HasForeignKey(x => x.ObservationId)
                .IsRequired();
            entity.HasOne(x => x.Attribute)
                .WithMany(x => x.ObservationAttributeValues)
                .HasForeignKey(x => x.AttributeId)
                .IsRequired();
        });
    }
}
