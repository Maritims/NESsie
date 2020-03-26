using NESsie.Components;
using System;

namespace NESsie
{
    class Program
    {
        static void Main(string[] args)
        {
            var nes = new NintendoEntertainmentSystem();
            var opcodes = CPU6502.getOpcodes();
            foreach(var opcode in opcodes)
            {
                Console.WriteLine(opcode);
            }

            Console.ReadLine();
        }
    }
}
