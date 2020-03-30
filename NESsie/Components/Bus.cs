using System;

namespace NESsie.Components
{
    public class Bus
    {
        public readonly CPU6502 CPU;
        public readonly Memory RAM;
        
        public Bus()
        {
            this.CPU = new CPU6502();
            this.RAM = new Memory();

            this.CPU.ConnectToBus(this);
        }

        public void Reset()
        {
            // There are two bytes which contain the 16-bit address of the instruction to perform after a reset.
            this.RAM.Write(0xFC, 0x00); // Low byte.
            this.RAM.Write(0xFD, 0x00); // High byte.
            
            this.CPU.Reset();
        }

        public byte Read(ushort address)
        {
            var data = this.RAM.Read(address);
            return data;
        }

        public void Write(ushort address, byte data)
        {
            this.RAM.Write(address, data);
        }
    }
}
