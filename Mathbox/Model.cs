using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using System.Windows.Media.Media3D;

namespace I_Robot.Mathbox
{
    public class Model3D : ModelBuilder
    {
        #region POLYGON

        // polygon object
        public class Polygon
        {
            public enum TYPE
            {
                Polygon = 0,
                Vector = 1,
                Dot = 2,
                Unknown = 3
            }

            public readonly UInt16 Address; // base address of polygon
            public readonly UInt16 IndexListAddress; // base address of index list for polygon
            public UInt16 ControlFlags; // control flags for the polygon
            public Vector3D? Normal = null; // only shaded polygons have a normal vector
            public List<Point3D> Points = new List<Point3D>();
            public bool IsValid { get; private set; }

            static internal bool TryCreatePolygon(ref UInt16 address, out Polygon polygon)
            {
                Polygon p = new Polygon(ref address);
                polygon = p.IsValid ? p : null;
                return p.IsValid | p.IndexListAddress == 0x8000;
            }

            private Polygon(ref UInt16 address)
            {
                Address = address;

                if (!ROM.TryRead(address++, out IndexListAddress))
                    return;

               //if (!ROM.TryRead(address++, out ControlFlags))
//                    return;

                if (IndexListAddress < 0x2000 || IndexListAddress >= 0x8000)
                    return; // failed

                // get polygon control flags
                ControlFlags = ROM[address++];

                IsValid = true;
            }



            static internal bool Parse(Mathbox.Model3D model, UInt16 address)
            {
                // scan forever until exit condition reached
                for (; ; )
                {
                    // ensure no duplicate polygons
                    foreach (Polygon p in model.mPolygons)
                    {
                        if (p.Address == address)
                            return true; // everything OK, we just can't add any more polys
                    }

                    // create and initialize a new polygon
                    if (!Polygon.TryCreatePolygon(ref address, out Polygon polygon))
                        return false;
                    if (polygon == null)
                        return true; // end of polygon list reached

                    // is this polygon drawn?
                    if ((polygon.ControlFlags & 0xB000) <= 0x8000)
                    {
                        // yes

                        // scan the vertex list
                        // first vertex specifies the (optional) normal vector
                        // remaining vertices are part of the polygon
                        int list = polygon.IndexListAddress;
                        if (list < 0x2000 || list >= 0x8000)
                            return false;
                        UInt16 offset = ROM[list++];
                        if ((offset & 0x4000) == 0)
                        {
                            // normal vector exists
                            Vector3D normal = model.Vector(offset);
                            normal.Normalize();
                            polygon.Normal = normal;
                        }
                        while (offset < 0x8000)
                        {
                            offset = ROM[list++];
                            polygon.Points.Add(model.Point(offset));
                        }
                        // int num_indices = list - polygon.IndexListAddress;

                        // make sure the polygon type matches the number of vertices
                        switch (polygon.Points.Count)
                        {
                            case 0: return false;
                            case 1:
                                // set to "DOT"
                                polygon.ControlFlags &= 0xFCFF;
                                polygon.ControlFlags |= (UInt16)TYPE.Dot << 8;
                                break;
                            case 2:
                                if (polygon.Type == TYPE.Polygon)
                                {
                                    polygon.ControlFlags &= 0xFCFF;
                                    polygon.ControlFlags |= (UInt16)TYPE.Vector << 8;
                                }
                                break;
                            default:
                                if (polygon.Type == TYPE.Vector)
                                    polygon.Points.Add(polygon.Points[0]); // close the vector list
                                break;
                        }

                        // shade certain objects
                        if (polygon.Type == TYPE.Polygon)
                        {
                            switch (model.Address)
                            {
                                case 0x3892: // eyeball
                                case 0x38A4: // eyeball
                                case 0x38B6: // eyeball
                                case 0x38C8: // eyeball
                                case 0x38DA: // eyeball
                                    if (polygon.ColorIndex > 0 && polygon.ColorIndex <= 7)
                                        polygon.ControlFlags |= 0x40; // make shaded
                                    break;
                                //case 0x5767: // transporter
                                //case 0x577B: // transporter
                                //case 0x5791: // transporter
                                case 0x5B56: // tanker
                                case 0x5B68: // spike
                                case 0x5B6E: // spike
                                case 0x5B72: // spike
                                case 0x5B8C: // spike
                                case 0x5BB5: // spike
                                case 0x5BE7: // spike
                                case 0x5C22: // spike
                                case 0x5F08: // colored "big ball"
                                case 0x692F: // hand
                                case 0x730A: // cube
                                case 0x7318: // cube
                                case 0x7326: // cube
                                case 0x7334: // cube
                                case 0x7342: // cube
                                case 0x7350: // cube
                                case 0x735E: // cube
                                             //case 0x755D: // dodecahedron
                                case 0x77D0: // ring
                                case 0x7DF4: // viewer killer
                                    polygon.ControlFlags |= 0x40; // make shaded
                                    break;
                            }
                        }

                        // fix objects
                        switch (model.Address)
                        {
                            case 0x2958: // robot visor
                            case 0x29EE: // robot visor
                            case 0x2A84: // robot visor
                            case 0x2B1A: // robot visor
                            case 0x2C00: // robot visor
                                if (polygon.ColorIndex == 0x38 || polygon.ColorIndex == 0x39)
                                {
                                    for (int n = 0; n < polygon.Points.Count; n++)
                                        polygon.Points[n] = polygon.Points[n] + new Vector3D(-0.1, 0, 0);
                                }
                                break;
                            case 0x51E0: // bird eye
                            case 0x5234: // bird eye
                            case 0x5288: // bird eye
                                if (polygon.ColorIndex == 0x38 || polygon.ColorIndex == 0x39)
                                {
                                    for (int n = 0; n < polygon.Points.Count; n++)
                                    {
                                        polygon.Points[n] = new Point3D(
                                            polygon.Points[n].X * 15f / 18f,
                                            polygon.Points[n].Y - 6f,
                                            polygon.Points[n].Z + 3f);
                                    }
                                }
                                break;
                        }

                        model.mPolygons.Add(polygon);
                        if (polygon.Shaded && polygon.Type == TYPE.Polygon)
                            model.mShadedPolygons.Add(polygon);
                        else
                            model.mUnshadedPolygons.Add(polygon);
                    }

                    // manage branching
                    if (polygon.ControlFlags >= 0x8000)
                    {
                        // find address of branch
                        UInt16 branch_addr = address;
                        branch_addr += ROM[address++];

                        // what type of branch is this
                        switch (polygon.ControlFlags & 0x3000)
                        {
                            case 0x0000: // branch always
                                address = branch_addr; // take the branch
                                break;
                            case 0x1000: // branch if surface visible
                            case 0x2000: // branch if surface hidden
                            case 0x3000: // branch never (never draw)
                                         // parse branch
                                if (!Parse(model, branch_addr))
                                    return false; // something went wrong
                                break; // now continue parsing the original branch
                        }
                    }
                }
            }

