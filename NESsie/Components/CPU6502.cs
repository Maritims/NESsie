using System;
using System.Collections.Generic;

namespace NESsie.Components
{

    public class CPU6502
    {
        class Instruction
        {
            public Func<byte> Operate;
            public Func<byte> AddressMode;
            public ushort Cycles;

            public Instruction(Func<byte> operation, Func<byte> addressMode, ushort cycles)
            {
                this.Operate = operation;
                this.AddressMode = addressMode;
                this.Cycles = cycles;
            }
        }

        public int Cycles { private get; set; }
        public int Clock { get; set; }
        Bus Bus { get; set; }
        readonly Instruction[] InstructionSetOpCodeMatrix;

        byte A = 0x00;  // A register
        byte X = 0x00;  // X register
        byte Y = 0x00;  // Y register
        byte ProcessorStatus = 0x00;
        ushort ProgramCounter = 0x00;
        byte StackPointer = 0x00;

        byte aluInput = 0x00;
        ushort absoluteAddress = 0x000; // The absolute memory address to be used in the current operation at any given time.
        ushort relativeAddress = 0x000; // The relative memory address to be used in the current branching operation if there is one.

        public CPU6502(Bus bus)
        {
            this.Bus = bus;
            this.InstructionSetOpCodeMatrix = new Instruction[]
            {
                new Instruction(BRK, IMP, 7), new Instruction(ORA, INX, 6), null, null, null, new Instruction(ORA, ZP0, 3), new Instruction(ASL, ZP0, 5), null, new Instruction(PHP, IMP, 3), new Instruction(ORA, IMM, 2), new Instruction(ASL, IMP, 2), null, null, new Instruction(ORA, ABS, 4), new Instruction(ASL, ABS, 6), null,
                new Instruction(BPL, REL, 2), new Instruction(ORA, IZY, 5), null, null, null, new Instruction(ORA, ZPX, 4), new Instruction(ASL, ZPX, 4), null, new Instruction(CLC, IMP, 2), new Instruction(ORA, ABY, 4), null, null, null, new Instruction(ORA, ABX, 4), new Instruction(ASL, ABX, 7), null,
                new Instruction(JSR, ABS, 6), new Instruction(AND, IZX, 6), null, null, new Instruction(BIT, ZP0, 3), new Instruction(AND, ZP0, 3), new Instruction(ROL, ZP0, 5), null, new Instruction(PLP, IMP, 4), new Instruction(AND, IMM, 2), new Instruction(ROL, IMP, 2), null, new Instruction(BIT, ABS, 4), new Instruction(AND, ABS, 4), new Instruction(ROL, ABS, 6), new Instruction(ROL, ABS, 6), null,
                new Instruction(BMI, REL, 2), new Instruction(AND, IZY, 5), null, null, null, new Instruction(AND, ZPX, 4), new Instruction(ROL, ZPX, 6), null, new Instruction(SEC, IMP, 2), new Instruction(AND, ABY, 4), null, null, null, new Instruction(AND, ABX, 4), new Instruction(ROL, ABX, 7), null,
                new Instruction(RTI, IMP, 6), new Instruction(EOR, IZX, 6), null, null, null, new Instruction(EOR, ZP0, 3), new Instruction(LSR, ZP0, 5), null, new Instruction(PHA, IMP, 3), new Instruction(EOR, IMM, 2), new Instruction(LSR, IMP, 2), null, new Instruction(JMP, ABS, 3), new Instruction(EOR, ABS, 4), new Instruction(LSR, ABS, 6), null,
                new Instruction(BVC, REL, 2), new Instruction(EOR, IZY, 5), null, null, null, new Instruction(EOR, ZPX, 4), new Instruction(LSR, ZPX, 6), null, new Instruction(CLI, IMP, 2), new Instruction(EOR, ABY, 4), null, null, null, new Instruction(EOR, ABX, 4), new Instruction(LSR, ABX, 7), null,
                new Instruction(RTS, IMP, 6), new Instruction(ADC, IZX, 6), null, null, null, new Instruction(ADC, ZP0, 3), new Instruction(ROR, ZP0, 5), null, new Instruction(PLA, IMP, 4), new Instruction(ADC, IMM, 2), new Instruction(ROR, IMP, 2), null, new Instruction(JMP, IND, 5), new Instruction(ADC, ABS, 4), new Instruction(ROR, ABS, 6), null,
                new Instruction(BVS, REL, 2), new Instruction(ADC, IZY, 5), null, null, null, new Instruction(ADC, ZPX, 4), new Instruction(ROR, ZPX, 6), null, new Instruction(SEI, IMP, 2), new Instruction(ADC, ABY, 4), null, null, null, new Instruction(ADC, ABX, 4), new Instruction(ROR, ABX, 7), null,
                null, new Instruction(STA, IZX, 6), null, null, new Instruction(STY, ZP0, 3), new Instruction(STA, ZP0, 3), new Instruction(STX, ZP0, 3), null, new Instruction(DEY, IMP, 2), null, new Instruction(TXA, IMP, 2), null, new Instruction(STY, ABS, 4), new Instruction(STA, ABS, 4), new Instruction(STX, ABS, 4), null,
                new Instruction(BCC, REL, 2), new Instruction(STA, IZY, 6), null, null, new Instruction(STY, ZPX, 4), new Instruction(STA, ZPX, 4), new Instruction(STX, ZPY, 4), null, new Instruction(TYA, IMP, 2), new Instruction(STA, ABY, 5), new Instruction(TXS, IMP, 2), null, null, new Instruction(STA, ABX, 5), null, null,
                new Instruction(LDY, IMM, 2), new Instruction(LDA, IZX, 6), new Instruction(LDX, IMM, 2), null, new Instruction(LDY, ZP0, 3), new Instruction(LDA, ZP0, 3), new Instruction(LDX, ZP0, 3), null, new Instruction(TAY, IMP, 2), new Instruction(LDA, IMM, 2), new Instruction(TAX, IMP, 2), null, new Instruction(LDY, ABS, 4), new Instruction(LDA, ABS, 4), new Instruction(LDX, ABS, 4), null,
                new Instruction(BCS, REL, 2), new Instruction(LDA, IZY, 5), null, null, new Instruction(LDY, ZPX, 4), new Instruction(LDA, ZPX, 4), new Instruction(LDX, ZPY, 4), null, new Instruction(CLV, IMP, 2), new Instruction(LDA, ABY, 4), new Instruction(TSX, IMP, 2), null, new Instruction(LDY, ABX, 4), new Instruction(LDA, ABX, 4), new Instruction(LDX, ABY, 4), null,
                new Instruction(CPY, IMM, 2), new Instruction(CMP, IZX, 6), null, null, new Instruction(CPY, ZP0, 3), new Instruction(CMP, ZP0, 3), new Instruction(DEC, ZP0, 5), null, new Instruction(IZY, IMP, 2), new Instruction(CMP, IMM, 2), new Instruction(DEX, IMP, 2), null, new Instruction(CPY, ABS, 4), new Instruction(CMP, ABS, 4), new Instruction(DEC, ABS, 6), null,
                new Instruction(BNE, REL, 2), new Instruction(CMP, IZY, 5), null, null, null, new Instruction(CMP, ZPX, 3), new Instruction(DEC, ZPX, 6), null, new Instruction(CLD, IMP, 2), new Instruction(CMP, ABY, 4), null, null, null, new Instruction(CMP, ABX, 4), new Instruction(DEC, ABX, 7), null,
                new Instruction(CPX, IMM, 2), new Instruction(SBC, IZX, 6), null, null, new Instruction(CPX, ZP0, 3), new Instruction(SBC, ZP0, 3), new Instruction(INC, ZP0, 5), null, new Instruction(IZX, IMP, 2), new Instruction(SBC, IMM, 2), new Instruction(NOP, IMP, 2), null, new Instruction(CPX, ABS, 4), new Instruction(SBC, ABS, 4), new Instruction(INC, ABS, 6), null,
                new Instruction(BEQ, REL, 2), new Instruction(SBC, IZY, 5), null, null, null, new Instruction(SBC, ZPX, 4), new Instruction(INC, ZPX, 6), null, new Instruction(SED, IMP, 2), new Instruction(SBC, ABY, 4), null, null, null, new Instruction(SBC, ABX, 4), new Instruction(INC, ABX, 7), null
            };
        }

