using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using System.IO;
using System.Collections;

namespace I_Robot.Mathbox
{
    public class Mesh
    {
        public enum PolygonType
        {
            Polygon = 0,
            Vector = 1,
            Dot = 2,
            Unknown = 3
        }

        // ROM file shared by all objects
        static Mathbox.Memory ROM = new Mathbox.Memory();

        // dictionary for quick lookup of previously parsed objects
        static Dictionary<UInt16, Mathbox.Mesh> Lookup = new Dictionary<ushort, Mesh>();

        // base address of mesh in ROM
        public readonly UInt16 Address;

        // address of vertices in ROM
        public readonly UInt16 VertexBase;

        // list of polygons that comprise this object
        public List<Polygon> Polygons = new List<Polygon>();
        public List<Polygon> ShadedPolygons = new List<Polygon>();
        public List<Polygon> UnshadedPolygons = new List<Polygon>();

        public List<UInt16> VertexAddressList = new List<UInt16>();

        // private constructor -- must call Parse() to create an object
        private Mesh(UInt16 address, UInt16 vertex_base)
        {
            Address = address;
            VertexBase = vertex_base;
        }

        // create a vertex declaration, which tells the graphics card what kind of
        // data to expect during a draw call. We're drawing using
        // VertexPositionColors, so we'll use those vertex elements.
        static VertexDeclaration VertexDeclaration = null;

