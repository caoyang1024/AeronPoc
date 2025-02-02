using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AeronPublisher;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<AeronWorker>();

        var host = builder.Build();
        host.Run();
    }
}