using Konamiman.Z80dotNet;

namespace Retroworks.Components;

/// <summary>
/// This module is the emulator for a serial I/O interface based on the 
/// 6850 Asynchronous Communications Interface Adapter(ACIA)
/// Control registers (read and write)
/// Bit Control write Control read
///  0    Counter divide select 1    Receive data register full
///  1    Counter divide select 2    Transmit data register empty
///  2    Word select 1              Data carrier detect(/DCD) input
///  3    Word seelct 2              Clear to send(/CTS) input
///  4    Word select 3              Framing error
///  5    Transmit contol 1          Receiver overrun
///  6    Transmit control 2         Parity error
///  7    Receive interrupt enable Interrupt request
///
/// Control register write
/// Bit   7   6   5   4   3   2   1   0
///       |   |   |   |   |   |   |   |
///       |   |   |   |   |   |   0   0     Clock divide 1
///       |   |   |   |   |   |   0   1     Clock divide 16
/// >     |   |   |   |   |   |   1   0  >  Clock divide 64
///       |   |   |   |   |   |   1   1     Master reset
///       |   |   |   |   |   |
///       |   |   |   0   0   0     7 data bits, even parity, 2 stop bits
///       |   |   |   0   0   1     7 data bits, odd parity,  2 stop bits
///       |   |   |   0   1   0     7 data bits, even parity, 1 stop bit
///       |   |   |   0   1   1     7 data bits, odd parity,  1 stop bit
///       |   |   |   1   0   0     8 data bits, no parity,   2 stop bits
///       |   |   |   1   0   1  >  8 data bits, no parity,   1 stop bit
///       |   |   |   1   1   0     8 data bits, even parity, 1 stop bit
///       |   |   |   1   1   1     8 data bits, odd parity,  1 stop bit
///       |   |   |
///       |   0   0  >  /RTS = low(ready), tx interrupt disabled
///       |   0   1     /RTS = low(ready), tx interrupt enabled
///       |   1   0     /RTS = high(not ready), tx interrupt disabled 
///       |   1   1     /RTS = low, tx break, tx interrupt disabled
///       |
///       0  >  Receive interrupt disabled
///       1     Receive interrupt enabled
///
/// Control register read
/// Bit   7   6   5   4   3   2   1   0
///       |   |   |   |   |   |   |   |
///       |   |   |   |   |   |   |   +-------  Receive data register full
///       |   |   |   |   |   |   +-------  Transmit data register empty
///       |   |   |   |   |   +-------  Data carrier detect(/DCD)
///       |   |   |   |   +-------  Clear to send(/CTS)
///       |   |   |   +-------  Framing error
///       |   |   +-------  Receiver overrun
///       |   +-------  Parity error
///       +-------  Interrupt request

/// Externally definitions required:
/// kACIABase: .EQU kACIA2; I/O base address
/// kACIACont: .EQU kACIABase+0    ;I/O address of control register
/// kACIAData: .EQU kACIABase+1    ;I/O address of data register

/// Hard coded constants don't use EQUs so this file can be included 
/// without breaking the use of only local labels(@<label>)
///
/// Control register values
/// kACIA1Rst: .EQU 0b00000011     ;Master reset
/// kACIA1Ini: .EQU 0b00010110     ;No int, RTS low, 8+1, /64
///
/// Status(control) register bit numbers
/// kACIARxRdy: .EQU 0             ;Receive data available bit number
/// kACIATxRdy: .EQU 1             ;Transmit data empty bit number
///
/// Device detection, test 1
/// This test just reads from the devices' status (control) register
/// and looks for register bits in known states:
/// /CTS input bit = low
/// /DCD input bit = low
/// WARNING
/// Sometimes at power up the Tx data reg empty bit is zero, but
/// recovers after device initialised.So test 1 excludes this bit.
/// kACIAMsk1: .EQU  0b00001100    ;Mask for known bits in control reg
/// kACIATst1: .EQU  0b00000000    ;Test value following masking
///
/// Device detection, test 2
/// This test just reads from the devices' status (control) register
/// and looks for register bits in known states:
/// /CTS input bit = low
/// /DCD input bit = low
/// Transmit data register empty bit = high
/// kACIAMsk2: .EQU  0b00001110    ;Mask for known bits in control reg
/// kACIATst2: .EQU  0b00000010    ;Test value following masking
/// </summary>
public class ACIA_6850 : IZ80InterruptSource
{
    public event EventHandler? NmiInterruptPulse;

    private const ushort Control = 0;
    private const ushort Data = 1;

    private readonly ushort _base;
    private readonly IEmulatorDevice.TxData _txData;
    private readonly Queue<byte> _rxData = new();

    private byte _controlRegister;
    private byte _statusRegister;
    private byte _rxRegister;

    public ACIA_6850(ushort port, IEmulatorDevice.TxData txData)
    {
        _base = port;
        _txData = txData;
    }

    public bool IntLineIsActive { get; private set; }

    public byte? ValueOnDataBus { get; private set; }

    public byte? ReadPort(ushort port)
    {
        byte value;
        switch (port - _base)
        {
            case Control:
                value = _statusRegister;
                break;
            case Data:
                if (_rxData.Count > 0)
                {
                    _rxRegister = _rxData.Dequeue();
                }

                if (_rxData.Count == 0)
                {
                    _statusRegister &= 0x7E; // Clear interrupt and receive data register full bit
                    IntLineIsActive = false;
                }
                else
                {
                    _statusRegister |= (byte)(_controlRegister & 0x80); // Set interrupt request if enabled
                    _statusRegister |= 0x01; // Set receive data register full
                }
                value = _rxRegister;
                break;
            default:
                return null;
        }

        //Debug.WriteLine($"ACIA read  {port:X2} -> {value:X2}");
        return value;

    }

    public void WritePort(ushort port, byte value)
    {
        switch (port - _base)
        {
            case Control:
                _controlRegister = value;
                if ((value & 0x03) == 0x03)
                {
                    // Master reset
                    _statusRegister = 0x00;
                    break;
                }
                _statusRegister |= 0x02; // Transmit data register empty
                break; ;
            case Data:
                _txData?.Invoke(value);
                _statusRegister |= 0x02; // Transmit data register empty
                break;
            default:
                return;
        }

        //Debug.WriteLine($"ACIA write {port:X2} <- {value:X2}");
    }

    public void RxData(byte value)
    {
        _rxData.Enqueue(value);
        _statusRegister |= 0x01; // Receive data register full
        if ((_controlRegister & 0x80) != 0) // Receive interrupt enabled
        {
            _statusRegister |= 0x80; // Interrupt request
            IntLineIsActive = true;
        }
    }
}
