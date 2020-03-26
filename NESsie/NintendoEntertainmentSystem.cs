using NESsie.Components;

namespace NESsie
{
    public class NintendoEntertainmentSystem
    {
        public readonly CPU6502 CPU;
        public readonly PixelProcessingUnit PPU;
        public readonly AudioProcessingUnit APU;
        public readonly Memory Memory;

        public NintendoEntertainmentSystem()
        {
            this.CPU = new CPU6502();
        }
    }
}