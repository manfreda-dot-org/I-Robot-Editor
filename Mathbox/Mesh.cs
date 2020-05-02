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
    /// <summary>
    /// Represents the mesh of a 3D object in the Mathbox ROM
    /// Meshes are essentially lists of Surface objects
    /// </summary>
    public class Mesh : IReadOnlyList<Mesh.Surface>
    {
        #region OBJECT_ADDRESSES
        public const UInt16 BLACK_BOX_2104 = 0x2104;
        public const UInt16 BLACK_BOX_2108 = 0x2108;
        public const UInt16 BLACK_BOX_210C = 0x210c;
        public const UInt16 BLACK_BOX_2110 = 0x2110;
        public const UInt16 VECTOR_A_RED = 0x2183;
        public const UInt16 VECTOR_A_FLASHING = 0x218b;
        public const UInt16 VECTOR_B_FLASHING = 0x2193;
        public const UInt16 VECTOR_C_FLASHING = 0x219f;
        public const UInt16 VECTOR_D_FLASHING = 0x21a7;
        public const UInt16 VECTOR_D_RED = 0x21b1;
        public const UInt16 VECTOR_E_FLASHING = 0x21bb;
        public const UInt16 VECTOR_E_RED = 0x21c5;
        public const UInt16 VECTOR_F_FLASHING = 0x21cf;
        public const UInt16 VECTOR_G_FLASHING = 0x21d7;
        public const UInt16 VECTOR_H_FLASHING = 0x21e3;
        public const UInt16 VECTOR_I_FLASHING = 0x21eb;
        public const UInt16 VECTOR_J_FLASHING = 0x21f3;
        public const UInt16 VECTOR_K_FLASHING = 0x21fb;
        public const UInt16 VECTOR_L_FLASHING = 0x2203;
        public const UInt16 VECTOR_M_FLASHING = 0x2209;
        public const UInt16 VECTOR_N_RED = 0x2213;
        public const UInt16 VECTOR_N_FLASHING = 0x221b;
        public const UInt16 VECTOR_O_FLASHING = 0x2223;
        public const UInt16 VECTOR_O_RED = 0x222d;
        public const UInt16 VECTOR_P_RED = 0x2237;
        public const UInt16 VECTOR_P_FLASHING = 0x2241;
        public const UInt16 VECTOR_Q_FLASHING = 0x224b;
        public const UInt16 VECTOR_R_FLASHING = 0x2251;
        public const UInt16 VECTOR_R_RED = 0x225d;
        public const UInt16 VECTOR_S_FLASHING = 0x2269;
        public const UInt16 VECTOR_S_RED = 0x2275;
        public const UInt16 VECTOR_T_RED = 0x2281;
        public const UInt16 VECTOR_T_FLASHING = 0x2287;
        public const UInt16 VECTOR_U_FLASHING = 0x228d;
        public const UInt16 VECTOR_V_FLASHING = 0x2295;
        public const UInt16 VECTOR_W_FLASHING = 0x229b;
        public const UInt16 VECTOR_X_FLASHING = 0x22a5;
        public const UInt16 VECTOR_Y_FLASHING = 0x22ab;
        public const UInt16 VECTOR_Z_FLASHING = 0x22b3;
        public const UInt16 VECTOR_1_FLASHING = 0x22bb;
        public const UInt16 VECTOR_2_FLASHING = 0x22bf;
        public const UInt16 VECTOR_3_FLASHING = 0x22c9;
        public const UInt16 VECTOR_4_FLASHING = 0x22d3;
        public const UInt16 VECTOR_6_FLASHING = 0x22db;
        public const UInt16 VECTOR_7_FLASHING = 0x22e5;
        public const UInt16 VECTOR_8_FLASHING = 0x22eb;
        public const UInt16 VECTOR_9_FLASHING = 0x22f7;
        public const UInt16 VECTOR_0_RED = 0x2301;
        public const UInt16 VECTOR_1_RED = 0x230b;
        public const UInt16 VECTOR_2_RED = 0x230f;
        public const UInt16 VECTOR_3_RED = 0x2319;
        public const UInt16 VECTOR_4_RED = 0x2323;
        public const UInt16 VECTOR_5_RED = 0x232b;
        public const UInt16 VECTOR_6_RED = 0x2337;
        public const UInt16 VECTOR_7_RED = 0x2341;
        public const UInt16 VECTOR_8_RED = 0x2347;
        public const UInt16 VECTOR_9_RED = 0x2353;
        public const UInt16 VECTOR_A_BLACK = 0x235d;
        public const UInt16 VECTOR_B_BLACK = 0x2365;
        public const UInt16 VECTOR_C_BLACK = 0x2371;
        public const UInt16 VECTOR_D_BLACK = 0x2379;
        public const UInt16 VECTOR_E_BLACK = 0x2383;
        public const UInt16 VECTOR_F_BLACK = 0x238d;
        public const UInt16 VECTOR_G_BLACK = 0x2395;
        public const UInt16 VECTOR_H_BLACK = 0x23a1;
        public const UInt16 VECTOR_I_BLACK = 0x23a9;
        public const UInt16 VECTOR_J_BLACK = 0x23b1;
        public const UInt16 VECTOR_K_BLACK = 0x23b9;
        public const UInt16 VECTOR_L_BLACK = 0x23c1;
        public const UInt16 VECTOR_M_BLACK = 0x23c7;
        public const UInt16 VECTOR_N_BLACK = 0x23d1;
        public const UInt16 VECTOR_O_BLACK = 0x23d9;
        public const UInt16 VECTOR_P_BLACK = 0x23e3;
        public const UInt16 VECTOR_Q_BLACK = 0x23ed;
        public const UInt16 VECTOR_R_BLACK = 0x23f3;
        public const UInt16 VECTOR_S_BLACK = 0x23ff;
        public const UInt16 VECTOR_T_BLACK = 0x240b;
        public const UInt16 VECTOR_U_BLACK = 0x2411;
        public const UInt16 VECTOR_V_BLACK = 0x2419;
        public const UInt16 VECTOR_W_BLACK = 0x241f;
        public const UInt16 VECTOR_X_BLACK = 0x2429;
        public const UInt16 VECTOR_Y_BLACK = 0x242f;
        public const UInt16 VECTOR_Z_BLACK = 0x2437;
        public const UInt16 VECTOR_1_BLACK = 0x243f;
        public const UInt16 VECTOR_2_BLACK = 0x2443;
        public const UInt16 VECTOR_3_BLACK = 0x244d;
        public const UInt16 VECTOR_4_BLACK = 0x2457;
        public const UInt16 VECTOR_6_BLACK = 0x245f;
        public const UInt16 VECTOR_7_BLACK = 0x2469;
        public const UInt16 VECTOR_8_BLACK = 0x246f;
        public const UInt16 VECTOR_9_BLACK = 0x247b;
        public const UInt16 ROBOT_ARMS_DOWN = 0x2958;
        public const UInt16 ROBOT_LEFT_ARM_UP = 0x29ee;
        public const UInt16 ROBOT_RIGHT_ARM_UP = 0x2a84;
        public const UInt16 ROBOT_BOTH_ARMS_UP = 0x2b1a;
        public const UInt16 ROBOT_PIECE_0 = 0x2bb0;
        public const UInt16 ROBOT_PIECE_1 = 0x2bc0;
        public const UInt16 ROBOT_PIECE_2 = 0x2bd2;
        public const UInt16 ROBOT_PIECE_3 = 0x2be8;
        public const UInt16 ROBOT_PIECE_4 = 0x2c00;
        public const UInt16 ROBOT_PIECE_5 = 0x2c22;
        public const UInt16 ROBOT_PIECE_6 = 0x2c30;
        public const UInt16 ROBOT_RED = 0x2c3e;
        public const UInt16 BIG_BROTHER_MOUTH_OPEN = 0x34a2;
        public const UInt16 BIG_BROTHER_MOUTH_CLOSED = 0x3578;
        public const UInt16 EYEBALL_RED = 0x3892;
        public const UInt16 EYEBALL_BLUE = 0x38a4;
        public const UInt16 EYEBALL_GREEN = 0x38b6;
        public const UInt16 EYEBALL_CYAN = 0x38c8;
        public const UInt16 EYEBALL_YELLOW = 0x38da;
        public const UInt16 EYEBALL_RED_SHADED = 0x3930;
        public const UInt16 SPEECH_BUBBLE_0 = 0x4039;
        public const UInt16 SPEECH_BUBBLE_1 = 0x403f;
        public const UInt16 SAW = 0x4146;
        public const UInt16 BLOCK_LETTER_I = 0x4582;
        public const UInt16 BLOCK_LETTER_COMMA = 0x45a4;
        public const UInt16 BLOCK_LETTER_R = 0x45bc;
        public const UInt16 BLOCK_LETTER_O = 0x45f4;
        public const UInt16 BLOCK_LETTER_B = 0x461e;
        public const UInt16 BLOCK_LETTER_T = 0x4660;
        public const UInt16 STARFIELD_0 = 0x4ae8;
        public const UInt16 STARFIELD_1 = 0x4aec;
        public const UInt16 STARFIELD_2 = 0x4af0;
        public const UInt16 STARFIELD_3 = 0x4af4;
        public const UInt16 STARFIELD_4 = 0x4af8;
        public const UInt16 STARFIELD_5 = 0x4afc;
        public const UInt16 STARFIELD_6 = 0x4b00;
        public const UInt16 STARFIELD_7 = 0x4b04;
        public const UInt16 STARFIELD_8 = 0x4b08;
        public const UInt16 STARFIELD_9 = 0x4b18;
        public const UInt16 STARFIELD_10 = 0x4b1c;
        public const UInt16 STARFIELD_11 = 0x4b24;
        public const UInt16 STARFIELD_12 = 0x4b28;
        public const UInt16 STARFIELD_13 = 0x4b38;
        public const UInt16 STARFIELD_14 = 0x4b3c;
        public const UInt16 STARFIELD_15 = 0x4b44;
        public const UInt16 SHARK = 0x4d17;
        public const UInt16 TRUNCATED_CUBOCTAHEDRON_POINTS_FLASHING = 0x4f8f;
        public const UInt16 TRUNCATED_CUBOCTAHEDRON_POINTS_BLUE = 0x4f93;
        public const UInt16 TRUNCATED_CUBOCTAHEDRON_POINTS_RED = 0x4f97;
        public const UInt16 TRUNCATED_CUBOCTAHEDRON_POINTS_GREEN = 0x4f9b;
        public const UInt16 TRUNCATED_CUBOCTAHEDRON_POINTS_YELLOW = 0x4f9f;
        public const UInt16 TRUNCATED_CUBOCTAHEDRON_POINTS_ORANGE = 0x4fa3;
        public const UInt16 TRUNCATED_CUBOCTAHEDRON_POINTS_WHITE = 0x4fa7;
        public const UInt16 TRUNCATED_CUBOCTAHEDRON_POINTS_PURPLE = 0x4fab;
        public const UInt16 TRUNCATED_CUBOCTAHEDRON = 0x4faf;
        public const UInt16 BIRD_WINGS_UP = 0x51e0;
        public const UInt16 BIRD_WINGS_MIDDLE = 0x5234;
        public const UInt16 BIRD_WINGS_DOWN = 0x5288;
        public const UInt16 SMALL_OCTAGON_WITH_BLACK_SQUARE = 0x531a;
        public const UInt16 SMALL_OCTAGON_DOTS_FLASHING = 0x5320;
        public const UInt16 SHIELD_OUTLINE_FLASHING = 0x53e5;
        public const UInt16 SHIELD_FLASHING = 0x53ed;
        public const UInt16 SHIELD_53F5 = 0x53f5;
        public const UInt16 SHIELD_5407 = 0x5407;
        public const UInt16 SHIELD_5419 = 0x5419;
        public const UInt16 SHIELD_542B = 0x542b;
        public const UInt16 SHIELD_543D = 0x543d;
        public const UInt16 SHIELD_544F = 0x544f;
        public const UInt16 SHIELD_5461 = 0x5461;
        public const UInt16 SHIELD_5473 = 0x5473;
        public const UInt16 SHIELD_5485 = 0x5485;
        public const UInt16 SHIELD_5497 = 0x5497;
        public const UInt16 SHIELD_54A3 = 0x54a3;
        public const UInt16 SHIELD_54AD = 0x54ad;
        public const UInt16 SHIELD_54B5 = 0x54b5;
        public const UInt16 SHIELD_54BB = 0x54bb;
        public const UInt16 SHIELD_YELLOW1 = 0x54bf;
        public const UInt16 SHIELD_YELLOW2 = 0x54c7;
        public const UInt16 SHIELD_YELLOW3 = 0x54cf;
        public const UInt16 SHIELD_YELLOW4 = 0x54d7;
        public const UInt16 SHIELD_YELLOW5 = 0x54df;
        public const UInt16 SHIELD_YELLOW6 = 0x54e7;
        public const UInt16 SHIELD_YELLOW7 = 0x54ef;
        public const UInt16 SHIELD_YELLOW8 = 0x5501;
        public const UInt16 SHIELD_YELLOW9 = 0x5513;
        public const UInt16 SHIELD_YELLOW10 = 0x5525;
        public const UInt16 SHIELD_YELLOW11 = 0x5537;
        public const UInt16 SHIELD_YELLOW12 = 0x5549;
        public const UInt16 SHIELD_YELLOW13 = 0x555b;
        public const UInt16 SHIELD_YELLOW14 = 0x556d;
        public const UInt16 SHIELD_557F = 0x557f;
        public const UInt16 SHIELD_5591 = 0x5591;
        public const UInt16 SHIELD_55A3 = 0x55a3;
        public const UInt16 SHIELD_55B5 = 0x55b5;
        public const UInt16 SHIELD_55C7 = 0x55c7;
        public const UInt16 SHIELD_55D9 = 0x55d9;
        public const UInt16 SHIELD_RED = 0x55eb;
        public const UInt16 SHIELD_PURPLE = 0x55fd;
        public const UInt16 SHIELD_DARKRED = 0x560f;
        public const UInt16 SHIELD_DARKPURPLE = 0x5621;
        public const UInt16 TRANSPORTER_BOTTOM = 0x5767;
        public const UInt16 TRANSPORTER_TOP = 0x577b;
        public const UInt16 TRANSPORTER_ENDS = 0x5791;
        public const UInt16 TRANSPORTER_LINES = 0x599c;
        public const UInt16 TANKER = 0x5b56;
        public const UInt16 PORCUPINE_0 = 0x5b68;
        public const UInt16 PORCUPINE_1 = 0x5b6e;
        public const UInt16 PORCUPINE_2 = 0x5b72;
        public const UInt16 PORCUPINE_3 = 0x5b8c;
        public const UInt16 PORCUPINE_4 = 0x5bb5;
        public const UInt16 PORCUPINE_5 = 0x5be7;
        public const UInt16 PORCUPINE_6 = 0x5c22;
        public const UInt16 BEACH_BALL_COLORED = 0x5f08;
        public const UInt16 BEACH_BALL_CYAN = 0x5f82;
        public const UInt16 STARBURST_YELLOW = 0x640c;
        public const UInt16 STARBURST_WHITE = 0x643e;
        public const UInt16 STARBURST_DARKPURPLE = 0x6470;
        public const UInt16 STARBURST_ORANGE = 0x64a2;
        public const UInt16 STARBURST_RED = 0x64d4;
        public const UInt16 FLAT_OCTAGON_BLUE = 0x6562;
        public const UInt16 FLAT_OCTAGON_YELLOW = 0x6566;
        public const UInt16 FLAT_OCTAGON_ORANGE = 0x656a;
        public const UInt16 FLAT_OCTAGON_RED = 0x656e;
        public const UInt16 FLAT_OCTAGON_GREEN = 0x6572;
        public const UInt16 FLAT_OCTAGON_FLASHING = 0x6576;
        public const UInt16 FLAT_OCTAGON_PURPLE = 0x657a;
        public const UInt16 FLAT_OCTAGON_DARK_BLUE = 0x657e;
        public const UInt16 FLAT_OCTAGON_DARK_YELLOW = 0x6582;
        public const UInt16 FLAT_OCTAGON_DARK_ORANGE = 0x6586;
        public const UInt16 FLAT_OCTAGON_DARK_RED = 0x658a;
        public const UInt16 FLAT_OCTAGON_DARK_GREEN = 0x658e;
        public const UInt16 FLAT_OCTAGON_DARK_PURPLE = 0x6592;
        public const UInt16 EVIL_EYE_BACKGROUND = 0x676a;
        public const UInt16 EVIL_EYE_RED_PUPIL = 0x676e;
        public const UInt16 EVIL_EYE_YELLOW_PUPIL = 0x6774;
        public const UInt16 EVIL_EYE_GREEN_PUPIL = 0x677a;
        public const UInt16 EVIL_EYE_MASK_RECTANGLE = 0x6780;
        public const UInt16 EVIL_EYE_MASK_BOTTOM = 0x6784;
        public const UInt16 HUGE_EYE_BACKGROUND = 0x678c;
        public const UInt16 HUGE_EYE_RED_PUPIL = 0x6790;
        public const UInt16 HUGE_EYE_MASK = 0x6796;
        public const UInt16 HUGE_EYE_BLASTED = 0x679e;
        public const UInt16 HAND = 0x692f;
        public const UInt16 SPHERE_POINTS_FLASHING = 0x6ba3;
        public const UInt16 SPHERE_POINTS_BLUE = 0x6ba7;
        public const UInt16 SPHERE_POINTS_RED = 0x6bab;
        public const UInt16 SPHERE_POINTS_GREEN = 0x6baf;
        public const UInt16 SPHERE_POINTS_YELLOW = 0x6bb3;
        public const UInt16 SPHERE_POINTS_ORANGE = 0x6bb7;
        public const UInt16 SPHERE_POINTS_WHITE = 0x6bbb;
        public const UInt16 SPHERE_POINTS_PURPLE = 0x6bbf;
        public const UInt16 SPHERE_POINTS_COLORED = 0x6bc3;
        public const UInt16 SAUCER_1 = 0x6d19;
        public const UInt16 SAUCER_2 = 0x6d2d;
        public const UInt16 SAUCER_3 = 0x6d41;
        public const UInt16 SAUCER_4 = 0x6d55;
        public const UInt16 UNKNOWN = 0x71ac;
        public const UInt16 JEWEL = 0x728d;
        public const UInt16 CUBE_RED8 = 0x730a;
        public const UInt16 CUBE_WHITE = 0x7318;
        public const UInt16 CUBE_GREEN = 0x7326;
        public const UInt16 CUBE_YELLOW = 0x7334;
        public const UInt16 CUBE_BLUE = 0x7342;
        public const UInt16 CUBE_ORANGE = 0x7350;
        public const UInt16 CUBE_PURPLE = 0x735e;
        public const UInt16 CUBE_COLORED = 0x736c;
        public const UInt16 LOG_8 = 0x741e;
        public const UInt16 LOG_7 = 0x7434;
        public const UInt16 LOG_6 = 0x7438;
        public const UInt16 LOG_5 = 0x743e;
        public const UInt16 LOG_4 = 0x7446;
        public const UInt16 LOG_3 = 0x7450;
        public const UInt16 LOG_2 = 0x745c;
        public const UInt16 LOG_1 = 0x746a;
        public const UInt16 DODECAHEDRON_COLORED = 0x755d;
        public const UInt16 DODECAHEDRON_RED = 0x75fc;
        public const UInt16 DODECAHEDRON_BLUE = 0x7616;
        public const UInt16 DODECAHEDRON_YELLOW = 0x7630;
        public const UInt16 DODECAHEDRON_GREEN = 0x764a;
        public const UInt16 DODECAHEDRON_ORANGE = 0x7664;
        public const UInt16 DODECAHEDRON_WHITE = 0x767e;
        public const UInt16 COLORED_RING = 0x77d0;
        public const UInt16 YELLOW_RING = 0x7802;
        public const UInt16 BULLET = 0x7924;
        public const UInt16 LASER = 0x795f;
        public const UInt16 CLOUD_WHITE = 0x7d51;
        public const UInt16 CLOUD_YELLOW = 0x7d55;
        public const UInt16 CLOUD_ORANGE = 0x7d59;
        public const UInt16 CLOUD_PURPLE = 0x7d5d;
        public const UInt16 CLOUD_RED_1 = 0x7d61;
        public const UInt16 CLOUD_RED_2 = 0x7d65;
        public const UInt16 CLOUD_RED_3 = 0x7d69;
        public const UInt16 CLOUD_RED_4 = 0x7d6d;
        public const UInt16 VIEWER_KILLER_COLORED = 0x7df4;
        public const UInt16 VIEWER_KILLER_CYAN = 0x7e32;
        public const UInt16 STELLATED_OCTAHEDRON = 0x7f28;
        public const UInt16 WIREFRAME_TETRA = 0x7fdc;
        public const UInt16 SOLID_TETRA = 0x7fe6;
        public const UInt16 TETRA = 0xb314;
        #endregion

        #region SURFACE

        /// <summary>
        /// Represents a surface to render in the mesh
        /// </summary>
        public sealed class Surface : System.IEquatable<Surface>
        {
            public enum TYPE
            {
                Polygon = 0,
                Vector = 1,
                Dot = 2,
                Unknown = 3
            }

            public enum CULL
            {
                AlwaysBranch = 0,
                BranchIfVisible = 1,
                BranchIfHidden = 2,
                BranchAlways = 3
            }

            /// <summary>
            /// Structure that represents the surface control flags
            /// xxxxxxxx xxxxxxxx
            /// |||||||| ||||||||
            /// |||||||| || \\\\\\___ palette color index
            /// |||||||| | \_________ color index is shaded according to directional light
            /// ||||||||  \__________ unknown
            /// |||||| \\____________ render as Surface.TYPE dot/vector/polygon
            /// |||| \\______________ unknown
            /// || \\________________ cull instruction
            /// | \__________________ unknown
            ///  \___________________ enable culling instruction
            /// </summary>
            public class FLAGS
            {
                public const UInt16 SHADED = 0x40;
                public const UInt16 CULL_INSTRUCTION_ENABLED = 0x8000;

                public UInt16 Value;

                public FLAGS(UInt16 value) { Value = value; }

                public int ColorIndex { get { return Value & 0x3F; } }
                public bool IsShaded
                {
                    get { return (Value & SHADED) != 0; }
                    set
                    {
                        if (value)
                            Value |= SHADED;
                        else
                            unchecked { Value &= (UInt16)~SHADED; }
                    }
                }
                public TYPE Type
                {
                    get { return (TYPE)((Value >> 8) & 3); }
                    set { Value = (UInt16)((Value & 0xFCFF) | (((UInt16)value & 3) << 8)); }
                }

                public CULL CullInstruction { get { return (CULL)((Value >> 12) & 3); } }

                public bool IsCullInstruction { get { return (Value & CULL_INSTRUCTION_ENABLED) != 0; } }


                public static implicit operator UInt16(FLAGS value) { return (UInt16)value.Value; }
            }


            /// <summary>
            /// Base address of the surface
            /// </summary>
            public readonly UInt16 Address;

            /// <summary>
            /// Base address of the table of vertex offset for the surface
            /// </summary>
            public readonly UInt16 VertexOffsetTable;

            /// <summary>
            /// Surface control flags
            /// </summary>
            public FLAGS Flags { get; private set; }

            /// <summary>
            /// Optional normal vector for this surface
            /// Only shaded surfaces have a valid normal vector
            /// </summary>
            public Vector3D? Normal { get; private set; }

            /// <summary>
            /// List of points that make up this surface
            /// </summary>
            public IReadOnlyList<Point3D> Points => mPoints;
            public readonly List<Point3D> mPoints = new List<Point3D>();

            internal bool IsValid { get; private set; }

            static internal bool TryCreateSurface(ref UInt16 address, out Surface surface)
            {
                Surface s = new Surface(ref address);
                surface = s.IsValid ? s : null;
                return s.IsValid;
            }

            private Surface(ref UInt16 address)
            {
                Address = address;

                // read the 
                if (!ROM.TryRead(address++, out VertexOffsetTable))
                    return; // failed
                if (VertexOffsetTable < 0x2000 || VertexOffsetTable > 0x8000)
                    return; // failed
                if (VertexOffsetTable != 0x8000)
                {
                    // read the surface control flags
                    if (!ROM.TryRead(address++, out var flags))
                        return; // failed
                    Flags = new FLAGS(flags);
                }

                IsValid = true;
            }


            static internal bool Parse(Mathbox.Mesh mesh, UInt16 address)
            {
                // scan forever until exit condition reached
                for (; ; )
                {
                    // ensure no duplicate surfaces
                    foreach (Surface p in mesh.mSurfaces)
                    {
                        if (p.Address == address)
                            return true; // everything OK, we just can't add any more polys
                    }

                    // try and create a new surface object
                    if (!Surface.TryCreateSurface(ref address, out Surface surface))
                        return false;

                    /// is this the end of the surface list?
                    if (surface.VertexOffsetTable == 0x8000)
                        return true; // end of surface list reached

                    // should the surface be drawn?
                    if (!surface.Flags.IsCullInstruction || surface.Flags.CullInstruction == CULL.AlwaysBranch)
                    {
                        // yes

                        // scan the vertex index list
                        int pIndices = surface.VertexOffsetTable;

                        // read the first offset
                        if (!ROM.TryRead(pIndices++, out UInt16 offset))
                            return false;

                        // first vertex specifies the (optional) normal vector
                        if ((offset & 0x4000) == 0)
                        {
                            // normal vector exists
                            Vector3D normal = mesh.Vector(offset);
                            normal.Normalize();
                            surface.Normal = normal;
                        }

                        // remaining vertices are the points that make up the surface
                        while (offset < 0x8000)
                        {
                            if (!ROM.TryRead(pIndices++, out offset))
                                return false;
                            surface.mPoints.Add(mesh.Point(offset));
                        }

                        // make sure the surface type matches the number of vertices
                        switch (surface.mPoints.Count)
                        {
                            case 0: return false;
                            case 1:
                                // always set to "DOT"
                                surface.Flags.Type = TYPE.Dot;
                                break;
                            case 2:
                                // must be dot or vector
                                if (surface.Type == TYPE.Polygon)
                                    surface.Flags.Type = TYPE.Vector;
                                break;
                            default:
                                if (surface.Type == TYPE.Vector)
                                    surface.mPoints.Add(surface.mPoints[0]); // close the vector list
                                break;
                        }

                        // shade certain objects
                        if (surface.Type == TYPE.Polygon)
                        {
                            switch (mesh.Address)
                            {
                                case 0x3892: // eyeball
                                case 0x38A4: // eyeball
                                case 0x38B6: // eyeball
                                case 0x38C8: // eyeball
                                case 0x38DA: // eyeball
                                    if (surface.ColorIndex > 0 && surface.ColorIndex <= 7)
                                        surface.Flags.IsShaded = true; // make shaded
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
                                    surface.Flags.IsShaded = true; // make shaded
                                    break;
                            }
                        }

                        // fix objects
                        switch (mesh.Address)
                        {
                            case 0x2958: // robot visor
                            case 0x29EE: // robot visor
                            case 0x2A84: // robot visor
                            case 0x2B1A: // robot visor
                            case 0x2C00: // robot visor
                                if (surface.ColorIndex == 0x38 || surface.ColorIndex == 0x39)
                                {
                                    for (int n = 0; n < surface.mPoints.Count; n++)
                                        surface.mPoints[n] = surface.mPoints[n] + new Vector3D(-0.1, 0, 0);
                                }
                                break;
                            case 0x51E0: // bird eye
                            case 0x5234: // bird eye
                            case 0x5288: // bird eye
                                if (surface.ColorIndex == 0x38 || surface.ColorIndex == 0x39)
                                {
                                    for (int n = 0; n < surface.mPoints.Count; n++)
                                    {
                                        surface.mPoints[n] = new Point3D(
                                            surface.mPoints[n].X * 15f / 18f,
                                            surface.mPoints[n].Y - 6f,
                                            surface.mPoints[n].Z + 3f);
                                    }
                                }
                                break;
                        }

                        // this is a good surface

                        // check for duplicates
                        bool keep = true;
                        foreach (Surface s in mesh.mSurfaces)
                        {
                            if (surface.Equals(s))
                            {
                                keep = false;
                                break;
                            }
                        }

                        // keep surface if it is unique
                        if (keep)
                            mesh.mSurfaces.Add(surface);
                    }

                    // manage branching
                    if (surface.Flags.IsCullInstruction)
                    {
                        // find address of branch
                        UInt16 branch_addr = address;
                        if (!ROM.TryRead(address++, out UInt16 offset))
                            return false;
                        branch_addr += offset;

                        // what type of branch is this
                        switch (surface.Flags.CullInstruction)
                        {
                            case CULL.AlwaysBranch:
                                address = branch_addr; // take the branch
                                break;
                            case CULL.BranchIfVisible: // branch if surface visible
                            case CULL.BranchIfHidden: // branch if surface hidden
                            case CULL.BranchAlways: // branch never (never draw)
                                                    // parse branch
                                if (!Parse(mesh, branch_addr))
                                    return false; // something went wrong
                                break; // now continue parsing the original branch
                        }
                    }
                }
            }

            public int ColorIndex { get { return Flags.ColorIndex; } }
            public bool IsShaded { get { return Flags.IsShaded; } }
            public TYPE Type { get { return (TYPE)((Flags >> 8) & 3); } }

            public Material Material
            {
                get
                {
                    int index = ColorIndex;
                    if (IsShaded)
                        index |= 7;
                    return Palette.DiffuseMaterial[index];
                }
            }

            public Material EmissiveMaterial
            {
                get
                {
                    int index = ColorIndex;
                    if (IsShaded)
                        index |= 7;
                    return Palette.EmissiveMaterial[index];
                }
            }

            public override int GetHashCode()
            {
                int hash = 13;
                hash = (hash * 7) + Points.Count;
                hash = (hash * 7) + (IsShaded ? 0x55 : 0xAA);
                hash = (hash * 7) + ColorIndex;
                hash = (hash * 7) + (int)Type;
                if (Normal.HasValue)
                    hash = (hash * 7) + Normal.Value.GetHashCode();
                else
                    hash = (hash * 7) + 0xC0FFEE;
                foreach (Point3D p in Points)
                    hash = (hash * 7) + p.GetHashCode();
                return hash;
            }

            public bool Equals(Surface other)
            {
                if (System.Object.ReferenceEquals(this, other)) return true;
                if (this == null || other == null) return false;
                if (Points.Count != other.Points.Count) return false;
                if (IsShaded != other.IsShaded) return false;
                if (ColorIndex != other.ColorIndex) return false;
                if (Type != other.Type) return false;
                if (Normal != other.Normal) return false;
                for (int i = 0; i < Points.Count; i++)
                {
                    if (Points[i] != other.Points[i])
                        return false;
                }
                System.Diagnostics.Debug.Assert(this.GetHashCode() == other.GetHashCode());
                return true;
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as Surface);
            }

            public static bool operator ==(Surface a, Surface b)
            {
                // If both are null, or both are same instance, return true
                if (System.Object.ReferenceEquals(a, b)) return true;

                if (((object)a == null) || ((object)b == null))
                    return false;

                return a.Equals(b);
            }

            public static bool operator !=(Surface a, Surface b)
            {
                return !(a == b);
            }
        }

        #endregion


        #region STATIC

        // ROM file shared by all objects
        static readonly Mathbox.Memory ROM = new Mathbox.Memory();

        // dictionary for quick lookup of found meshes
        static readonly Dictionary<UInt16, Mesh> Lookup = new Dictionary<ushort, Mesh>();

        // list of all valid mathbox meshes
        static public readonly IReadOnlyList<Mesh> MeshList;

        static Mesh()
        {
            List<Mesh> list = new List<Mesh>();

            // try to build all objects in the mathbox
            for (UInt16 address = 0x2000; address < 0x8000; address++)
            {
                Mesh mesh = TryGetMeshAt(address);
                if (mesh != null)
                {
                    Lookup[address] = mesh;
                    list.Add(Lookup[address]);
                }
            }

            MeshList = list;
        }

        static public bool TryGetMeshAt(UInt16 address, out Mesh mesh)
        {
            return Lookup.TryGetValue(address, out mesh);
        }

        static Mathbox.Mesh TryGetMeshAt(UInt16 address)
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
                Mesh mesh = new Mathbox.Mesh(address, vertex_base);
                return mesh.IsValid ? mesh : null;
            }
            catch
            {
                return null;
            }
        }
        #endregion

        /// <summary>
        /// base address of mesh in ROM
        /// </summary>
        public readonly UInt16 Address;

        /// <summary>
        /// address of the vertex table for this mesh
        /// </summary>
        public readonly UInt16 VertexTable;

        // list of surfaces that comprise this object
        List<Surface> mSurfaces = new List<Surface>();

        // same list of surfaces, but sorted by shaded / unshaded
        public readonly IReadOnlyList<Surface> ShadedSurfaces;
        public readonly IReadOnlyList<Surface> UnshadedSurfaces;

        internal bool IsValid { get; private set; }

        // private constructor -- must call Parse() to create an object
        private Mesh(UInt16 address, UInt16 vertex_table_base)
        {
            // remember the address and the base of the vertex table
            Address = address;
            VertexTable = vertex_table_base;

            // try and parse the surface list
            if (!Surface.Parse(this, (UInt16)(address + 1)))
                return; // bad object
            if (mSurfaces.Count == 0)
                return; // bad object

            // this is a good object
            IsValid = true;

#if DEBUG
            // check for duplicate surfaces
            for (int n = 0; n < mSurfaces.Count; n++)
                for (int m = n + 1; m < mSurfaces.Count; m++)
                    System.Diagnostics.Debug.Assert(mSurfaces[n] != mSurfaces[m]);
#endif

            // pre-sort the surfaces to save time
            List<Surface> shaded = new List<Surface>();
            List<Surface> unshaded = new List<Surface>();
            foreach (Surface surface in mSurfaces)
            {
                if (surface.IsShaded && surface.Type == Surface.TYPE.Polygon)
                    shaded.Add(surface);
                else
                    unshaded.Add(surface);
            }
            ShadedSurfaces = shaded;
            UnshadedSurfaces = unshaded;
        }

        public int Count => ((IReadOnlyList<Surface>)mSurfaces).Count;
        public Surface this[int index] => ((IReadOnlyList<Surface>)mSurfaces)[index];
        public IEnumerator<Surface> GetEnumerator() { return ((IReadOnlyList<Surface>)mSurfaces).GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return ((IReadOnlyList<Surface>)mSurfaces).GetEnumerator(); }

        /// <summary>
        /// Reads a point from the vertex table at the specified offset
        /// </summary>
        /// <param name="offset">offset into the vertex table</param>
        /// <returns>Point3D</returns>
        Point3D Point(UInt16 offset)
        {
            offset &= 0x3FFF;
            UInt16 address = (UInt16)(VertexTable + offset);

            return new Point3D(
                (Int16)(ROM[address++]),
                (Int16)(ROM[address++]),
                (Int16)(ROM[address++]));
        }

        /// <summary>
        /// Reads a vector from from the vertex table at the specified offset
        /// </summary>
        /// <param name="offset">offset into the vertex table</param>
        /// <returns>Vector3D</returns>
        Vector3D Vector(UInt16 offset)
        {
            offset &= 0x3FFF;
            UInt16 address = (UInt16)(VertexTable + offset);

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