        // Clock: Get the next instruction and execute it
        public void PerformClockCycle()
        {
            if(Cycles == 0)
            {
                // The U flag is always 1.
                this.SetFlag(FLAGS6502.U, true);

                var opcode = this.Bus.Read(this.ProgramCounter);
                
                // Increment PC as the opcode byte has now been read from the bus.
                this.ProgramCounter++;

                // Get the amount of cycles required to perform the current instruction.
                var instruction = this.InstructionSetOpCodeMatrix[opcode];
                this.Cycles = instruction.Cycles;
                var additionalCycleRequiredByAddressMode = instruction.AddressMode();
                var additionalCycleRequiredByOperation = instruction.Operate();
                this.Cycles += additionalCycleRequiredByAddressMode & additionalCycleRequiredByOperation;

                // The U flag is always 1.
                SetFlag(FLAGS6502.U, true);
            }

            // The current cycle has now finished. Decrement the remaining number of cycles required to perform the current instruction.
            this.Cycles--;
        }

        /// <summary>
        /// Take two bytes and turn them into a 16 bit representation.
        /// </summary>
        private ushort ConvertTo16BitValue(byte highByte, byte lowByte)
        {
            // Left shift highByte by 8 to make room for the lowByte, then OR them to get the full 16 bit address.
            var value = (ushort)((byte)(highByte << 8) | lowByte);
            return value;
        }

