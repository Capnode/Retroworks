using Konamiman.Z80dotNet;

namespace Retroworks.Components;

public interface IEmulatorDevice
{
    delegate void TxData(byte data);

    string Name { get; }
    string Description { get; }
    string Manufacturer { get; }
    IMemory Memory { get; }
    void RxData(byte data);
    DigitalIO? DigitalIO { get; }

    void Initialize(TxData txData, string? cfPath);
    void Start(CancellationToken cancel);
}
