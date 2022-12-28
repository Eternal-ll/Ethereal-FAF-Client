﻿//------------------------------------------------------------------
// (c) Copywrite Jianzhong Zhang
// This code is under The Code Project Open License
// Please read the attached license document before using this class
//------------------------------------------------------------------

// class for a 3d pyramid.
// version 0.1

//				   0  
//				/  | \		 
//			   /   1  \
//            /  /   \ \  
//          3/----------2 

namespace WPFChart3D
{
    public class Pyramid3D : Mesh3D
    {
        public Pyramid3D(double size)
        {
            SetMesh();
            double W = size;
            double L = size * System.Math.Sqrt(3)/2;
            double H = size * System.Math.Sqrt(2.0/3.0);
            SetData(W, L, H);
        }

        public Pyramid3D(double W, double L, double H)
        {
            SetMesh();
            SetData(W, L, H);
        }

        // set mesh structure (triangle connection)
        void SetMesh()
        {
            SetSize(4, 4);
            SetTriangle(0, 0, 2, 1);
            SetTriangle(1, 0, 3, 2);
            SetTriangle(2, 0, 1, 3);
            SetTriangle(3, 1, 2, 3);
        }

        // set vertices position
        public void SetData(double W, double L, double H)
	    {
		    SetPoint(0, 0, 0, H);
		    SetPoint(1, 0, L/2, 0);
            SetPoint(2, +W / 2, -L / 2, 0);
            SetPoint(3, -W / 2, -L / 2, 0);
            m_xMin = - W / 2;
            m_xMax = + W / 2;
            m_yMin = - L / 2;
            m_yMax = + L / 2;
            m_zMin = - H / 2;
            m_zMax = + H / 2;
        }
    }
}
