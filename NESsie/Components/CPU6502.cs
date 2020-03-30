using System;
using System.Collections.Generic;

namespace NESsie.Components
{

    public class CPU6502
    {
        public bool Verbose;

        public int Cycles { get; private set; }
        public byte Opcode { get; private set; } = 0x00;
        public bool HasCompleted()
        {
            var hasCompleted = this.Cycles == 0;
            return hasCompleted;
        }
        public readonly CpuInstruction[] InstructionSetOpcodeMatrix;
        public byte A { get; private set; } = 0x00;  // A register
        public byte X { get; private set; } = 0x00;  // X register
        public byte Y { get; private set; } = 0x00;  // Y register
        public byte ProcessorStatus { get; private set; } = 0x00;
        public ushort ProgramCounter { get; private set; } = 0x00;
        public byte StackPointer { get; private set; } = 0x00;

        Bus Bus { get; set; }
        byte aluInput = 0x00;
        ushort absoluteAddress = 0x000; // The absolute memory address to be used in the current operation at any given time.
        ushort relativeAddress = 0x000; // The relative memory address to be used in the current branching operation if there is one.

        public CPU6502()
        {
            this.InstructionSetOpcodeMatrix = new CpuInstruction[]
            {
                instr(BRK, IMP, 7), instr(ORA, INX, 6), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(ORA, ZP0, 3), instr(ASL, ZP0, 5), instr(XXX, IMP, 2), instr(PHP, IMP, 3), instr(ORA, IMM, 2), instr(ASL, IMP, 2), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(ORA, ABS, 4), instr(ASL, ABS, 6), instr(XXX, IMP, 2),
                instr(BPL, REL, 2), instr(ORA, IZY, 5), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(ORA, ZPX, 4), instr(ASL, ZPX, 4), instr(XXX, IMP, 2), instr(CLC, IMP, 2), instr(ORA, ABY, 4), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(ORA, ABX, 4), instr(ASL, ABX, 7), instr(XXX, IMP, 2),
                instr(JSR, ABS, 6), instr(AND, IZX, 6), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(BIT, ZP0, 3), instr(AND, ZP0, 3), instr(ROL, ZP0, 5), instr(XXX, IMP, 2), instr(PLP, IMP, 4), instr(AND, IMM, 2), instr(ROL, IMP, 2), instr(XXX, IMP, 2), instr(BIT, ABS, 4), instr(AND, ABS, 4), instr(ROL, ABS, 6), instr(ROL, ABS, 6), instr(XXX, IMP, 2),
                instr(BMI, REL, 2), instr(AND, IZY, 5), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(AND, ZPX, 4), instr(ROL, ZPX, 6), instr(XXX, IMP, 2), instr(SEC, IMP, 2), instr(AND, ABY, 4), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(AND, ABX, 4), instr(ROL, ABX, 7), instr(XXX, IMP, 2),
                instr(RTI, IMP, 6), instr(EOR, IZX, 6), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(EOR, ZP0, 3), instr(LSR, ZP0, 5), instr(XXX, IMP, 2), instr(PHA, IMP, 3), instr(EOR, IMM, 2), instr(LSR, IMP, 2), instr(XXX, IMP, 2), instr(JMP, ABS, 3), instr(EOR, ABS, 4), instr(LSR, ABS, 6), instr(XXX, IMP, 2),
                instr(BVC, REL, 2), instr(EOR, IZY, 5), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(EOR, ZPX, 4), instr(LSR, ZPX, 6), instr(XXX, IMP, 2), instr(CLI, IMP, 2), instr(EOR, ABY, 4), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(EOR, ABX, 4), instr(LSR, ABX, 7), instr(XXX, IMP, 2),
                instr(RTS, IMP, 6), instr(ADC, IZX, 6), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(ADC, ZP0, 3), instr(ROR, ZP0, 5), instr(XXX, IMP, 2), instr(PLA, IMP, 4), instr(ADC, IMM, 2), instr(ROR, IMP, 2), instr(XXX, IMP, 2), instr(JMP, IND, 5), instr(ADC, ABS, 4), instr(ROR, ABS, 6), instr(XXX, IMP, 2),
                instr(BVS, REL, 2), instr(ADC, IZY, 5), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(ADC, ZPX, 4), instr(ROR, ZPX, 6), instr(XXX, IMP, 2), instr(SEI, IMP, 2), instr(ADC, ABY, 4), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(ADC, ABX, 4), instr(ROR, ABX, 7), instr(XXX, IMP, 2),
                instr(XXX, IMP, 2), instr(STA, IZX, 6), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(STY, ZP0, 3), instr(STA, ZP0, 3), instr(STX, ZP0, 3), instr(XXX, IMP, 2), instr(DEY, IMP, 2), instr(XXX, IMP, 2), instr(TXA, IMP, 2), instr(XXX, IMP, 2), instr(STY, ABS, 4), instr(STA, ABS, 4), instr(STX, ABS, 4), instr(XXX, IMP, 2),
                instr(BCC, REL, 2), instr(STA, IZY, 6), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(STY, ZPX, 4), instr(STA, ZPX, 4), instr(STX, ZPY, 4), instr(XXX, IMP, 2), instr(TYA, IMP, 2), instr(STA, ABY, 5), instr(TXS, IMP, 2), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(STA, ABX, 5), instr(XXX, IMP, 2), instr(XXX, IMP, 2),
                instr(LDY, IMM, 2), instr(LDA, IZX, 6), instr(LDX, IMM, 2), instr(XXX, IMP, 2), instr(LDY, ZP0, 3), instr(LDA, ZP0, 3), instr(LDX, ZP0, 3), instr(XXX, IMP, 2), instr(TAY, IMP, 2), instr(LDA, IMM, 2), instr(TAX, IMP, 2), instr(XXX, IMP, 2), instr(LDY, ABS, 4), instr(LDA, ABS, 4), instr(LDX, ABS, 4), instr(XXX, IMP, 2),
                instr(BCS, REL, 2), instr(LDA, IZY, 5), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(LDY, ZPX, 4), instr(LDA, ZPX, 4), instr(LDX, ZPY, 4), instr(XXX, IMP, 2), instr(CLV, IMP, 2), instr(LDA, ABY, 4), instr(TSX, IMP, 2), instr(XXX, IMP, 2), instr(LDY, ABX, 4), instr(LDA, ABX, 4), instr(LDX, ABY, 4), instr(XXX, IMP, 2),
                instr(CPY, IMM, 2), instr(CMP, IZX, 6), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(CPY, ZP0, 3), instr(CMP, ZP0, 3), instr(DEC, ZP0, 5), instr(XXX, IMP, 2), instr(IZY, IMP, 2), instr(CMP, IMM, 2), instr(DEX, IMP, 2), instr(XXX, IMP, 2), instr(CPY, ABS, 4), instr(CMP, ABS, 4), instr(DEC, ABS, 6), instr(XXX, IMP, 2),
                instr(BNE, REL, 2), instr(CMP, IZY, 5), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(CMP, ZPX, 3), instr(DEC, ZPX, 6), instr(XXX, IMP, 2), instr(CLD, IMP, 2), instr(CMP, ABY, 4), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(CMP, ABX, 4), instr(DEC, ABX, 7), instr(XXX, IMP, 2),
                instr(CPX, IMM, 2), instr(SBC, IZX, 6), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(CPX, ZP0, 3), instr(SBC, ZP0, 3), instr(INC, ZP0, 5), instr(XXX, IMP, 2), instr(IZX, IMP, 2), instr(SBC, IMM, 2), instr(NOP, IMP, 2), instr(XXX, IMP, 2), instr(CPX, ABS, 4), instr(SBC, ABS, 4), instr(INC, ABS, 6), instr(XXX, IMP, 2),
                instr(BEQ, REL, 2), instr(SBC, IZY, 5), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(SBC, ZPX, 4), instr(INC, ZPX, 6), instr(XXX, IMP, 2), instr(SED, IMP, 2), instr(SBC, ABY, 4), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(SBC, ABX, 4), instr(INC, ABX, 7), instr(XXX, IMP, 2)
            };
        }

