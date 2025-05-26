using Avalonia.Input;
using Retroworks.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Retroworks.RCBus.Services.Internals;

internal class EmulatorConnection : IConnection
{
    public event EventHandler<DataReceivedEventArgs>? DataReceived;
    public event EventHandler<EventArgs>? Opened;
    public event EventHandler<EventArgs>? Closed;

    private IEmulatorDevice? _device;
    private CancellationTokenSource? _cancel;

    public EmulatorConnection(IEnumerable<IEmulatorDevice> devices)
    {
        Devices = devices;
        if (devices == null) throw new ArgumentNullException(nameof(devices));
    }

    public IEnumerable<IEmulatorDevice> Devices { get; }
    public bool IsConnected { get; set; }

    public bool Connect()
    {
        throw new NotImplementedException();
    }

    public bool Connect(IEmulatorDevice device, string? filename, string? cfPath)
    {
        IsConnected = true;
        _device = device ?? throw new ArgumentNullException(nameof(device));
        _cancel = new CancellationTokenSource();
//        throw new NotImplementedException("This method is not implemented yet. Please use the overload with filename and cfPath.");
        Task.Run(() =>
        {
            device.Initialize(ReceiveData, cfPath);
            //RxData("\e[37m\e[41m");
            if (!string.IsNullOrEmpty(filename))
            {
                if (filename.EndsWith(".hex", StringComparison.OrdinalIgnoreCase))
                {
                    LoadHexFile(device, filename);
                }
                else
                {
                    LoadBinaryFile(device, filename);
                }
            }

            device.Start(_cancel.Token);
        }, _cancel.Token).ContinueWith((m) =>  
        {
            if (m.IsFaulted)
            {
                ReceiveData($"Error: {m.Exception?.Message}\r\n");
            }
            else
            {
                ReceiveData($"Execution Completed\r\n");
            }

            IsConnected = false;
            Closed?.Invoke(this, EventArgs.Empty);
        });

        Opened?.Invoke(this, EventArgs.Empty);
        return IsConnected;
    }

    public void Disconnect()
    {
        _cancel?.Cancel();
        IsConnected = false;
        Closed?.Invoke(this, EventArgs.Empty);
    }

    public void KeyPressed(Key key, char c, KeyModifiers modifiers)
    {
        throw new NotImplementedException();
    }

    public void SetTerminalWindowSize(int columns, int rows, int width, int height)
    {
    }

    public void SendData(byte[] data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        foreach (var chr in data)
        {
            _device?.RxData(chr);
        }
    }

    private void ReceiveData(byte chr)
    {
        //Debug.Write(Printable(chr));
        DataReceived?.Invoke(this, new DataReceivedEventArgs { Data = new[] { chr } });
    }

    private void ReceiveData(string text)
    {
        DataReceived?.Invoke(this, new DataReceivedEventArgs { Data = Encoding.ASCII.GetBytes(text) });
    }

    private string Printable(byte chr)
    {
        if (chr >= 0x20 && chr < 0x7f || chr == 0x0A || chr == 0x0D || chr == 0x09 || chr == 0x08)
        {
            return ($"{chr}");
        }
        else
        {
            return($"<{chr:X2}>");
        }
    }

    /// <summary>
    /// Reads an Intel HEX file and loads its contents into the emulator memory.
    /// </summary>
    /// <param name="filePath">The path to the HEX file.</param>
    private void LoadHexFile(IEmulatorDevice device, string filePath)
    {
        if (!File.Exists(filePath))
        {
            ReceiveData($"The file '{filePath}' does not exist.\r\n");
            return;
        }

        ReceiveData($"Loading '{Path.GetFileName(filePath)}'\r\n");
        using var reader = new StreamReader(filePath);
        string? line;
        var totalBytes = 0;
        while ((line = reader.ReadLine()) != null)
        {
            if (!line.StartsWith(":")) continue;

            // Parse the HEX file line
            var byteCount = Convert.ToInt32(line.Substring(1, 2), 16);
            var address = Convert.ToInt32(line.Substring(3, 4), 16);
            var recordType = Convert.ToInt32(line.Substring(7, 2), 16);
            var data = line.Substring(9, byteCount * 2);

            if (recordType == 0x00) // Data record
            {
                for (int i = 0; i < byteCount; i++)
                {
                    var byteValue = Convert.ToByte(data.Substring(i * 2, 2), 16);
                    device.Memory[address + i] =  byteValue;
                }

                totalBytes += byteCount;
            }
            else if (recordType == 0x01) // End of file record
            {
                break;
            }
        }

        ReceiveData($"Loaded {totalBytes} bytes\r\n");
    }

    private void LoadBinaryFile(IEmulatorDevice device, string filePath)
    {
        if (!File.Exists(filePath))
        {
            ReceiveData($"The file '{filePath}' does not exist.\r\n");
            return;
        }

        // Open filePath for reading
        ReceiveData($"Loading '{Path.GetFileName(filePath)}'\r\n");
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(fileStream);
        var memory = device.Memory as BankedMemory ?? throw new ApplicationException("Memory is not banked");
        var totalBytes = 0;

        // Read the file in chunks of 256 bytes
        byte[] buffer = new byte[256];
        int bytesRead;
        while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
        {
            if (totalBytes + bytesRead > memory.Size)
            {
                ReceiveData($"File too large for memory ({totalBytes + bytesRead} bytes)\r\n");
                break;
            }
            memory.Bank = (byte)(totalBytes / memory.BankSize);
            var address = totalBytes % memory.BankSize;
            device.Memory.SetContents(address, buffer, 0, bytesRead);
            totalBytes += bytesRead;
        }

        // Close the file
        memory.Bank = 0;
        fileStream.Close();
        reader.Close();

        // Notify the user
        ReceiveData($"Loaded {totalBytes} bytes\r\n");
    }
}
