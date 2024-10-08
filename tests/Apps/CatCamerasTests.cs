﻿using NetDaemon.Apps.Cameras;
using NetDaemon.HassModel.Entities;
using NetDaemon.Tests.TestHelpers;

namespace NetDaemon.Tests.Apps;

public class CatCamerasTests : TestBase
{
    [Test]
    public void CatCameras_EveryoneAway_CamerasTurnedOn()
    {
        TestScheduler.AdvanceToNow();
        HaMock.TriggerStateChange(Entities.Person.Allison, "home");
        HaMock.TriggerStateChange(Entities.Person.Owen, "away");
        HaMock.TriggerStateChange(Entities.Switch.CatCameraUpSmartPlug, "off");
        HaMock.TriggerStateChange(Entities.Switch.CatCameraDownSmartPlug, "off");
        Context.GetApp<CatCameras>();
        
        HaMock.TriggerStateChange(Entities.Person.Allison, "away");
        TestScheduler.AdvanceBy(TimeSpan.FromMinutes(15).Ticks);
        Assert.Multiple(() =>
        {
            Assert.That(Entities.Switch.CatCameraUpSmartPlug.IsOn(), Is.True);
            Assert.That(Entities.Switch.CatCameraDownSmartPlug.IsOn(), Is.True);
        });
    }
    
    [Test]
    public void CatCameras_NotEveryoneAway_CamerasNotTurnedOn()
    {
        TestScheduler.AdvanceToNow();
        HaMock.TriggerStateChange(Entities.Person.Allison, "home");
        HaMock.TriggerStateChange(Entities.Person.Owen, "home");
        HaMock.TriggerStateChange(Entities.Switch.CatCameraUpSmartPlug, "off");
        HaMock.TriggerStateChange(Entities.Switch.CatCameraDownSmartPlug, "off");
        Context.GetApp<CatCameras>();
        
        HaMock.TriggerStateChange(Entities.Person.Allison, "away");
        TestScheduler.AdvanceBy(TimeSpan.FromMinutes(15).Ticks);
        Assert.Multiple(() =>
        {
            Assert.That(Entities.Switch.CatCameraUpSmartPlug.IsOn(), Is.False);
            Assert.That(Entities.Switch.CatCameraDownSmartPlug.IsOn(), Is.False);
        });
    }

    [TestCase("person.allison")]
    [TestCase("person.owen")]
    public void CatCameras_EveryoneHome_CamerasTurnedOff(string personId)
    {
        HaMock.TriggerStateChange(Entities.Person.Allison, "away");
        HaMock.TriggerStateChange(Entities.Person.Owen, "away");
        HaMock.TriggerStateChange(Entities.Switch.CatCameraUpSmartPlug, "on");
        HaMock.TriggerStateChange(Entities.Switch.CatCameraDownSmartPlug, "on");
        Context.GetApp<CatCameras>();
        
        HaMock.TriggerStateChange(new Entity(HaMock.Object, personId), "home");
        Assert.Multiple(() =>
        {
            Assert.That(Entities.Switch.CatCameraUpSmartPlug.IsOn(), Is.False);
            Assert.That(Entities.Switch.CatCameraDownSmartPlug.IsOn(), Is.False);
        });
    }
}