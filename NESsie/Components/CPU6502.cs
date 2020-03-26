namespace NESsie.Components
{
    public class CPU6502
    {
        private int AccumulatorRegister;
        private int XIndexRegister;
        private int YIndexRegister;
        private int ProcessorStatusRegister;
        private int ProgramCounterRegister;
        private int StackPointerRegister;
        private bool CarryFlag;
        private bool DecimalModeFlag;
        private bool InterruptDisableFlag;
        private bool NegativeFlag;
        private bool OverflowFlag;
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

            return opcodes.toArray();
        }
    }
}