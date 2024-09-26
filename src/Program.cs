using System.Reflection;
using Microsoft.Extensions.Hosting;
using NetDaemon.Extensions.Scheduler;
using NetDaemon.Extensions.Tts;
using NetDaemon.Runtime;

try
{
    await Host.CreateDefaultBuilder(args)
        //.UseNetDaemonAppSettings()
        //.RegisterAppSettingsJsonToHost()
        .UseNetDaemonRuntime()
        .UseNetDaemonTextToSpeech()
        .ConfigureServices((context, services) =>
            services
                .ConfigureNetDaemonServices(context.Configuration)
                .AddAppsFromAssembly(Assembly.GetExecutingAssembly())
                .AddNetDaemonStateManager()
                .AddNetDaemonScheduler()
                .AddHomeAssistantGenerated()
        )
        .Build()
        .RunAsync()
        .ConfigureAwait(false);
}
catch (Exception e)
{
    Console.WriteLine($"Failed to start host... {e}");
    throw;
}