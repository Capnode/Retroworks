# Retroworks RCBus requirements

## Scope

This document describes the observable behavior of the RCBus desktop application and its bundled emulated hardware. A reimplementation may use any language or UI framework. Names of libraries and implementation classes are not requirements; byte-level protocols, hardware behavior, user workflows, and persisted settings are. Where the current implementation is incomplete, the limitation is recorded rather than silently specifying a new behavior.

## Application and navigation

- The application shall provide three navigable views: serial terminal, RCBus emulator, and settings/about. No view is selected on first launch; only the subsequently selected view is visible. Selecting an already active serial or emulator view toggles its configuration panel; the terminal occupies the remaining width. Each configuration panel can also be resized, with separate persisted widths (360 by default).
- On startup, the saved theme shall be applied before the main UI appears. Light and dark themes are selectable; without a saved theme, the platform default is used. The settings view shall display application title, version, author, copyright, description, and icon credit from application metadata.
- Reset settings shall clear saved application settings and request the platform-default theme. It does not erase emulator disk images or program files.
- The desktop application shall use a writable per-user `Retroworks.RCBus` application-data directory for its settings and daily rolling log. Unhandled exceptions shall be logged; on Windows the current crash-report path launches a separate window showing cause, exception type, message, stack trace, and an OK action. The launcher constructs an `.exe` path from the assembly path, so the crash window is not verified on macOS; packaging support must not be taken as proof that it works there.

## Distribution and platform support

