using System;

namespace NESsie.Components
{
    public partial class CPU6502
    {
        private CpuInstruction instr(Func<byte> instruction, Func<byte> addressMode, ushort cycles)
        {
            return new CpuInstruction(instruction, addressMode, cycles);
        }

        /// <summary>
        /// Take two bytes and turn them into a 16 bit representation.
        /// </summary>
        private static ushort ConvertTo16BitValue(byte highByte, byte lowByte)
        {
            // Left shift highByte by 8 to make room for the lowByte, then OR them to get the full 16 bit address.
            var value = (ushort)((byte)(highByte << 8) | lowByte);
            return value;
        }

        private bool HasMemoryPageChanged(byte highByte)
        {
            // We check to see whether or not the address changes the page by creating a mask for the four left-most bits and comparing it with the highByte shifted to the left by 8.
            if ((this.absoluteAddress & 0xFF00) != (highByte << 8))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool HasCompleted()
        {
            var hasCompleted = this.Cycles == 0;
            return hasCompleted;
        }

        private bool GetFlag(byte flag)
        {
            var isSet = (this.ProcessorStatus & flag) > 0;
            return isSet;
        }

        /// <summary>
        /// Sets or clears a flag in the status register.
        /// </summary>
        /// <param name="flag">The flag to set or clear</param>
        /// <param name="setFlag">Sets the flag if true, clears it if false</param>
        private void SetFlag(byte flag, bool setFlag)
        {
            if (setFlag)
            {
                this.ProcessorStatus |= flag;
            }
            else
            {
                this.ProcessorStatus &= (byte)~flag;
            }
        }

        private void GetAluInput()
        {
            // There is no point in getting ALU input for implied address mode because there is no ALU input to get.
            var instruction = this.InstructionSetOpcodeMatrix[Opcode + 1];
            if (instruction.AddressMode != IMP)
            {
                this.aluInput = this.Bus.Read(this.absoluteAddress);
            }
        }

        private static bool IsZero(byte register)
        {
            return register == 0x00;
        }

        private static bool IsNegative(byte register)
        {
            return (register & 0x80) > 0;
        }
    }
}
