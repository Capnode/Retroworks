using Konamiman.Z80dotNet;
using Retroworks.RCBus.Devices.Components;
using System;
using System.Threading;
using static Retroworks.RCBus.Devices.IEmulatorDevice;

namespace Retroworks.RCBus.Devices;

/// <summary>
/// https://smallcomputercentral.com/firmware/firmware-scm-f1/
/// </summary>
public class SCM_F1 : IEmulatorDevice
{
    private const decimal CpuFrequency = 7.3728m; // MHz
    private const int BankSize = 0x8000; // 32KB
    private const int MemorySize = 1024 * 1024; // 1MB
    private const int PortSize = 256;
    private const ushort BankPort = 0x78;
    private const ushort SioPort1 = 0x80;
    private const ushort SioPort2 = 0x84;
    private const ushort CtcPort1 = 0x88;
    private const ushort CtcPort2 = 0x8C;
    private const ushort AciaPort1 = 0xA2;
    private const ushort AciaPort2 = 0xA4;
    private const ushort SerialPort = 0x28;
    private const ushort CfCardPort = 0x90;
    private const ushort DigitalIoPort = 0xA0;
    private const ushort StatusLedPort = 0x08;
    private const int FloatingBus = 0xff;

    private Z80Processor? _processor;
    private BankedMemory? _memory;
    private ACIA_6850? _acia;

    public string Name => "SCM-F1";
    public string Description => "";
    public string Manufacturer => "Small Computer Central";
    public IMemory Memory => _memory ?? throw new NullReferenceException(nameof(_memory));
    public DigitalIO? DigitalIO { get; private set; } = new DigitalIO(DigitalIoPort);

    public void Initialize(TxData txData, string? cfPath)
    {
        _processor = new Z80Processor();
        _processor.ClockFrequencyInMHz = CpuFrequency;
        _memory = new BankedMemory(MemorySize, BankSize, BankPort);
        _processor.Memory = _memory;
        _processor.PortsSpace = new PortMemory(PortSize, ReadPort, WritePort);
        _acia = new ACIA_6850(AciaPort1, txData);
    }

    public void Start(CancellationToken cancel)
    {
        _processor?.Start(cancel);
    }

    public void RxData(byte data)
    {
        _acia?.RxData(data);
    }

    private byte ReadPort(ushort port)
    {
        byte value = FloatingBus;
        value &= DigitalIO?.ReadPort(port) ?? FloatingBus;
        value &= _acia?.ReadPort(port) ?? FloatingBus;
        //Debug.WriteLine($"Port read  {port:X2} -> {value:X2}");
        return value;
    }

    private void WritePort(ushort port, byte value)
    {
        //Debug.WriteLine($"Port write {port:X2} <- {value:X2}");
        _memory?.WritePort(port, value);
        DigitalIO?.WritePort(port, value);
        _acia?.WritePort(port, value);
    }

    public void Initialize(TxData txData)
    {
        throw new NotImplementedException();
    }
}