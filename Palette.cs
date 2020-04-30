using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace I_Robot
{
    static public class Palette
    {
        static public System.Drawing.Color[] RegularColor = new System.Drawing.Color[64];
        static public Microsoft.Xna.Framework.Graphics.Color[] XnaColor = new Microsoft.Xna.Framework.Graphics.Color[64];
        static Random Random = new Random();

        static Palette()
        {
            Add8Colors(System.Drawing.Color.White, 8 * 0);
            Add8Colors(System.Drawing.Color.Red, 8 * 1);
            Add8Colors(System.Drawing.Color.Orange, 8 * 2);
            Add8Colors(System.Drawing.Color.Yellow, 8 * 3);
            Add8Colors(System.Drawing.Color.Lime, 8 * 4);
            Add8Colors(System.Drawing.Color.Cyan, 8 * 5);
            Add8Colors(System.Drawing.Color.Blue, 8 * 6);
            Add8Colors(System.Drawing.Color.Purple, 8 * 7);
        }

        static void Add8Colors(System.Drawing.Color color, int index)
        {
            for (int n = 0; n < 8; n++)
            {
                double scale = n / 7.0;
                byte r = (byte)Math.Round(color.R * scale);
                byte g = (byte)Math.Round(color.G * scale);
                byte b = (byte)Math.Round(color.B * scale);
                RegularColor[index] = System.Drawing.Color.FromArgb(r, g, b);
                XnaColor[index++] = new Microsoft.Xna.Framework.Graphics.Color(r, g, b);
            }
        }

        static public void Cycle()
        {
            for (int n = 56; n <= 58; n++)
            {
                int r = Random.Next();
                RegularColor[n] = System.Drawing.Color.FromArgb(r & 0xFFFFFF);
                XnaColor[n] = new Microsoft.Xna.Framework.Graphics.Color((byte)r, (byte)(r >> 8), (byte)(r >> 16));
            }
        }
    }

}
