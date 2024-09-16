namespace NetDaemon.apps.Internet;

[NetDaemonApp]
public class Internet
{
    public Internet(IHaContext context)
    {

    }
}

public class InternetConfiguration
{
    public SwitchEntity InternetModemSmartPlug { get; set; }
}