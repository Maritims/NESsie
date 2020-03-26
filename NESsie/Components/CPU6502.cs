using System.Collections.Generic;

namespace NESsie.Components
{
    public class CPU6502
    {
        private Memory RAM;

        public int Cycles { private get; set; }
        public int Clock { get; set; }

        private int AccumulatorRegister;
        private int XIndexRegister;
        private int YIndexRegister;
        private int ProcessorStatusRegister;
        
        private int _programCounterRegister;
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
        }
        private int StackPointerRegister;

        private bool CarryFlag;
        private bool DecimalModeFlag;
        private bool InterruptDisableFlag;
        private bool NegativeFlag;
        private bool OverflowFlag;

        public CPU6502()
        {
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