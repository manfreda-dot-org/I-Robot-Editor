using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace I_Robot
{
    class StarfieldModel3D : ModelBuilder
    {
        public StarfieldModel3D()
        {
            Material m = new EmissiveMaterial(new SolidColorBrush(Colors.White));
            Random r = new Random();
            for (int n = 0; n < 500; n++)
            {
                double theta = Math.PI * 2 * r.NextDouble();
                double phi = Math.Acos(2 * r.NextDouble() - 1);

                double x = Math.Cos(theta) * Math.Sin(phi);
                double y = Math.Sin(theta) * Math.Sin(phi);
                double z = Math.Cos(phi);

                Vector3D normal = new Vector3D(x, y, z);
                normal.Normalize();

                Vector3D v2 = Vector3D.CrossProduct(normal, new Vector3D(0, 1, 0));
                v2.Normalize();
                Vector3D v3 = Vector3D.CrossProduct(normal, v2);
                v3.Normalize();

                v2 *= 0.003;
                v3 *= 0.003;

                Point3D p0 = new Point3D(normal.X, normal.Y, normal.Z);
                Point3D p1 = p0 + v2;
                Point3D p2 = p1 + v3;
                Point3D p3 = p2 - v2;
                AddRectangle(p0, p1, p2, p3, m);

                byte b = (byte)r.Next();
                m = new EmissiveMaterial(new SolidColorBrush(Color.FromRgb(b,b,b)));
            }
        }
    }
}