        public void Render(XnaView view)
        {
            if (VertexDeclaration == null)
                VertexDeclaration = new VertexDeclaration(view.Device, VertexPositionColorNormal.VertexElements);

            view.Device.RenderState.PointSpriteEnable = true;
            view.Device.RenderState.PointSize = 5;

            view.PrimitiveBatch3D.Begin(VertexDeclaration);

            // render unshaded

            if (UnshadedPolygons.Count > 0)
            {
                view.PrimitiveBatch3D.Effect.LightingEnabled = false;
                view.PrimitiveBatch3D.Effect.CommitChanges();
                foreach (Polygon p in UnshadedPolygons)
                {
                    p.Recolor();
                    // render based on polygon type
                    switch (p.Type)
                    {
                        case Mathbox.Mesh.PolygonType.Dot:
                            view.Device.DrawUserPrimitives<VertexPositionColorNormal>(PrimitiveType.PointList, p.XnaVertices, 0, p.XnaVertices.Length);
                            break;
                        case Mathbox.Mesh.PolygonType.Vector:
                            view.Device.DrawUserPrimitives<VertexPositionColorNormal>(PrimitiveType.LineStrip, p.XnaVertices, 0, p.XnaVertices.Length - 1);
                            break;
                        case Mathbox.Mesh.PolygonType.Polygon:
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
                    p.Recolor();
                    // render based on polygon type
                    switch (p.Type)
                    {
                        case Mathbox.Mesh.PolygonType.Dot:
                            view.Device.DrawUserPrimitives<VertexPositionColorNormal>(PrimitiveType.PointList, p.XnaVertices, 0, p.XnaVertices.Length);
                            break;
                        case Mathbox.Mesh.PolygonType.Vector:
                            view.Device.DrawUserPrimitives<VertexPositionColorNormal>(PrimitiveType.LineStrip, p.XnaVertices, 0, p.XnaVertices.Length - 1);
                            break;
                        case Mathbox.Mesh.PolygonType.Polygon:
                            view.Device.DrawUserPrimitives<VertexPositionColorNormal>(PrimitiveType.TriangleFan, p.XnaVertices, 0, p.XnaVertices.Length - 2);
                            break;
                    }
                }
            }
            view.PrimitiveBatch3D.End();

            view.Device.RenderState.PointSpriteEnable = false;
        }

        // polygon object
        public class Polygon
        {
            public UInt16 Address; // base address of polygon
            public UInt16 IndexListAddress; // base address of index list for polygon
            public int IndexCount; // number of indices
            public UInt16 ControlFlags; // control flags for the polygon
            public Vector3? Normal = null; // only shaded polygons have a normal vector
            public List<Vector3> Vertices = new List<Vector3>();
            public VertexPositionColorNormal[] XnaVertices = null;

            private Polygon()
            {
            }

            public void Recolor()
            {
                int index = ColorIndex;
                if (index >= 56 && index <= 58)
                {
                    Microsoft.Xna.Framework.Graphics.Color color = Palette.XnaColor[index];
                    for (int n = 0; n < XnaVertices.Length; n++)
                        XnaVertices[n].Color = color;
                }
            }

            static public bool Parse(Mathbox.Mesh mesh, UInt16 address)
            {
                // scan forever until exit condition reached
                for (; ; )
                {
                    // ensure no duplicate polygons
                    foreach (Polygon p in mesh.Polygons)
                    {
                        if (p.Address == address)
                            return true; // everything OK, we just can't add any more polys
                    }

                    // create and initialize a new polygon
                    Polygon polygon = new Polygon();
                    polygon.Address = address;

                    // read address of polygon vertex list
                    polygon.IndexListAddress = ROM[address++];
                    if (polygon.IndexListAddress < 0x2000)
                        return false; // failed
                    if (polygon.IndexListAddress == 0x8000)
                        return true; // end of polygon reached

                    // get polygon control flags
                    polygon.ControlFlags = ROM[address++];

                    // is this polygon drawn?
                    if ((polygon.ControlFlags & 0xB000) <= 0x8000)
                    {
                        // yes

                        // scan the vertex list
                        // first vertex specifies the (optional) normal vector
                        // remaining vertices are part of the polygon
                        int list = polygon.IndexListAddress;
                        UInt16 offset = ROM[list++];
                        if ((offset & 0x4000) == 0)
                        {
                            // normal vector exists
                            Vector3 normal = mesh.Vertex(offset);
                            normal.Normalize();
                            polygon.Normal = normal;
                        }
                        while (offset < 0x8000)
                        {
                            offset = ROM[list++];
                            polygon.Vertices.Add(mesh.Vertex(offset));
                        }
                        polygon.IndexCount = list - polygon.IndexListAddress;

                        // make sure the polygon type matches the number of vertices
                        switch (polygon.Vertices.Count)
                        {
                            case 0: return false;
                            case 1:
                                // set to "DOT"
                                polygon.ControlFlags &= 0xFCFF;
                                polygon.ControlFlags |= (UInt16)PolygonType.Dot << 8;
                                break;
                            case 2:
                                if (polygon.Type == PolygonType.Polygon)
                                {
                                    polygon.ControlFlags &= 0xFCFF;
                                    polygon.ControlFlags |= (UInt16)PolygonType.Vector << 8;
                                }
                                break;
                            default:
                                if (polygon.Type == PolygonType.Vector)
                                    polygon.Vertices.Add(polygon.Vertices[0]); // close the vector list
                                break;
                        }

                        // shade certain objects
                        if (polygon.Type == PolygonType.Polygon)
                        {
                            switch (mesh.Address)
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

                        // now, build vertex buffer
                        polygon.XnaVertices = new VertexPositionColorNormal[polygon.Vertices.Count];
                        int index = 0;
                        foreach (Vector3 v in polygon.Vertices)
                        {
                            polygon.XnaVertices[index].Position = v;
                            polygon.XnaVertices[index].Color = polygon.XnaColor;
                            polygon.XnaVertices[index].Normal = polygon.Normal ?? new Vector3(0, 0, 1);
                            index++;
                        }

                        // fix objects
                        switch (mesh.Address)
                        {
                            case 0x2958: // robot visor
                            case 0x29EE: // robot visor
                            case 0x2A84: // robot visor
                            case 0x2B1A: // robot visor
                            case 0x2C00: // robot visor
                                if (polygon.ColorIndex == 0x38 || polygon.ColorIndex == 0x39)
                                {
                                    for (int n = 0; n < polygon.XnaVertices.Length; n++)
                                        polygon.XnaVertices[n].Position.X -= .1f;
                                }
                                break;
                            case 0x51E0: // bird eye
                            case 0x5234: // bird eye
                            case 0x5288: // bird eye
                                if (polygon.ColorIndex == 0x38 || polygon.ColorIndex == 0x39)
                                {
                                    for (int n = 0; n < polygon.XnaVertices.Length; n++)
                                    {
                                        polygon.XnaVertices[n].Position.X *= 15f / 18f;
                                        polygon.XnaVertices[n].Position.Y -= 6f;
                                        polygon.XnaVertices[n].Position.Z += 3f;
                                    }
                                }
                                break;
                        }

                        mesh.Polygons.Add(polygon);
                        if (polygon.Shaded && polygon.Type == PolygonType.Polygon)
                            mesh.ShadedPolygons.Add(polygon);
                        else
                            mesh.UnshadedPolygons.Add(polygon);
                    }

                    // manage branching
                    if (polygon.ControlFlags >= 0x8000)
                    {
                        // find address of branch
                        UInt16 branch = address;
                        branch += ROM[address++];

                        // what type of branch is this
                        switch (polygon.ControlFlags & 0x3000)
                        {
                            case 0x0000: // branch always
                                address = branch; // take the branch
                                break;
                            case 0x1000: // branch if surface visible
                            case 0x2000: // branch if surface hidden
                            case 0x3000: // branch never (never draw)
                                         // parse branch
                                if (!Parse(mesh, branch))
                                    return false; // something went wrong
                                break; // now continue parsing the original branch
                        }
                    }
                }
            }

            public int ColorIndex { get { return ControlFlags & 0x3F; } }
            public bool Shaded { get { return (ControlFlags & 0x40) != 0; } }
            public PolygonType Type { get { return (PolygonType)((ControlFlags >> 8) & 3); } }

            public System.Drawing.Color RegularColor
            {
                get
                {
                    int index = ColorIndex;
                    if (Shaded)
                        index += 7;
                    return Palette.RegularColor[index];
                }
            }
            public Microsoft.Xna.Framework.Graphics.Color XnaColor
            {
                get
                {
                    int index = ColorIndex;
                    if (Shaded)
                        index |= 7;
                    return Palette.XnaColor[index];
                }
            }
        }

        static public Mathbox.Mesh GetMeshAt(UInt16 address)
        {
            // save time if we've already parsed an object at this address
            if (Lookup.TryGetValue(address, out Mesh mesh))
                return mesh;
            mesh = TryGetMesh(address);
            Lookup[address] = mesh;
            return mesh;
        }

        static Mathbox.Mesh TryGetMesh(UInt16 address)
        {
            // don't bother with these addresses
            if (address == 0x40DC || address == 0x4344)
                return null;

            // at this point we should try and parse an object
            // the parsing could fail, in which case the catch block will catch us
            try
            {
                // sanity check -- verify address and vertex base
                if (address < 0x2000 || address >= 0x8000) return null;
                UInt16 vertex_base = ROM[address];
                if (vertex_base < 0x2000 || vertex_base >= 0x8000) return null;

                Mesh mesh = new Mathbox.Mesh(address, vertex_base);

                // parse polygons
                if (!Mathbox.Mesh.Polygon.Parse(mesh, (UInt16)(address + 1)))
                    return null; // toss the object
                if (mesh.Polygons.Count == 0)
                    return null; // toss the object

                // remove duplicate polygons
                //john TBD

                // this mesh is good

                // remember the mesh
                Lookup[address] = mesh;

                return mesh;
            }
            catch
            {
                return null;
            }
        }

        public void Dump(TextWriter stream)
        {
            stream.WriteLine($"Object 0x{Address.ToString("X4")} ({new Memory.CpuBankAddress(Address)})");
            stream.WriteLine($"\tVertices 0x{VertexBase.ToString("X4")} ({new Memory.CpuBankAddress(VertexBase)})");
            stream.WriteLine("\tPolygons = " + Polygons.Count);
            foreach (Polygon p in Polygons)
            {
                string s = $"\t\t 0x{p.Address.ToString("X4")} ({new Memory.CpuBankAddress(p.Address)})";
                s += "\t" + p.Type.ToString();
                s += "\tcolor=" + p.ColorIndex.ToString("X2");
                s += "\tshaded=" + p.Shaded.ToString();

                if (p.Normal.HasValue)
                    s += "\tnormal=" + VectorString(p.Normal.Value);

                s += "\tvertices={";
                bool flag = false;
                foreach (Vector3 v in p.Vertices)
                {
                    if (flag)
                        s += ",";
                    s += VectorString(v);
                    flag = true;
                }
                s += "}";

                stream.WriteLine(s);
                stream.Flush();
            }
            stream.WriteLine();
        }


        Vector3 Vertex(UInt16 address)
        {
            UInt16 offset = (UInt16)(address & 0x3FFF);
            address = (UInt16)(offset + VertexBase);

            VertexAddressList.Add((UInt16)(address + 0));
            VertexAddressList.Add((UInt16)(address + 1));
            VertexAddressList.Add((UInt16)(address + 2));

            Vector3 v;
            v.X = (Int16)(ROM[address++]);
            v.Y = (Int16)(ROM[address++]);
            v.Z = (Int16)(ROM[address++]);
            return v;
        }

        String VectorString(Vector3 v)
        {
            String s = "{";
            s += v.X.ToString();
            s += ",";
            s += v.Y.ToString();
            s += ",";
            s += v.Z.ToString();
            s += "}";
            return s;
        }

        public override string ToString()
        {
            return Address.ToString("X4");
        }
    }

    public class MeshList : IReadOnlyList<Mesh>
    {
        readonly List<Mesh> List = new List<Mesh>();

        public MeshList()
        {
            for (UInt16 a = 0x2000; a < 0x8000; a++)
            {
                Mathbox.Mesh obj = Mathbox.Mesh.GetMeshAt(a);
                if (obj != null)
                    List.Add(obj);
            }
        }

        public Mesh this[int index] => List[index];
        public int Count => List.Count;
        public IEnumerator<Mesh> GetEnumerator() { return ((IReadOnlyList<Mesh>)List).GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return ((IReadOnlyList<Mesh>)List).GetEnumerator(); }
    }
}
