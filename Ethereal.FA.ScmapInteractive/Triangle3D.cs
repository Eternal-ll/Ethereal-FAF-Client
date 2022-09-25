//------------------------------------------------------------------
// (c) Copywrite Jianzhong Zhang
// This code is under The Code Project Open License
// Please read the attached license document before using this class
//------------------------------------------------------------------

// A triangle in 3D space
// version 0.1

namespace WPFChart3D
{
    public class Triangle3D
    {
        public Triangle3D(int m0, int m1, int m2)
        {
            n0 = m0; n1 = m1; n2 = m2;
        }

        public int n0, n1, n2;                      // vertex indice of the triangle
    }
}
