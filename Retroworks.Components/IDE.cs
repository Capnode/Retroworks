using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Retroworks.Components;

public class IDE
{
    // IDE register offsets (primary channel)
    // | Offset | Register Name        | Direction | Description                        |
    // |--------|----------------------|-----------|------------------------------------|
    // | 0      | Data                 | R/W       | 16-bit data register               |
    // | 1      | Error / Features     | R / W     | Error (read) / Features (write)    |
    // | 2      | Sector Count         | R/W       | Number of sectors to transfer      |
    // | 3      | Sector Number        | R/W       | Starting sector (LBA low)          |
    // | 4      | Cylinder Low         | R/W       | Cylinder address (low byte)        |
    // | 5      | Cylinder High        | R/W       | Cylinder address (high byte)       |
    // | 6      | Drive/Head           | R/W       | Drive select and head number       |
    // | 7      | Status / Command     | R / W     | Status (read) / Command (write)    |
    private const int REGISTER_DATA = 0;           // 16-bit
    private const int REGISTER_ERROR = 1;          // read
    private const int REGISTER_FEATURES = 1;       // write
    private const int REGISTER_SECTOR_COUNT = 2;
    private const int REGISTER_SECTOR_NUMBER = 3;
    private const int REGISTER_CYLINDER_LOW = 4;
    private const int REGISTER_CYLINDER_HIGH = 5;
    private const int REGISTER_DRIVE_HEAD = 6;
    private const int REGISTER_STATUS = 7;         // read
    private const int REGISTER_COMMAND = 7;         // write

    // Status register bits
    // | Bit | Name   | Description                |
    // |-----|--------|----------------------------|
    // | 7   | BSY    | Busy                       |
    // | 6   | DRDY   | Device Ready               |
    // | 5   | DF     | Device Fault               |
    // | 4   | DSC    | Disk Seek Complete         |
    // | 3   | DRQ    | Data Request (ready)       |
    // | 2   | CORR   | Corrected Data             |
    // | 1   | IDX    | Index                      |
    // | 0   | ERR    | Error                      |
    private const byte STATUS_BSY = 0x80;
    private const byte STATUS_DRDY = 0x40;
    private const byte STATUS_DF = 0x20;
    private const byte STATUS_DSC = 0x10;
    private const byte STATUS_DRQ = 0x08;
    private const byte STATUS_CORR = 0x04;
    private const byte STATUS_IDX = 0x02;
    private const byte STATUS_ERR = 0x01;

    // Error register bits
    // | Bit | Name   | Description                |
    // |-----|--------|----------------------------|
    // | 7   | BBK0   | Bad Block                  |
    // | 6   | UNC    | Uncorrelated data          |
    // | 5   | MC     | Media change               |
    // | 4   | IDNF   | ID not found               |
    // | 3   | MCR    | Media change request       |
    // | 2   | ABRT   | Aborted command            |
    // | 1   | TK0NF  | Track 0 not found          |
    // | 0   | AMNF   | Address mark not found     |
    private const byte ERROR_BBK0 = 0x80;
    private const byte ERROR_UNC = 0x40;
    private const byte ERROR_MC = 0x20;
    private const byte ERROR_IDNF = 0x10;
    private const byte ERROR_NCR = 0x08;
    private const byte ERROR_ABRT = 0x04;
    private const byte ERROR_TK0NF = 0x02;
    private const byte ERROR_AMNF = 0x01;

