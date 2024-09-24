using System.Collections.Generic;
using NetDaemon.HassModel.Entities;

namespace NetDaemon.Apps.Cameras;

/// <summary>
/// Automations for cat cameras.
/// </summary>
[NetDaemonApp]
public class CatCameras
{
    private readonly IEntities entities;
    private readonly IServices services;
    private readonly ILogger<CatCameras> logger;
    private readonly List<SwitchEntity> cameras;
    
    /// <summary>
    /// Sets up automations.
    /// </summary>
    public CatCameras(IHaContext context, ILogger<CatCameras> logger)
    {
        entities = new Entities(context);
        services = new Services(context);
        this.logger = logger;
        cameras =
        [
            entities.Switch.CatCameraUpSmartPlug,
            entities.Switch.CatCameraDownSmartPlug
        ];

        entities.Person.Allison
            .StateChanges()
            .Where(x => x.New?.State == "home")
            .Subscribe(_ => TurnOffCameras());
        entities.Person.Owen
            .StateChanges()
            .Where(x => x.New?.State == "home")
            .Subscribe(_ => TurnOffCameras());
        entities.InputBoolean.CatCamerasOn
            .StateChanges()
            .Where(x => x.New.IsOn())
            .Subscribe(_ => TurnOnCameras());
    }

    /// <summary>
    /// Turns on the cameras if nobody is home.
    /// </summary>
    private void TurnOnCameras()
    {
        if (IsAnyoneHome())
        {
            return;
        }
        
        SetCamerasState(true);
    }

    /// <summary>
    /// Turns off the cameras if anyone is home.
    /// </summary>
    private void TurnOffCameras()
    {
        if (!IsAnyoneHome())
        {
            return;
        }
        
        SetCamerasState(false);

        if (entities.InputBoolean.CatCamerasOn.IsOn())
        {
            entities.InputBoolean.CatCamerasOn.TurnOff();
        }
    }

    /// <summary>
    /// Turns on/off the cameras based on the input. Notifies Owen if any were turned on or off.
    /// </summary>
    private void SetCamerasState(bool isOn)
    {
        var cameraUpdated = false;
        foreach (var camera in cameras)
        {
            switch (isOn)
            {
                case true when camera.IsOff():
                    camera.TurnOn();
                    cameraUpdated = true;
                    break;
                case false when camera.IsOn():
                    camera.TurnOff();
                    cameraUpdated = true;
                    break;
            }
        }

        if (!cameraUpdated)
        {
            return;
        }

        var stateString = isOn ? "on" : "off";
        logger.LogInformation("Cat cameras turned {State}", stateString);
        services.Notify.Owen($"Cat cameras have been turned {stateString}", "Cameras");
    }

    /// <summary>
    /// Returns if anyone is actively home.
    /// </summary>
    private bool IsAnyoneHome()
        => entities.Person.Owen.State == "home" || entities.Person.Allison.State == "home";
}