        private CpuInstruction instr(Func<byte> instruction, Func<byte> addressMode, ushort cycles)
        {
            return new CpuInstruction(instruction, addressMode, cycles);
        }

        public void ConnectToBus(Bus bus)
        {
            this.Bus = bus;
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
                this.ProcessorStatus &= (byte)~flag;
            }
        }

        private void GetAluInput()
        {
            // There is no point in getting ALU input for implied address mode because there is no ALU input to get.
            var instruction = this.InstructionSetOpcodeMatrix[Opcode + 1];
            if(instruction.AddressMode != IMP)
            {
                this.aluInput = this.Bus.Read(this.absoluteAddress);
            }
        }

        // Clock: Get the next instruction and execute it
        public void Clock()
        {
            if (Cycles == 0)
            {
                // The U flag is always 1.
                this.SetFlag(FLAGS6502.U, true);

                // Set the opcode byte for the current instuction.
                this.Opcode = this.Bus.Read(this.ProgramCounter);

                // Increment PC as the opcode byte has now been read from the bus.
                this.ProgramCounter++;

                // Get the amount of cycles required to perform the current instruction. Add 1 when looking up the instruction since the matrix is null indexed.
                var instruction = this.InstructionSetOpcodeMatrix[Opcode + 1];

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

        public void Reset()
        {
            this.absoluteAddress = 0xFD;
            ushort absaddr2 = (ushort)(absoluteAddress + 1);
            var lowByte = this.Bus.Read(absoluteAddress);
            var highByte = this.Bus.Read(absaddr2);

            this.ProgramCounter = this.ConvertTo16BitValue(highByte, lowByte);

            this.A = 0x00;
            this.X = 0x00;
            this.Y = 0x00;
            this.StackPointer = 0xFD;
            this.ProcessorStatus = (byte)(0x00 | FLAGS6502.U);

            this.relativeAddress = 0x0000;
            this.absoluteAddress = 0x0000;
            this.aluInput = 0x00;

            // The required amount of cycles to perform a CPU reset.
            this.Cycles = 8;
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

            if (HasMemoryPageChanged(highByte))
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

            if (HasMemoryPageChanged(highByte))
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

        /// <summary>
        /// Dummy instruction
        /// </summary>
        /// <returns></returns>
        byte XXX() { return 0; }
        byte ADC() { return 0; }
        /// <summary>
        /// Instruction: Bitwise logic AND
        /// Function: A & M
        /// </summary>
        /// <returns>Whether or not this is a potential candidate for an additional clock cycle. 1 if it is, 0 if not.</returns>
        byte AND()
        {
            this.GetAluInput();
            this.A = (byte)(this.A & this.aluInput);
            this.SetFlag(FLAGS6502.N, true);
            this.SetFlag(FLAGS6502.Z, true);
            return 1;
        }
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
        /// <summary>
        /// Instruction: Clears the Cary flag.
        /// </summary>
        /// <returns>Whether or not this is a potential candidate for an additional clock cycle. 1 if it is, 0 if not.</returns>
        byte CLC()
        {
            this.SetFlag(FLAGS6502.C, false);
            return 0;
        }
        /// <summary>
        /// Instruction: Clears the decimal flag.
        /// </summary>
        /// <returns>Whether or not this is a potential candidate for an additional clock cycle. 1 if it is, 0 if not.</returns>
        byte CLD()
        {
            this.SetFlag(FLAGS6502.D, false);
            return 0;
        }
        /// <summary>
        /// Instruction: Clears the Interrupt Disable flag.
        /// </summary>
        /// <returns>Whether or not this is a potential candidate for an additional clock cycle. 1 if it is, 0 if not.</returns>
        byte CLI()
        {
            this.SetFlag(FLAGS6502.I, false);
            return 0;
        }
        /// <summary>
        /// Instruction: Clears the Overflow flag.
        /// </summary>
        /// <returns>Whether or not this is a potential candidate for an additional clock cycle. 1 if it is, 0 if not.</returns>
        byte CLV()
        {
            this.SetFlag(FLAGS6502.V, false);
            return 0;
        }
        /// <summary>
        /// Instruction: Compare accumulator with memory.
        /// Function: A - M
        /// </summary>
        /// <returns>Whether or not this is a potential candidate for an additional clock cycle. 1 if it is, 0 if not.</returns>
        byte CMP()
        {
            this.GetAluInput();
            var temp = this.A - this.aluInput;
            var isPageBoundaryReached = this.A >= temp;
            var isAccumulatorZero = (temp & 0x00FF) == 0x0000;
            var isAccumulatorNegative = (temp & 0x0080) == 0x0080; // Accumulator is negative if the most significant bit is set.
            this.SetFlag(FLAGS6502.C, isPageBoundaryReached);
            this.SetFlag(FLAGS6502.Z, isAccumulatorZero);
            this.SetFlag(FLAGS6502.N, isAccumulatorNegative);
            return 0;
        }
        /// <summary>
        /// Instruction: Compare X register with memory.
        /// Function: X - M
        /// </summary>
        /// <returns>Whether or not this is a potential candidate for an additional clock cycle. 1 if it is, 0 if not.</returns>
        byte CPX()
        {
            this.GetAluInput();
            var temp = this.X - this.aluInput;
            var isPageBoundaryReached = this.X >= temp;
            var isRegisterZero = (temp & 0x00FF) == 0x0000;
            var isRegisterNegative = (temp & 0x0080) == 0x0080; // X register is negative if the most significant bit is set.
            this.SetFlag(FLAGS6502.C, isPageBoundaryReached);
            this.SetFlag(FLAGS6502.Z, isRegisterZero);
            this.SetFlag(FLAGS6502.N, isRegisterNegative);
            return 0;
        }
        /// <summary>
        /// Instruction: Compare Y register with memory.
        /// Function: X - Y
        /// </summary>
        /// <returns>Whether or not this is a potential candidate for an additional clock cycle. 1 if it is, 0 if not.</returns>
        byte CPY() {
            this.GetAluInput();
            var temp = this.Y - this.aluInput;
            var isPageBoundaryCrossed = this.Y >= temp;
            var isRegisterZero = (temp & 0x00FF) == 0x0000;
            var isRegisterNegative = (temp & 0x0080) == 0x0080; // Y register is negative if the most significant bit is set.
            this.SetFlag(FLAGS6502.C, isPageBoundaryCrossed);
            this.SetFlag(FLAGS6502.Z, isRegisterZero);
            this.SetFlag(FLAGS6502.N, isRegisterNegative);
            return 0;
        }
        byte DEC() { return 0; }
        byte DEX() { return 0; }
        byte DEY() { return 0; }
        byte EOR() { return 0; }
        byte INC() { return 0; }
        byte INX() { return 0; }
        byte INY() { return 0; }
        byte JMP() { return 0; }
        byte JSR() { return 0; }
        /// <summary>
        /// Instruction: Load memory into accumulator.
        /// </summary>
        /// <returns></returns>
        byte LDA()
        {
            this.GetAluInput();
            this.A = this.aluInput;
            var isAccumulatorZero = this.A == 0x00;
            var isAccumulatorNegative = (this.A & 0x80) > 0;
            this.SetFlag(FLAGS6502.Z, isAccumulatorZero);
            this.SetFlag(FLAGS6502.N, isAccumulatorNegative);
            return 1;
        }
        /// <summary>
        /// Instruction: Load memory into X register.
        /// </summary>
        /// <returns></returns>
        byte LDX()
        {
            this.GetAluInput();
            this.X = this.aluInput;
            var isRegisterZero = this.X == 0x00;
            var isRegisterNegative = (this.X & 0x80) > 0;
            this.SetFlag(FLAGS6502.Z, isRegisterZero);
            this.SetFlag(FLAGS6502.N, isRegisterNegative);
            return 1;
        }
        /// <summary>
        /// Instruction: Load memory into Y register.
        /// </summary>
        /// <returns></returns>
        byte LDY()
        {
            this.GetAluInput();
            this.Y = this.aluInput;
            var isRegisterZero = this.Y == 0x00;
            var isRegisterNegative = (this.Y & 0x80) > 0;
            this.SetFlag(FLAGS6502.Z, isRegisterZero);
            this.SetFlag(FLAGS6502.N, isRegisterNegative);
            return 1;
        }
        byte LSR() { return 0; }
        /// <summary>
        /// Instruction: No operation.
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// Instruction: Set the Carry flag.
        /// </summary>
        /// <returns></returns>
        byte SEC()
        {
            this.SetFlag(FLAGS6502.C, true);
            return 0;
        }
        /// <summary>
        /// Instrction: Set the Decimal flag.
        /// </summary>
        /// <returns></returns>
        byte SED()
        {
            this.SetFlag(FLAGS6502.D, true);
            return 0;
        }
        /// <summary>
        /// Instruction: Set the Interrupt Disable flag.
        /// </summary>
        /// <returns></returns>
        byte SEI()
        {
            this.SetFlag(FLAGS6502.I, true);
            return 0;
        }
        /// <summary>
        /// Instruction: Store the value of the accumulator in memory.
        /// </summary>
        /// <returns></returns>
        byte STA()
        {
            this.Bus.Write(this.absoluteAddress, this.A);
            return 0;
        }
        /// <summary>
        /// Instruction: Store the value of the X register in memory.
        /// </summary>
        /// <returns></returns>
        byte STX()
        {
            this.Bus.Write(this.absoluteAddress, this.X);
            return 0;
        }
        /// <summary>
        /// Instruction: Store the value of the Y register in memory.
        /// </summary>
        /// <returns></returns>
        byte STY()
        {
            this.Bus.Write(this.absoluteAddress, this.Y);
            return 0;
        }
        byte TAX() { return 0; }
        byte TAY() { return 0; }
        byte TSX() { return 0; }
        byte TXA() { return 0; }
        byte TXS() { return 0; }
        byte TYA() { return 0; }


    }
}