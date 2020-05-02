using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace I_Robot
{
    static public class Palette
    {
        static readonly Color W = Colors.White;
        static readonly Color R = Colors.Red;
        static readonly Color O = Colors.Orange;
        static readonly Color Y = Colors.Yellow;
        static readonly Color G = Colors.Lime;
        static readonly Color C = Colors.Cyan;
        static readonly Color B = Colors.Blue;
        static readonly Color P = Colors.Purple;

        static readonly List<Color[]> ColorGroup = new List<Color[]>();
        static public readonly Color[] Color = new Color[64];
        static public readonly SolidColorBrush[] Brush = new SolidColorBrush[64];
        static public readonly DiffuseMaterial[] DiffuseMaterial = new DiffuseMaterial[64];
        static public readonly EmissiveMaterial[] EmissiveMaterial = new EmissiveMaterial[64];
        static readonly Random Random = new Random();

        static Palette()
        {
            // default I Robot palettes
            ColorGroup.Add(new Color[8] { W, R, O, Y, G, C, B, P });
            ColorGroup.Add(new Color[8] { P, Y, B, W, C, R, G, O });
            ColorGroup.Add(new Color[8] { R, B, Y, C, P, O, W, G });
            ColorGroup.Add(new Color[8] { Y, P, G, W, C, R, O, B });
            ColorGroup.Add(new Color[8] { R, Y, W, C, B, G, O, P });
            ColorGroup.Add(new Color[8] { G, B, P, O, R, Y, C, W });

            SetColorGroup(0);
        }

        static void Add8Colors(Color color, int index)
        {
            for (int n = 0; n < 8; n++)
            {
                double scale = n / 7.0;
                byte r = (byte)Math.Round(color.R * scale);
                byte g = (byte)Math.Round(color.G * scale);
                byte b = (byte)Math.Round(color.B * scale);
                Color[index] = System.Windows.Media.Color.FromRgb(r, g, b);
                Brush[index] = new SolidColorBrush(Color[index]);
                DiffuseMaterial[index] = new DiffuseMaterial(Brush[index]);
                EmissiveMaterial[index] = new EmissiveMaterial(Brush[index]);
                index++;
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
                Color c = System.Windows.Media.Color.FromRgb((byte)Random.Next(), (byte)Random.Next(), (byte)Random.Next());
                Color[n] = c;
                Brush[n].Color = c;
                DiffuseMaterial[n].Color = c;
                EmissiveMaterial[n].Color = c;
            }
        }
    }

}
