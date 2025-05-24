using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace Retroworks.RCBus.Models;

public class SerialPortModel
{
    public static readonly List<int> DataBits = [7, 8];
    public static readonly List<int> BaudRates =
    [
        110,
        300,
        600,
        1200,
        2400,
        4800,
        9600,
        14400,
        19200,
        38400,
        57600,
        115200,
        230400,
        460800,
        921600,
    ];

    public SerialPortModel()
    {
        PortName = string.Empty;
        Speed = 9600;
        Parity = Parity.None;
        DataBit = 8;
        StopBit = StopBits.One;
        Handshake = Handshake.None;
    }

    public SerialPortModel(string portName, int speed, Parity parity, int dataBit, StopBits stopBit, Handshake handshake, int txCharDelay, int txLineDelay)
    {
        PortName = portName;
        Speed = speed;
        Parity = parity;
        DataBit = dataBit;
        StopBit = stopBit;
        Handshake = handshake;
        TxCharDelay = txCharDelay;
        TxLineDelay = txLineDelay;
    }

    public SerialPortModel(string json)
    {
        var model = JsonConvert.DeserializeObject<SerialPortModel>(json);
        if (model == null) throw new NullReferenceException(nameof(model));
        PortName = model.PortName;
        Speed = model.Speed;
        Parity = model.Parity;
        DataBit = model.DataBit;
        StopBit = model.StopBit;
        Handshake = model.Handshake;
        TxCharDelay = model.TxCharDelay;
        TxLineDelay = model.TxLineDelay;
    }

    public string PortName { get; set; } = string.Empty;
    public int Speed { get; set; }
    public Parity Parity { get; set; }
    public int DataBit { get; set; }
    public StopBits StopBit { get; set; }
    public Handshake Handshake { get; set; }
    public int TxCharDelay { get; set; }
    public int TxLineDelay { get; set; }

    public string ToJson() => JsonConvert.SerializeObject(this);
}
