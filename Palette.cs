using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace I_Robot
{
    static public class Palette
    {
        static readonly Color W = Color.White;
        static readonly Color R = Color.Red;
        static readonly Color O = Color.Orange;
        static readonly Color Y = Color.Yellow;
        static readonly Color G = Color.Lime;
        static readonly Color C = Color.Cyan;
        static readonly Color B = Color.Blue;
        static readonly Color P = Color.Purple;

        static readonly List<Color[]> ColorGroup = new List<Color[]>();
        static public readonly System.Drawing.Color[] RegularColor = new System.Drawing.Color[64];
        static public readonly Microsoft.Xna.Framework.Graphics.Color[] XnaColor = new Microsoft.Xna.Framework.Graphics.Color[64];
        static readonly Random Random = new Random();

        static Palette()
        {
            // default I Robot palettes
            ColorGroup.Add(new System.Drawing.Color[8] { W, R, O, Y, G, C, B, P });
            ColorGroup.Add(new System.Drawing.Color[8] { P, Y, B, W, C, R, G, O });
            ColorGroup.Add(new System.Drawing.Color[8] { R, B, Y, C, P, O, W, G });
            ColorGroup.Add(new System.Drawing.Color[8] { Y, P, G, W, C, R, O, B });
            ColorGroup.Add(new System.Drawing.Color[8] { R, Y, W, C, B, G, O, P });
            ColorGroup.Add(new System.Drawing.Color[8] { G, B, P, O, R, Y, C, W });

            SetColorGroup(0);
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

        static public void SetColorGroup(int group)
        {
            Color[] c = ColorGroup[group];

            for (int n=0; n<8; n++)
                Add8Colors(c[n], 8 * n);
        }

        static public void CycleColors()
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