    // Common IDE commands
    private const byte CMD_IDENTIFY_DEVICE = 0xEC; // Identify device command
    private const byte CMD_READ_SECTORS = 0x20;    // Read sectors command
    private const byte CMD_WRITE_SECTORS = 0x30;   // Write sectors command
    private const byte CMD_FLUSH_CACHE = 0xE7;     // Flush cache command
    private const byte CMD_SET_FEATURES = 0xEF;    // Set features command
    private const byte CMD_READ_MULTIPLE = 0xC4;   // Read multiple command
    private const byte CMD_WRITE_MULTIPLE = 0xC5;  // Write multiple command
    private const byte CMD_SET_MULTIPLE_MODE = 0xC6; // Set multiple mode command
    private const byte CMD_READ_DMA = 0xC8;        // Read DMA command
    private const byte CMD_WRITE_DMA = 0xCA;       // Write DMA command
    private const byte CMD_STANDBY_IMMEDIATE = 0xE0; // Standby immediate command
    private const byte CMD_SLEEP = 0xE6;           // Sleep command
    private const byte CMD_FLUSH_CACHE_EXT = 0xEA; // Flush cache extended command
    private const byte CMD_IDENTIFY_DEVICE_EXT = 0xEC; // Identify device extended command
    private const byte CMD_READ_NATIVE_MAX_ADDRESS = 0xF8; // Read native max address command
    private const byte CMD_READ_NATIVE_MAX_ADDRESS_EXT = 0xF9; // Read native max address extended command
    private const byte CMD_SET_MAX_ADDRESS = 0xF6; // Set max address command
    private const byte CMD_SET_MAX_ADDRESS_EXT = 0xF7; // Set max address extended command
    private const byte CMD_READ_LOG_EXT = 0xF5;   // Read log extended command
    private const byte CMD_WRITE_LOG_EXT = 0xF4;  // Write log extended command
    private const byte CMD_READ_LOG = 0xF3;       // Read log command
    private const byte CMD_WRITE_LOG = 0xF2;      // Write log command
    private const byte CMD_READ_DMA_EXT = 0xC8;   // Read DMA extended command
    private const byte CMD_WRITE_DMA_EXT = 0xCA;  // Write DMA extended command
    private const byte CMD_READ_MULTIPLE_EXT = 0xC4; // Read multiple extended command
    private const byte CMD_WRITE_MULTIPLE_EXT = 0xC5; // Write multiple extended command
    private const byte CMD_SET_MULTIPLE_MODE_EXT = 0xC6; // Set multiple mode extended command
    private const byte CMD_READ_SECTORS_EXT = 0x20; // Read sectors extended command
    private const byte CMD_WRITE_SECTORS_EXT = 0x30; // Write sectors extended command
    private const byte CMD_READ_SECTORS_DMA = 0xC8; // Read sectors DMA command
    private const byte CMD_WRITE_SECTORS_DMA = 0xCA; // Write sectors DMA command
    private const byte CMD_READ_SECTORS_DMA_EXT = 0xC8; // Read sectors DMA extended command
    private const byte CMD_WRITE_SECTORS_DMA_EXT = 0xCA; // Write sectors DMA extended command
    private const byte CMD_READ_SECTORS_PIO = 0x20; // Read sectors PIO command
    private const byte CMD_WRITE_SECTORS_PIO = 0x30; // Write sectors PIO command
    private const byte CMD_READ_SECTORS_PIO_EXT = 0x20; // Read sectors PIO extended command
    private const byte CMD_WRITE_SECTORS_PIO_EXT = 0x30; // Write sectors PIO extended command
    private const byte CMD_READ_SECTORS_PIO_DMA = 0xC8; // Read sectors PIO DMA command
    private const byte CMD_WRITE_SECTORS_PIO_DMA = 0xCA; // Write sectors PIO DMA command
    private const byte CMD_READ_SECTORS_PIO_DMA_EXT = 0xC8; // Read sectors PIO DMA extended command
    private const byte CMD_WRITE_SECTORS_PIO_DMA_EXT = 0xCA; // Write sectors PIO DMA extended command

    private readonly ushort _base;
    private readonly int _drive;
    private FileStream _cfFile;
    private byte[] _data = new byte[512]; // Buffer for data

    private byte _dataRegister;         // 16-bit data register
    private byte _errorRegister;          // Error (read)
    private byte _featuresRegister;       // Features (write)
    private byte _sectorCountRegister;
    private byte _sectorNumberRegister;
    private byte _cylinderLowRegister;
    private byte _cylinderHighRegister;
    private byte _driveHeadRegister;
    private byte _statusRegister;         // Status (read)
    private byte _commandRegister;        // Command (write)
    private int _dataIndex;

    public IDE(ushort port, int drive, string? cfPath)
    {
        _base = port;
        _drive = drive;

        _dataRegister = 0;
        _errorRegister = 0;
        _featuresRegister = 0;
        _sectorCountRegister = 0;
        _sectorNumberRegister = 0;
        _cylinderLowRegister = 0;
        _cylinderHighRegister = 0;
        _driveHeadRegister = 0;
        _statusRegister = STATUS_DRDY | STATUS_DSC; // Device ready after reset
        _commandRegister = 0;

        if (cfPath == null) return;

        // Open CF card file cfPath for binary read/write operations
        if (File.Exists(cfPath))
        {
            _cfFile = new FileStream(cfPath, FileMode.Open, FileAccess.ReadWrite);
            // Read 512 bytes of data from the file into the buffer
            //_cfFile.Seek(0, SeekOrigin.Begin); // Set position to the beginning of the file
            //int bytesRead = _cfFile.Read(_data, 0, _data.Length);
            //DumpHex();
            //_cfFile.Seek(512, SeekOrigin.Begin); // Set position to the beginning of the file
            //bytesRead = _cfFile.Read(_data, 0, _data.Length);
            //DumpHex();
            //_cfFile.Seek(1024, SeekOrigin.Begin); // Set position to the beginning of the file
            //bytesRead = _cfFile.Read(_data, 0, _data.Length);
            //DumpHex();
        }
        else
        {
            _cfFile = new FileStream(cfPath, FileMode.CreateNew, FileAccess.ReadWrite);
            _cfFile.SetLength(1024 * 1024 * 1024); // Set size to 1GB (example)
        }
        _cfFile.Seek(0, SeekOrigin.Begin); // Set position to the beginning of the file
        //_cfFileStream.Flush(); // Flush the stream to ensure data is written
        //_cfFileStream.Close(); // Close the stream when done
        //_cfFileStream.Dispose(); // Dispose of the stream to release resources
    }

