using System;

namespace NESsie.Components
{
    public static class FLAGS6502
    {
        public static byte C = 1 << 0;
        public static byte Z = 1 << 1;
        public static byte I = 1 << 2;
        public static byte D = 1 << 3;
        public static byte B = 1 << 4;
        public static byte U = 1 << 5;
        public static byte V = 1 << 6;
        public static byte N = 1 << 7;
}
}
