using System;
using System.Collections.Generic;
using System.Text;

namespace NESsie.Components
{
    public class CpuInstruction
    {
        public Func<byte> Operate;
        public Func<byte> AddressMode;
        public ushort Cycles;

        public CpuInstruction(Func<byte> operation, Func<byte> addressMode, ushort cycles)
        {
            this.Operate = operation;
            this.AddressMode = addressMode;
            this.Cycles = cycles;
        }

        public override string ToString()
        {
            return $"({this.Operate.Method.Name}, {this.AddressMode.Method.Name}, {this.Cycles})";
        }
    }
}