- The desktop project targets Windows x86/x64 and macOS Intel (`osx-x64`) and Apple Silicon (`osx-arm64`). A macOS build shall provide a native-architecture `.app` bundle inside an architecture-specific DMG, alongside an Applications-folder shortcut. The declared minimum macOS version is 10.15. These are build/package targets, not evidence that all runtime paths have been exercised on each platform.
- The macOS packaging workflow publishes a self-contained Release build and names each output `Retroworks-RCBus-<version>-<runtime>.dmg`, for example `Retroworks-RCBus-0.1.0-osx-arm64.dmg`. The convenience build for both architectures produces two separate DMGs, not a single universal app binary. The bundle executable is `Retroworks.RCBus`; the application identifier is `com.capnode.retroworks.rcbus`, and the package version is `0.1.0` (distinct from the About page's displayed assembly version `0.1.0.0`). A replacement need not use the same build tools but shall preserve the architecture-specific installable application and version/identity information when reproducing this distribution workflow.
- The project declares macOS serial-device and USB entitlements, but the DMG script does not sign or notarize its output. Do not assume that declaring entitlements grants device access in an unsigned package. The current macOS packaging path does not establish working file-type associations or additional privacy permissions; a separate plist template exists but is not used by the packaging script.

### UI state transitions

| Starting state / action | Result |
| --- | --- |
| New process, before choosing a page | Navigation rail visible; no content page selected; serial and emulator power off; both configuration panels initially expanded. |
| Choose Serial, Emulator, or Settings | Only that page is visible; the other connections are not implicitly stopped. |
| Choose the already selected Serial or Emulator page | Toggle that page's configuration panel between its saved width and zero; do not toggle power. |
| Choose an unselected page, then return to a connection page | Preserve the connection and that page's terminal and panel expansion state. |
| Serial Power with no port or no saved configuration for the selected port | No connection, no new powered state. |
| Serial Power with configured port / Power again | Open port and create a fresh terminal / close port, turn off indicator, clear terminal. |
| Emulator Power with no profile / selected profile | Do nothing / create a fresh terminal and start the selected machine asynchronously. |
| Emulator Power while running / CPU completes | Request stop and clear terminal / turn off indicator, leave completion message and terminal visible. |
| Settings Reset | Clear stored keys and request the platform theme; existing program and disk files remain untouched. |

The navigation controls are icon buttons with tooltips; serial and emulator pages have a power button, selection controls, a resizable left panel, and a terminal to the right. Emulator inputs are switches below their LEDs, with output LEDs above. Settings has `Settings` (reset and theme) and `About` (metadata and icon credit) tabs. The desktop window title is `Retroworks RCBus`. Current About values are title `Retroworks.RCBus`, displayed assembly version `0.1.0.0`, author/company `Capnode AB`, copyright notice U+00A9 followed by ` 2025`, description `RCBus toolkit with VT100 terminal for emulator and serial port.`, and credit `Retro icons created by Darius Dan - Flaticon.com`. These are functional/layout requirements, not a promise of pixel-identical fonts or licensed icon assets.

## Serial connection

- The serial view shall enumerate available operating-system ports on entry and refresh the list when the port selector opens. It shall allow selection of a port, baud rate (110, 300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 38400, 57600, 115200, 230400, 460800, 921600), 7 or 8 data bits, parity, stop bits, flow control, and transmission delays in milliseconds per character and per carriage return.
- Serial settings shall be persisted separately for each port, together with the last selected port. An unconfigured port cannot be powered on. While connected, port and framing selectors are disabled. Power toggles opening and closing the port and clears the terminal on disconnect.
- The opened port shall use the chosen framing/handshake, enabled RTS and DTR, CRLF as its newline, 4096-byte read/write buffers, and 1500 ms read/write timeouts. Incoming bytes shall be forwarded to the terminal unchanged. Outgoing bytes shall be sent in order, either as one write when no delay applies or byte by byte with the configured character delay, substituting the line delay after CR (`0x0D`).

## Settings persistence

- Settings shall include theme, selected serial port, per-port serial configuration, selected emulator profile, per-profile program and CompactFlash paths, per-profile digital input byte, and both panel widths. Per-port serial configuration is stored as a JSON object with `PortName`, `Speed`, `Parity`, `DataBit`, `StopBit`, `Handshake`, `TxCharDelay`, and `TxLineDelay` fields.
- A profile's program and CompactFlash file selectors shall accept one local file each and display the selected basename while retaining the full path. Settings changes shall survive application restart.

### Persisted keys and values

| Key | Value / fallback |
| --- | --- |
| `Theme` | `Light` or `Dark`; absent means platform theme. |
| `SerialPort` | Last selected OS port name; absent means no selection. |
| `<port name>` | JSON with fields `PortName`, `Speed`, `Parity`, `DataBit`, `StopBit`, `Handshake`, `TxCharDelay`, `TxLineDelay`; values are port name and numeric settings. |
| `DeviceName` | Last selected machine profile; absent means no selection. |
| `<profile>:ProgramPath`, `<profile>:CfPath` | Local paths; absent means no file. |
| `<profile>:InPort` | Decimal string from `0` to `255`; absent is interpreted as `0` when selecting the profile. |
| `SerialPanelWidth`, `EmulatorPanelWidth` | Width in UI units; absent or invalid values fall back to `360`. |

Selecting a port with no saved JSON clears its displayed serial parameters to zero/enum defaults; the UI does not automatically apply the model's standalone 9600/8/N/1 defaults. Changing a serial parameter saves its port JSON immediately. Refreshing to an empty OS port list clears the current UI port selection. Input switch changes, theme changes, file choices and panel resizing are saved when made. Reset clears persisted values but does not automatically reload already visible selections and fields; a restart restores their absent-key defaults. A reimplementation need not use `user.config`, but must preserve these logical keys and round-trip behaviors if importing existing settings.

For compatibility with the existing serialized form, enum fields use numeric values: parity `None=0`, `Odd=1`, `Even=2`, `Mark=3`, `Space=4`; stop bits `None=0`, `One=1`, `Two=2`, `OnePointFive=3`; handshake `None=0`, `XOnXOff=1`, `RequestToSend=2`, `RequestToSendXOnXOff=3`. For example, a 9600/8/N/1 port with no handshake or transmit delays may be stored under its port-name key as:

```json
{"PortName":"COM7","Speed":9600,"Parity":0,"DataBit":8,"StopBit":1,"Handshake":0,"TxCharDelay":0,"TxLineDelay":0}
```

JSON property order and whitespace are not meaningful. The serial-port implementation sets RTS and DTR even when the selected handshake is `None`; it uses CRLF for the port's `NewLine` setting, while terminal keystrokes such as Enter still transmit the single byte `0D`.

## Terminal interaction

- Both connection views shall have a separate resizable, scrollable VT100/XTerm-style terminal. Incoming connection bytes shall be parsed incrementally, including escape/control sequences split across reads. The screen shall handle cursor movement, scrolling/history, character sets, text attributes (including 16-color and RGB colors, bold, underline, blink, conceal, reverse video), and double-width/double-height rows. Terminal-generated responses and keyboard sequences shall be sent back through the active connection. The portable acceptance subset and byte sequences are specified below; full compatibility with every XTerm extension is not established by this document.
- A new connection shall create a new terminal. Its default colors are black text on white in the light theme and green text on black otherwise. Terminal resizing shall resize its character grid based on the available area and monospaced glyph dimensions; there is currently no serial or emulator-side window-size protocol.
- Keyboard input shall be translated according to the terminal's active cursor-key mode. Mouse tracking shall forward pointer events when enabled by the terminal protocol; otherwise, left-button drag selects text and copies it to the clipboard and right click pastes clipboard text as UTF-8 bytes. Wheel scrolling navigates terminal history; Ctrl+wheel changes terminal font size, clamped to 2-20. The cursor shall reflect focus and blink. Ctrl+F10/F11/F12 toggle sequence, view, and terminal debugging respectively.

### Portable terminal acceptance subset

Here `ESC` means byte `1B`, `CSI` means `ESC [` (`1B 5B`), and terminal input and output are byte streams, not lines. Process partial escape sequences across incoming chunks. The following is the minimum independently testable subset, not a claim that every XTerm sequence is covered:

| Input / action | Expected effect or outbound bytes |
| --- | --- |
| `41 42 43` (`ABC`) | Display `ABC` in successive cells. |
| `ESC [ 2 J` (`1B 5B 32 4A`) | Clear displayed screen; subsequent printable bytes render on the cleared screen. |
| `ESC [ 2 ; 3 H` | Place cursor at row 2, column 3 (one-based); next printed character occupies that cell. |
| `ESC [ 31 m` then `41`, then `ESC [ 0 m` | Render `A` in red, reset following text to default attributes. |
| `ESC [ ? 1 h` then Up key | Enable application cursor-key mode; send `1B 4F 41` instead of normal `1B 5B 41`. `ESC [ ? 1 l` restores normal mode. |
| Enter, Backspace, Shift+Tab, Escape, Ctrl+A | Send respectively `0D`, `08`, `1B 5B 5A`, `1B 1B`, `01`. The double-ESC mapping is the existing keyboard behavior, not a VT standard assumption. |
| F1, F5, Delete | Send respectively `1B 4F 50`, `1B 5B 31 35 7E`, `1B 5B 33 7E`. |
| Paste the string `A` followed by U+00E9 | Send UTF-8 bytes `41 C3 A9`; do not translate pasted text through the key map. |

CSI SGR attributes include foreground/background indexed colors, bright colors and 24-bit RGB, as well as bold, underline, blink, conceal and inverse. Ordinary keyboard characters are sent as UTF-8; an incoming byte stream is decoded according to the terminal's currently selected character encoding. Font size starts at 20 UI units; the user can scroll history or change font size with Ctrl+wheel without changing connection settings. Do not substitute line-oriented input for incremental escape parsing.

## Emulator lifecycle and program loading

- The emulator view shall offer profiles `SCM-F1`, `SCM-F2`, `SCM-S2`, `SCM-S3`, `SCM-S7`, and `SCM-S8`. A profile must be selected before powering on. While running, profile and file selectors are disabled. Power-on initializes the selected machine, optionally loads a program, then starts its Z80 processor on a background execution path. Power-off requests cancellation and clears the terminal; natural completion or failure reports `Execution Completed` or `Error: ...` in the terminal and turns off the power indicator. The current implementation does not clear the terminal on natural completion.
- A selected program whose filename ends in `.hex` (case insensitive) is interpreted as Intel HEX. Only record types `00` (data) and `01` (end) are processed: `00` writes each byte to the current memory mapping at the record's 16-bit address; `01` stops reading. Lines without a leading colon are ignored. Other record types are ignored; extended addresses and checksums are not interpreted or verified. No selected program means execution starts with initialized, zero-filled memory.
- Any other program filename is loaded as raw binary in file order, in 256-byte chunks, across consecutive 32 KiB banks of the 1 MiB memory. The loader stops before a chunk that would exceed memory, reports that the file is too large, restores bank zero, and reports the number of bytes actually loaded. Missing program files are reported in the terminal and do not prevent CPU start. Successful loads report the basename and loaded byte count.
- Terminal bytes sent to a running emulator shall be passed in order to its ACIA receiver; bytes transmitted by the emulated ACIA shall appear on the same terminal. A file chosen as a CompactFlash image is only used by profiles that actually attach IDE (currently `SCM-S7`).

## Emulated machine and I/O

| Profile | ACIA control/data | Digital I/O | IDE registers | ACIA interrupt source |
| --- | --- | --- | --- | --- |
| SCM-F1, SCM-F2 | `0xA2` / `0xA3` | `0xA0` | none | no |
| SCM-S2, SCM-S3, SCM-S8 | `0x80` / `0x81` | `0x00` | none | no |
| SCM-S7 | `0x80` / `0x81` | `0x00` | `0x10`-`0x17` | yes |

- All six profiles shall run a Z80-compatible processor configured for 7.3728 MHz with a 256-port I/O space, 1 MiB of memory in 32 KiB banks, and a bank control at ports `0x78` and `0x79`. The CPU begins at address `0x0000` after reset, executes standard Z80 machine instructions with their documented register, flag, stack, I/O and interrupt effects, and stops on cancellation or its normal stop conditions. Exact cycle timing relative to wall-clock time and the undocumented-instruction subset have not been verified; neither should be inferred from the nominal clock frequency.
- Addresses `0x0000`-`0x7FFF` shall select a 32 KiB bank (`bank * 0x8000 + address`). Addresses `0x8000`-`0xFFFF` shall map to the shared upper 32 KiB at physical addresses `0xF8000`-`0xFFFFF`, independent of bank selection. Writing either bank port selects `value >> 1`; memory starts at bank zero. Unhandled I/O reads yield `0xFF`. Reads from multiple devices at a port are combined by bitwise AND; writes are broadcast to mapped devices.
- The digital I/O port shall return an eight-bit input value on read and update eight output indicators on write. The UI shall offer eight input switches with red input indicators and eight green output indicators, displayed from bit 7 at the left to bit 0 at the right. Switching inputs shall update the emulated input byte and persist its decimal value for that profile; restoring that profile shall restore the input switches and byte.
- The ACIA 6850 interface shall expose status/control at its base port and receive/transmit data at base+1. A control write with low two bits set (`value & 3 == 3`) resets status; other control writes set transmitter-empty status (`0x02`). A data write transmits one byte and sets transmitter-empty. Incoming bytes queue in arrival order and set receiver-full (`0x01`); when receive interrupt is enabled by control bit 7, they also assert interrupt/status bit 7. Data reads dequeue the next byte; when the queue empties, receiver-full and interrupt are cleared. Only `SCM-S7` registers the ACIA as a CPU interrupt source. This specifies the implemented subset, not every feature of a physical 6850.
- On `SCM-S7`, an IDE device at `0x10`-`0x17` exposes the conventional data, error/features, sector count, sector number, cylinder low/high, drive/head, and status/command register offsets. Only drive 0 is connected; selecting another drive returns `0xFF` on all register reads and ignores writes except to drive/head (which permits reselecting drive 0). At initialization status is ready and seek-complete (`0x50`). The implemented `IDENTIFY DEVICE` (`0xEC`) populates a 512-byte identity block using geometry 1986 cylinders, 16 heads, 63 sectors/track and sets data-request; reading data drains that block and clears data-request. `SET FEATURES` (`0xEF`) recognizes feature values `0x01` and `0x03`. Although command constants exist for sector I/O and other ATA commands, their behavior is not implemented; do not infer functional disk reads/writes from those constants.
- For `SCM-S7`, an existing CompactFlash path is opened for read/write; a missing path is created as a 1 GiB file. No path means no backing file is opened. The image is currently not read or written by the implemented IDE command handlers.

### Byte-level machine contracts

All hex numbers below are bytes unless an address or a 16-bit word is explicitly named. A fresh machine starts with zeroed RAM and bank 0. The shared upper RAM starts at physical `0xF8000`. For example, a write of `5A` at CPU address `0x0000` in bank 0 changes physical `0x00000`; after writing `03` to I/O port `0x78`, the selected bank is 1 and a write of `A5` at `0x0000` changes physical `0x08000` without changing bank 0. A write of `66` at CPU address `0x8000` changes physical `0xF8000` and remains visible after any bank switch. Port `0x79` has the same bank-select effect as `0x78`. The `0x8000` boundary is shared RAM, not part of the selected bank. Bank 31's lower window also maps to physical `0xF8000`-`0xFFFFF`, aliasing the shared upper RAM.

For each profile's ACIA base port `P`, the following sequence is reproducible through its registers before running CPU code:

| Action | Expected status at `P`; read at `P+1` if applicable |
| --- | --- |
| Fresh ACIA | `00` |
| Write `80` to `P` (enable receive interrupt) | `02` (transmitter empty) |
| Deliver incoming bytes `41`, `42` | `83` (receiver full, transmitter empty, interrupt asserted) |
| Read `P+1` once | Returns `41`; status still `83` because `42` remains queued |
| Read `P+1` again | Returns `42`; status becomes `02`, interrupt deasserted |
| Write `55` to `P+1` | Transmit `55` to terminal; status retains transmitter-empty bit `02` |
| Write `03` to `P` | Master reset clears status to `00` |

The receiver queue is not cleared by a control-register master reset in the current implementation. An empty data read returns the last received data byte (initially `00`), not a new byte. This is an implemented subset; no ACIA framing, handshaking, baud-clock, carrier, or overrun behavior is modeled. On `SCM-S7` only, receive interrupt state is also wired into the Z80 interrupt-source interface.

For the digital port `D`, selecting a profile without a saved `<profile>:InPort` sets the visible input byte to `00`. Turning on only input switches 8 and 1 writes `81` to the input latch and persists decimal `129`; a CPU read from `D` returns `81`. A CPU write of `A5` to `D` illuminates only output LEDs 8, 6, 3 and 1 (bits 7, 5, 2 and 0). Other I/O addresses without responding devices read `FF`.

The `SCM-S7` IDE subset uses register offsets `0` data, `1` error/features, `2` sector count, `3` sector number, `4` cylinder low, `5` cylinder high, `6` drive/head and `7` status/command, relative to `0x10`. At start, reading `0x17` gives `50`. Writing `10` to drive/head `0x16` selects drive 1 and makes reads at `0x10`-`0x17` yield `FF`; writing `00` to `0x16` selects drive 0 again. Writing `EC` to `0x17` (IDENTIFY) yields status `58`, then 512 successive reads of `0x10` return the identity block and leave status `50`. A data read beyond the block repeats the last data byte; sector read (`20`) and write (`30`) commands do not transfer image data.

The IDENTIFY data is 256 little-endian 16-bit words. Initialize the 512-byte block to zero, then set the following words (indices are decimal). Values not listed remain zero:

| Word indices | Values (hex unless stated otherwise) |
| --- | --- |
| 0, 1, 3, 5, 6 | `848A`, `07C2` (1986 cylinders), `0010` (16 heads), `0240` (576), `003F` (63 sectors/track) |
| 7, 8 | `001E`, `8BE0` (total sectors `0x001E8BE0` = 2,001,888) |
| 20, 21, 22, 47 | `0002`, `0002`, `0004`, `0004` |
| 49, 51, 53 | `0300`, `0200`, `0003` |
| 54, 55, 56, 57, 58, 59, 60, 61 | `07C2`, `0010`, `003F`, `8BE0`, `001E`, `0100`, `8BE0`, `001E` |
| 63, 64, 65-68, 80 | `0007`, `0003`, `0078` for each of 65-68, `0010` |
| 83, 84, 86, 87 | `4004`, `4000`, `0004`, `4000` |

After populating the words, overwrite bytes 20-39 (words 10-19) with ASCII `    1060512B03M93035`, bytes 46-53 (words 23-26) with ASCII `DH X.423`, and bytes 54-93 (words 27-46) with ASCII `aSDnsi kDSFC-J0142` followed by spaces to 40 bytes. The first eight identity bytes must therefore be `8A 84 C2 07 00 00 10 00`, and bytes 120-123 (words 60-61) must be `E0 8B 1E 00`. This describes the existing identity reply, not a complete ATA device.

### File loading examples and boundaries

| Input to a newly initialized machine | Required effect before CPU execution |
| --- | --- |
| No program path | RAM remains zero; CPU still starts. |
| Empty `.bin` | Report `Loaded 0 bytes`; start with zero RAM and bank 0. |
| Binary with 32,769 bytes: first byte `11`, byte at offset 32,767 `22`, byte at offset 32,768 `33` | Bank 0 `0x0000` = `11`, bank 0 `0x7FFF` = `22`, bank 1 `0x0000` = `33`; report `Loaded 32769 bytes`; restore bank 0. |
| Binary of exactly 1,048,576 bytes | Load all 32 lower banks, report `Loaded 1048576 bytes`, restore bank 0; the last 32 KiB (bank 31) also overwrites shared upper RAM through its physical alias. |
| Binary of 1,048,577 bytes | Load first 1,048,576 bytes; report `File too large for memory (1048577 bytes)` and `Loaded 1048576 bytes`; restore bank 0. |
| `.hex` containing `:030000003E41C9B5` then `:00000001FF` | Write `3E 41 C9` to bank 0 CPU addresses `0000`-`0002`; report `Loaded 3 bytes`. |
| `.hex` containing `:01800000AAD5` then `:00000001FF` | Write `AA` to shared RAM CPU address `8000` (physical `0xF8000`); report `Loaded 1 bytes`. |
| `.hex` containing `:020000040001F9` then `:00000001FF` | Ignore extended-linear-address record; report `Loaded 0 bytes`. |
| Missing selected file | Report `The file '<path>' does not exist.` followed by CRLF, then start CPU anyway. |

HEX counts refer only to data-record payload bytes. Record checksums are not checked, so changing a checksum alone does not change what is written. Other record types, including extended-address records, are ignored, not applied to later data addresses. Both successful loader messages (`Loading '<basename>'` and `Loaded N bytes`) are terminated by CRLF (`0D 0A`); malformed record fields instead fault the asynchronous emulator task and generate an `Error: ...` message. The paths and filenames in these messages come from the chosen local file.

## Implementation boundaries and verification

- The requirements above describe the observable implementation, not the full RCBus hardware standard. Declared but unwired SIO, CTC, second ACIA, LED, and CompactFlash ports on other profiles are not supported hardware. Bare parameterless connection methods and direct key-event methods in the connection interface are unimplemented; interaction goes through byte-based terminal input/output.
- Preserve existing settings keys and JSON fields when interoperability with saved installations is needed. In a fresh implementation, the storage format and UI technology may differ as long as equivalent user-visible state survives restarts. The `user.config` key/value format is the current storage format, not a language requirement.
- Verify a reimplementation against the examples and the matrix below, using an isolated settings store, temporary program/CF files and a loopback or fake serial device. Do not require physical RCBus hardware. The published Zilog Z80 CPU User Manual (UM0080) and ECMA-48 control-sequence specification can be used for instruction and terminal semantics beyond the explicitly enumerated acceptance subset; neither requires access to this source repository.

### End-to-end verification matrix

| ID | Setup and stimulus | Observable expected result |
| --- | --- | --- |
| UI-01 | Fresh settings; start, open Serial, open Settings, return to Serial, select Serial again | First screen has no page content; Serial then Settings then Serial are exclusive; last selection collapses the serial panel without disconnecting or discarding its terminal. |
| UI-02 | Resize Serial panel to 420, Emulator panel to 300, restart | The two widths remain independent, 420 and 300; without stored widths each is 360. |
| UI-03 | Choose Dark; select `SCM-S2`, a program path and input switches 8 and 1; restart | Dark theme, profile, path basename/full path and input byte `81` (saved decimal `129`) return. Settings Reset then restart removes those selections and requests system theme, but leaves selected files on disk. |
| SER-01 | No port, then a port with no configuration; press Power in each case | Both remain disconnected; the port list can be refreshed by opening its selector. |
| SER-02 | Configure a test serial port to 9600, 8 data bits, no parity, one stop bit, no handshake; connect; inject `41 42` | Port opens with RTS/DTR set; `AB` appears on terminal; framing selectors cannot be changed until disconnected; disconnect clears the terminal. |
| SER-03 | Send `41 0D 42` with character delay 2 ms and line delay 7 ms | Port receives bytes in exactly that order; delay after `0D` is 7 ms, and after each other byte 2 ms. With both delays zero, one bulk write is used. This tests the requested pacing policy, not exact OS scheduler latency. |
| TERM-01 | Split `1B 5B 32 4A` across two receive chunks, then send `41`; send `1B 5B 32 3B 33 48 42` | Parser waits for the split clear-screen sequence, then displays `A` after clearing; `B` appears at row 2, column 3. |
| TERM-02 | Send `1B 5B 3F 31 68`; press Up; send `1B 5B 3F 31 6C`; press Up | Connection receives `1B 4F 41`, then `1B 5B 41`. |
| TERM-03 | Select text with pointer without mouse tracking; right-click with clipboard containing `A` and U+00E9 | Selection copies screen text; paste sends `41 C3 A9` in order. |
| CPU-01 | Load bytes `3E 41 32 00 80 F3 76` at bank 0 address `0000`; run | Z80 executes `LD A,41`, `LD (8000),A`, `DI`, `HALT`; shared RAM at physical `0xF8000` becomes `41`. With the default stop-on-DI+HALT behavior execution completes and the power indicator turns off. |
| MEM-01 | Start empty, write `5A` to CPU `0000`, write `03` to port `78`, write `A5` to CPU `0000`, write `66` to CPU `8000`; switch back to bank 0 | Bank 0 `0000` reads `5A`; bank 1 `0000` reads `A5`; either bank's `8000` reads `66`. Selecting bank 31 and reading `0000` also returns `66` because of the physical alias. |
| LOAD-01 | Load each no-file, empty, 32,769-byte, exactly-1-MiB, 1-MiB-plus-one, HEX and missing-file fixture from the table above | Assert each specified memory byte, message, loaded byte count, bank-zero restoration, and whether CPU start occurs. |
| F1-01 | Select `SCM-F1`; read unmapped port `A1`; write control `80` at `A2`, receive `41`, read `A3`, write `55` to `A3`; write output byte `A5` at `A0` | Unmapped read `FF`; ACIA status/queue follows table above and terminal receives byte `55`; DIO output LEDs 8, 6, 3, 1 turn on. No IDE or ACIA CPU interrupt source is installed. |
| F2-01 | Repeat F1-01 on `SCM-F2` | Same port map and behavior; `SCM-F2` remains a separately selectable profile. |
| S2-01 | Select `SCM-S2`; use ACIA `80`/`81` and DIO `00` | ACIA byte queue, DIO input/output and `FF` on an unmapped port behave as above; no IDE or ACIA CPU interrupt source. |
| S3-01 | Repeat S2-01 on `SCM-S3` | Same wired ports and behavior; separate profile and persisted input/file settings. |
| S7-01 | Select `SCM-S7`; use ACIA `80`/`81` and DIO `00`; inspect interrupt after enabled ACIA receive | Same byte I/O as S2 plus an asserted Z80 ACIA interrupt source; IDE responds only on `10`-`17`. |
| S7-02 | In S7, read IDE status, select drive 1 then drive 0, write `EC` to command port and read 512 data bytes | Observe `50`, then `FF` for unselected drive, then `58` after IDENTIFY; data block and final `50` match the identity contract above. No CF sector transfer occurs. |
| S8-01 | Repeat S2-01 on `SCM-S8`; probe IDE status `17` | S2-style ACIA/DIO; unmapped `17` returns `FF`, and selecting a CF path does not add IDE. |
| PKG-01 | On macOS, build once for `osx-x64` and once for `osx-arm64` | Each DMG name contains version `0.1.0` and its own runtime identifier; each contains `Retroworks RCBus.app` and an Applications shortcut. The app bundle declares minimum macOS 10.15 and executable `Retroworks.RCBus`. Running the app, device access, signing, and the crash window require separate on-device verification. |

Application failures are observable but do not imply recovery semantics that are not implemented: a serial-port open exception is not handled inside the serial page; an emulator worker fault is reported as `Error: ...` on its terminal and turns off its indicator. Manually stopping an emulator requests cancellation; its task may then report `Execution Completed` as it exits. A serial port's `Closed` event is not raised by this implementation, so automatic serial-disconnect indication must not be assumed.

These fixtures define a self-contained, testable core, not a proof of pixel-identical UI or complete XTerm/Z80/ATA compatibility. Additional escape sequences, undocumented Z80 behavior, full ATA sector commands, platform serial-driver behavior, precise visual assets and all failure races are not specified by the existing program's requirements. Do not silently treat them as covered by a passing matrix.