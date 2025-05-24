namespace Retroworks.Components;

public class DigitalIO
{
    public Action<byte>? OutData;

    private const ushort Data = 0x00; // Data port
    private readonly ushort _base;

    public DigitalIO(ushort port)
    {
        _base = port;
    }

    public byte InData { get; set; } = 0xff; // Default value for unhandled ports

    public byte? ReadPort(ushort port)
    {
        byte value;
        switch (port - _base)
        {
            case Data:
                value = InData;
                break;
            default:
                return null;
        }

        //Debug.WriteLine($"DigitalIO read  {port:X2} -> {value:X2}");
        return value;
    }

    public void WritePort(ushort port, byte value)
    {
        switch (port - _base)
        {
            case Data:
                OutData?.Invoke(value);
                break;
            default:
                return;
        }

        //Debug.WriteLine($"DigitalIO write {port:X2} <- {value:X2}");
    }
}
