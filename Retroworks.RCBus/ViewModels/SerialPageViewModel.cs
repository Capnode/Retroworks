using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using Retroworks.RCBus.Services;
using CommunityToolkit.Mvvm.Input;
using System;
using Retroworks.RCBus.Models;
using Avalonia.Controls;
using System.IO.Ports;
using System.Linq;
using Retroworks.RCBus.Services.Internals;
using VtNetCore.VirtualTerminal;
using Avalonia.Styling;
using VtNetCore.VirtualTerminal.Enums;
using VtNetCore.VirtualTerminal.Model;
using Avalonia;

namespace Retroworks.RCBus.ViewModels;

public partial class SerialPageViewModel : ViewModelBase
{
    public static List<int> BaudRates => SerialPortModel.BaudRates;
    public static List<int> DataBits => SerialPortModel.DataBits;
    public static List<StopBits> StopBits => Enum.GetValues(typeof(StopBits)).Cast<StopBits>().ToList();
    public static List<Parity> ParityTypes => Enum.GetValues(typeof(Parity)).Cast<Parity>().ToList();
    public static List<Handshake> FlowControlTypes => Enum.GetValues(typeof(Handshake)).Cast<Handshake>().ToList();

    private readonly ISettings _settings;
    private readonly SerialConnection _serial;
    private bool _pending;

    /// <summary>
    /// Design-time only constructor
    /// Allow nullable PageFactory for now in designer... ideally get it working
    /// </summary>
#pragma warning disable CS8618
    public SerialPageViewModel()
    {
    }
#pragma warning restore CS8618

    public SerialPageViewModel(ISettings settings, IEnumerable<IConnection> connections)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _serial = connections?.OfType<SerialConnection>().First() ?? throw new ArgumentNullException(nameof(connections));

        if (Design.IsDesignMode) return;
        SerialPorts = _serial.Devices;
        SerialPort = _settings.GetString(nameof(SerialPort));
        Connection = _serial;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PanelWidth))]
    private bool _panelExpanded = true;

    [ObservableProperty]
    public IConnection? _connection;

    [ObservableProperty]
    private VirtualTerminalController? _terminal;

    [ObservableProperty]
    public bool _isPower;

    [ObservableProperty]
    private string[] _serialPorts = [];

    [ObservableProperty]
    private string? _serialPort;
    partial void OnSerialPortChanged(string? value)
    {
        GetSettings();
        if (value == default) return;
        var old = _settings.GetString(nameof(SerialPort));
        if (old == value) return;
        _settings.SetString(nameof(SerialPort), value);
    }

    [ObservableProperty]
    private int _serialSpeed;
    partial void OnSerialSpeedChanged(int value) => SaveSettings();

    [ObservableProperty]
    private int _serialBit;
    partial void OnSerialBitChanged(int value) => SaveSettings();

    [ObservableProperty]
    private Parity _serialParity;
    partial void OnSerialParityChanged(Parity value) => SaveSettings();

    [ObservableProperty]
    private Handshake _serialHandshake;
    partial void OnSerialHandshakeChanged(Handshake value) => SaveSettings();

    [ObservableProperty]
    private StopBits _serialStopBit;
    partial void OnSerialStopBitChanged(StopBits value) => SaveSettings();

    [ObservableProperty]
    private int _txCharDelay;
    partial void OnTxCharDelayChanged(int value) => SaveSettings();

    [ObservableProperty]
    private int _txLineDelay;
    partial void OnTxLineDelayChanged(int value) => SaveSettings();

    [RelayCommand]
    private void UpdateSerialPorts()
    {
        var ports = _serial.Devices;
        if (!ports.Any())
        {
            SerialPort = null;
        }

        SerialPorts = ports;
    }

    [RelayCommand]
    private void Power()
    {
        if (string.IsNullOrEmpty(SerialPort)) return;
        if (_pending) return;
        _pending = true;
        if (IsPower)
        {
            _serial.Disconnect();
            IsPower = false;
            Terminal = default;
        }
        else
        {
            var json = _settings.GetString(SerialPort);
            if (!string.IsNullOrEmpty(json))
            {
                if (Application.Current?.ActualThemeVariant == ThemeVariant.Light)
                {
                    TerminalAttribute.DefaultBackground = ETerminalColor.White;
                    TerminalAttribute.DefaultForeground = ETerminalColor.Black;
                }
                else
                {
                    TerminalAttribute.DefaultBackground = ETerminalColor.Black;
                    TerminalAttribute.DefaultForeground = ETerminalColor.Green;
                }
                Terminal = new VirtualTerminalController();
                var serialPortModel = new SerialPortModel(json);
                _serial.Connect(serialPortModel);
                IsPower = true;
            }
        }

        _pending = false;
    }

    public double PanelWidth
    {
        get => PanelExpanded ? _settings.SerialPanelWidth : 0;
        set
        {
            if (!PanelExpanded) return;
            if (_settings == null) return;
            if (value == PanelWidth) return;
            _settings.SerialPanelWidth = value;
            OnPropertyChanged();
        }
    }

    private void GetSettings()
    {
        if (_pending) return;
        _pending = true;

        var json = string.Empty;
        if (SerialPort != default)
        {
            json = _settings.GetString(SerialPort);
        }

        if (string.IsNullOrEmpty(json))
        {
            SerialSpeed = default;
            SerialParity = default;
            SerialBit = default;
            SerialStopBit = default;
            SerialHandshake = default;
            TxCharDelay = default;
            TxLineDelay = default;
        }
        else
        {
            var serialPortModel = new SerialPortModel(json);
            SerialPort = serialPortModel.PortName;
            SerialSpeed = serialPortModel.Speed;
            SerialParity = serialPortModel.Parity;
            SerialBit = serialPortModel.DataBit;
            SerialStopBit = serialPortModel.StopBit;
            SerialHandshake = serialPortModel.Handshake;
            TxCharDelay = serialPortModel.TxCharDelay;
            TxLineDelay = serialPortModel.TxLineDelay;
        }

        _pending = false;
    }

    private void SaveSettings()
    {
        if (_settings == default) throw new NullReferenceException(nameof(_settings));
        if (string.IsNullOrEmpty(SerialPort)) return;
        if (_pending) return;
        var json = new SerialPortModel(SerialPort, SerialSpeed, SerialParity, SerialBit, SerialStopBit, SerialHandshake, TxCharDelay, TxLineDelay).ToJson();
        var old = _settings.GetString(SerialPort);
        if (old == json) return;
        _settings.SetString(SerialPort, json);
    }
}
