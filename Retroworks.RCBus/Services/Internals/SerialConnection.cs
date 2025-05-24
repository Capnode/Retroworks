using Avalonia.Input;
using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Threading;
using Retroworks.RCBus.Models;

namespace Retroworks.RCBus.Services.Internals;
/// <summary>
/// SerialConnection class implements IConnection interface for serial communication.
/// It manages the connection to a serial port, handles data transmission and reception,
/// and provides events for connection status changes.
/// </summary>
/// 

internal class SerialConnection : IConnection
{
    private SerialPort? _terminal;
    private int _txCharDelay;
    private int _txLineDelay;

    public event EventHandler<DataReceivedEventArgs>? DataReceived;
    public event EventHandler<EventArgs> Closed;

    public string[] Devices => SerialPort.GetPortNames();
    public bool IsConnected => _terminal != null && _terminal.IsOpen;

    public bool Connect()
    {
        throw new NotImplementedException();
    }

    public bool Connect(SerialPortModel port)
    {
        _terminal = new SerialPort(port.PortName, port.Speed, port.Parity, port.DataBit, port.StopBit);
        _terminal.NewLine = "\r\n";
        _terminal.Handshake = port.Handshake;
        _terminal.RtsEnable = true;
        _terminal.DtrEnable = true;
        _terminal.Encoding = Encoding.Unicode;
        _terminal.ReadBufferSize = 4096;
        _terminal.WriteBufferSize = 4096;
        _terminal.ReadTimeout = 1500;
        _terminal.WriteTimeout = 1500;
        _txCharDelay = port.TxCharDelay;
        _txLineDelay = port.TxLineDelay;
        _terminal.Open();
        var buffer = new byte[_terminal.ReadBufferSize];
        _terminal.DataReceived += (sender, e) =>
        {
            var count = _terminal.Read(buffer, 0, buffer.Length);
            var data = new byte[count];
            Array.Copy(buffer, data, count);
            //foreach (var chr in data)
            //{
            //    Debug.Write(Printable(chr));
            //}

            DataReceived?.Invoke(this, new DataReceivedEventArgs { Data = data });
        };
        return IsConnected;
    }

    public void Disconnect()
    {
        _terminal?.Close();
        _terminal?.Dispose();
        _terminal = null;
    }

    public void KeyPressed(Key key, char c, KeyModifiers modifiers)
    {
        throw new NotImplementedException();
    }

    public void SendData(byte[] data)
    {
        if (_terminal == default) return;
        if (_txCharDelay == 0 && _txCharDelay == 0)
        {
            _terminal.Write(data, 0, data.Length);
            return;
        }

        for (int i = 0; i < data.Length; i++)
        {
            _terminal.Write(data, i, 1);
            if (data[i] == '\r')
            {
                Thread.Sleep(_txLineDelay);
            }
            else
            {
                Thread.Sleep(_txCharDelay);
            }
        }
    }

    public void SetTerminalWindowSize(int columns, int rows, int width, int height)
    {
    }

    private string Printable(byte chr)
    {
        if (chr >= 0x20 && chr < 0x7f || chr == 0x0A || chr == 0x0D || chr == 0x09 || chr == 0x08)
        {
            return ($"{chr}");
        }
        else
        {
            return ($"<{chr:X2}>");
        }
    }
}
