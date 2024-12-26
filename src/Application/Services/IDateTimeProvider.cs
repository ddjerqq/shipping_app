namespace Application.Services;

[Obsolete("Use DateTimeOffset.UtcNow instead")]
public interface IDateTimeProvider
{
    public DateTime UtcNow { get; }
}