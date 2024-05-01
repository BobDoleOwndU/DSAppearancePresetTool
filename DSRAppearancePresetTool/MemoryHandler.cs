using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DSAppearancePresetTool
{
    public static class MemoryHandler
    {
        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000
        } //ProcessAccessFlags

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION64
        {
            public ulong BaseAddress;
            public ulong AllocationBase;
            public int AllocationProtect;
            public int __alignment1;
            public ulong RegionSize;
            public int State;
            public int Protect;
            public int Type;
            public int __alignment2;
        } //MEMORY_BASIC_INFORMATION64

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern int CloseHandle(IntPtr hProcess);

        [DllImport("kernel32.dll")]
        public static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION64 lpBuffer, uint dwLength);

        public static AppearanceData ReadAppearanceData()
        {
            Process[] processes = Process.GetProcesses();

            foreach (Process p in processes)
            {
                if (p.MainWindowTitle == Remastered.WindowTitle)
                {
                    Remastered.process = p;
                    return Remastered.ReadAppearanceData();
                } //if
                else if (p.MainWindowTitle == PTDE.WindowTitle)
                {
                    PTDE.process = p;
                    return PTDE.ReadAppearanceData();
                } //else if
            } //foreach

            throw new Exception("Couldn't find Dark Souls process!");
        } //ReadAppearanceData

        public static void WriteAppearanceData(AppearanceData appearanceData)
        {
            Process[] processes = Process.GetProcesses();

            foreach (Process p in processes)
            {
                if (p.MainWindowTitle == Remastered.WindowTitle)
                {
                    Remastered.process = p;
                    Remastered.WriteAppearanceData(appearanceData);
                    return;
                } //if
                else if (p.MainWindowTitle == PTDE.WindowTitle)
                {
                    PTDE.process = p;
                    PTDE.WriteAppearanceData(appearanceData);
                    return;
                } //else if
            } //foreach

            throw new Exception("Couldn't find Dark Souls process!");
        } //WriteAppearanceData

        public static void SearchStringToValues(string searchString, out byte[] searchBytes, out string[] searchMask)
        {
            searchMask = searchString.Split(' ');
            searchBytes = new byte[searchMask.Length];

            for (int i = 0; i < searchMask.Length; i++)
            {
                if (searchMask[i] == "?")
                {
                    searchBytes[i] = 0;
                } //if
                else
                {
                    searchBytes[i] = byte.Parse(searchMask[i], System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                } //else
            } //for
        } //SearchStringToValues
    } //class
} //namespace
