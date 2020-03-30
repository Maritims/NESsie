using System.Collections.Generic;

namespace NESsie
{
    public class ConsoleBuffer
    {
        private List<string> Lines = new List<string>();
        public int BufferWidth { get; private set; }
        public int BufferHeight { get; private set; }

        public ConsoleBuffer(int bufferWidth)
        {
            this.BufferWidth = bufferWidth;
        }

        public void AddToBuffer(string line)
        {
            this.Lines.Insert(0, line);
            if (this.Lines.Count == this.BufferHeight)
            {
                this.Lines.RemoveAt(this.BufferHeight - 1);
            }
        }

        public int GetLineCount()
        {
            return Lines.Count;
        }

        public string GetLine(int i)
        {
            return Lines[i];
        }
    }
}
