using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace DSRAppearancePresetTool
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
        static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern int CloseHandle(IntPtr hProcess);

        [DllImport("kernel32.dll")]
        static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION64 lpBuffer, uint dwLength);

        private static IntPtr GetCharDataAddress()
        {
            Process process = Process.GetProcessesByName("DarkSoulsRemastered").FirstOrDefault();

            if(process == null)
            {
                throw new Exception("Failed to get Dark Souls Remastered process!");
            } //if

            IntPtr processHandle = OpenProcess(ProcessAccessFlags.All, false, process.Id);
            IntPtr moduleAddress = IntPtr.Zero;
            long moduleSize = 0;

            foreach(ProcessModule m in process.Modules)
            { 
                if(m.ModuleName == "DarkSoulsRemastered.exe")
                {
                    moduleAddress = m.BaseAddress;
                    moduleSize = m.ModuleMemorySize;
                    break;
                } //if
            } //foreach

            IntPtr charDataAddress = IntPtr.Zero;

            long searchStart = (long)moduleAddress;
            long searchEnd = searchStart + moduleSize;

            IntPtr address = (IntPtr)searchStart;
            IntPtr bytesRead;
            byte[] buffer;

            string searchString = "48 8B 05 ? ? ? ? 45 33 ED 48 8B F1 48 85 C0";
            byte[] searchBytes;
            string[] searchMask;

            SearchStringToValues(searchString, out searchBytes, out searchMask);

            do
            {
                MEMORY_BASIC_INFORMATION64 m;
                int result = VirtualQueryEx(processHandle, address, out m, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION64)));

                buffer = new byte[m.RegionSize];
                ReadProcessMemory(processHandle, address, buffer, buffer.Length, out bytesRead);
                bool found = false;

                for(int i = 0; i < buffer.Length - searchBytes.Length; i++)
                {
                    found = true;

                    for(int j = 0; j < searchBytes.Length; j++)
                    {
                        if (searchMask[j] != "?")
                        {
                            if (searchBytes[j] != buffer[i + j])
                            {
                                found = false;
                                break;
                            } //if
                        } //if
                    } //for

                    if (found)
                    {
                        charDataAddress = address + i;
                        break;
                    } //if
                } //for

                if (found)
                    break;

                address = (IntPtr)((ulong)address + m.RegionSize);
            } //do
            while ((long)address < searchEnd);

            buffer = new byte[4];

            ReadProcessMemory(processHandle, charDataAddress + 3, buffer, buffer.Length, out bytesRead);
            charDataAddress = (IntPtr)(charDataAddress + BitConverter.ToInt32(buffer, 0) + 7);
            ReadProcessMemory(processHandle, charDataAddress, buffer, buffer.Length, out bytesRead);
            charDataAddress = (IntPtr)BitConverter.ToInt32(buffer, 0);

            CloseHandle(processHandle);

            return charDataAddress;
        } //ReadMemory

        private static void SearchStringToValues(string searchString, out byte[] searchBytes, out string[] searchMask)
        {
            searchMask = searchString.Split(' ');
            searchBytes = new byte[searchMask.Length];

            for(int i = 0; i < searchMask.Length; i++)
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

        public static AppearanceData ReadAppearanceData()
        {
            AppearanceData appearanceData = new AppearanceData();
            IntPtr charDataAddress = GetCharDataAddress();

            Process process = Process.GetProcessesByName("DarkSoulsRemastered").FirstOrDefault();

            if (process == null)
            {
                throw new Exception("Failed to get Dark Souls Remastered process!");
            } //if

            IntPtr processHandle = OpenProcess(ProcessAccessFlags.All, false, process.Id);
            byte[] buffer = new byte[4];
            IntPtr bytesRead;

            ReadProcessMemory(processHandle, charDataAddress + 0x10, buffer, buffer.Length, out bytesRead);

            charDataAddress = (IntPtr)BitConverter.ToInt32(buffer, 0);

            buffer = new byte[1];

            //Sex
            ReadProcessMemory(processHandle, charDataAddress + 0xca, buffer, buffer.Length, out bytesRead);
            appearanceData.sex = buffer[0];

            buffer = new byte[1];

            //Physique
            ReadProcessMemory(processHandle, charDataAddress + 0xcf, buffer, buffer.Length, out bytesRead);
            appearanceData.physique = buffer[0];

            buffer = new byte[4];

            //Hairstyle
            ReadProcessMemory(processHandle, charDataAddress + 0x354, buffer, buffer.Length, out bytesRead);
            appearanceData.hairstyle = BitConverter.ToInt32(buffer, 0);

            buffer = new byte[12];

            //Hair Color
            ReadProcessMemory(processHandle, charDataAddress + 0x4c0, buffer, buffer.Length, out bytesRead);
            appearanceData.hairColor = buffer;

            buffer = new byte[12];

            //Eye Color
            ReadProcessMemory(processHandle, charDataAddress + 0x4d0, buffer, buffer.Length, out bytesRead);
            appearanceData.eyeColor = buffer;

            buffer = new byte[50];

            //Face Data
            ReadProcessMemory(processHandle, charDataAddress + 0x4e0, buffer, buffer.Length, out bytesRead);
            appearanceData.faceData = buffer;

            buffer = new byte[50];

            //Skin Color
            ReadProcessMemory(processHandle, charDataAddress + 0x512, buffer, buffer.Length, out bytesRead);
            appearanceData.skinColor = buffer;

            CloseHandle(processHandle);

            return appearanceData;
        } //ReadCharacterData

        public static void WriteAppearanceData(AppearanceData appearanceData)
        {
            IntPtr charDataAddress = GetCharDataAddress();

            Process process = Process.GetProcessesByName("DarkSoulsRemastered").FirstOrDefault();

            if (process == null)
            {
                throw new Exception("Failed to get Dark Souls Remastered process!");
            } //if

            IntPtr processHandle = OpenProcess(ProcessAccessFlags.All, false, process.Id);
            byte[] buffer = new byte[4];
            IntPtr bytesWritten;

            ReadProcessMemory(processHandle, charDataAddress + 0x10, buffer, buffer.Length, out bytesWritten);

            charDataAddress = (IntPtr)BitConverter.ToInt32(buffer, 0);

            //Sex
            buffer = new byte[1];
            buffer[0] = appearanceData.sex;
            WriteProcessMemory(processHandle, charDataAddress + 0xca, buffer, buffer.Length, out bytesWritten);

            //Physique
            buffer[0] = appearanceData.physique;
            WriteProcessMemory(processHandle, charDataAddress + 0xcf, buffer, buffer.Length, out bytesWritten);

            //Hairstyle
            buffer = BitConverter.GetBytes(appearanceData.hairstyle);
            WriteProcessMemory(processHandle, charDataAddress + 0x354, buffer, buffer.Length, out bytesWritten);

            //Hair Color
            buffer = appearanceData.hairColor;
            WriteProcessMemory(processHandle, charDataAddress + 0x4c0, buffer, buffer.Length, out bytesWritten);

            //Eye Color
            buffer = appearanceData.eyeColor;
            WriteProcessMemory(processHandle, charDataAddress + 0x4d0, buffer, buffer.Length, out bytesWritten);

            //Face Data
            buffer = appearanceData.faceData;
            WriteProcessMemory(processHandle, charDataAddress + 0x4e0, buffer, buffer.Length, out bytesWritten);

            //Face Data
            buffer = appearanceData.skinColor;
            WriteProcessMemory(processHandle, charDataAddress + 0x512, buffer, buffer.Length, out bytesWritten);

            CloseHandle(processHandle);
        } //WriteCharacterData
    } //class
} //namespace
