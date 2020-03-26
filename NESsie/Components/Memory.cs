using System.Linq;

namespace NESsie.Components
{
    public class Memory
    {
        // The internal RAM of the console is 2KB - https://wiki.nesdev.com/w/index.php/CPU_memory_map
        public static readonly int MemorySize = 0x400f;

        private readonly int[] InternalRam;
        private readonly Mapper Mapper;
        private readonly AudioProcessingUnit APU;
        private readonly PixelProcessingUnit PPU;

        public Memory(Mapper mapper)
        {
            this.Mapper = mapper;
            this.InternalRam = Enumerable.Repeat(255, MemorySize).ToArray();
        }

        public void Write(int memoryAddress, int value)
        {
            this.InternalRam[memoryAddress] = value;
        }
    }
}