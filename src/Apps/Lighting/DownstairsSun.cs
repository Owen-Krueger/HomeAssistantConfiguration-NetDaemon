using System.Reactive.Concurrency;
using NetDaemon.HassModel.Entities;

namespace NetDaemon.apps.Lighting;

/// <summary>
/// Automations to set downstairs lighting depending on sun elevation.
/// </summary>
[NetDaemonApp]
public class DownstairsSun
{
    private readonly IEntities entities;
    private readonly IServices services;
    private readonly ILogger<DownstairsSun> logger;

    /// <summary>
    /// Sets up automations.
    /// </summary>
    public DownstairsSun(IHaContext context, IScheduler scheduler, ILogger<DownstairsSun> logger)
    {
        entities = new Entities(context);
        services = new Services(context);
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
            .StateAllChanges()
            .Where(x =>
                x.New?.Attributes?.Elevation < 10 &&
                x.New?.Attributes?.Elevation >= 10)
            .Subscribe(_ => SetDownstairsLightLevel());
    }

    /// <summary>
    /// Sets the downstairs light brightness based on the sun elevation.
    /// </summary>
    private void SetDownstairsLightLevel()
    {
        var brightnessAttribute = entities.Light.DownstairsLights.Attributes?.Brightness;
        var elevation = entities.Sun.Sun.Attributes?.Elevation ?? 0;
        
        var brightness = 0;
        if (brightnessAttribute is not null && !int.TryParse(brightnessAttribute?.ToString(), out brightness))
        {
            // Unable to pull brightness or elevation.
            logger.LogError("Unable to parse brightness integer: {Brightness}.", brightnessAttribute);
            return;
        }

        var downstairsLightsTarget = ServiceTarget.FromEntity(entities.Light.DownstairsLights.EntityId);
        switch (elevation)
        {
            case >= 10 when brightness != 255:
                logger.LogInformation("Setting downstairs lights to 100% brightness.");
                services.Light.TurnOn(downstairsLightsTarget, new LightTurnOnParameters { Brightness = 255 });
                return;
            case < 10 when brightness != 128:
                logger.LogInformation("Setting downstairs to 50% brightness.");
                services.Light.TurnOn(downstairsLightsTarget, new LightTurnOnParameters { Brightness = 128 });
                return;
        }
    }
}