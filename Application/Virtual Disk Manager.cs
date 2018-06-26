using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

/**
    Virtual Disk Functions
    https://msdn.microsoft.com/en-us/library/windows/desktop/dd323699
*/

namespace VirtualDiskManager
{
    class VHD
    {
        [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
        public static extern Int32 OpenVirtualDisk(ref STORAGE_TYPE Type, String File, ACCESS_MASK Mask, OPEN_FLAG Flag, ref OPEN_PARAMETERS Parameters, ref IntPtr Handle);



        [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
        public static extern Int32 AttachVirtualDisk(IntPtr Handle, IntPtr SecurityDescriptor, ATTACH_FLAG Flag, Int32 ProviderSpecificFlags, ref ATTACH_PARAMETERS Parameters, IntPtr Overlapped);



        [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
        public static extern Int32 DetachVirtualDisk(IntPtr Handle, DETACH_FLAG Flag, Int32 ProviderSpecificFlags);



        [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
        public static extern Int32 GetVirtualDiskPhysicalPath(IntPtr Handle, ref Int32 DiskPathSizeInBytes, StringBuilder Path);



        [DllImportAttribute("kernel32.dll", SetLastError = true)]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        public static extern Boolean CloseHandle(IntPtr Handle);



        [DllImport("kernel32.dll", EntryPoint = "AllocConsole", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int AllocConsole();



        public static Int32 OS = Int32.Parse(
                    Environment.OSVersion.Version.Major.ToString() +
                    Environment.OSVersion.Version.Minor.ToString());










        static void Install(String ProgrammFiles)
        {
            ConsoleKeyInfo input;
            bool install = false;
            
            do
            {
                input = Console.ReadKey(true);
                if (input.Key == ConsoleKey.Enter) { install = true;  break; }
            }

            while (input.Key != ConsoleKey.Escape);

            if (!install) return;

            Console.WriteLine("     Installing...                   ");

            try
            {
                removeRegistry();
                
                addRegistry(".vhd");
                if (OS > 61) addRegistry(".vhdx");

                string Application = Assembly.GetExecutingAssembly().Location;
                Directory.CreateDirectory(ProgrammFiles);
                File.Copy(Application, ProgrammFiles + "\\Virtual Disk Manager.exe", true);

                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("     Installation Successfully!                    ");
            }

            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("     Error: {0}                  ", e.Message);
            }

            input = Console.ReadKey(true);
        }










        static void uninstall(String ProgrammFiles)
        {
            ConsoleKeyInfo input;
            bool uninstall = false;

            do
            {
                input = Console.ReadKey(true);
                if (input.Key == ConsoleKey.Delete) { uninstall = true; break; }
            }

            while (input.Key != ConsoleKey.Escape);

            if (!uninstall) return;

            Console.WriteLine("     Removing...                 ");

            try
            {
                removeRegistry();

                Directory.Delete(ProgrammFiles, true);

                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("     Successfully Removed!                    ");
            }

            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("     Error: {0}                  ", e.Message);
            }

            input = Console.ReadKey(true);
        }










        static void removeRegistry()
        {
            if (Registry.ClassesRoot.OpenSubKey(".vhd", true) != null)
                Registry.ClassesRoot.DeleteSubKeyTree(".vhd");

            if (Registry.ClassesRoot.OpenSubKey(".vhdx", true) != null)
                Registry.ClassesRoot.DeleteSubKeyTree(".vhdx");

            if (Registry.ClassesRoot.OpenSubKey("vhd_auto_file", true) != null)
                Registry.ClassesRoot.DeleteSubKeyTree("vhd_auto_file");

            if (Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.vhd", true) != null)
                Registry.CurrentUser.DeleteSubKeyTree(@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.vhd");
        }










        static void addRegistry(String extension)
        {
            RegistryKey VHD = Registry.ClassesRoot.CreateSubKey(extension);

            RegistryKey DefaultIcon = VHD.CreateSubKey("DefaultIcon");
            DefaultIcon.SetValue(null, "C:\\Program Files\\Virtual Disk Manager\\Virtual Disk Manager.exe");

            RegistryKey shell = VHD.CreateSubKey("shell\\Virtual Disk Manager");
            shell.SetValue("MUIVerb", "Virtual Disk Manager");
            shell.SetValue("Position", "Bottom");
            shell.SetValue("Icon", "C:\\Program Files\\Virtual Disk Manager\\Virtual Disk Manager.exe");
            shell.SetValue("ExtendedSubCommandsKey", extension + "\\Virtual Disk Manager\\Option");

            RegistryKey command = shell.CreateSubKey("command");
            command.SetValue(null, "\"C:\\Program Files\\Virtual Disk Manager\\Virtual Disk Manager.exe\" \"%1\"");


            
            RegistryKey option = VHD.CreateSubKey("Virtual Disk Manager\\Option\\shell");

            // Attach

            RegistryKey attach = option.CreateSubKey("Attach");
            attach.SetValue("MUIVerb", "Attach");
            attach.SetValue("Icon", "C:\\Program Files\\Virtual Disk Manager\\Virtual Disk Manager.exe,1");

            RegistryKey commandAttach = attach.CreateSubKey("command");
            commandAttach.SetValue(null, "\"C:\\Program Files\\Virtual Disk Manager\\Virtual Disk Manager.exe\" \"-attach\" \"%1\"");

            // Detach

            RegistryKey detach = option.CreateSubKey("Detach");
            detach.SetValue("MUIVerb", "Detach");
            detach.SetValue("Icon", "C:\\Program Files\\Virtual Disk Manager\\Virtual Disk Manager.exe,2");
            detach.SetValue("CommandFlags", "32", RegistryValueKind.DWord);

            RegistryKey commandDetach = detach.CreateSubKey("command");
            commandDetach.SetValue(null, "\"C:\\Program Files\\Virtual Disk Manager\\Virtual Disk Manager.exe\" \"-detach\" \"%1\"");

            // Toggle

            RegistryKey toggle = option.CreateSubKey("Toggle");
            toggle.SetValue("MUIVerb", "Toggle");
            toggle.SetValue("Icon", "C:\\Program Files\\Virtual Disk Manager\\Virtual Disk Manager.exe,3");
            toggle.SetValue("CommandFlags", "32", RegistryValueKind.DWord);

            RegistryKey commandToggle = toggle.CreateSubKey("command");
            commandToggle.SetValue(null, "\"C:\\Program Files\\Virtual Disk Manager\\Virtual Disk Manager.exe\" \"-toggle\" \"%1\"");

            // Attach as Read-Only

            RegistryKey attach_r = option.CreateSubKey("Attach_R");
            attach_r.SetValue("MUIVerb", "Attach -Read Only");
            attach_r.SetValue("Icon", "C:\\Program Files\\Virtual Disk Manager\\Virtual Disk Manager.exe,1");
            // attach_r.SetValue("CommandFlags", "64", RegistryValueKind.DWord);

            RegistryKey commandAttach_r = attach_r.CreateSubKey("command");
            commandAttach_r.SetValue(null, "\"C:\\Program Files\\Virtual Disk Manager\\Virtual Disk Manager.exe\" \"-attach-r\" \"%1\"");
        }










        static void Main(string[] args)
        {

            if (args.Length == 0)
            {
                AllocConsole();
                Console.BackgroundColor = ConsoleColor.White;
                Console.Clear();
                Console.Title = "Virtual Disk Manager v" +
                    Assembly.GetExecutingAssembly().GetName().Version.Major + "." +
                    Assembly.GetExecutingAssembly().GetName().Version.Minor;
                // Console.SetBufferSize(80, 20);

                Console.WriteLine("\n");
                Console.WriteLine("     Available commands:\n");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("         -attach-r   [path]  as Read-Only");
                Console.WriteLine("         -attach     [path]");
                Console.WriteLine("         -detach     [path]");
                Console.WriteLine("         -toggle     [path]");
                Console.WriteLine("\n");

                string ProgrammFiles = Environment.ExpandEnvironmentVariables(@"%SystemDrive%\Program Files\Virtual Disk Manager");
                
                Console.Out.NewLine = "\r\r";
                Console.ForegroundColor = ConsoleColor.Blue;

                if (Directory.Exists(ProgrammFiles))
                {
                    Console.WriteLine("     Press <Delete> to uninstall");
                    uninstall(ProgrammFiles);
                }
                else
                {
                    Console.WriteLine("     Press <Enter> to install");
                    Install(ProgrammFiles);
                }

                return;
            }

            if (args.Length == 2)
            {
                string cmd = args[0];
                string path = args[1];

                if (cmd == "-attach")
                {
                    Attach(path, false);
                    return;
                }



                if (cmd == "-attach-r")
                {
                    Attach(path, true);
                    return;
                }



                if (cmd == "-detach")
                {
                    Detach(path);
                    return;
                }



                if (cmd == "-toggle")
                {
                    Toggle(path);
                    return;
                }
            }

            for (int i = 0; i < args.Length; i++)
            {
                string file = args[i];
                Toggle(file);
            }
        }










        static IntPtr Open(String File)
        {

            IntPtr Handle = IntPtr.Zero;

            try
            {
                /*
                    DeviceID    VHD
                    VendorID    VIRTUAL_STORAGE_TYPE_VENDOR_MICROSOFT
                */

                var StorageType = new STORAGE_TYPE();

                StorageType.DeviceID =
                    Path.GetExtension(File) == ".iso" ? 1 :
                    Path.GetExtension(File) == ".vhd" ? 2 :
                    Path.GetExtension(File) == ".vhdx" ? 3 : 0;

                StorageType.VendorID = new Guid("EC984AEC-A0F9-47e9-901F-71415A66345B");

                var Parameters = new OPEN_PARAMETERS();
                Parameters.Version = DISK_VERSION.VERSION_1;
                Parameters.Version1.RWDepth = 1;

                int Result = OpenVirtualDisk(ref StorageType, File, ACCESS_MASK.ACCESS_ALL, OPEN_FLAG.NONE, ref Parameters, ref Handle);
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










        public static void Attach(String File, bool RO)
        {
            IntPtr Handle = Open(File);
            AttachDisk(Handle, RO);
        }

        public static void AttachDisk(IntPtr Handle, bool RO)
        {
            try
            {
                var Parameters = new ATTACH_PARAMETERS();
                Parameters.Version = ATTACH_VERSION.VERSION_1;

                var Flag = RO ? ATTACH_FLAG.PERMANENT_LIFETIME | ATTACH_FLAG.READ_ONLY : ATTACH_FLAG.PERMANENT_LIFETIME;

                int Result = AttachVirtualDisk(Handle, IntPtr.Zero, Flag, 0, ref Parameters, IntPtr.Zero);
                if (Result != 0)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Error {0}.", Result));
                }
            }

            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}\n{1}", e.Message, e.StackTrace);
            }

            CloseHandle(Handle);
        }










        public static void Detach(String File)
        {
            IntPtr Handle = Open(File);
            DetachDisk(Handle);
        }

        public static void DetachDisk(IntPtr Handle)
        {
            try
            {
                int Result = DetachVirtualDisk(Handle, DETACH_FLAG.NONE, 0);
                if (Result != 0)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Error {0}.", Result));
                }
            }

            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}\n{1}", e.Message, e.StackTrace);
            }

            CloseHandle(Handle);
        }










        public static void Toggle(String File)
        {
            IntPtr Handle = Open(File);

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
                }

                else if (Result == 55) // is not available
                {
                    AttachDisk(Handle, false);
                }

                else {
                    throw new Win32Exception(Result);
                }
            }

            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}\n{1}", e.Message, e.StackTrace);
                CloseHandle(Handle);
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