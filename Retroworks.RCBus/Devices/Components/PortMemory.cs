using Konamiman.Z80dotNet;
using System;

namespace Retroworks.RCBus.Devices.Components;

/// <summary>
/// Represents a trivial memory implementation in which all the addresses are RAM 
/// and the values written are simply read back. This is the default implementation
/// of <see cref="IMemory"/>.
/// </summary>
internal class PortMemory : IMemory
{
    public delegate byte ReadPort(ushort port);
    public delegate void WritePort(ushort port, byte value);

    private ReadPort ReadByte { get; }
    private WritePort WriteByte { get; }

    public PortMemory(int size, ReadPort readPort, WritePort writePort)
    {
        if (size < 1) throw new InvalidOperationException("Memory size must be greater than zero");
        if (readPort == null) throw new ArgumentNullException(nameof(readPort));
        if (writePort == null) throw new ArgumentNullException(nameof(writePort));

        Size = size;
        ReadByte = readPort;
        WriteByte = writePort;
    }

    public int Size { get; private set; }

    public byte this[int port]
    {
        get
        {
            return ReadByte?.Invoke((ushort)port) ?? 0xff;
        }
        set
        {
            WriteByte?.Invoke((ushort)port, value);
        }
    }

    public void SetContents(int startAddress, byte[] contents, int startIndex = 0, int? length = null)
    {
        throw new NotImplementedException("SetContents is not implemented for PortMemory");
    }

    public byte[] GetContents(int startAddress, int length)
    {
        throw new NotImplementedException("GetContents is not implemented for PortMemory");
    }
}
