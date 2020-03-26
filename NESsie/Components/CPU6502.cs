using System.Collections.Generic;

namespace NESsie.Components
{
    public class CPU6502
    {
        private Memory Memory;

        public int Cycles { private get; set; }
        public int Clock { get; set; }

        private int AccumulatorRegister = 0; // A
        private int XIndexRegister = 0; // X
        private int YIndexRegister = 0; // Y
        private int ProcessorStatusRegister = 0; // P
        private int _programCounterRegister = 0;
        public int ProgramCounterRegister
        {
            private get
            {
                return this._programCounterRegister;
            }
            set
            {
                this._programCounterRegister = value & 0xffff;
            }
        } // PC
        private int StackPointerRegister = 0; // S

        private bool CarryFlag;
        private bool DecimalModeFlag;
        private bool InterruptDisableFlag;
        private bool NegativeFlag;
        private bool OverflowFlag;

        private static string[] opcodes = getOpcodes();

        public CPU6502(Memory memory)
        {
            this.Memory = memory;
        }

        /// <summary>
        /// Sets CPU power up state - https://wiki.nesdev.com/w/index.php/CPU_power_up_state
        /// </summary>
        public void SetPowerUpState()
        {
            for (int memoryAddress = 0; memoryAddress < 0x800; ++memoryAddress)
            {
                this.Memory.Write(memoryAddress, 0xff);
            }

            this.Memory.Write(0x4017, 0x00); // frame irq enabled
            this.Memory.Write(0x4015, 0x00); // all channels disabled

            for (int memoryAddress = 0x4000; memoryAddress < Memory.MemorySize; ++memoryAddress)
            {
                this.Memory.Write(memoryAddress, 0x00);
            }

            this.Memory.Write(0x4010, 0x00);
            this.Memory.Write(0x4011, 0x00);
            this.Memory.Write(0x4012, 0x00);
            this.Memory.Write(0x4013, 0x00);

            for (int i = 0x4000; i < 0x400f; ++9)
            {
                this.Memory.Write(i, 0x00);
            }
        }

        public static string[] getOpcodes()
        {
            var opcodes = new List<string>()
            {
                "BRK",
                "ORA (d, x)",
                "STP",
                "SLO",
                "NOP"
            };

            // TODO: opcode propagation

            return opcodes.ToArray();
        }
    }
}