            public int ColorIndex { get { return ControlFlags & 0x3F; } }
            public bool Shaded { get { return (ControlFlags & 0x40) != 0; } }
            public TYPE Type { get { return (TYPE)((ControlFlags >> 8) & 3); } }

            public Material Material
            {
                get
                {
                    int index = ColorIndex;
                    if (Shaded)
                        index |= 7;
                    return Palette.DiffuseMaterial[index];
                }
            }

            public Material EmissiveMaterial
            {
                get
                {
                    int index = ColorIndex;
                    if (Shaded)
                        index |= 7;
                    return Palette.EmissiveMaterial[index];
                }
            }
        }

#endregion


#region STATIC

        // ROM file shared by all objects
        static Mathbox.Memory ROM = new Mathbox.Memory();

        // dictionary for quick lookup of previously parsed objects
        static Model3D[] Models = new Model3D[0x8000];

        // list of all valid mathbox objects
        static public readonly IReadOnlyList<Model3D> ModelList;

        static Model3D()
        {
            List<Model3D> list = new List<Model3D>();

            // try to build all objects in the mathbox
            for (UInt16 address = 0x2000; address < 0x8000; address++)
            {
                Models[address] = TryGetModelAt(address);
                if (Models[address] != null)
                    list.Add(Models[address]);
            }

            ModelList = list;
        }

        static public Model3D GetModelAt(UInt16 address)
        {
            return Models[address];
        }

        static Mathbox.Model3D TryGetModelAt(UInt16 address)
        {
            // don't bother with these addresses
            if (address == 0x40DC || address == 0x4344)
                return null;
            if (!ROM.TryRead(address, out UInt16 vertex_base))
                return null;
            if (vertex_base < 0x2000 || vertex_base >= 0x8000)
                return null;

            // at this point we should try and parse an object
            // the parsing could fail, in which case the catch block will catch us
            try
            {
                Model3D model = new Mathbox.Model3D(address, vertex_base);
                return model.IsValid ? model : null;
            }
            catch
            {
                return null;
            }
        }
#endregion

        // base address of mesh in ROM
        public readonly UInt16 Address;

        // address of vertices in ROM
        public readonly UInt16 VertexBase;

