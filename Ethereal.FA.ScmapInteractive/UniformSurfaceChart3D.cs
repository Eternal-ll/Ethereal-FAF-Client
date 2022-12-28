//------------------------------------------------------------------
// (c) Copywrite Jianzhong Zhang
// This code is under The Code Project Open License
// Please read the attached license document before using this class
//------------------------------------------------------------------

// class of a special surface chart, (uniform grid in x-y direction)
// version 0.1


using System.Collections;
using System.Windows.Media.Media3D;
using System.Windows.Media;

namespace WPFChart3D
{
    class UniformSurface3D : UniformSurfaceChart3D
    {
        public void SetHeightmap(float[,] heights, int xNo, int yNo, float xMin, float xMax, float yMin, float yMax)
        {
            SetDataNo(xNo * yNo);
            m_nGridXNo = xNo;
            m_nGridYNo = yNo;
            m_xMin = xMin;
            m_xMax = xMax;
            m_yMin = yMin;
            m_yMax = yMax;
            float dx = (m_xMax - m_xMin) / ((float)xNo - 1);
            float dy = (m_yMax - m_yMin) / ((float)yNo - 1);
            for (int i = 0; i < xNo; i++)
            {
                for (int j = 0; j < yNo; j++)
                {
                    float xV = m_xMin + dx * ((float)(i));
                    float yV = m_yMin + dy * ((float)(j));
                    m_vertices[j * xNo + i] = new Vertex3D();
                    SetPoint(i, j, xV, yV, 0);
                }
            }
        }
    }
    class UniformSurfaceChart3D: SurfaceChart3D
    {

        public void SetPoint(int i, int j, float x, float y, float z)
        {
            int nI = j * m_nGridXNo + i;
            m_vertices[nI].x = x;
            m_vertices[nI].y = y;
            m_vertices[nI].z = z;
        }

        public void SetZ(int i, int j, float z)
        {
            m_vertices[j*m_nGridXNo + i].z = z;
        }

        public void SetColor(int i, int j, Color color)
        {
            int nI = j * m_nGridXNo + i;
            m_vertices[nI].color = color;
       }

        public virtual void SetGrid(int xNo, int yNo, float xMin, float xMax, float yMin, float yMax)
        {
            SetDataNo(xNo * yNo);
            m_nGridXNo = xNo;
            m_nGridYNo = yNo;
            m_xMin = xMin;
            m_xMax = xMax;
            m_yMin = yMin;
            m_yMax = yMax;
            float dx = (m_xMax - m_xMin) / ((float)xNo - 1);
            float dy = (m_yMax - m_yMin) / ((float)yNo - 1);
            for (int i = 0; i < xNo; i++)
            {
                for (int j = 0; j < yNo; j++)
                {
                    float xV = m_xMin + dx * ((float)(i));
                    float yV = m_yMin + dy * ((float)(j));
                    m_vertices[j * xNo + i] = new Vertex3D();
                    SetPoint(i, j, xV, yV, 0);
                }
            }
         
        }

        // convert the uniform surface chart to a array of Mesh3D (only one element)
        public ArrayList GetMeshes()
        {
            ArrayList meshes = new ArrayList();
            ColorMesh3D surfaceMesh = new ColorMesh3D();

            surfaceMesh.SetSize(m_nGridXNo * m_nGridYNo, 2 * (m_nGridXNo - 1) * (m_nGridYNo - 1));

            for (int i = 0; i < m_nGridXNo; i++)
            {
                for (int j = 0; j < m_nGridYNo; j++)
                {
                    int nI = j * m_nGridXNo + i;
                    Vertex3D vert = m_vertices[nI];
                    m_vertices[nI].nMinI = nI;
                    surfaceMesh.SetPoint(nI, new Point3D(vert.x, vert.y, vert.z));
                    surfaceMesh.SetColor(nI, vert.color);
                }
            }
            // set triangle
            int nT = 0;
            for (int i = 0; i < m_nGridXNo-1; i++)
            {
                for (int j = 0; j < m_nGridYNo-1; j++)
                {
                    int n00 = j * m_nGridXNo + i;
                    int n10 = j * m_nGridXNo + i + 1;
                    int n01 = (j + 1) * m_nGridXNo + i;
                    int n11 = (j + 1) * m_nGridXNo + i + 1;

                    surfaceMesh.SetTriangle(nT, n00, n10, n01);
                    nT++;
                    surfaceMesh.SetTriangle(nT, n01, n10, n11);
                    nT++;
                }
            }
            meshes.Add(surfaceMesh);

            return meshes;
        }
        
        protected int m_nGridXNo, m_nGridYNo;               // the grid number on each axis
    }
}
