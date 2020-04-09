using System;

namespace NESsie.Components
{
    public partial class CPU6502
    {
        public static ushort STACK_POINTER_BOTTOM = 0x0100;
        public static ushort STACK_POINTER_TOP = 0x01FF;

        public bool Verbose;

        public int Cycles { get; private set; }
        public byte Opcode { get; private set; } = 0x00;
        public CpuInstruction[] InstructionSetOpcodeMatrix { get; set; }

        public byte A { get; private set; } = 0x00;  // A register
        public byte X { get; private set; } = 0x00;  // X register
        public byte Y { get; private set; } = 0x00;  // Y register
        public byte ProcessorStatus { get; private set; } = 0x00;
        public ushort ProgramCounter { get; private set; } = 0x00; // A reference to the address the CPU is working on at any given time.
        public byte StackPointer { get; private set; } = 0x00;

        Bus Bus { get; set; }
        byte aluInput { get; set; } = 0x00;
        ushort absoluteAddress { get; set; } = 0x000; // The absolute memory address to be used in the current operation at any given time.
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
                instr(CPY, IMM, 2), instr(CMP, IZX, 6), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(CPY, ZP0, 3), instr(CMP, ZP0, 3), instr(DEC, ZP0, 5), instr(XXX, IMP, 2), instr(INY, IMP, 2), instr(CMP, IMM, 2), instr(DEX, IMP, 2), instr(XXX, IMP, 2), instr(CPY, ABS, 4), instr(CMP, ABS, 4), instr(DEC, ABS, 6), instr(XXX, IMP, 2),
                instr(BNE, REL, 2), instr(CMP, IZY, 5), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(CMP, ZPX, 3), instr(DEC, ZPX, 6), instr(XXX, IMP, 2), instr(CLD, IMP, 2), instr(CMP, ABY, 4), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(CMP, ABX, 4), instr(DEC, ABX, 7), instr(XXX, IMP, 2),
                instr(CPX, IMM, 2), instr(SBC, IZX, 6), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(CPX, ZP0, 3), instr(SBC, ZP0, 3), instr(INC, ZP0, 5), instr(XXX, IMP, 2), instr(IZX, IMP, 2), instr(SBC, IMM, 2), instr(NOP, IMP, 2), instr(XXX, IMP, 2), instr(CPX, ABS, 4), instr(SBC, ABS, 4), instr(INC, ABS, 6), instr(XXX, IMP, 2),
                instr(BEQ, REL, 2), instr(SBC, IZY, 5), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(SBC, ZPX, 4), instr(INC, ZPX, 6), instr(XXX, IMP, 2), instr(SED, IMP, 2), instr(SBC, ABY, 4), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(XXX, IMP, 2), instr(SBC, ABX, 4), instr(INC, ABX, 7), instr(XXX, IMP, 2)
            };
        }

        public void ConnectToBus(Bus bus)
        {
            this.Bus = bus;
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

        #region Instructions
        /// <summary>
        /// Dummy instruction
        /// </summary>
        /// <returns></returns>
        byte XXX() { throw new NotImplementedException(); }
        byte ADC() {
            this.GetAluInput();
            return 1;
        }
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
        /// <summary>
        /// Function: Shift one bit left (memory or accumulator).
        /// </summary>
        /// <returns></returns>
        byte ASL()
        {
            this.GetAluInput();
            var isMsbSet = (this.aluInput & 0x1000) > 0;
            var temp = (byte)(this.aluInput << 1);
            this.SetFlag(FLAGS6502.C, isMsbSet);
            this.SetFlag(FLAGS6502.Z, IsZero(temp));
            this.SetFlag(FLAGS6502.N, IsNegative(temp));

            if (this.InstructionSetOpcodeMatrix[this.Opcode].AddressMode == IMP)
            {
                this.A = temp;
            }
            else
            {
                this.Bus.Write(this.absoluteAddress, temp);
            }

            return 0;
        }
        byte BCC() { throw new NotImplementedException(); }
        byte BCS() { throw new NotImplementedException(); }
        byte BEQ() { throw new NotImplementedException(); }
        byte BIT() { throw new NotImplementedException(); }
        byte BMI() { throw new NotImplementedException(); }
        byte BNE() { throw new NotImplementedException(); }
        byte BPL() { throw new NotImplementedException(); }
        byte BRK() { throw new NotImplementedException(); }
        byte BVC() { throw new NotImplementedException(); }
        byte BVS() { throw new NotImplementedException(); }
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
            var temp = (byte)(this.A - this.aluInput);
            var isPageBoundaryReached = this.A >= temp;
            var isAccumulatorZero = (temp & 0xFF) == 0x00;
            var isAccumulatorNegative = (temp & 0x80) == 0x80; // Accumulator is negative if the most significant bit is set.
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
            byte temp = (byte)(this.X - this.aluInput);
            var isPageBoundaryReached = this.X >= temp;
            var isRegisterZero = (temp & 0xFF) == 0x00;
            var isRegisterNegative = (temp & 0x80) == 0x80; // X register is negative if the most significant bit is set.
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
            byte temp = (byte)(this.Y - this.aluInput);
            var isPageBoundaryCrossed = this.Y >= temp;
            var isRegisterZero = (temp & 0xFF) == 0x00;
            var isRegisterNegative = (temp & 0x80) == 0x80; // Y register is negative if the most significant bit is set.
            this.SetFlag(FLAGS6502.C, isPageBoundaryCrossed);
            this.SetFlag(FLAGS6502.Z, isRegisterZero);
            this.SetFlag(FLAGS6502.N, isRegisterNegative);
            return 0;
        }
        /// <summary>
        /// Instruction: Decrement value in memory.
        /// Function: M - 1
        /// </summary>
        /// <returns></returns>
        byte DEC() {
            this.GetAluInput();
            this.aluInput--;
            this.Bus.Write(this.absoluteAddress, this.aluInput);
            return 0;
        }
        /// <summary>
        /// Instruction: Decrement value of X register.
        /// Function: X - 1
        /// </summary>
        /// <returns></returns>
        byte DEX()
        {
            this.X--;
            var isRegisterZero = (this.X & 0xFF) == 0x00;
            var isRegisterNegative = (this.X & 0x80) == 0x80;
            this.SetFlag(FLAGS6502.Z, isRegisterZero);
            this.SetFlag(FLAGS6502.N, isRegisterNegative);
            return 0;
        }
        /// <summary>
        /// Instruction: Decrement value of Y register.
        /// Function: Y - 1
        /// </summary>
        /// <returns></returns>
        byte DEY()
        {
            this.Y--;
            var isRegisterZero = (this.Y & 0xFF) == 0x00;
            var isRegisterNegative = (this.X & 0x80) == 0x80;
            this.SetFlag(FLAGS6502.Z, isRegisterZero);
            this.SetFlag(FLAGS6502.N, isRegisterNegative);
            return 0;
        }
        /// <summary>
        /// Instruction: Excluse OR memory with accumulator.
        /// Function: A ^ M
        /// </summary>
        /// <returns></returns>
        byte EOR()
        {
            this.GetAluInput();
            this.A = (byte)(this.A ^ this.aluInput);
            this.SetFlag(FLAGS6502.Z, IsZero(this.A));
            this.SetFlag(FLAGS6502.N, IsNegative(this.A));
            return 1;
        }
        /// <summary>
        /// Instruction: Increment data in memory.
        /// Function: M + 1
        /// </summary>
        /// <returns></returns>
        byte INC()
        {
            this.GetAluInput();
            var data = (byte)(this.aluInput + 1);
            this.Bus.Write(this.absoluteAddress, data);
            var isDataZero = (data & 0xFF) == 0x00;
            var isDataNegative = (data & 0x80) == 0x80;
            this.SetFlag(FLAGS6502.Z, isDataZero);
            this.SetFlag(FLAGS6502.N, isDataNegative);
            return 0;
        }
        /// <summary>
        /// Instruction: Increment value in X register.
        /// Function: X + 1
        /// </summary>
        /// <returns></returns>
        byte INX()
        {
            this.X++;
            var isRegisterZero = (this.X & 0xFF) == 0x00;
            var isRegisterNegative = (this.X & 0x80) == 0x80;
            this.SetFlag(FLAGS6502.Z, isRegisterZero);
            this.SetFlag(FLAGS6502.N, isRegisterNegative);
            return 0;
        }
        /// <summary>
        /// Instruction: Increment value in Y register.
        /// Function: Y + 1
        /// </summary>
        /// <returns></returns>
        byte INY()
        {
            this.Y++;
            var isRegisterZero = (this.Y & 0xFF) == 0x00;
            var isRegisterNegative = (this.Y & 0x80) == 0x80;
            this.SetFlag(FLAGS6502.Z, isRegisterZero);
            this.SetFlag(FLAGS6502.N, isRegisterNegative);
            return 0;
        }
        /// <summary>
        /// Instruction: Jump to a new address.
        /// </summary>
        /// <returns></returns>
        byte JMP()
        {
            this.ProgramCounter = this.absoluteAddress;
            return 0;
        }
        /// <summary>
        /// Instruction: Jump to sub-routine.
        /// Function: Decrement program counter and push it to the stack. The decremented is the address - 1 which is the return address to go to after JSR. The return is performed by the RTS instruction.
        /// Keep in mind that an address is 16 bit while the stack is 8 bit.
        /// We will therefore have to first write the high byte of the 16 bit address to the stack, decrement the stack pointer and write the low byte of the 16 bit address to the stack.
        /// We do it in this order because a stack can only be read from the top and downwards. When the address is retrieved from the stack we expect to first encounter the high byte, then the low byte and combine these to get the 16 bit address.
        /// </summary>
        /// <returns></returns>
        byte JSR()
        {
            this.ProgramCounter--;
            ushort addressInStackPointer = (ushort)(STACK_POINTER_BOTTOM + this.StackPointer);

            // We shift the 16 bit address 8 times to the right to make sure that the high byte is in the right spot.
            // After shifting to the right the high byte is now in the 8 right most bits instead of in the 8 left most bits.
            // We then apply a bit mask because it's a good habit for ensuring we don't get any unwanted bits.
            byte highByteOfReturnAddress = (byte)((this.ProgramCounter >> 8) & 0x00FF);
            byte lowByteOfReturnAddress = (byte)(this.ProgramCounter & 0x00FF); // By masking the program counter we ensure that we only get the 

            this.Bus.Write(addressInStackPointer, highByteOfReturnAddress);
            this.StackPointer--; // We decrement the stack pointer so that we can write the low byte of the return address underneath the stack position in which we wrote the high byte of the return address.
            addressInStackPointer = (ushort)(STACK_POINTER_BOTTOM + this.StackPointer);
            this.Bus.Write(addressInStackPointer, lowByteOfReturnAddress);

            this.ProgramCounter = this.absoluteAddress; // We store the memory address to jump to in the program counter to transfer program control to that memory address.

            return 0;
        }
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
        /// <summary>
        /// Instruction: Shift one bit right (memory or accumulator)
        /// Function: M >> 1
        /// </summary>
        /// <returns></returns>
        byte LSR()
        {
            this.GetAluInput();
            byte temp = (byte)(this.aluInput >> 1);
            var isBitZeroSet = (this.aluInput & 0x0001) > 0;
            var tempIsZero = temp == 0x00;
            var tempIsNegative = (temp & 0x80) > 0;
            this.SetFlag(FLAGS6502.C, isBitZeroSet);
            this.SetFlag(FLAGS6502.Z, tempIsZero);
            this.SetFlag(FLAGS6502.N, tempIsNegative);

            if (this.InstructionSetOpcodeMatrix[this.Opcode].AddressMode == IMP)
            {
                this.A = temp;
            }
            else
            {
                this.Bus.Write(this.absoluteAddress, temp);
            }

            return 1;
        }
        /// <summary>
        /// Instruction: No operation.
        /// </summary>
        /// <returns></returns>
        byte NOP() { return 0; }
        /// <summary>
        /// Instruction: OR memory with accumulator
        /// Function: A |= M
        /// </summary>
        /// <returns></returns>
        byte ORA()
        {
            this.GetAluInput();
            this.A |= this.aluInput;
            var isAccumulatorZero = this.A == 0x00;
            var isAccumulatorNegative = (this.A & 0x80) > 0;
            this.SetFlag(FLAGS6502.Z, isAccumulatorZero);
            this.SetFlag(FLAGS6502.N, isAccumulatorNegative);
            return 0;
        }
        /// <summary>
        /// Instruction: Push accumulator on stack.
        /// Function: A -> StackPointer
        /// </summary>
        /// <returns></returns>
        byte PHA()
        {
            ushort addressInStackPointer = (ushort)(STACK_POINTER_BOTTOM + this.StackPointer);
            this.Bus.Write(addressInStackPointer, this.A);
            this.StackPointer--; // Always decrement the stack pointer after pushing to the stack.
            return 0;
        }
        /// <summary>
        /// Instruction: Push processor status on stack.
        /// Function: PS -> StackPointer
        /// </summary>
        /// <returns></returns>
        byte PHP()
        {
            ushort addressInStackPointer = (ushort)(STACK_POINTER_BOTTOM + this.StackPointer);
            byte temp = (byte)(this.ProcessorStatus | FLAGS6502.B | FLAGS6502.U); // The Break flag is always set when we happen upon this instruction. The U flag is always set as explained in the clock method.
            this.Bus.Write(addressInStackPointer, temp);

            // Naturally these flags are cleared after push the processor status on the stack.
            this.SetFlag(FLAGS6502.B, false);
            this.SetFlag(FLAGS6502.U, false);

            this.StackPointer--; // Always decrement the stack pointer after pushing to the stack.
            return 0;
        }
        /// <summary>
        /// Instruction: Pull accumulator from the stack.
        /// Function: A = StackPointer
        /// </summary>
        /// <returns></returns>
        byte PLA()
        {
            this.StackPointer++; // Always increment the stack pointer before pulling from it.
            ushort addressInStackPointer = (ushort)(STACK_POINTER_BOTTOM + this.StackPointer);
            this.A = this.Bus.Read(addressInStackPointer);
            var isAccumulatorZero = this.A == 0x00;
            var isAccumulatorNegative = (this.A & 0x80) > 0;
            this.SetFlag(FLAGS6502.Z, isAccumulatorZero);
            this.SetFlag(FLAGS6502.N, isAccumulatorNegative);
            return 0;
        }
        /// <summary>
        /// Instruction: Pull processor status from the stack.
        /// 
        /// </summary>
        /// <returns></returns>
        byte PLP()
        {
            this.StackPointer++; // Always increment the stack pointer before pulling from it.
            ushort addressInStackPointer = (ushort)(STACK_POINTER_BOTTOM + this.StackPointer);
            this.ProcessorStatus = this.Bus.Read(addressInStackPointer);
            this.SetFlag(FLAGS6502.U, true); // The U flag is always set as explained in the clock method.
            return 0;
        }
        /// <summary>
        /// Instruction: Rotate one bit left (memory or accumulator).
        /// </summary>
        /// <returns></returns>
        byte ROL()
        {
            this.GetAluInput();
            var temp = (byte)(this.aluInput << 1);
            var isMsbSet = (temp & 0x01) > 0;
            if (isMsbSet)
            {
                temp |= 0x0001;
                this.SetFlag(FLAGS6502.C, true);
            }
            var tempIsZero = temp == 0x00;
            var tempIsNegative = (temp & 0x80) > 0;
            this.SetFlag(FLAGS6502.Z, tempIsZero);
            this.SetFlag(FLAGS6502.N, tempIsNegative);

            if (this.InstructionSetOpcodeMatrix[this.Opcode].AddressMode == IMP)
            {
                this.A = temp;
            }
            else
            {
                this.Bus.Write(this.absoluteAddress, temp);
            }

            return 0;
        }
        /// <summary>
        /// Instruction: Rotate one bit right (memory or accumulator).
        /// </summary>
        /// <returns></returns>
        byte ROR()
        {
            this.GetAluInput();
            var temp = (byte)(this.aluInput >> 1);
            var isLsbSet = (temp & 0x0001) > 0;
            if(isLsbSet)
            {
                temp |= 0x10;
                this.SetFlag(FLAGS6502.C, true);
            }
            var tempIsZero = temp == 0x00;
            var tempIsNegative = (temp & 0x80) > 0;
            this.SetFlag(FLAGS6502.Z, tempIsZero);
            this.SetFlag(FLAGS6502.N, tempIsNegative);

            if(this.InstructionSetOpcodeMatrix[this.Opcode].AddressMode == IMP)
            {
                this.A = temp;
            }
            else
            {
                this.Bus.Write(this.absoluteAddress, temp);
            }

            return 0;
        }
        /// <summary>
        /// Function: Return from interrupt.
        /// </summary>
        /// <returns></returns>
        byte RTI()
        {
            this.StackPointer++; // Always increment the stack pointer before pulling from the stack.
            var addressInStackPointer = (ushort)(STACK_POINTER_BOTTOM + this.StackPointer);
            this.ProcessorStatus = this.Bus.Read(addressInStackPointer);
            this.SetFlag(FLAGS6502.B, false); // We're no longer breaking so we set this to false.
            this.SetFlag(FLAGS6502.U, false); // Isn't this always set? Why should this be false? Is it because we're returning from an interrupt?

            this.StackPointer++; // Always increment the stack pointer before pulling from the stack.
            addressInStackPointer = (ushort)(STACK_POINTER_BOTTOM + this.StackPointer);
            var temp = this.Bus.Read(addressInStackPointer);
            this.ProgramCounter = (byte)(temp << 8); // The program counter is 16 bit so we shift the byte from the stack into the high byte of the program counter or else it'd end up in the low byte, that'd be wrong.

            return 0;
        }
        /// <summary>
        /// Function: Return from subroutine.
        /// </summary>
        /// <returns></returns>
        byte RTS()
        {
            this.StackPointer++; // Always increment the stack pointer before pulling from the stack.
            var addressInStackPointer = (ushort)(STACK_POINTER_BOTTOM + this.StackPointer);
            var lowByte = this.Bus.Read(addressInStackPointer);

            this.StackPointer++; // Always increment the stack pointer before pulling from the stack.
            addressInStackPointer = (ushort)(STACK_POINTER_BOTTOM + this.StackPointer);
            var highByte = this.Bus.Read(addressInStackPointer);

            this.ProgramCounter = (ushort)(ConvertTo16BitValue(highByte, lowByte) + 1);

            return 0;
        }
        byte SBC() { throw new NotImplementedException(); }
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
        /// <summary>
        /// Instruction: Transfer A to X.
        /// Function: X = A
        /// </summary>
        /// <returns></returns>
        byte TAX()
        {
            this.X = this.A;
            this.SetFlag(FLAGS6502.Z, IsZero(this.X));
            this.SetFlag(FLAGS6502.N, IsNegative(this.X));
            return 0;
        }
        /// <summary>
        /// Instruction: Transfer A to Y.
        /// Function: Y = A
        /// </summary>
        /// <returns></returns>
        byte TAY()
        {
            this.Y = this.A;
            this.SetFlag(FLAGS6502.Z, IsZero(this.Y));
            this.SetFlag(FLAGS6502.N, IsNegative(this.Y));
            return 0;
        }
        /// <summary>
        /// Instruction: Transfer stack pointer to X.
        /// Function: X = stack pointer
        /// </summary>
        /// <returns></returns>
        byte TSX()
        {
            this.X = this.StackPointer;
            this.SetFlag(FLAGS6502.Z, IsZero(this.X));
            this.SetFlag(FLAGS6502.N, IsNegative(this.X));
            return 0;
        }
        /// <summary>
        /// Instruction: Transfer the value of the X register to the accumulator.
        /// Function: A = X
        /// </summary>
        /// <returns></returns>
        byte TXA() {
            this.A = X;
            this.SetFlag(FLAGS6502.Z, IsZero(this.A));
            this.SetFlag(FLAGS6502.N, IsNegative(this.A));
            return 0;
        }
        /// <summary>
        /// Instruction: Transfer the value of the X register to the stack pointer.
        /// Function: StackPointer = X
        /// </summary>
        /// <returns></returns>
        byte TXS()
        {
            this.StackPointer = this.X;
            this.SetFlag(FLAGS6502.Z, IsZero(this.StackPointer));
            this.SetFlag(FLAGS6502.N, IsNegative(this.StackPointer));
            return 0;
        }
        /// <summary>
        /// Instruction: Transfer the value of the Y regsiter to the accumulator.
        /// Function: A = Y
        /// </summary>
        /// <returns></returns>
        byte TYA()
        {
            this.A = this.Y;
            this.SetFlag(FLAGS6502.Z, IsZero(this.A));
            this.SetFlag(FLAGS6502.N, IsNegative(this.A));
            return 0;
        }
        #endregion
    }
}