    /// <summary>
    /// Read from IDE register (offset 0-7)
    /// </summary>
    public byte? ReadPort(ushort port)
    {
        byte value = 0xff;
        switch (port - _base)
        {
            case REGISTER_DATA: // Data register (16-bit)
                if (UnselectedDrive()) break;
                if (_dataIndex < _data.Length)
                {
                    _dataRegister = _data[_dataIndex++];
                }

                if (_dataIndex < _data.Length)
                {
                    _statusRegister |= STATUS_DRQ; // Set data request bit
                }
                else
                {
                    _statusRegister &= unchecked((byte)~STATUS_DRQ); // Clear data request bit
                }
                value = _dataRegister;
                break;
            case REGISTER_ERROR: // Error register (read)
                if (UnselectedDrive()) break;
                value = _errorRegister;
                break;
            case REGISTER_SECTOR_COUNT:
                if (UnselectedDrive()) break;
                value = _sectorCountRegister;
                break;
            case REGISTER_SECTOR_NUMBER:
                if (UnselectedDrive()) break;
                value = _sectorNumberRegister;
                break;
            case REGISTER_CYLINDER_LOW:
                if (UnselectedDrive()) break;
                value = _cylinderLowRegister;
                break;
            case REGISTER_CYLINDER_HIGH:
                if (UnselectedDrive()) break;
                value = _cylinderHighRegister;
                break;
            case REGISTER_DRIVE_HEAD:
                if (UnselectedDrive()) break;
                value = _driveHeadRegister;
                break;
            case REGISTER_STATUS: // Status register (read)
                if (UnselectedDrive()) break;
                value = _statusRegister;
                break;
            default:
                return null;
        }

        Debug.WriteLine($"IDE read  {port:X2} -> {value:X2}");
        return value;
    }

    /// <summary>
    /// Write to IDE register (offset 0-7)
    /// </summary>
    public void WritePort(ushort port, byte value)
    {
        switch (port - _base)
        {
            case REGISTER_DATA: // Data register (16-bit)
                if (UnselectedDrive()) break;
                _dataRegister = value;
                break;
            case REGISTER_FEATURES: // Features register (write)
                if (UnselectedDrive()) break;
                _featuresRegister = value;
                break;
            case REGISTER_SECTOR_COUNT:
                if (UnselectedDrive()) break;
                _sectorCountRegister = value;
                 break;
            case REGISTER_SECTOR_NUMBER:
                if (UnselectedDrive()) break;
                _sectorNumberRegister = value;
                break;
            case REGISTER_CYLINDER_LOW:
                if (UnselectedDrive()) break;
                _cylinderLowRegister = value;
                break;
            case REGISTER_CYLINDER_HIGH:
                if (UnselectedDrive()) break;
                _cylinderHighRegister = value;
                break;
            case REGISTER_DRIVE_HEAD:
                _driveHeadRegister = value;
                break;
            case REGISTER_COMMAND: // Command register (write)
                if (UnselectedDrive()) break;
                _commandRegister = value;
                ExecuteCommand(_commandRegister);
                break;
            default:
                return;
        }

        Debug.WriteLine($"IDE write {port:X2} <- {value:X2}");
    }

    private bool UnselectedDrive()
    {
        return (_driveHeadRegister & 0x10) != _drive << 4;
    }