        private bool hasMemoryPageChanged(byte highByte)
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

        /// <summary>
        /// Sets or clears a flag in the status register.
        /// </summary>
        /// <param name="flag">The flag to set or clear</param>
        /// <param name="setFlag">Sets the flag if true, clears it if false</param>
        private void SetFlag(byte flag, bool setFlag)
        {
            if(setFlag)
            {
                this.ProcessorStatus |= flag;
            } else
            {
                this.ProcessorStatus &= flag;
            }
        }

        /// <summary>
        /// Implied address mode. The operation is to be performed on the accumulator.
        /// </summary>
        /// <returns>Additional required clock cycles</returns>
        byte IMP()
        {
            this.aluInput = this.A;
            return 0;
        }
        /// <summary>
        /// Immediate address mode. The address is in the second byte.
        /// </summary>
        /// <returns>Additional required clock cycles</returns>
        byte IMM()
        {
            this.absoluteAddress = this.ProgramCounter++;
            return 0;
        }
        /// <summary>
        /// Zero page addressing: We are addressing a location in the first 0xFF bytes of the address range.
        /// </summary>
        /// /// <returns>Additional required clock cycles</returns>
        byte ZP0()
        {
            this.absoluteAddress = this.Bus.Read(this.ProgramCounter);
            this.ProgramCounter++;
            this.absoluteAddress &= 0x00FF;
            return 0;
        }
        /// <summary>
        /// Zero page addressing with X register offset.
        /// </summary>
        /// /// <returns>Additional required clock cycles</returns>
        byte ZPX()
        {
            this.absoluteAddress = (ushort)(this.Bus.Read(this.ProgramCounter) + this.X);
            this.ProgramCounter++;
            this.absoluteAddress &= 0x00FF;
            return 0;
        }
        /// <summary>
        /// Zero page addressing with Y register offset.
        /// </summary>
        /// /// <returns>Additional required clock cycles</returns>
        byte ZPY()
        {
            this.absoluteAddress = (ushort)(this.Bus.Read(this.ProgramCounter) + this.Y);
            this.ProgramCounter++;
            this.absoluteAddress &= 0x00FF;
            return 0;
        }
        /// <summary>
        /// Absolute addressing with a 16 bit address spread over two 8 bit bytes.
        /// </summary>
        /// /// <returns>Additional required clock cycles</returns>
        byte ABS()
        {
            var lowByte = this.Bus.Read(this.ProgramCounter);
            this.ProgramCounter++;
            var highByte = this.Bus.Read(this.ProgramCounter);
            this.ProgramCounter++;
            this.absoluteAddress = (ushort)this.ConvertTo16BitValue(highByte, lowByte);
            return 0;
        }
        /// <summary>
        /// Absolute addressing with X register offset.
        /// </summary>
        /// <returns>Additional required clock cycles</returns>
        byte ABX()
        {
            var lowByte = this.Bus.Read(this.ProgramCounter);
            this.ProgramCounter++;
            var highByte = this.Bus.Read(this.ProgramCounter);
            this.ProgramCounter++;
            this.absoluteAddress = (ushort)(this.ConvertTo16BitValue(highByte, lowByte) + this.X);

            if (hasMemoryPageChanged(highByte))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        byte ABY()
        {
            var lowByte = this.Bus.Read(this.ProgramCounter);
            this.ProgramCounter++;
            var highByte = this.Bus.Read(this.ProgramCounter);
            this.ProgramCounter++;
            this.absoluteAddress = (ushort)(this.ConvertTo16BitValue(highByte, lowByte) + this.Y);

            if (hasMemoryPageChanged(highByte))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        byte REL()
        {
            this.relativeAddress = this.Bus.Read(this.ProgramCounter);
            this.ProgramCounter++;
            // TODO: Figure out what if(addr_rel & 0x80) addr_rel |= 0xFF00; means 
            if ((this.relativeAddress & (1 << 7)) != 0)
                this.relativeAddress |= 0xFF;

            return 0;
        }
        /// <summary>
        /// Indirect access mode: The 6502 pointer implementation.
        /// The instruction contains a pointer to a location in memory where the actual memory address is stored.
        /// That actual memory address contains the data we're interested in.
        /// </summary>
        /// <returns></returns>
        byte IND()
        {
            ushort addressPointerLowByte = this.Bus.Read(this.ProgramCounter);
            this.ProgramCounter++;
            ushort addressPointerHighByte = this.Bus.Read(this.ProgramCounter);
            this.ProgramCounter++;
            ushort addressPointer = (ushort)((addressPointerHighByte << 8) | addressPointerLowByte);

            ushort addressHighByte, addressLowByte;
            if (addressPointerLowByte == 0x00FF)
            {
                addressPointer &= 0xFF;
                addressHighByte = (byte)(this.Bus.Read(addressPointer) << 8);
                addressLowByte = this.Bus.Read(addressPointer);
            }
            else
            {
                addressHighByte = (ushort)(this.Bus.Read((ushort)(addressPointer + 1)) << 8);
                addressLowByte = this.Bus.Read(addressPointer);
            }

            this.absoluteAddress = (ushort)(addressHighByte | addressLowByte);
            return 0;
        }
        /// <summary>
        /// Indirect addressing with X
        /// </summary>
        /// <returns></returns>
        byte IZX()
        {
            var tmp = this.Bus.Read(this.ProgramCounter);
            var pageZeroMemoryLocation = (ushort)(tmp + this.X);
            var lowByte = this.Bus.Read((ushort)(pageZeroMemoryLocation & 0x00FF));
            var highByte = this.Bus.Read((ushort)((pageZeroMemoryLocation + 1) & 0x00FF));
            this.absoluteAddress = this.ConvertTo16BitValue(highByte, lowByte);
            return 0;
        }
        /// <summary>
        /// Indirect addressing with Z
        /// </summary>
        /// <returns></returns>
        byte IZY()
        {
            var tmp = this.Bus.Read(this.ProgramCounter);
            var pageZeroMemoryLocation = (ushort)(tmp + this.Y);
            var lowByte = this.Bus.Read((ushort)(pageZeroMemoryLocation & 0x00FF));
            var highByte = this.Bus.Read((ushort)((pageZeroMemoryLocation + 1) & 0x00FF));
            this.absoluteAddress = this.ConvertTo16BitValue(highByte, lowByte);
            return 0;
        }

        byte ADC() { return 0; }
        byte AND() { return 0; }
        byte ASL() { return 0; }
        byte BCC() { return 0; }
        byte BCS() { return 0; }
        byte BEQ() { return 0; }
        byte BIT() { return 0; }
        byte BMI() { return 0; }
        byte BNE() { return 0; }
        byte BPL() { return 0; }
        byte BRK() { return 0; }
        byte BVC() { return 0; }
        byte BVS() { return 0; }
        byte CLC() { return 0; }
        byte CLD() { return 0; }
        byte CLI() { return 0; }
        byte CLV() { return 0; }
        byte CMP() { return 0; }
        byte CPX() { return 0; }
        byte CPY() { return 0; }
        byte DEC() { return 0; }
        byte DEX() { return 0; }
        byte DEY() { return 0; }
        byte EOR() { return 0; }
        byte INC() { return 0; }
        byte INX() { return 0; }
        byte INY() { return 0; }
        byte JMP() { return 0; }
        byte JSR() { return 0; }
        byte LDA() { return 0; }
        byte LDX() { return 0; }
        byte LDY() { return 0; }
        byte LSR() { return 0; }
        byte NOP() { return 0; }
        byte ORA() { return 0; }
        byte PHA() { return 0; }
        byte PHP() { return 0; }
        byte PLA() { return 0; }
        byte PLP() { return 0; }
        byte ROL() { return 0; }
        byte ROR() { return 0; }
        byte RTI() { return 0; }
        byte RTS() { return 0; }
        byte SBC() { return 0; }
        byte SEC() { return 0; }
        byte SED() { return 0; }
        byte SEI() { return 0; }
        byte STA() { return 0; }
        byte STX() { return 0; }
        byte STY() { return 0; }
        byte TAX() { return 0; }
        byte TAY() { return 0; }
        byte TSX() { return 0; }
        byte TXA() { return 0; }
        byte TXS() { return 0; }
        byte TYA() { return 0; }


    }
}