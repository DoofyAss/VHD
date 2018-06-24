using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

/**
    Virtual Disk Functions
    https://msdn.microsoft.com/en-us/library/windows/desktop/dd323699
*/

namespace VirtualDiskManager
{
    [Guid("27a7c1e7-1ad7-4619-92e9-3dbf756cbe75")]
    [ComVisible(true)]
    public class VHD
    {
        [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
        public static extern Int32 OpenVirtualDisk(ref STORAGE_TYPE Type, String Path, ACCESS_MASK Mask, OPEN_FLAG Flag, ref OPEN_PARAMETERS Parameters, ref IntPtr Handle);



        [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
        public static extern Int32 AttachVirtualDisk(IntPtr Handle, IntPtr SecurityDescriptor, ATTACH_FLAG Flag, Int32 ProviderSpecificFlags, ref ATTACH_PARAMETERS Parameters, IntPtr Overlapped);



        [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
        public static extern Int32 DetachVirtualDisk(IntPtr Handle, DETACH_FLAG Flag, Int32 ProviderSpecificFlags);



        [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
        public static extern Int32 GetVirtualDiskPhysicalPath(IntPtr Handle, ref Int32 DiskPathSizeInBytes, StringBuilder Path);



        [DllImportAttribute("kernel32.dll", SetLastError = true)]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        public static extern Boolean CloseHandle(IntPtr Handle);










        public static IntPtr Open(String Path)
        {

            IntPtr Handle = IntPtr.Zero;

            try
            {
                /*
                    DeviceID    VHD
                    VendorID    VIRTUAL_STORAGE_TYPE_VENDOR_MICROSOFT
                */

                var StorageType = new STORAGE_TYPE();
                StorageType.DeviceID = 2;
                StorageType.VendorID = new Guid("EC984AEC-A0F9-47e9-901F-71415A66345B");

                var Parameters = new OPEN_PARAMETERS();
                Parameters.Version = DISK_VERSION.VERSION_1;
                Parameters.Version1.RWDepth = 1;

                int Result = OpenVirtualDisk(ref StorageType, Path, ACCESS_MASK.ACCESS_ALL, OPEN_FLAG.NONE, ref Parameters, ref Handle);
                if (Result != 0)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Error {0}.", Result));
                }
            }

            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}\n{1}", e.Message, e.StackTrace);
            }

            return Handle;
        }









        [ComVisible(true)]
        public int Attach(String Path)
        {
            IntPtr Handle = Open(Path);
            return AttachDisk(Handle);
        }

        public int AttachDisk(IntPtr Handle)
        {
            try
            {
                var Parameters = new ATTACH_PARAMETERS();
                Parameters.Version = ATTACH_VERSION.VERSION_1;
                int Result = AttachVirtualDisk(Handle, IntPtr.Zero, ATTACH_FLAG.PERMANENT_LIFETIME, 0, ref Parameters, IntPtr.Zero);

                if (Result == 0)
                {
                    return 1;
                }

                if (Result != 0)
                {
                    return 0;
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Error {0}.", Result));
                }
            }

            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}\n{1}", e.Message, e.StackTrace);
                return 0;
            }

            CloseHandle(Handle);
            return 0;
        }









        [ComVisible(true)]
        public int Detach(String Path)
        {
            IntPtr Handle = Open(Path);
            return DetachDisk(Handle);
        }

        public int DetachDisk(IntPtr Handle)
        {
            try
            {
                int Result = DetachVirtualDisk(Handle, DETACH_FLAG.NONE, 0);

                if (Result == 0)
                {
                    return 1;
                }

                if (Result != 0)
                {
                    return 0;
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Error {0}.", Result));
                }
            }

            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}\n{1}", e.Message, e.StackTrace);
                return 0;
            }

            CloseHandle(Handle);
            return 0;
        }









        [ComVisible(true)]
        public int Toggle(String Path)
        {
            IntPtr Handle = Open(Path);

            try
            {
                int size = 256;
                var path = new StringBuilder(size / 2);

                int Result = GetVirtualDiskPhysicalPath(Handle, ref size, path);

                if (Result == 122)
                {
                    path.Capacity = size / 2;
                    Result = GetVirtualDiskPhysicalPath(Handle, ref size, path);
                }

                if (Result == 0)
                {
                    // path.ToString(0, size / 2 - 1)

                    DetachDisk(Handle);
                    return -1;
                }

                else if (Result == 55) // is not available
                {
                    AttachDisk(Handle);
                    return 1;
                }

                else {
                    throw new Win32Exception(Result);
                }
            }

            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}\n{1}", e.Message, e.StackTrace);
                CloseHandle(Handle);
                return 0;
            }
        }










    }

    /*
        Const
    */

    public struct STORAGE_TYPE
    {
        public Int32 DeviceID;
        public Guid VendorID;
    }

    public enum ACCESS_MASK
    {
        ATTACH_RO = 0x00010000,
        ACCESS_ALL = 0x003f0000
    }

    public enum OPEN_FLAG
    {
        NONE = 0x00000000
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct OPEN_PARAMETERS
    {
        public DISK_VERSION Version;
        public DISK_VERSION_1 Version1;
    }

    public enum DISK_VERSION
    {
        VERSION_1 = 1
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DISK_VERSION_1
    {
        public Int32 RWDepth;
    }

    public enum ATTACH_FLAG
    {
        READ_ONLY = 0x00000001,
        PERMANENT_LIFETIME = 0x00000004
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct ATTACH_PARAMETERS
    {
        public ATTACH_VERSION Version;
        public ATTACH_VERSION_1 Version1;
    }

    public enum ATTACH_VERSION
    {
        UNSPECIFIED = 0,
        VERSION_1 = 1
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct ATTACH_VERSION_1
    {
        public Int32 Reserved;
    }

    public enum DETACH_FLAG
    {
        NONE = 0x00000000
    }
}