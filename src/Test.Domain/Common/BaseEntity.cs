namespace Test.Domain.Common;

/// <summary>
/// Ortak alanlar tek yerde. Audit alanlarını handler'lar değil,
/// Infrastructure'daki SaveChanges interceptor'ı doldurur.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public DateTime? DeletedDate { get; set; }
    public bool IsDeleted { get; set; }

    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}
