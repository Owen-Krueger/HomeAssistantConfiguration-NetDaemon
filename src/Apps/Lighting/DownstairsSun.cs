using System.Reactive.Concurrency;

namespace NetDaemon.apps.Lighting;

/// <summary>
/// Automations to set downstairs lighting depending on sun elevation.
/// </summary>
[NetDaemonApp]
public class DownstairsSun
{
    private readonly IEntities entities;
    private readonly ILogger<DownstairsSun> logger;

    /// <summary>
    /// Sets up automations.
    /// </summary>
    public DownstairsSun(IHaContext context, IScheduler scheduler, ILogger<DownstairsSun> logger)
    {
        entities = new Entities(context);
        this.logger = logger;

        entities.Light.DownstairsLights
            .StateChanges()
            .WhenStateIsFor(x => x?.State == "on", TimeSpan.FromSeconds(5), scheduler)
            .Subscribe(_ => SetDownstairsLightLevel());
        // Around sunset
        entities.Sun.Sun
            .StateChanges()
            .Where(x =>
                int.Parse(x.Old?.State ?? "0") >= 10 &&
                int.Parse(x.New?.State ?? "0") < 10)
            .Subscribe(_ => SetDownstairsLightLevel());
        // Around sunrise
        entities.Sun.Sun
            .StateChanges()
            .Where(x =>
                int.Parse(x.Old?.State ?? "0") < 10 &&
                int.Parse(x.New?.State ?? "0") >= 10)
            .Subscribe(_ => SetDownstairsLightLevel());
    }

    /// <summary>
    /// Sets the downstairs light brightness based on the sun elevation.
    /// </summary>
    private void SetDownstairsLightLevel()
    {
        var brightnessAttribute = entities.Light.DownstairsLights.Attributes?.Brightness;
        var elevationAttribute = entities.Sun.Sun.Attributes?.Elevation;
        if (!int.TryParse(brightnessAttribute?.ToString(), out var brightness) ||
            !int.TryParse(elevationAttribute?.ToString(), out var elevation))
        {
            // Unable to pull brightness or elevation.
            logger.LogError("Unable to parse attributes. Brightness attribute: {Brightness}. Elevation attribute: {Elevation}.",
                brightnessAttribute, elevationAttribute);
            return;
        }

        switch (elevation)
        {
            case >= 10 when brightness != 255:
                logger.LogInformation("Setting downstairs lights to 100% brightness.");
                entities.Light.DownstairsLights.CallService("light.turn_on", new { brightness = 255 });
                return;
            case < 10 when brightness != 128:
                logger.LogInformation("Setting downstairs to 50% brightness.");
                entities.Light.DownstairsLights.CallService("light.turn_on", new { brightness = 128 });
                return;
        }
    }
}