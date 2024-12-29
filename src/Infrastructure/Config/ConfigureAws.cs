using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using Application;
using Application.Services;
using Domain.Common;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Config;

public sealed class ConfigureAws : ConfigurationBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        var key = "AWS_ACCESS_KEY_ID".FromEnvRequired();
        var secret = "AWS_SECRET_ACCESS_KEY".FromEnvRequired();

        var options = new AWSOptions
        {
            Profile = "default",
            Region = RegionEndpoint.EUNorth1,
            Credentials = new BasicAWSCredentials(key, secret),
        };
        services.AddDefaultAWSOptions(options);
        services.AddAWSService<IAmazonS3>();

        services.AddScoped<IFileStore, AmazonS3FileStore>();
    }
}