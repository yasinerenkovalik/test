using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Test.Application.Common.Interfaces;
using Test.Domain.Common;

namespace Test.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Audit + soft delete tek yerde:
/// Added -> CreatedDate/CreatedBy, Modified -> UpdatedDate/UpdatedBy,
/// Deleted -> fiziksel silme yerine IsDeleted=true + DeletedDate. Hepsi UTC.
/// </summary>
public sealed class AuditableEntityInterceptor(ICurrentUserService currentUser) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Apply(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Apply(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Apply(DbContext? context)
    {
        if (context is null) return;

        var now = DateTime.UtcNow;
        var userId = currentUser.UserId;

        // ToList: döngü içinde entry state'i değişiyor.
        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>().ToList())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedDate = now;
                    entry.Entity.CreatedBy = userId;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedDate = now;
                    entry.Entity.UpdatedBy = userId;
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedDate = now;
                    entry.Entity.UpdatedDate = now;
                    entry.Entity.UpdatedBy = userId;
                    break;
            }
        }
    }
}
