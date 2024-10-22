using Moq;
using NetDaemon.Utilities;

namespace NetDaemon.Tests.Utilities;

public class TriggerUtilitiesTests
{
    [Test]
    public void DisposeTriggers_ListOfTriggers_TriggersDisposed()
    {
        var disposableMock = new Mock<IDisposable>();
        List<IDisposable> triggers =
        [
            disposableMock.Object
        ];
        var result = triggers.DisposeTriggers();
        
        Assert.That(result, Is.Empty);
        disposableMock.Verify(x => x.Dispose(), Times.Once);
    }

    [Test]
    public void UpdateAutomationTriggers_TriggersTurnedOnAndOff_ExpectedTriggers()
    {
        var disposableMock = new Mock<IDisposable>();
        List<IDisposable> triggers =
        [
            disposableMock.Object
        ];

        triggers = TriggerUtilities.UpdateAutomationTriggers(triggers, false, SetUpTriggers);
        Assert.That(triggers, Has.Count.Zero);

        triggers = TriggerUtilities.UpdateAutomationTriggers(triggers, true, SetUpTriggers);
        Assert.That(triggers, Has.Count.EqualTo(2));

        triggers = triggers[..1];
        triggers = TriggerUtilities.UpdateAutomationTriggers(triggers, true, SetUpTriggers);
        Assert.That(triggers, Has.Count.EqualTo(1));
    }

    private static List<IDisposable> SetUpTriggers()
        =>
        [
            new Mock<IDisposable>().Object,
            new Mock<IDisposable>().Object
        ];
}