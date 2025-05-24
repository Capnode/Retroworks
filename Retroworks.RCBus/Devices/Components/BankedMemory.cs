using Konamiman.Z80dotNet;
using System;
using System.Diagnostics;
using System.Linq;

namespace Retroworks.RCBus.Devices.Components;

/// <summary>
/// Represents a banked memory implementation in which all the addresses are mapped in banks.
/// This is an extended implementation of <see cref="IMemory"/>.
/// </summary>
public class BankedMemory : IMemory
{
    const ushort Register0 = 0;
    const ushort Register1 = 1;

    public delegate byte? ReadMemory(int bank, int address);
    public delegate bool WriteMemory(int bank, int address, byte value);

    public ReadMemory? ReadByte { get; set; } = default;
    public WriteMemory? WriteByte { get; set; } = default;

    private const int MaxAddress = 0xFFFF;

    private readonly byte[] _memory;
    private readonly ushort _base;
    private readonly int _upperRam;

    public BankedMemory(int size, int bankSize, ushort port)
    {
        if (size < 1) throw new InvalidOperationException("Memory size must be greater than zero");
        Size = size;
        BankSize = bankSize;
        _base = port;

        _memory = new byte[size];
        _upperRam = size - 2 * bankSize; // 2 banks of RAM
    }

    public int Size { get; }
    public int BankSize { get; }
    public byte Bank { get; set; }

    public byte this[int address]
    {
        get
        {
            var value = ReadByte?.Invoke(Bank, address);
            if (value != null)
            {
                return (byte)value;
            }

            var mappedAddress = AddressMapper(address);
            return _memory[mappedAddress];
        }
        set
        {
            if (WriteByte?.Invoke(Bank, address, value) ?? false) return;
            var mappedAddress = AddressMapper(address);
            _memory[mappedAddress] = value;
        }
    }

    public void SetContents(int startAddress, byte[] contents, int startIndex = 0, int? length = null)
    {
        if (contents == null)
            throw new ArgumentNullException("contents");

        if (length == null)
            length = contents.Length;

        if (startIndex + length > contents.Length)
            throw new IndexOutOfRangeException("startIndex + length cannot be greater than contents.length");

        if (startIndex < 0)
            throw new IndexOutOfRangeException("startIndex cannot be negative");

        if (startAddress + length > MaxAddress + 1)
            throw new IndexOutOfRangeException("startAddress + length cannot go beyond the memory size");

        var offset = AddressMapper(0);
        Array.Copy(
            sourceArray: contents,
            sourceIndex: startIndex,
            destinationArray: _memory,
            destinationIndex: offset + startAddress,
            length: length.Value
            );
    }

    public byte[] GetContents(int startAddress, int length)
    {
        if (startAddress > MaxAddress)
            throw new IndexOutOfRangeException("startAddress cannot go beyond memory size");

        if (startAddress + length > MaxAddress + 1)
            throw new IndexOutOfRangeException("startAddress + length cannot go beyond memory size");

        if (startAddress < 0)
            throw new IndexOutOfRangeException("startAddress cannot be negative");

        var address = AddressMapper(startAddress);
        return _memory.Skip(address).Take(length).ToArray();
    }

    internal void WritePort(ushort port, byte value)
    {
        switch (port - _base)
        {
            case Register0:
            case Register1:
                Bank = (byte)(value >> 1);
                break;
            default:
                return;
        }

        //Debug.WriteLine($"Bank write {port:X2} -> {value:X2}");
    }

    private int AddressMapper(int address)
    {
        if (address < BankSize)
        {
            return address + Bank * BankSize;
        }

        return _upperRam + address;
    }
}
