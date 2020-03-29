using System.Linq;

namespace NESsie.Components
{
    public class Memory
    {
        private readonly int[] WRAM;
        private readonly Mapper Mapper;
        private readonly AudioProcessingUnit APU;
        private readonly PixelProcessingUnit PPU;

        public Memory(Mapper mapper)
        {
            this.Mapper = mapper;
            // The internal RAM of the console is 2KB - https://wiki.nesdev.com/w/index.php/CPU_memory_map
            this.WRAM = Enumerable.Repeat(0xff, 0x800).ToArray();
        }

        public void Write(int memoryAddress, int value)
        {
            this.WRAM[memoryAddress] = value;
        }
    }
}