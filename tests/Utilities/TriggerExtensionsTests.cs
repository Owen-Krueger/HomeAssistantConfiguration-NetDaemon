using Moq;
using NetDaemon.Utilities;

namespace NetDaemon.Tests.Utilities;

public class TriggerExtensionsTests
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
}