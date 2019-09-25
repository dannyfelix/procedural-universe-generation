using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Universe
{
    internal class DirectBitmap : IDisposable
    {
        public DirectBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Bits = new int[width * height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb,
                BitsHandle.AddrOfPinnedObject());
        }

        private Bitmap Bitmap { get; }
        public int[] Bits { get; }
        public bool Disposed { get; private set; }
        public int Height { get; }
        public int Width { get; }

        protected GCHandle BitsHandle { get; }

        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            Bitmap.Dispose();
            BitsHandle.Free();
        }

        public void SetPixel(int x, int y, Color colour)
        {
            var index = x + y * Width;
            var col = colour.ToArgb();

            Bits[index] = col;
        }

        public Color GetPixel(int x, int y)
        {
            var index = x + y * Width;
            var col = Bits[index];
            var result = Color.FromArgb(col);

            return result;
        }

        public void Save(string path)
        {
            if (!Directory.Exists(Directory.GetParent(path).ToString())) Directory.CreateDirectory(Directory.GetParent(path).ToString());
            Bitmap.Save(path);
            Dispose();
        }
    }
}