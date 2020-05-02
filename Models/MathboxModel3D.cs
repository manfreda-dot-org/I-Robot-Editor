using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace I_Robot
{
    public class MathboxModel3D : ModelBuilder
    {
        const double LINE_SIZE = 3;
        const double POINT_SIZE = 4;

        #region STATIC

        static Dictionary<Mathbox.Mesh, MathboxModel3D> Lookup = new Dictionary<Mathbox.Mesh, MathboxModel3D>();

        static MathboxModel3D()
        {
            foreach (var m in Mathbox.Mesh.MeshList)
                Lookup[m] = new MathboxModel3D(m);
        }

        static public bool TryGetModel(UInt16 address, out MathboxModel3D model)
        {
            model = null;
            if (Mathbox.Mesh.TryGetMeshAt(address, out var m))
                return TryGetModel(m, out model);
            return false;
        }

        static public bool TryGetModel(Mathbox.Mesh m, out MathboxModel3D model)
        {
            return Lookup.TryGetValue(m, out model);
        }
        
        #endregion


        Mathbox.Mesh mModel= null;

        private MathboxModel3D(UInt16 address)
        {
            if (Mathbox.Mesh.TryGetMeshAt(address, out var model))
                Model = model;
        }

        private MathboxModel3D(Mathbox.Mesh model)
        {
            Model = model;
        }

        /// <summary>
        /// Gets/sets the level that is used to build this model
        /// </summary>
        public Mathbox.Mesh Model
        {
            get { return mModel; }
            private set
            {
                // is the level changing?
                if (value != null && mModel != value)
                {
                    mModel = value;

                    // clear the group
                    Group.Children.Clear();

                    foreach (Mathbox.Mesh.Surface polygon in value)
                    {
                        // add triangles to model
                        switch (polygon.Type)
                        {
                            case Mathbox.Mesh.Surface.TYPE.Dot:
                                foreach (Point3D p in polygon.mPoints)
                                    AddPoint(p, POINT_SIZE, polygon.Material);
                                break;
                            case Mathbox.Mesh.Surface.TYPE.Vector:
                                for (int n = 1; n < polygon.mPoints.Count; n++)
                                    AddCylinder(polygon.mPoints[n], polygon.mPoints[n - 1], LINE_SIZE, 8, polygon.Material);
                                break;
                            case Mathbox.Mesh.Surface.TYPE.Polygon:
                                if (polygon.Normal.HasValue)
                                    AddTriangleFan(polygon.mPoints, polygon.Normal.Value, polygon.Material);
                                else
                                    AddTriangleFan(polygon.mPoints, polygon.Material);
                                break;
                        }
                    }
                }
            }
        }

    }
}
