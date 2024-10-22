using System.Collections.Generic;
using System.Reactive.Concurrency;
using NetDaemon.Apps.State;
using NetDaemon.Extensions;
using NetDaemon.HassModel.Entities;
using NetDaemon.Models;
using NetDaemon.Utilities;

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

        entities.InputSelect.HomeState
            .StateChanges()
            .Where(x => x.Entity.GetEnumFromState<HomeStateEnum>() == HomeStateEnum.Away)
            .Subscribe(_ => TurnOnCameras());
        entities.InputSelect.HomeState
            .StateChanges()
            .Where(x => x.Entity.GetEnumFromState<HomeStateEnum>() == HomeStateEnum.Home)
            .Subscribe(_ => TurnOffCameras());
    }

    /// <summary>
    /// Turns on the cameras if nobody is home.
    /// </summary>
    private void TurnOnCameras()
    {
        if (entities.IsAnyoneHome())
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
        if (!entities.IsAnyoneHome())
        {
            return;
        }
        
        SetCamerasState(false);
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
}