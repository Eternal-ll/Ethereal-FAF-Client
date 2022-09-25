//------------------------------------------------------------------
// (c) Copywrite Jianzhong Zhang
// This code is under The Code Project Open License
// Please read the attached license document before using this class
//------------------------------------------------------------------

// base class for 3D chart
// version 0.1

using System;
using System.Collections;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Windows.Media;

namespace WPFChart3D
{
    class Chart3D
    {
        public Chart3D()
        {
        }

        public static int SHAPE_NO = 5;

        public WPFChart3D.Vertex3D this[int n]
        {
            get
            {
                return (Vertex3D)m_vertices[n];
            }
            set
            {
                m_vertices[n] = value;
            }
        }

        public float XCenter()
        {
            return (m_xMin + m_xMax) / 2;
        }

        public float YCenter()
        {
            return (m_yMin + m_yMax) / 2;
        }

        public float XRange()
        {
            return m_xMax - m_xMin;
        }
        public float YRange()
        {
            return m_yMax - m_yMin;
        }
        public float ZRange()
        {
            return m_zMax - m_zMin;
        }
        public float XMin()
        {
            return m_xMin;
        }

        public float XMax()
        {
            return m_xMax;
        }
        public float YMin()
        {
            return m_yMin;
        }

        public float YMax()
        {
            return m_yMax;
        }
        public float ZMin()
        {
            return m_zMin;
        }

        public float ZMax()
        {
            return m_zMax;
        }

        public int GetDataNo()
        {
            return m_vertices.Length;
        }

        public void SetDataNo(int nSize)
        {
            m_vertices = new Vertex3D[nSize];
        }

        public void GetDataRange()
        {
            int nDataNo = GetDataNo();
            if (nDataNo == 0) return;
            m_xMin = Single.MaxValue;
            m_yMin = Single.MaxValue;
            m_zMin = Single.MaxValue;
            m_xMax = Single.MinValue;
            m_yMax = Single.MinValue;
            m_zMax = Single.MinValue;
            for (int i = 0; i < nDataNo; i++)
            {
                float xV = this[i].x;
                float yV = this[i].y;
                float zV = this[i].z;
                if (m_xMin > xV) m_xMin = xV;
                if (m_yMin > yV) m_yMin = yV;
                if (m_zMin > zV) m_zMin = zV;
                if (m_xMax < xV) m_xMax = xV;
                if (m_yMax < yV) m_yMax = yV;
                if (m_zMax < zV) m_zMax = zV;
            }
        }

        public void SetAxes(float x0, float y0, float z0, float xL, float yL, float zL)
        {
            m_xAxisLength = xL;
            m_yAxisLength = yL;
            m_zAxisLength = zL;
            m_xAxisCenter = x0;
            m_yAxisCenter = y0;
            m_zAxisCenter = z0;
            m_bUseAxes = true;
        }

        public void SetAxes()
        {
            SetAxes(0.05f);
        }

        public void SetAxes(float margin)
        {
            float xRange = m_xMax - m_xMin;
            float yRange = m_yMax - m_yMin;
            float zRange = m_zMax - m_zMin;

            float xC = m_xMin - margin * xRange;
            float yC = m_yMin - margin * yRange;
            float zC = m_zMin - margin * zRange;
            float xL = (1 + 2 * margin) * xRange;
            float yL = (1 + 2 * margin) * yRange;
            float zL = (1 + 2 * margin) * zRange;

            SetAxes(xC, yC, zC, xL, yL, zL);
        }

        // add the axes mesh to the Mesh3D array
        // if you are using the projection matrix which is not uniform along all the axess, you need change this function
        public void AddAxesMeshes(ArrayList meshs)
        {
            if (!m_bUseAxes) return;

            float radius = (m_xAxisLength+m_yAxisLength+m_zAxisLength) / (3*m_axisLengthWidthRatio);

            Mesh3D xAxisCylinder = new Cylinder3D(radius, radius, m_xAxisLength, 6);
            xAxisCylinder.SetColor(m_axisColor);
            TransformMatrix.Transform(xAxisCylinder, new Point3D( m_xAxisCenter + m_xAxisLength / 2, m_yAxisCenter, m_zAxisCenter), 0, 90);
            meshs.Add(xAxisCylinder);

            Mesh3D xAxisCone = new Cone3D(2 * radius, 2 * radius, radius * 5, 6);
            xAxisCone.SetColor(m_axisColor);
            TransformMatrix.Transform(xAxisCone, new Point3D(m_xAxisCenter + m_xAxisLength, m_yAxisCenter, m_zAxisCenter), 0, 90);
            meshs.Add(xAxisCone);
         
            Mesh3D yAxisCylinder = new Cylinder3D(radius, radius, m_yAxisLength, 6);
            yAxisCylinder.SetColor(m_axisColor);
            TransformMatrix.Transform(yAxisCylinder, new Point3D(m_xAxisCenter , m_yAxisCenter+ m_yAxisLength / 2, m_zAxisCenter), 90, 90);
            meshs.Add(yAxisCylinder);
            
            Mesh3D yAxisCone = new Cone3D(2 * radius, 2 * radius, radius * 5, 6);
            yAxisCone.SetColor(m_axisColor);
            TransformMatrix.Transform(yAxisCone, new Point3D(m_xAxisCenter, m_yAxisCenter + m_yAxisLength, m_zAxisCenter), 90, 90);
            meshs.Add(yAxisCone);
   
            Mesh3D zAxisCylinder = new Cylinder3D(radius, radius, m_zAxisLength, 6);
            zAxisCylinder.SetColor(m_axisColor);
            TransformMatrix.Transform(zAxisCylinder, new Point3D(m_xAxisCenter , m_yAxisCenter, m_zAxisCenter + m_zAxisLength / 2), 0, 0);
            meshs.Add(zAxisCylinder);
                         
            Mesh3D zAxisCone = new Cone3D(2 * radius, 2 * radius, radius * 5, 6);
            zAxisCone.SetColor(m_axisColor);
            TransformMatrix.Transform(zAxisCone, new Point3D(m_xAxisCenter, m_yAxisCenter, m_zAxisCenter + m_zAxisLength), 0, 0);
            meshs.Add(zAxisCone);
            
        }

        // select 
        public virtual void Select(ViewportRect rect, TransformMatrix matrix, Viewport3D viewport3d)
        {
        }

        // highlight selected model
        public virtual void HighlightSelection(System.Windows.Media.Media3D.MeshGeometry3D meshGeometry, System.Windows.Media.Color selectColor)
        {
        }

        public enum SHAPE { BAR, ELLIPSE, CYLINDER, CONE, PYRAMID };    // shape of the 3d dot in the plot

        protected Vertex3D [] m_vertices;                               // 3d plot data
        protected float m_xMin, m_xMax, m_yMin, m_yMax, m_zMin, m_zMax; // data range

        private float m_axisLengthWidthRatio = 200;                     // axis length / width ratio
        private float m_xAxisLength, m_yAxisLength, m_zAxisLength;      // axis length
        private float m_xAxisCenter, m_yAxisCenter, m_zAxisCenter;      // axis start point
        private bool m_bUseAxes = true;                                // use axis
        public Color m_axisColor = Color.FromRgb(0, 0, 196);            // axis color

    }
}
