namespace Application.Services;

public interface IFileStore
{
    /// <summary>
    /// Create a file and return the key to the file.
    /// </summary>
    /// <returns>The sha256 hash of the file - the key for the file</returns>
    public Task<string> CreateFileAsync(Stream fileStream, string fileExt, CancellationToken ct = default);

    /// <summary>
    /// Gets all files in a bucket
    /// </summary>
    /// <returns>All files in the bucket</returns>
    public Task<IEnumerable<(string Key, string PresignedUrl)>> GetAllFilesAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets a presigned file url for the given key (if found)
    /// </summary>
    /// <returns>The presigned url, if the key exists.</returns>
    public Task<string?> GetPresignedFileUrlAsync(string key, CancellationToken ct = default);
}