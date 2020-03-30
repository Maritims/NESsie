using NESsie.Components;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NESsie
{
    class Program
    {
        static void Main(string[] args)
        {
            var nes = new Bus();
            nes.CPU.Verbose = true;

            /*
             * *=$8000
             * LDX 10
             */
            var hexAssemblyCode = "A2 0A 8E 00 00 A9 0A CD 00 00".Replace(" ", "");
            ushort memoryOffset = 0x80;
            for(var i = 0; i < hexAssemblyCode.Length; i++)
            {
                if(i % 2 == 0)
                {
                    var hexCode = hexAssemblyCode.Substring(i, 2);
                    var hexCodeByte = Convert.ToByte(hexCode, 16);
                    nes.RAM.Write(memoryOffset, hexCodeByte);
                    memoryOffset++;
                }
            }

            nes.RAM.Write(0xFC, 0x00);
            nes.RAM.Write(0xFD, 0x80);

            nes.CPU.Reset();
            do
            {
                nes.CPU.Clock();
            }
            while (!nes.CPU.HasCompleted());

            Console.WriteLine("Hit space to clock the CPU - or enter to exit.");

            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Spacebar)
                {
                    do
                    {
                        nes.CPU.Clock();
                        DrawCPU(nes.CPU);
                        DrawRAM(nes.RAM, 0x00);
                    }
                    while (!nes.CPU.HasCompleted());
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    break;
                }
            }
            Console.WriteLine("Ready to exit. Press any button to continue.");
            Console.ReadLine();
        }

        static void DrawCPU(CPU6502 cpu)
        {
            Console.SetCursorPosition(0, 2);
            var flags = typeof(FLAGS6502).GetFields(BindingFlags.Public | BindingFlags.Static);
            Console.Write($"Cycles: {cpu.Cycles} - Opcode: {cpu.Opcode}, Instruction: {cpu.InstructionSetOpcodeMatrix[cpu.Opcode + 1]} - X: {cpu.X}, Y: {cpu.Y}, A: {cpu.A} - Flags: ");
            for(var i = 0; i < flags.Length; i++)
            {
                var flagName = flags[i].Name;
                var flagValue = (byte)flags[i].GetValue(null);
                Console.ForegroundColor = ((cpu.ProcessorStatus & flagValue) > 0) ? ConsoleColor.Green : ConsoleColor.Red;
                Console.Write(flagName);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write((i + 1) < flags.Length ? " | " : "\r\n");
            }
        }

        static void DrawRAM(Memory ram, ushort address)
        {
            Console.SetCursorPosition(0, 3);
            var sb = new StringBuilder();
            for(var x = 0; x < 256; x++)
            {
                if(x % 16 == 0)
                {
                    sb.Append("\r\n");
                }

                var data = ram.Read(address);
                sb.Append($"{data.ToString("X2")} ");
                address++;
            }
            var line = sb.ToString();
            Console.WriteLine(line);
        }
    }
}