    /// <summary>
    /// Execute an IDE command (e.g., IDENTIFY, READ, WRITE)
    /// </summary>
    private void ExecuteCommand(byte command)
    {
        // Example: handle IDENTIFY DEVICE (0xEC)
        switch (command)
        {
            case CMD_IDENTIFY_DEVICE: // IDENTIFY DEVICE
                CreateIdentityBlock(1986, 16, 63);

                // Set the status register to indicate device ready
                _statusRegister = STATUS_DRDY | STATUS_DSC | STATUS_DRQ;
                _dataIndex = 0; // Reset data index for reading
                break;
            case CMD_SET_FEATURES: // SET FEATURES
                switch (_featuresRegister)
                {
                    case 0x01: // Enable write cache, 8 bit
                    case 0x03: // Disable read look-ahead, 8 bit
                        break;
                    default:
                        _statusRegister |= STATUS_ERR; // Set error bit for unsupported features
                        _errorRegister |= ERROR_ABRT; // Unsupported feature
                        break;
                }
                break;
            default:
                // Set error bit for unsupported commands
                _statusRegister &= unchecked((byte)~STATUS_DF); // Clear device fault
                _errorRegister = ERROR_ABRT;
                break;
        }

        _statusRegister &= unchecked((byte)~STATUS_DF); // Clear device fault
        _errorRegister = 0x00;
    }

    private void CreateIdentityBlock(ushort cylinders, ushort heads, ushort sectorsPerTrack)
    {
        var sectors = cylinders * heads * sectorsPerTrack;
        Array.Clear(_data, 0, _data.Length);
        var words = MemoryMarshal.Cast<byte, ushort>(_data.AsSpan());
        words[0] = 0x848A; // General Configuration
        words[1] = cylinders; // Number of Cylinders
        words[2] = 0; // Word 2
        words[3] = heads; // Number of Heads
        words[4] = 0; // Word 4
        words[5] = 576; // Word 5
        words[6] = sectorsPerTrack; // Sectors per Track
        words[7] = (ushort)(sectors >> 16); // Word 7
        words[8] = (ushort)sectors; // Word 8
        words[9] = 0; // Word 9
        Encoding.ASCII.GetBytes("1060512B03M93035".PadLeft(20, ' '), 0, 20, _data, 2 * 10); // Serial Number
        words[20] = 2; // Word 20
        words[21] = 2; // Word 21
        words[22] = 4; // Word 22
        Encoding.ASCII.GetBytes("DH X.423".PadRight(8, ' '), 0, 8, _data, 2 * 23); // Firmware Revision
        Encoding.ASCII.GetBytes("aSDnsi kDSFC-J0142".PadRight(40, ' '), 0, 40, _data, 2 * 27); // Model Number
        words[47] = 4; // Max Sectors per Interrupt
        words[48] = 0; // Word 48
        words[49] = 768; // LBA size (in sectors)
        words[50] = 0; // Word 50
        words[51] = 512; // PIO cycle time
        words[52] = 0; // Word 52
        words[53] = 3; // Geometry words
        words[54] = cylinders; // Word 54
        words[55] = heads; // Word 55
        words[56] = sectorsPerTrack; // Word 56
        words[57] = (ushort)sectors; // Word 57
        words[58] = (ushort)(sectors >> 16); // Word 58
        words[59] = 256; // Word 59
        words[60] = (ushort)sectors; // Word 60
        words[61] = (ushort)(sectors >> 16); // Word 61
        words[62] = 0; // Word 62
        words[63] = 7; // Word 63
        words[64] = 3; // Word 64
        words[65] = 120; // Word 65
        words[66] = 120; // Word 66
        words[67] = 120; // Word 67
        words[68] = 120; // Word 68
        words[69] = 0; // Word 69
        words[70] = 0; // Word 70
        words[71] = 0; // Word 71
        words[72] = 0; // Word 72
        words[73] = 0; // Word 73
        words[74] = 0; // Word 74
        words[75] = 0; // Word 75
        words[76] = 0; // Word 76
        words[77] = 0; // Word 77
        words[78] = 0; // Word 78
        words[79] = 0; // Word 79
        words[80] = 16; // Word 80
        words[81] = 0; // Word 81
        words[82] = 0; // Word 82
        words[83] = 16388; // Word 83
        words[84] = 16384; // Word 84
        words[85] = 0; // Word 85
        words[86] = 4; // Word 86
        words[87] = 16384; // Word 87
        words[88] = 0; // Word 88
        words[89] = 0; // Word 89
    }

    private void DumpHex(byte[] data)
    {
        const int bytesPerLine = 16;
        for (int i = 0; i < data.Length; i += bytesPerLine)
        {
            var hex = new StringBuilder();
            var ascii = new StringBuilder();
            for (int j = 0; j < bytesPerLine; j++)
            {
                if (i + j < data.Length)
                {
                    byte b = data[i + j];
                    hex.AppendFormat("{0:X2} ", b);
                    ascii.Append(b >= 32 && b <= 126 ? (char)b : '.');
                }
                else
                {
                    hex.Append("   ");
                    ascii.Append(' ');
                }
            }
            Debug.WriteLine($"{i:X4}: {hex} {ascii}");
        }
    }
}
