using System.Reflection;
using Application.Common;
using Application.Services;
using Domain.Aggregates;
using Generated;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Persistence.Interceptors;

namespace Persistence;

public sealed class AppDbContext(
    DbContextOptions<AppDbContext> options,
    ConvertDomainEventsToOutboxMessagesInterceptor convertDomainEventsToOutboxMessagesInterceptor)
    : IdentityDbContext<User, IdentityRole<UserId>, UserId>(options), IAppDbContext
{
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.Load(nameof(Persistence)));
        base.OnModelCreating(builder);
        SnakeCaseRename(builder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(convertDomainEventsToOutboxMessagesInterceptor);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        builder.ConfigureGeneratedConverters();

        var types =
            from Type type in Assembly.GetAssembly(typeof(AppDbContext))!.GetTypes()
            let baseType = type.BaseType
            where baseType?.IsGenericType ?? false
            where baseType?.GetGenericTypeDefinition() == typeof(ValueConverter<,>)
            let propertyType = baseType.GetGenericArguments().First()
            select (propertyType, type);

        foreach (var (propertyType, converterType) in types)
            builder
                .Properties(propertyType)
                .HaveConversion(converterType);

        base.ConfigureConventions(builder);
    }

    private static void SnakeCaseRename(ModelBuilder builder)
    {
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            var entityTableName = entity.GetTableName()!;
            entityTableName = entityTableName.Contains("AspNet") ? entityTableName.Replace("AspNet", string.Empty).TrimEnd('s') : entityTableName;
            entityTableName = entityTableName.ToSnakeCase();

            entity.SetTableName(entityTableName);

            foreach (var property in entity.GetProperties())
                property.SetColumnName(property.GetColumnName().ToSnakeCase());

            foreach (var key in entity.GetKeys())
                key.SetName(key.GetName()!.ToSnakeCase());

            foreach (var key in entity.GetForeignKeys())
                key.SetConstraintName(key.GetConstraintName()!.ToSnakeCase());

            foreach (var index in entity.GetIndexes())
                index.SetDatabaseName(index.GetDatabaseName()!.ToSnakeCase());
        }
    }
}