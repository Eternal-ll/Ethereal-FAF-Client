//------------------------------------------------------------------
// (c) Copywrite Jianzhong Zhang
// This code is under The Code Project Open License
// Please read the attached license document before using this class
//------------------------------------------------------------------

// A dot in 3D space
// version 0.1

namespace WPFChart3D
{
    class Vertex3D
    {
        public System.Windows.Media.Color color;    // color of the dot
        public float x, y, z;                       // location of the dot
        public int nMinI, nMaxI;                    // link to the viewport positions array index
        public bool selected = false;               // is this dot selected by user
    }
}
