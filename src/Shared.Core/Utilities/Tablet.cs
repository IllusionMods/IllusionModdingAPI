using System;
using System.Runtime.InteropServices;

namespace KKAPI
{
    /// <summary>
    /// Represents a data packet structure used for interacting with tablet hardware,
    /// containing information about button states, positional coordinates, and
    /// pressure sensitivity. This structure is primarily utilized for processing
    /// input data from digitizing devices.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Packet
    {
        public IntPtr pkContext;        		// PK_CONTEXT
        public uint pkStatus;           		// PK_STATUS
        public uint pkTime;             		// PK_TIME
        public uint pkChanged;          		// PK_CHANGED
        public uint pkSerialNumber;     		// PK_SERIAL_NUMBER
        public uint pkCursor;           		// PK_CURSOR
        public uint pkButtons;          		// PK_BUTTONS
        public int pkX;                 		// PK_X
        public int pkY;                 		// PK_Y
        public int pkZ;                 		// PK_Z
        public uint pkNormalPressure;   		// PK_NORMAL_PRESSURE
        public uint pkTangentPressure;  		// PK_TANGENT_PRESSURE
        public Orientation pkOrientation;       // PK_ORIENTATION
        public Rotation pkRotation;             // PK_ROTATION
    }

    public struct Orientation
    {
        public uint orAzimuth;
        public uint orAltitude;
        public uint orTwist;
    }

    public struct Rotation
    {
        public uint roPitch;
        public uint roRoll;
        public uint roYaw;
    }

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

        [StructLayout(LayoutKind.Sequential)]
        private struct Axis
        {
            public int axMin;
            public int axMax;
            public uint axUnits;
            public int axResolution;
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
        private float _currentPressure;
        private uint _maxPressure = 65535;
        private readonly object _lockObject = new object();
        private const int MAX_PACKETS = 128;
        private int _packetSize;
        private int _bufferSize;
        private IntPtr _packetBuffer;

        public float CurrentPressure
        {
            get
            {
                lock (_lockObject)
                {
                    return _currentPressure;
                }
            }
        }

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

        internal Tablet() { }

        /// <summary>
        /// Initializes the tablet by setting up the context and required configurations to enable digitizer input.
        /// </summary>
        /// <returns>
        /// True if the tablet is successfully initialized and ready for digitizer input;
        /// otherwise, false if initialization fails or the necessary system components are unavailable.
        /// </returns>
        public bool Initialize()
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

                context.lcPktData = PK_CONTEXT |
                                    PK_STATUS |
                                    PK_TIME |
                                    PK_CHANGED |
                                    PK_SERIAL_NUMBER |
                                    PK_CURSOR |
                                    PK_BUTTONS |
                                    PK_X |
                                    PK_Y |
                                    PK_Z |
                                    PK_NORMAL_PRESSURE |
                                    PK_TANGENT_PRESSURE |
                                    PK_ORIENTATION |
                                    PK_ROTATION;

                context.lcOptions = CXO_MESSAGES | CXO_SYSTEM;
                context.lcPktMode = 0;
                context.lcMoveMask = PK_X | PK_Y | PK_NORMAL_PRESSURE;
                context.lcPktRate = 100;

                context.lcOutOrgX = 0;
                context.lcOutOrgY = 0;
                context.lcOutExtX = 5000;
                context.lcOutExtY = 5000;

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
        /// Queries the tablet for the latest packet of data if available.
        /// </summary>
        /// <param name="data">Outputs the last received packet data if the query is successful.</param>
        /// <returns>
        /// True if a packet was successfully read; otherwise, false if no data is available
        /// or if the tablet is not initialized.
        /// </returns>
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
        /// Queries the tablet for multiple packets of data if available.
        /// </summary>
        /// <param name="data">Outputs the array of received packet data if the query is successful or null if empty.</param>
        /// <returns>
        /// True if one or more packets were successfully read; otherwise, false if no data is available
        /// or if the tablet is not initialized.
        /// </returns>
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
        /// Check if Wintab is available
        /// </summary>
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
        /// Get the default digitizing context
        /// </summary>
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
        /// Get the maximum pressure value
        /// </summary>
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
                        if (axis.axMax > 0)
                        {
                            return (uint)axis.axMax;
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