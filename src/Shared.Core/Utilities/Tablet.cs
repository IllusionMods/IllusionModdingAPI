using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace KKAPI
{
#pragma warning disable CS1591
    /// <summary>
    /// Represents a Wintab packet containing tablet input data such as position, pressure, and orientation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This structure maps directly to the Wintab PACKET structure and must maintain sequential layout
    /// with single-byte packing for correct COM interop with the Wintab driver.
    /// </para>
    /// <para>
    /// The fields included in this packet are determined by the <c>lcPktData</c> mask specified when
    /// opening the tablet context. Only fields corresponding to bits set in that mask will contain
    /// valid data.
    /// </para>
    /// </remarks>
    /// <seealso href="https://developer-docs.wacom.com/intuos-cintiq-business-702/docs/wintab-packet"/>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Packet
    {
        /// <summary>
        /// Handle to the tablet context that generated this packet.
        /// </summary>
        public IntPtr Context;

        /// <summary>
        /// Status flags indicating the current state of the cursor relative to the context.
        /// </summary>
        /// <remarks>
        /// Bit 0 (TPS_PROXIMITY): Set when the cursor is within the context's input area.
        /// Bit 1 (TPS_QUEUE_ERR): Set if packets were lost due to queue overflow.
        /// Bit 2 (TPS_MARGIN): Set when the cursor is in the context's margin area.
        /// Bit 3 (TPS_GRAB): Set when the context has grabbed input exclusively.
        /// Bit 4 (TPS_INVERT): Set when the cursor is inverted (e.g., eraser end of stylus).
        /// </remarks>
        public uint Status;

        /// <summary>
        /// Timestamp of the packet in milliseconds, relative to Windows system time.
        /// </summary>
        public uint Time;

        /// <summary>
        /// Bitmask indicating which packet fields have changed since the previous packet.
        /// </summary>
        /// <remarks>
        /// Each bit corresponds to a PK_* field constant. This is useful for efficient
        /// change detection without comparing all field values.
        /// </remarks>
        public uint Changed;

        /// <summary>
        /// Unique serial number of the physical device generating the packet.
        /// </summary>
        /// <remarks>
        /// This value uniquely identifies the physical tool (stylus, airbrush, etc.) and
        /// persists across sessions, allowing applications to associate settings with specific tools.
        /// </remarks>
        public uint SerialNumber;

        /// <summary>
        /// Index of the cursor type currently in use.
        /// </summary>
        /// <remarks>
        /// This identifies which cursor (stylus tip, eraser, airbrush, etc.) generated
        /// the packet. Use <c>WTInfo</c> with <c>WTI_CURSORS</c> to query cursor capabilities.
        /// </remarks>
        public uint Cursor;

        /// <summary>
        /// Button state bitmask for the current cursor.
        /// </summary>
        /// <remarks>
        /// Each bit represents a button's state. The meaning of each bit depends on the
        /// cursor type and button mapping configuration.
        /// </remarks>
        public uint Buttons;

        /// <summary>
        /// X coordinate in tablet units.
        /// </summary>
        /// <remarks>
        /// The coordinate space is defined by the context's input area (<c>lcInOrgX</c>/<c>lcInExtX</c>)
        /// and may be scaled to output coordinates based on the context's output area settings.
        /// </remarks>
        public int X;

        /// <summary>
        /// Y coordinate in tablet units.
        /// </summary>
        /// <remarks>
        /// The coordinate space is defined by the context's input area (<c>lcInOrgY</c>/<c>lcInExtY</c>)
        /// and may be scaled to output coordinates based on the context's output area settings.
        /// </remarks>
        public int Y;

        /// <summary>
        /// Z coordinate (height above tablet surface) in tablet units.
        /// </summary>
        /// <remarks>
        /// Not all tablets support Z-axis input. Check device capabilities before relying on this value.
        /// </remarks>
        public int Z;

        /// <summary>
        /// Tip pressure value, typically ranging from 0 to the device's maximum pressure level.
        /// </summary>
        /// <remarks>
        /// Query <c>WTI_DEVICES</c> with <c>DVC_NPRESSURE</c> to determine the device's pressure range.
        /// A value of 0 typically indicates the stylus is hovering (not touching the surface).
        /// </remarks>
        public uint NormalPressure;

        /// <summary>
        /// Tangential (barrel) pressure for devices that support it, such as airbrushes.
        /// </summary>
        /// <remarks>
        /// This value represents the finger wheel position on airbrush-style devices.
        /// Query <c>WTI_DEVICES</c> with <c>DVC_TPRESSURE</c> to determine availability and range.
        /// </remarks>
        public float NormalizedPressure;

        /// <summary>
        /// Orientation of the cursor in 3D space (azimuth, altitude, and twist).
        /// </summary>
        /// <seealso cref="Orientation"/>
        public Orientation Orientation;

        /// <summary>
        /// Rotation data for devices that support full 3D rotation tracking.
        /// </summary>
        /// <remarks>
        /// This is typically used by 3D input devices rather than standard styluses.
        /// </remarks>
        /// <seealso cref="Rotation"/>
        public Rotation Rotation;
    }

    /// <summary>
    /// Represents the 3D orientation of a stylus or cursor relative to the tablet surface.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Orientation data is commonly used for brush dynamics in drawing applications,
    /// allowing stroke width or opacity to vary based on pen tilt and rotation.
    /// </para>
    /// <para>
    /// Query <c>WTI_DEVICES</c> with <c>DVC_ORIENTATION</c> to determine the range
    /// and resolution of each axis for the connected device.
    /// </para>
    /// </remarks>
    public struct Orientation
    {
        /// <summary>
        /// Clockwise rotation of the cursor around the vertical axis, measured from the positive Y axis.
        /// </summary>
        /// <remarks>
        /// Typically reported in tenths of a degree, ranging from 0 to 3600.
        /// </remarks>
        public uint Azimuth;

        /// <summary>
        /// Angle of the cursor relative to the tablet surface.
        /// </summary>
        /// <remarks>
        /// A value representing perpendicular to the surface indicates the pen is upright.
        /// Lower values indicate increasing tilt. Typically reported in tenths of a degree.
        /// </remarks>
        public uint Altitude;

        /// <summary>
        /// Clockwise rotation of the cursor around its own axis.
        /// </summary>
        /// <remarks>
        /// Represents barrel rotation for styluses that support it. Typically reported
        /// in tenths of a degree, ranging from 0 to 3600.
        /// </remarks>
        public uint Twist;
    }

    /// <summary>
    /// Represents the 3D rotation of a tablet input device.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This structure is used by devices that support full rotational tracking, such as
    /// 3D mice or specialized input devices. Standard styluses typically use
    /// <see cref="Orientation"/> instead.
    /// </para>
    /// <para>
    /// Values are reported in device-specific units. Query <c>WTI_DEVICES</c> with
    /// <c>DVC_ROTATION</c> to determine the range and resolution of each axis.
    /// </para>
    /// </remarks>
    public struct Rotation
    {
        /// <summary>
        /// Rotation around the lateral (side-to-side) axis.
        /// </summary>
        public uint Pitch;

        /// <summary>
        /// Rotation around the longitudinal (front-to-back) axis.
        /// </summary>
        public uint Roll;

        /// <summary>
        /// Rotation around the vertical axis.
        /// </summary>
        public uint Yaw;
    }

    public struct lcOut
    {
        public int xOrg;
        public int yOrg;
        public int xExt;
        public int yExt;

        public lcOut(int xOrg, int yOrg, int xExt, int yExt)
        {
            this.xOrg = xOrg;
            this.yOrg = yOrg;
            this.xExt = xExt;
            this.yExt = yExt;
        }
    }

#pragma warning restore CS1591

    /// <summary>
    /// Represents a tablet input device that utilizes WinTabAPI for digitizer integration.
    /// Provides functionality to interact with and manage the tablet state, including
    /// pressure sensitivity and packet data processing.
    /// </summary>
    internal class Tablet : IDisposable
    {
        #region Wintab P/Invoke Declarations

        [DllImport("Wintab32.dll", CharSet = CharSet.Auto)]
        private static extern bool WTInfoA(uint wCategory, uint nIndex, IntPtr lpOutput);

        [DllImport("Wintab32.dll", CharSet = CharSet.Auto)]
        private static extern uint WTInfoA(uint wCategory, uint nIndex, byte[] lpOutput);

        [DllImport("Wintab32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr WTOpenA(IntPtr hWnd, ref LogContext lpLogCtx, bool fEnable);

        [DllImport("Wintab32.dll", CharSet = CharSet.Auto)]
        private static extern bool WTClose(IntPtr hCtx);

        [DllImport("Wintab32.dll", CharSet = CharSet.Auto)]
        private static extern int WTPacketsGet(IntPtr hCtx, int cMaxPkts, IntPtr lpPkts);

        [DllImport("Wintab32.dll", CharSet = CharSet.Auto)]
        private static extern bool WTPacket(IntPtr hCtx, uint wSerial, IntPtr lpPkt);

        [DllImport("Wintab32.dll", CharSet = CharSet.Auto)]
        private static extern int WTQueueSizeGet(IntPtr hCtx);

        [DllImport("Wintab32.dll", CharSet = CharSet.Auto)]
        private static extern bool WTQueueSizeSet(IntPtr hCtx, int nPkts);

        [DllImport("Wintab32.dll", CharSet = CharSet.Auto)]
        private static extern bool WTGetA(IntPtr hCtx, ref LogContext lpLogCtx);

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();

        #region Constants

        private const uint WTI_INTERFACE = 1;
        private const uint IFC_SPECVERSION = 1;
        private const uint WTI_DEFCONTEXT = 3;
        private const uint WTI_DEFSYSCTX = 4;
        private const uint WTI_DEVICES = 100;
        private const uint DVC_NPRESSURE = 15;
        private const uint WTI_DDCTXS = 400;

        private const uint CXO_SYSTEM = 0x0001;
        private const uint CXO_PEN = 0x0002;
        private const uint CXO_MESSAGES = 0x0004;


        private const uint PK_CONTEXT = 0x0001;
        private const uint PK_STATUS = 0x0002;
        private const uint PK_TIME = 0x0004;
        private const uint PK_CHANGED = 0x0008;
        private const uint PK_SERIAL_NUMBER = 0x0010;
        private const uint PK_CURSOR = 0x0020;
        private const uint PK_BUTTONS = 0x0040;
        private const uint PK_X = 0x0080;
        private const uint PK_Y = 0x0100;
        private const uint PK_Z = 0x0200;
        private const uint PK_NORMAL_PRESSURE = 0x0400;
        private const uint PK_TANGENT_PRESSURE = 0x0800;
        private const uint PK_ORIENTATION = 0x1000;
        private const uint PK_ROTATION = 0x2000;

        #endregion

        #region Structs

        /// <summary>
        /// Describes the range and characteristics of a tablet axis.
        /// </summary>
        /// <remarks>
        /// This structure maps to the Wintab AXIS structure, used to query device capabilities
        /// such as pressure range, coordinate extents, and tilt limits via <c>WTInfo</c>.
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        private struct Axis
        {
            /// <summary>
            /// Minimum value reported on this axis.
            /// </summary>
            public int Min;

            /// <summary>
            /// Maximum value reported on this axis.
            /// </summary>
            public int Max;

            /// <summary>
            /// Physical units of measurement for this axis.
            /// </summary>
            /// <remarks>
            /// Common values: <c>TU_NONE</c> (0), <c>TU_INCHES</c> (1), <c>TU_CENTIMETERS</c> (2), <c>TU_CIRCLE</c> (3).
            /// </remarks>
            public uint Units;

            /// <summary>
            /// Axis resolution in lines per physical unit (as specified by <see cref="Units"/>).
            /// </summary>
            /// <remarks>
            /// For dimensionless axes (e.g., pressure), this value indicates the number of discrete levels.
            /// </remarks>
            public int Resolution;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private struct LogContext
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
            public string lcName;
            public uint lcOptions;
            public uint lcStatus;
            public uint lcLocks;
            public uint lcMsgBase;
            public uint lcDevice;
            public uint lcPktRate;
            public uint lcPktData;
            public uint lcPktMode;
            public uint lcMoveMask;
            public int lcBtnDnMask;
            public int lcBtnUpMask;
            public int lcInOrgX;
            public int lcInOrgY;
            public int lcInOrgZ;
            public int lcInExtX;
            public int lcInExtY;
            public int lcInExtZ;
            public int lcOutOrgX;
            public int lcOutOrgY;
            public int lcOutOrgZ;
            public int lcOutExtX;
            public int lcOutExtY;
            public int lcOutExtZ;
            public int lcSensX;
            public int lcSensY;
            public int lcSensZ;
            public bool lcSysMode;
            public int lcSysOrgX;
            public int lcSysOrgY;
            public int lcSysExtX;
            public int lcSysExtY;
            public int lcSysSensX;
            public int lcSysSensY;
        }

        #endregion

        #endregion

        #region Properties

        private IntPtr _context = IntPtr.Zero;
        private bool _isInitialized;
        private uint _maxPressure = 65535;
        private readonly object _lockObject = new object();
        private const int MAX_PACKETS = 128;
        private int _packetSize;
        private int _bufferSize;
        private IntPtr _packetBuffer;

        /// <summary>
        /// Represents the maximum pressure threshold supported by the tablet's pressure-sensitive input.
        /// </summary>
        /// <remarks>
        /// This property provides the upper limit of the pressure sensitivity range for the associated
        /// tablet device. Applications can use this value to normalize or scale pressure input values,
        /// ensuring compatibility across devices with different sensitivity configurations.
        /// The value is determined by the tablet's hardware and cannot be modified programmatically.
        /// </remarks>
        public uint MaxPressure => _maxPressure;

        /// <summary>
        /// Indicates whether the tablet has been successfully initialized and is ready for use.
        /// </summary>
        /// <remarks>
        /// This property returns true if the tablet has been initialized through the <see cref="Initialize"/> method
        /// and is in a state where it can process input and interact with digitizer functionalities.
        /// If false, the tablet has not been initialized or has been disposed, and any operations relying
        /// on the tablet's functionality may fail or raise errors.
        /// </remarks>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// An event that is triggered when an exception occurs during the operation of the tablet device.
        /// </summary>
        /// <remarks>
        /// The event is invoked with an <see cref="Exception"/> object that contains details about the error.
        /// Consumers of this event can subscribe to handle errors gracefully and take appropriate actions.
        /// Common scenarios where this event may be triggered include failures during initialization,
        /// packet processing, or context configuration.
        /// </remarks>
        public event Action<Exception> OnError;

        #endregion

        /// <summary>
        /// Initializes the Wintab tablet context with the specified output coordinates and options.
        /// </summary>
        /// <param name="lcCoords">
        /// Output coordinate mapping defining the origin and extent of the tablet's output space.
        /// </param>
        /// <param name="lco">
        /// Context options bitmask. Default is <c>5</c> (<c>CXO_SYSTEM | CXO_MESSAGES</c>).
        /// </param>
        /// <param name="lcpktMode">
        /// Packet mode flags specifying which data items are reported in relative mode.
        /// Default is <c>0</c> (all absolute).
        /// </param>
        /// <param name="lcmm">
        /// Movement mask specifying which packet data items generate move messages.
        /// Default is <c>1408</c> (<c>X | Y | BUTTONS</c>).
        /// </param>
        /// <param name="lcpktRate">
        /// Desired packet report rate in packets per second. Default is <c>100</c>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the tablet context was successfully opened and initialized;
        /// <see langword="false"/> if Wintab is unavailable, context creation failed, or an error occurred.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method allocates an unmanaged packet buffer, retrieves the default digitizing context,
        /// configures it with the specified parameters, and opens the context. The context is bound
        /// to the current active or foreground window.
        /// </para>
        /// <para>
        /// The packet data mask (<c>lcPktData</c>) is set to <c>16383</c>, requesting all standard
        /// packet fields including position, pressure, orientation, and rotation.
        /// </para>
        /// <para>
        /// After opening, the method queries the actual context settings, sets the packet queue size
        /// to <see cref="MAX_PACKETS"/>, and retrieves the device's maximum pressure value for
        /// normalization.
        /// </para>
        /// <para>
        /// Call <see cref="Dispose"/> to release resources when the tablet context is no longer needed.
        /// </para>
        /// <para>
        /// Any exceptions are caught and forwarded to the <see cref="OnError"/> event handler.
        /// </para>
        /// </remarks>
        public bool Initialize(lcOut lcCoords, uint lco = 5U, uint lcpktMode = 0U, uint lcmm = 1408U, uint lcpktRate = 100U)
        {
            try
            {
                _packetSize = Marshal.SizeOf(typeof(Packet));
                _bufferSize = _packetSize * MAX_PACKETS;
                _packetBuffer = Marshal.AllocHGlobal(_bufferSize);
                if (!IsWintabAvailable())
                {
                    return false;
                }

                var logContext = GetDefaultDigitizingContext();
                if (!logContext.HasValue)
                {

                    return false;
                }

                var context = logContext.Value;

                context.lcName = "WinTabReader";

                context.lcPktData = 16383U;

                context.lcOptions = lco;
                context.lcPktMode = lcpktMode;
                context.lcMoveMask = lcmm;
                context.lcPktRate = lcpktRate;

                context.lcOutOrgX = lcCoords.xOrg;
                context.lcOutOrgY = lcCoords.yOrg;
                context.lcOutExtX = lcCoords.xExt;
                context.lcOutExtY = lcCoords.yExt;

                IntPtr hwnd = GetActiveWindow();
                if (hwnd == IntPtr.Zero)
                    hwnd = GetForegroundWindow();

                _context = WTOpenA(hwnd, ref context, true);
                if (_context == IntPtr.Zero)
                {
                    return false;
                }

                LogContext actualContext = new LogContext();
                WTGetA(_context, ref actualContext);
                WTQueueSizeSet(_context, MAX_PACKETS);

                _maxPressure = GetMaxPressure();
                if (_maxPressure == 0)
                    _maxPressure = 32767;

                _isInitialized = true;

                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                return false;
            }
        }

        /// <summary>
        /// Retrieves the most recent packet from the tablet context's queue, discarding older packets.
        /// </summary>
        /// <param name="data">
        /// When this method returns <see langword="true"/>, contains the most recent packet from
        /// the queue. When this method returns <see langword="false"/>, contains the default value.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if a packet was retrieved; <see langword="false"/> if no packets
        /// were available, the tablet is not initialized, or an error occurred.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method drains all pending packets (up to <see cref="MAX_PACKETS"/>) and returns
        /// only the last one, which represents the most current tablet state. This is useful when
        /// only the latest position and pressure matter, such as for cursor display.
        /// </para>
        /// <para>
        /// For applications that need to process every packet (e.g., for accurate stroke rendering),
        /// use <see cref="QueryMulti"/> instead.
        /// </para>
        /// <para>
        /// Any exceptions are caught and forwarded to the <see cref="OnError"/> event handler.
        /// </para>
        /// </remarks>
        public bool Query(out Packet data)
        {
            data = default;
            if (!_isInitialized || _packetBuffer == IntPtr.Zero)
                return false;

            try
            {
                int numPackets = WTPacketsGet(_context, MAX_PACKETS, _packetBuffer);

                if (numPackets > 0)
                {
                    IntPtr lastPacketPtr = new IntPtr(_packetBuffer.ToInt64() + (numPackets - 1) * _packetSize);
                    data = (Packet)Marshal.PtrToStructure(lastPacketPtr, typeof(Packet));
                    return true;
                }
            }
            catch (Exception e)
            {
                OnError?.Invoke(e);
            }

            return false;
        }

        /// <summary>
        /// Retrieves all pending packets from the tablet context's queue.
        /// </summary>
        /// <param name="data">
        /// When this method returns <see langword="true"/>, contains an array of packets retrieved
        /// from the queue, or <see langword="null"/> if no packets were available. When this method
        /// returns <see langword="false"/>, the value is <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the query succeeded (even if no packets were available);
        /// <see langword="false"/> if the tablet is not initialized or an error occurred.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method retrieves up to <see cref="MAX_PACKETS"/> packets in a single call.
        /// If no packets are pending, the method returns <see langword="true"/> with
        /// <paramref name="data"/> set to <see langword="null"/>.
        /// </para>
        /// <para>
        /// On older Unity versions (KK/PH targets), packets are copied using a manual loop due to
        /// the unavailability of <see cref="Buffer.MemoryCopy"/>. On newer targets, the native
        /// memory copy is used for better performance.
        /// </para>
        /// <para>
        /// Any exceptions are caught and forwarded to the <see cref="OnError"/> event handler.
        /// </para>
        /// </remarks>
        public unsafe bool QueryMulti(out Packet[] data)
        {
            data = null;
            if (!_isInitialized || _packetBuffer == IntPtr.Zero)
                return false;

            try
            {
                int numPackets = WTPacketsGet(_context, MAX_PACKETS, _packetBuffer);
                if (numPackets == 0) return true;

                data = new Packet[numPackets];

                fixed (Packet* dest = data)
                {
#if KK || PH
                    Packet* src = (Packet*)_packetBuffer.ToPointer();
                    for (int i = 0; i < numPackets; i++)
                    {
                        dest[i] = src[i];
                    }
#else
                    Buffer.MemoryCopy(_packetBuffer.ToPointer(), dest, numPackets * _packetSize, numPackets * _packetSize);
#endif
                }

                return true;
            }
            catch (Exception e)
            {
                OnError?.Invoke(e);
            }

            return false;
        }

        /// <summary>
        /// Checks whether the Wintab driver is installed and available on the system.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if Wintab is installed and responding; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// This method calls <c>WTInfoA</c> with category and index set to zero, which queries
        /// whether the Wintab interface is present without retrieving any specific information.
        /// A <see cref="DllNotFoundException"/> or similar exception is caught and treated as
        /// Wintab being unavailable.
        /// </remarks>
        private bool IsWintabAvailable()
        {
            try
            {
                return WTInfoA(0, 0, IntPtr.Zero);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Retrieves the default digitizing context from the Wintab driver.
        /// </summary>
        /// <returns>
        /// A <see cref="LogContext"/> containing the default context settings, or <see langword="null"/>
        /// if no context could be retrieved.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method first attempts to retrieve the default digitizing context (<c>WTI_DEFCONTEXT</c>),
        /// which is optimized for drawing applications with full tablet resolution. If that fails,
        /// it falls back to the default system context (<c>WTI_DEFSYSCTX</c>), which maps tablet
        /// coordinates to screen coordinates.
        /// </para>
        /// <para>
        /// The returned context can be modified and passed to <c>WTOpen</c> to create a tablet context
        /// tailored to the application's needs.
        /// </para>
        /// <para>
        /// Any exceptions are caught and forwarded to the <see cref="OnError"/> event handler.
        /// </para>
        /// </remarks>
        private LogContext? GetDefaultDigitizingContext()
        {
            try
            {
                var buffer = new byte[Marshal.SizeOf(typeof(LogContext))];

                uint result = WTInfoA(WTI_DEFCONTEXT, 0, buffer);

                if (result == 0)
                {
                    result = WTInfoA(WTI_DEFSYSCTX, 0, buffer);
                }

                if (result > 0)
                {
                    IntPtr ptr = Marshal.AllocHGlobal(buffer.Length);
                    try
                    {
                        Marshal.Copy(buffer, 0, ptr, buffer.Length);
                        return (LogContext)Marshal.PtrToStructure(ptr, typeof(LogContext));
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(ptr);
                    }
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
            }

            return null;
        }

        /// <summary>
        /// Queries the tablet device for its maximum tip pressure level.
        /// </summary>
        /// <returns>
        /// The maximum pressure value supported by the device, or <see cref="ushort.MaxValue"/>
        /// as a fallback if the query fails.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method queries <c>WTI_DEVICES</c> with <c>DVC_NPRESSURE</c> to retrieve the
        /// pressure axis capabilities. The returned <see cref="Axis.Max"/> value represents
        /// the maximum pressure the device can report.
        /// </para>
        /// <para>
        /// The returned value is used to normalize pressure readings from <see cref="Packet.NormalPressure"/>
        /// into a 0.0–1.0 range for application use.
        /// </para>
        /// <para>
        /// If the query fails or returns an invalid value, <see cref="ushort.MaxValue"/> (65535) is
        /// returned as a reasonable default for most modern tablets.
        /// </para>
        /// <para>
        /// Any exceptions are caught and forwarded to the <see cref="OnError"/> event handler.
        /// </para>
        /// </remarks>
        private uint GetMaxPressure()
        {
            try
            {
                var axisBuffer = new byte[Marshal.SizeOf(typeof(Axis))];
                uint result = WTInfoA(WTI_DEVICES, DVC_NPRESSURE, axisBuffer);

                if (result > 0)
                {
                    IntPtr ptr = Marshal.AllocHGlobal(axisBuffer.Length);
                    try
                    {
                        Marshal.Copy(axisBuffer, 0, ptr, axisBuffer.Length);
                        var axis = (Axis)Marshal.PtrToStructure(ptr, typeof(Axis));
                        if (axis.Max > 0)
                        {
                            return (uint)axis.Max;
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(ptr);
                    }
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
            }

            return ushort.MaxValue;
        }

        /// <summary>
        /// Releases all resources used by this tablet context.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method closes the Wintab context handle and frees the unmanaged packet buffer.
        /// After disposal, the instance cannot be reused and all query methods will return failure.
        /// </para>
        /// <para>
        /// This method is safe to call multiple times. Any exceptions from <c>WTClose</c> are
        /// silently ignored to ensure cleanup completes.
        /// </para>
        /// <para>
        /// <see cref="GC.SuppressFinalize"/> is called to prevent redundant finalization if the
        /// object was disposed explicitly.
        /// </para>
        /// </remarks>
        public void Dispose()
        {
            if (_context != IntPtr.Zero)
            {
                try
                {
                    WTClose(_context);
                }
                catch
                {
                    // Ignore
                }

                _context = IntPtr.Zero;
            }

            _isInitialized = false;
            if (_packetBuffer != IntPtr.Zero)
                Marshal.FreeHGlobal(_packetBuffer);
            GC.SuppressFinalize(this);
        }

        ~Tablet()
        {
            Dispose();
        }
    }
}