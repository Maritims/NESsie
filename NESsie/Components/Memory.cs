using System.Linq;

namespace NESsie.Components
{
    public class Memory
    {
        private readonly int[] InternalRam;
        private readonly Mapper Mapper;
        private readonly AudioProcessingUnit APU;
        private readonly PixelProcessingUnit PPU;

        public Memory(Mapper mapper)
        {
            this.Mapper = mapper;
            this.InternalRam = Enumerable.Repeat(0xff, 2048).ToArray();
        }
    }
}