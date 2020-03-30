using System.Linq;

namespace NESsie.Components
{
    public class Memory
    {
        public readonly int Size;
        private readonly byte[] WRAM;

        public Memory()
        {
            this.Size = 64 * 1024;
            this.WRAM = Enumerable.Repeat<byte>(0xFF, this.Size).ToArray();
        }

        public byte Read(ushort address)
        {
            var data = this.WRAM[address];
            return data;
        }

        public void Write(ushort address, byte data)
        {
            this.WRAM[address] = data;
        }
    }
}