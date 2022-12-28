//------------------------------------------------------------------
// (c) Copywrite Jianzhong Zhang
// This code is under The Code Project Open License
// Please read the attached license document before using this class
//------------------------------------------------------------------

// class of 3d scatter plot .
// version 0.1

using System.Collections;
using System.Windows.Media.Media3D;
using System.Windows.Controls;
using System.Windows;

namespace WPFChart3D
{
    class ScatterChart3D: Chart3D
    {
        public WPFChart3D.ScatterPlotItem Get(int n)
        {
            return (ScatterPlotItem)m_vertices[n];
        }
        
        public void SetVertex(int n, WPFChart3D.ScatterPlotItem value)
        {
            m_vertices[n] = value;
        }

        // convert the 3D scatter plot into a array of Mesh3D object
        public ArrayList GetMeshes()
        {
            int nDotNo = GetDataNo();
            if (nDotNo == 0) return null;
            ArrayList meshs = new ArrayList();

            int nVertIndex = 0;
            for (int i = 0; i < nDotNo; i++)
            {
                ScatterPlotItem plotItem = Get(i);
                int nType = plotItem.shape % Chart3D.SHAPE_NO;
                float w = plotItem.w;
                float h = plotItem.h;
                Mesh3D dot;
                m_vertices[i].nMinI = nVertIndex;
                switch (nType)
                {
                    case (int)SHAPE.BAR:
                        dot = new Bar3D(0, 0, 0, w, w, h);
                        break;
                    case (int)SHAPE.CONE:
                        dot = new Cone3D(w, w, h, 7);
                        break;
                    case (int)SHAPE.CYLINDER:
                        dot = new Cylinder3D(w, w, h, 7);
                        break;
                    case (int)SHAPE.ELLIPSE:
                        dot = new Ellipse3D(w, w, h, 7);
                        break;
                    case (int)SHAPE.PYRAMID:
                        dot = new Pyramid3D(w, w, h);
                        break;
                    default:
                        dot = new Bar3D(0, 0, 0, w, w, h);
                        break;
                }
                nVertIndex += dot.GetVertexNo();
                m_vertices[i].nMaxI = nVertIndex - 1;

                TransformMatrix.Transform(dot, new Point3D(plotItem.x, plotItem.y, plotItem.z), 0, 0);
                dot.SetColor(plotItem.color);
                meshs.Add(dot);
            }
            AddAxesMeshes(meshs);

            return meshs;
        }

        // selection
        public override void Select(ViewportRect rect, TransformMatrix matrix, Viewport3D viewport3d)
        {
            int nDotNo = GetDataNo();
            if (nDotNo == 0) return;

            double xMin = rect.XMin();
            double xMax = rect.XMax();
            double yMin = rect.YMin();
            double yMax = rect.YMax();

            for (int i = 0; i < nDotNo; i++)
            {
                ScatterPlotItem plotItem = Get(i);
                Point pt = matrix.VertexToViewportPt(new Point3D(plotItem.x, plotItem.y, plotItem.z),
                    viewport3d);

                if ((pt.X > xMin) && (pt.X < xMax) && (pt.Y > yMin) && (pt.Y < yMax))
                {
                    m_vertices[i].selected = true;
                }
                else
                {
                    m_vertices[i].selected = false;
                }
            }
       }

        // highlight the selection
        public override void HighlightSelection(System.Windows.Media.Media3D.MeshGeometry3D meshGeometry, System.Windows.Media.Color selectColor)
        {
            int nDotNo = GetDataNo();
            if (nDotNo == 0) return;

            Point mapPt;
            for (int i = 0; i < nDotNo; i++)
            {
                if (m_vertices[i].selected)
                {
                    mapPt = TextureMapping.GetMappingPosition(selectColor, false);
                }
                else
                {
                    mapPt = TextureMapping.GetMappingPosition(m_vertices[i].color, false);
                }
                int nMin = m_vertices[i].nMinI;
                int nMax = m_vertices[i].nMaxI;
                for(int j=nMin; j<=nMax; j++)
                {
                    meshGeometry.TextureCoordinates[j] = mapPt;
                }
            }
        }
     
    }
}
