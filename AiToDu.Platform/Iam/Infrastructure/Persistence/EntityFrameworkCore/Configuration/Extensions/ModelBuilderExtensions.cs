using AiToDu.Platform.Iam.Domain.Model.Aggregates;
using AiToDu.Platform.Iam.Domain.Model.ValueObjects;
using AiToDu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.ValueConverters;
using Microsoft.EntityFrameworkCore;
namespace AiToDu.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyIamConfiguration(this ModelBuilder builder)
    {
        var user = builder.Entity<User>();
        user.ToTable("iam_users");
        user.HasKey(x => x.Id);
        user.Property(x => x.Id).HasConversion(new StronglyTypedIdConverter<UserId, int>(x => new UserId(x))).ValueGeneratedOnAdd();
        user.Property(x => x.Email).HasConversion(x => x.Value, x => new EmailAddress(x)).HasMaxLength(254).IsRequired();
        user.Property(x => x.FullName).HasMaxLength(160).IsRequired();
        user.Property(x => x.OrganizationName).HasMaxLength(160).IsRequired();
        user.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
        user.Property(x => x.Role).HasMaxLength(40).IsRequired();
        user.HasIndex(x => x.Email).IsUnique().HasDatabaseName("ux_users_email");
        user.HasIndex(x => x.PublicId).IsUnique();
        user.HasIndex(x => x.OrganizationId);
        user.Ignore(x => x.DomainEvents);
    }
}
