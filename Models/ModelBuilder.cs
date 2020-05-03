using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace I_Robot
{
    public class ModelBuilder
    {
        readonly protected Model3DGroup Group = new Model3DGroup();

        static readonly MeshGeometry3D DotMesh = IcoSphereCreator.CreateIcosahedron(1);

        public class IcoSphereCreator
        {
            private struct TriangleIndices
            {
                public int v1;
                public int v2;
                public int v3;

                public TriangleIndices(int v1, int v2, int v3)
                {
                    this.v1 = v1;
                    this.v2 = v2;
                    this.v3 = v3;
                }
            }

            private MeshGeometry3D geometry;
            private int index;
            private Dictionary<Int64, int> middlePointIndexCache;

            // add vertex to mesh, fix position to be on unit sphere, return index
            private int addVertex(Point3D p)
            {
                Vector3D normal = new Vector3D(p.X, p.Y, p.Z);
                normal.Normalize();
                geometry.Positions.Add(new Point3D(normal.X, normal.Y, normal.Z));
                geometry.Normals.Add(-normal);
                return index++;
            }

            // return index of point in the middle of p1 and p2
            private int getMiddlePoint(int p1, int p2)
            {
                // first check if we have it already
                bool firstIsSmaller = p1 < p2;
                Int64 smallerIndex = firstIsSmaller ? p1 : p2;
                Int64 greaterIndex = firstIsSmaller ? p2 : p1;
                Int64 key = (smallerIndex << 32) + greaterIndex;

                int ret;
                if (this.middlePointIndexCache.TryGetValue(key, out ret))
                {
                    return ret;
                }

                // not in cache, calculate it
                Point3D point1 = this.geometry.Positions[p1];
                Point3D point2 = this.geometry.Positions[p2];
                Point3D middle = new Point3D(
                    (point1.X + point2.X) / 2.0,
                    (point1.Y + point2.Y) / 2.0,
                    (point1.Z + point2.Z) / 2.0);

                // add vertex makes sure point is on unit sphere
                int i = addVertex(middle);

                // store it, return index
                this.middlePointIndexCache.Add(key, i);
                return i;
            }

            public MeshGeometry3D Create(int recursionLevel)
            {
                this.geometry = new MeshGeometry3D();
                this.middlePointIndexCache = new Dictionary<long, int>();
                this.index = 0;

                // create 12 vertices of a icosahedron
                var t = (1.0 + Math.Sqrt(5.0)) / 2.0;

                addVertex(new Point3D(-1, t, 0));
                addVertex(new Point3D(1, t, 0));
                addVertex(new Point3D(-1, -t, 0));
                addVertex(new Point3D(1, -t, 0));

                addVertex(new Point3D(0, -1, t));
                addVertex(new Point3D(0, 1, t));
                addVertex(new Point3D(0, -1, -t));
                addVertex(new Point3D(0, 1, -t));

                addVertex(new Point3D(t, 0, -1));
                addVertex(new Point3D(t, 0, 1));
                addVertex(new Point3D(-t, 0, -1));
                addVertex(new Point3D(-t, 0, 1));


                // create 20 triangles of the icosahedron
                var faces = new List<TriangleIndices>();

                // 5 faces around point 0
                faces.Add(new TriangleIndices(0, 11, 5));
                faces.Add(new TriangleIndices(0, 5, 1));
                faces.Add(new TriangleIndices(0, 1, 7));
                faces.Add(new TriangleIndices(0, 7, 10));
                faces.Add(new TriangleIndices(0, 10, 11));

                // 5 adjacent faces 
                faces.Add(new TriangleIndices(1, 5, 9));
                faces.Add(new TriangleIndices(5, 11, 4));
                faces.Add(new TriangleIndices(11, 10, 2));
                faces.Add(new TriangleIndices(10, 7, 6));
                faces.Add(new TriangleIndices(7, 1, 8));

                // 5 faces around point 3
                faces.Add(new TriangleIndices(3, 9, 4));
                faces.Add(new TriangleIndices(3, 4, 2));
                faces.Add(new TriangleIndices(3, 2, 6));
                faces.Add(new TriangleIndices(3, 6, 8));
                faces.Add(new TriangleIndices(3, 8, 9));

                // 5 adjacent faces 
                faces.Add(new TriangleIndices(4, 9, 5));
                faces.Add(new TriangleIndices(2, 4, 11));
                faces.Add(new TriangleIndices(6, 2, 10));
                faces.Add(new TriangleIndices(8, 6, 7));
                faces.Add(new TriangleIndices(9, 8, 1));


                // refine triangles
                for (int i = 0; i < recursionLevel; i++)
                {
                    var faces2 = new List<TriangleIndices>();
                    foreach (var tri in faces)
                    {
                        // replace triangle by 4 triangles
                        int a = getMiddlePoint(tri.v1, tri.v2);
                        int b = getMiddlePoint(tri.v2, tri.v3);
                        int c = getMiddlePoint(tri.v3, tri.v1);

                        faces2.Add(new TriangleIndices(tri.v1, a, c));
                        faces2.Add(new TriangleIndices(tri.v2, b, a));
                        faces2.Add(new TriangleIndices(tri.v3, c, b));
                        faces2.Add(new TriangleIndices(a, b, c));
                    }
                    faces = faces2;
                }

                // done, now add triangles to mesh
                foreach (var tri in faces)
                {
                    this.geometry.TriangleIndices.Add(tri.v1);
                    this.geometry.TriangleIndices.Add(tri.v2);
                    this.geometry.TriangleIndices.Add(tri.v3);
                }

                return this.geometry;
            }

            static public MeshGeometry3D CreateIcosahedron(int recursionLevel)
            {
                return new IcoSphereCreator().Create(recursionLevel);
            }
        }


        public ModelBuilder()
        {
        }

        public static implicit operator Model3DGroup(ModelBuilder obj)
        {
            return obj.Group;
        }

        public static implicit operator ModelVisual3D(ModelBuilder obj)
        {
            ModelVisual3D m = new ModelVisual3D();
            m.Content = obj.Group;
            return m;
        }

        protected void AddTriangle(Point3D p0, Point3D p1, Point3D p2, Color c)
        {
            AddTriangle(p0, p1, p2, new DiffuseMaterial(new SolidColorBrush(c)));
        }

        protected void AddTriangle(Point3D p0, Point3D p1, Point3D p2, Vector3D normal, Color c)
        {
            AddTriangle(p0, p1, p2, normal, new DiffuseMaterial(new SolidColorBrush(c)));
        }

        protected void AddTriangle(Point3D p0, Point3D p1, Point3D p2, Material material)
        {
            AddTriangle(p0, p1, p2, CalculateNormal(p0, p1, p2), material);
        }

        protected void AddTriangle(Point3D p0, Point3D p1, Point3D p2, Vector3D normal, Material material)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Positions.Add(p0);
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);
//            mesh.TriangleIndices.Add(0);
//            mesh.TriangleIndices.Add(1);
//            mesh.TriangleIndices.Add(2);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);

            GeometryModel3D model = new GeometryModel3D(mesh, material);
            model.BackMaterial = material;
            Group.Children.Add(model);
        }

        protected void AddTriangleFan(IReadOnlyList<Point3D> points, Material material)
        {
            AddTriangleFan(points, CalculateNormal(points[0], points[1], points[2]), material);
        }

        protected void AddTriangleFan(IReadOnlyList<Point3D> points, Vector3D normal, Material material)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            foreach (Point3D p in points)
                mesh.Positions.Add(p);
            int i = 2;
            do
            {
                mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(i - 1);
                mesh.TriangleIndices.Add(i++);
                mesh.Normals.Add(normal);
                mesh.Normals.Add(normal);
                mesh.Normals.Add(normal);
            } while (i < points.Count);

            GeometryModel3D model = new GeometryModel3D(mesh, material);
            model.BackMaterial = material;
            Group.Children.Add(model);
        }


        protected void AddRectangle(Point3D p0, Point3D p1, Point3D p2, Point3D p3, Color c)
        {
            AddRectangle(p0, p1, p2, p3, new DiffuseMaterial(new SolidColorBrush(c)));
        }

        protected void AddRectangle(Point3D p0, Point3D p1, Point3D p2, Point3D p3, Material material)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Positions.Add(p0);
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);
            mesh.Positions.Add(p0);
            mesh.Positions.Add(p2);
            mesh.Positions.Add(p3);
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(5);

            Vector3D normal = CalculateNormal(p0, p1, p2);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);

            GeometryModel3D model = new GeometryModel3D(mesh, material);
            model.BackMaterial = model.Material;
            Group.Children.Add(model);
        }


        static protected Vector3D CalculateNormal(Point3D p0, Point3D p1, Point3D p2)
        {
            Vector3D v0 = new Vector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            Vector3D v1 = new Vector3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            return Vector3D.CrossProduct(v1, v0);
        }

        protected void AddLine(Point3D a, Point3D b, double t, Color c)
        {
            AddLine(a, b, t, new DiffuseMaterial(new SolidColorBrush(c)));
        }

        protected void AddLine(Point3D a, Point3D b, double t, Material m)
        {
            Point3D p0 = new Point3D(a.X, a.Y, a.Z);
            Point3D p1 = new Point3D(a.X + t, a.Y, a.Z);
            Point3D p2 = new Point3D(b.X + t, b.Y, b.Z);
            Point3D p3 = new Point3D(b.X, b.Y, b.Z);
            Point3D p4 = new Point3D(a.X, a.Y + t, a.Z);
            Point3D p5 = new Point3D(a.X + t, a.Y + t, a.Z);
            Point3D p6 = new Point3D(b.X + t, b.Y + t, b.Z);
            Point3D p7 = new Point3D(b.X, b.Y + t, b.Z);
            //front side triangles
            AddTriangle(p3, p2, p6, m);
            AddTriangle(p3, p6, p7, m);
            //right side triangles
            AddTriangle(p2, p1, p5, m);
            AddTriangle(p2, p5, p6, m);
            //back side triangles
            AddTriangle(p1, p0, p4, m);
            AddTriangle(p1, p4, p5, m);
            //left side triangles
            AddTriangle(p0, p3, p7, m);
            AddTriangle(p0, p7, p4, m);
            //top side triangles
            AddTriangle(p7, p6, p5, m);
            AddTriangle(p7, p5, p4, m);
            //bottom side triangles
            AddTriangle(p2, p3, p0, m);
            AddTriangle(p2, p0, p1, m);
        }

        protected void AddCube(Point3D p0, double size, Color c)
        {
            AddCuboid(p0, size, size, size, new DiffuseMaterial(new SolidColorBrush(c)));
        }

        protected void AddCube(Point3D p0, double size, Material material)
        {
            AddCuboid(p0, size, size, size, material);
        }


        protected void AddCuboid(Point3D p0, double w, double h, double d, Color c)
        {
            AddCuboid(p0, w, h, d, new DiffuseMaterial(new SolidColorBrush(c)));
        }

        protected void AddCuboid(Point3D p0, double w, double h, double d, Material material)
        {
            Point3D p1 = new Point3D(w + p0.X, 0 + p0.Y, 0 + p0.Z);
            Point3D p2 = new Point3D(w + p0.X, 0 + p0.Y, d + p0.Z);
            Point3D p3 = new Point3D(0 + p0.X, 0 + p0.Y, d + p0.Z);
            Point3D p4 = new Point3D(0 + p0.X, h + p0.Y, 0 + p0.Z);
            Point3D p5 = new Point3D(w + p0.X, h + p0.Y, 0 + p0.Z);
            Point3D p6 = new Point3D(w + p0.X, h + p0.Y, d + p0.Z);
            Point3D p7 = new Point3D(0 + p0.X, h + p0.Y, d + p0.Z);

            //front
            AddTriangle(p3, p2, p6, material);
            AddTriangle(p3, p6, p7, material);

            //right
            AddTriangle(p2, p1, p5, material);
            AddTriangle(p2, p5, p6, material);

            //back
            AddTriangle(p1, p0, p4, material);
            AddTriangle(p1, p4, p5, material);

            //left
            AddTriangle(p0, p3, p7, material);
            AddTriangle(p0, p7, p4, material);

            //top
            AddTriangle(p7, p6, p5, material);
            AddTriangle(p7, p5, p4, material);

            //bottom
            AddTriangle(p2, p3, p0, material);
            AddTriangle(p2, p0, p1, material);
        }

        protected void AddPoint(Point3D p, double size, Material material)
        {
            Transform3DGroup transform = new Transform3DGroup();
            transform.Children.Add(new ScaleTransform3D(size, size, size));
            transform.Children.Add(new TranslateTransform3D(p.X, p.Y, p.Z));

            GeometryModel3D model = new GeometryModel3D(DotMesh, material);
            model.Transform = transform;
            Group.Children.Add(model);
        }

        protected void AddCylinder(Point3D point1, Point3D point2, double diameter, int num_sides, Material material)
        {
            double radius = diameter / 2;

            Vector3D axis = Point3D.Subtract(point2, point1);
            MeshGeometry3D mesh = new MeshGeometry3D();

            // Get two vectors perpendicular to the axis.
            Vector3D v1;
            if ((axis.Z < -0.01) || (axis.Z > 0.01))
                v1 = new Vector3D(axis.Z, axis.Z, -axis.X - axis.Y);
            else
                v1 = new Vector3D(-axis.Y - axis.Z, axis.X, axis.X);
            v1.Normalize();
            Vector3D v2 = Vector3D.CrossProduct(v1, axis); v2.Normalize();
            Vector3D axis_norm = axis; axis_norm.Normalize();

            // Make the vectors have length radius.
            v1 *= radius;
            v2 *= radius;
            Vector3D v3 = axis_norm * radius;

            Vector3D[] normals = new Vector3D[num_sides];
            Vector3D[] radials = new Vector3D[num_sides];
            for (int i = 0; i < num_sides; i++)
            {
                double theta = i * (2 * Math.PI / num_sides);
                Vector3D v = Math.Cos(theta) * v1 + Math.Sin(theta) * v2;
                radials[i] = v;
                v.Normalize();
                normals[i] = -v;
            }

            // Make the top end cap.
            // Make the end point.
            int pt0 = 0; // index of first point
            mesh.Positions.Add(point1 - v3);
            mesh.Normals.Add(axis_norm);

            // Make the top points.
            for (int i = 0; i < num_sides; i++)
            {
                mesh.Positions.Add(point1 + radials[i]);
                mesh.Normals.Add(normals[i]);
            }

            // Make the top triangles.
            int pt1 = mesh.Positions.Count - 1; // Index of last point.
            int pt2 = pt0 + 1;                  // Index of first point.
            for (int i = 0; i < num_sides; i++)
            {
                mesh.TriangleIndices.Add(pt0);
                mesh.TriangleIndices.Add(pt1);
                mesh.TriangleIndices.Add(pt2);
                pt1 = pt2++;
            }

            // Make the bottom end cap.
            // Make the end point.
            pt0 = mesh.Positions.Count; // Index of end_point2.
            mesh.Positions.Add(point2 + v3);
            mesh.Normals.Add(-axis_norm);

            // Make the bottom points.
            for (int i = 0; i < num_sides; i++)
            {
                mesh.Positions.Add(point2 + radials[i]);
                mesh.Normals.Add(normals[i]);
            }

            // Make the bottom triangles.
            pt1 = mesh.Positions.Count - 1; // Index of last point.
            pt2 = pt0 + 1;                  // Index of first point.
            for (int i = 0; i < num_sides; i++)
            {
                mesh.TriangleIndices.Add(num_sides + 1);    // end_point2
                mesh.TriangleIndices.Add(pt2);
                mesh.TriangleIndices.Add(pt1);
                pt1 = pt2++;
            }

            // Make the sides.
            // Add the points to the mesh.
            int first_side_point = mesh.Positions.Count;
            for (int i = 0; i < num_sides; i++)
            {
                Point3D p1 = point1 + radials[i];
                mesh.Positions.Add(p1);
                mesh.Normals.Add(normals[i]);
                Point3D p2 = p1 + axis;
                mesh.Positions.Add(p2);
                mesh.Normals.Add(normals[i]);
            }

            // Make the side triangles.
            pt1 = mesh.Positions.Count - 2;
            pt2 = pt1 + 1;
            int pt3 = first_side_point;
            int pt4 = pt3 + 1;
            for (int i = 0; i < num_sides; i++)
            {
                mesh.TriangleIndices.Add(pt1);
                mesh.TriangleIndices.Add(pt2);
                mesh.TriangleIndices.Add(pt4);

                mesh.TriangleIndices.Add(pt1);
                mesh.TriangleIndices.Add(pt4);
                mesh.TriangleIndices.Add(pt3);

                pt1 = pt3;
                pt3 += 2;
                pt2 = pt4;
                pt4 += 2;
            }

            GeometryModel3D model = new GeometryModel3D(mesh, material);
            model.BackMaterial = material;
            Group.Children.Add(model);
        }
    }
}