        // list of polygons that comprise this object
        List<Polygon> mPolygons = new List<Polygon>();
        List<Polygon> mShadedPolygons = new List<Polygon>();
        List<Polygon> mUnshadedPolygons = new List<Polygon>();
        internal bool IsValid { get; private set; }

        // private constructor -- must call Parse() to create an object
        private Model3D(UInt16 address, UInt16 vertex_base)
        {
            Address = address;
            VertexBase = vertex_base;

            if (!Polygon.Parse(this, (UInt16)(address + 1)))
                return; // toss the object
            if (mPolygons.Count == 0)
                return; // toss the object

            IsValid = true;

            // remove duplicate polygons

            // build the model
            foreach (Model3D.Polygon polygon in mPolygons)
            {
                // add triangles to model
                switch (polygon.Type)
                {
                    case Model3D.Polygon.TYPE.Dot:
                        foreach(Point3D p in polygon.Points)
                            AddPoint(p, 2, polygon.Material);
                        break;
                    case Model3D.Polygon.TYPE.Vector:
                        for (int n=1; n < polygon.Points.Count; n++)
                        {
                            AddLine(polygon.Points[n], polygon.Points[n - 1], 3, polygon.Material);
                        }
                        break;
                    case Model3D.Polygon.TYPE.Polygon:
                        if (polygon.Normal.HasValue)
                            AddTriangleFan(polygon.Points, polygon.Normal.Value, polygon.Material);
                        else
                            AddTriangleFan(polygon.Points, polygon.Material);
                        break;
                }
            }
        }

#if false
        public void Render(XnaView view)
        {
            // render unshaded

            if (UnshadedPolygons.Count > 0)
            {
                view.PrimitiveBatch3D.Effect.LightingEnabled = false;
                view.PrimitiveBatch3D.Effect.CommitChanges();
                foreach (Polygon p in UnshadedPolygons)
                {
                    // render based on polygon type
                    switch (p.Type)
                    {
                        case Mathbox.Model3D.PolygonType.Dot:
                            view.Device.DrawUserPrimitives<VertexPositionColorNormal>(PrimitiveType.PointList, p.XnaVertices, 0, p.XnaVertices.Length);
                            break;
                        case Mathbox.Model3D.PolygonType.Vector:
                            view.Device.DrawUserPrimitives<VertexPositionColorNormal>(PrimitiveType.LineStrip, p.XnaVertices, 0, p.XnaVertices.Length - 1);
                            break;
                        case Mathbox.Model3D.PolygonType.Polygon:
                            view.Device.DrawUserPrimitives<VertexPositionColorNormal>(PrimitiveType.TriangleFan, p.XnaVertices, 0, p.XnaVertices.Length - 2);
                            break;
                    }
                }
            }

            // render shaded
            if (ShadedPolygons.Count > 0)
            {
                view.PrimitiveBatch3D.Effect.LightingEnabled = true;
                view.PrimitiveBatch3D.Effect.CommitChanges();
                foreach (Polygon p in ShadedPolygons)
                {
                    // render based on polygon type
                    switch (p.Type)
                    {
                        case Mathbox.Model3D.PolygonType.Dot:
                            view.Device.DrawUserPrimitives<VertexPositionColorNormal>(PrimitiveType.PointList, p.XnaVertices, 0, p.XnaVertices.Length);
                            break;
                        case Mathbox.Model3D.PolygonType.Vector:
                            view.Device.DrawUserPrimitives<VertexPositionColorNormal>(PrimitiveType.LineStrip, p.XnaVertices, 0, p.XnaVertices.Length - 1);
                            break;
                        case Mathbox.Model3D.PolygonType.Polygon:
                            view.Device.DrawUserPrimitives<VertexPositionColorNormal>(PrimitiveType.TriangleFan, p.XnaVertices, 0, p.XnaVertices.Length - 2);
                            break;
                    }
                }
            }
            view.PrimitiveBatch3D.End();

            view.Device.RenderState.PointSpriteEnable = false;
        }
#endif

        Point3D Point(UInt16 offset)
        {
            offset &= 0x3FFF;
            UInt16 address = (UInt16)(offset + VertexBase);

            return new Point3D(
                (Int16)(ROM[address++]),
                (Int16)(ROM[address++]),
                (Int16)(ROM[address++]));
        }

        Vector3D Vector(UInt16 offset)
        {
            offset &= 0x3FFF;
            UInt16 address = (UInt16)(offset + VertexBase);

            return new Vector3D(
             (Int16)(ROM[address++]),
             (Int16)(ROM[address++]),
             (Int16)(ROM[address++]));
        }

        public override string ToString()
        {
            return Address.ToString("X4");
        }
    }

}
