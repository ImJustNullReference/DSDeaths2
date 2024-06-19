﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;


namespace DSDeaths
{
    //App Icon (https://icons8.com/icons/set/skull--static) from icons8.com

    class Program
    {
        const int PROCESS_WM_READ = 0x0010;
        const int PROCESS_QUERY_INFORMATION = 0x0400;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool IsWow64Process(IntPtr hProcess, ref bool Wow64Process);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(
            IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        static readonly Game[] games =
        {
            new Game("DARKSOULS", new int[] {0xF78700, 0x5C}, null),
            new Game("DarkSoulsII", new int[] {0x1150414, 0x74, 0xB8, 0x34, 0x4, 0x28C, 0x100}, new int[] {0x16148F0, 0xD0, 0x490, 0x104}),
            new Game("DarkSoulsIII", null, new int[] {0x47572B8, 0x98}),
            new Game("DarkSoulsRemastered", null, new int[] {0x1C8A530, 0x98}),
            new Game("Sekiro", null, new int[] {0x3D5AAC0, 0x90}),
            new Game("eldenring", null, new int[] {0x3CD4D88, 0x94})
        };
        private static bool _debugMode;

        static void Main(string[] args)
        {
            //Console.CancelKeyPress += delegate {
            //    Write(0);
            //};
            Console.Title = "Souls Death Listener";
            args.ToList().ForEach(a => 
            { 
                if(a.ToLower() == "-debug")
                    _debugMode = true;
            });

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("-----------------------------------WARNING-----------------------------------");
            Console.WriteLine(" Possible risk of BANS by trying to use with EAC enabled.");
            Console.WriteLine(" USE AT YOUR OWN RISK.");
            Console.WriteLine("-----------------------------------WARNING-----------------------------------");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();
            Console.WriteLine("-----------------------------------Original project by Quidrex-----------------------------------");
            Console.WriteLine("https://github.com/Quidrex/DSDeaths");
            Console.WriteLine();
            Console.WriteLine("-----------------------------------Fork by GeekCrunch-----------------------------------");
            Console.WriteLine();
            Console.WriteLine("App Icon (https://icons8.com/icons/set/skull--static) from icons8.com");
            Console.WriteLine("-----------------------------------Fork by GeekCrunch-----------------------------------");
            Console.WriteLine();

            MainWorker();

        }

        static bool Write(int value, string gameName)
        {
            var gameFileName = string.Concat("deaths_", gameName);
            try
            {
                if (value == 0)
                    return true;
                Console.WriteLine("YOU DIED! {0} death(s)", value);
                File.WriteAllText($"{gameFileName}.txt", value.ToString());
            }
            catch (IOException)
            {
                Console.WriteLine($"Could not write to {gameFileName}.txt.");
                return false;
            }
            return true;
        }

        static bool PeekMemory(in IntPtr processHandle, in IntPtr processBaseAddress, bool isX64, in int[] offsets, ref int value)
        {
            long address = processBaseAddress.ToInt64();
            byte[] buffer = new byte[8];
            int discard = 0;

            foreach (int offset in offsets)
            {
                if (address == 0)
                {
                    if (_debugMode)
                        Console.WriteLine("Encountered null pointer.");
                    return false;
                }

                address += offset;

                if (!ReadProcessMemory(processHandle, (IntPtr)address, buffer, 8, ref discard))
                {
                    if (_debugMode)
                        Console.WriteLine("Could not read game memory.");
                    return false;
                }

                address = isX64 ? BitConverter.ToInt64(buffer, 0) : BitConverter.ToInt32(buffer, 0);
            }

            value = (int)address;
            return true;
        }



        static bool ScanProcesses(ref Process proc, ref Game game)
        {
            foreach (Game g in games)
            {
                Process[] process = Process.GetProcessesByName(g.name);
                if (process.Length != 0)
                {
                    Console.WriteLine("Found: " + g.name);
                    proc = process[0];
                    game = g;
                    return true;
                }
            }
            return false;
        }

        static void MainWorker()
        {
            while (true)
            {
                //Write(0);
                Console.WriteLine("Looking for Dark Souls process...");

                Process proc = null;
                Game game = null;

                while (!ScanProcesses(ref proc, ref game))
                {
                    Thread.Sleep(500);
                }

                IntPtr handle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_WM_READ, false, proc.Id);
                IntPtr baseAddress = proc.MainModule.BaseAddress;
                int oldValue = 0, value = 0;

                bool isWow64 = false;
                if (IsWow64Process(handle, ref isWow64))
                {
                    Console.WriteLine("Found " + (isWow64 ? "32" : "64") + " bit variant.");
                    int[] offsets = isWow64 ? game.offsets32 : game.offsets64;

                    while (!proc.HasExited)
                    {
                        if (PeekMemory(handle, baseAddress, !isWow64, offsets, ref value))
                        {
                            if (value != oldValue)
                            {
                                oldValue = value;
                                Write(value, game.name);
                            }
                        }
                        Thread.Sleep(500);
                    }
                }

                Console.WriteLine("Process has exited.");
                Thread.Sleep(2000);
            }
        }
    }
}
