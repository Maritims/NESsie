using System;
using System.Collections.Generic;
using System.Text;

namespace NESsie.Components
{
    public partial class CPU6502
    {
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
            this.absoluteAddress &= 0xFF;
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
            this.absoluteAddress &= 0xFF;
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
            this.absoluteAddress &= 0xFF;
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
            if (addressPointerLowByte == 0xFF)
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
            var lowByte = this.Bus.Read((ushort)(pageZeroMemoryLocation & 0xFF));
            var highByte = this.Bus.Read((ushort)((pageZeroMemoryLocation + 1) & 0xFF));
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
            var lowByte = this.Bus.Read((ushort)(pageZeroMemoryLocation & 0xFF));
            var highByte = this.Bus.Read((ushort)((pageZeroMemoryLocation + 1) & 0xFF));
            this.absoluteAddress = this.ConvertTo16BitValue(highByte, lowByte);
            return 0;
        }
    }
}
