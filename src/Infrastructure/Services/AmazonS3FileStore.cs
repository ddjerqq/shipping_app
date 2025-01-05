using System.Security.Cryptography;
using Amazon.S3;
using Amazon.S3.Model;
using Application.Services;
using Domain.Common;

namespace Infrastructure.Services;

public sealed class AmazonS3FileStore(IAmazonS3 client) : IFileStore
{
    private static string BucketName => "AWS_BUCKET_NAME".FromEnvRequired();

    private async Task EnsureBucketExists(CancellationToken ct = default)
    {
        var exists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(client, BucketName);

        if (!exists)
            await client.PutBucketAsync(BucketName, ct);
    }

    public async Task<string> CreateFileAsync(Stream fileStream, string fileExt, CancellationToken ct = default)
    {
        await EnsureBucketExists(ct);

        var bytes = new byte[fileStream.Length];
        await fileStream.ReadExactlyAsync(bytes, ct);

        var hash = SHA256.HashData(bytes);
        var key = Convert.ToHexString(hash).ToLower();

        key += fileExt;

        using var ms = new MemoryStream(bytes);
        var request = new PutObjectRequest
        {
            BucketName = BucketName,
            Key = key,
            InputStream = ms,
        };
        await client.PutObjectAsync(request, ct);

        return key;
    }

    public async Task<IEnumerable<(string Key, string PresignedUrl)>> GetAllFilesAsync(CancellationToken ct)
    {
        await EnsureBucketExists(ct);

        var request = new ListObjectsV2Request
        {
            BucketName = BucketName,
        };
        var response = await client.ListObjectsV2Async(request, ct);

        var keys = response.S3Objects.Select(o => o.Key);

        return await Task.WhenAll(keys.Select(async key =>
        {
            var url = await GetPresignedFileUrlAsync(key, ct);
            return (key, url!);
        }));
    }

    public async Task<string?> GetPresignedFileUrlAsync(string key, CancellationToken ct = default)
    {
        // TODO Cache the urls here for some time
        await EnsureBucketExists(ct);

        var request = new GetObjectRequest
        {
            BucketName = BucketName,
            Key = key,
        };
        var result = await client.GetObjectAsync(request, ct);

        var urlRequest = new GetPreSignedUrlRequest
        {
            BucketName = BucketName,
            Key = result.Key,
            Expires = DateTime.UtcNow.AddMinutes(5),
        };

        return await client.GetPreSignedURLAsync(urlRequest);
    }
}