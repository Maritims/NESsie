using NESsie.Components;

namespace NESsie
{
    public class NintendoEntertainmentSystem
    {
        public readonly CPU6502 CPU;

        public NintendoEntertainmentSystem()
        {
            this.CPU = new CPU6502(new Memory(new Mapper()));
        }
    }
}