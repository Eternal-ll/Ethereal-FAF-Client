﻿//------------------------------------------------------------------
// (c) Copywrite Jianzhong Zhang
// This code is under The Code Project Open License
// Please read the attached license document before using this class
//------------------------------------------------------------------

// class translate mesh3d object to ModelVisual3D object.
// version 0.1

using System.Collections;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows;
using System;
using System.Windows.Controls;
using System.IO.Packaging;

namespace WPFChart3D
{
    public class Model3D : ModelVisual3D
    {
        private WPFChart3D.TextureMapping m_mapping = new TextureMapping();

        public Model3D()
        {
        }

        public void SetRGBColor()
        {
            m_mapping.SetRGBMaping();
        }

        public void SetPsedoColor()
        {
            m_mapping.SetPseudoMaping();
        }

        public void SetMapping(string file) => m_mapping.SetMaping(file);

        // set this ModelVisual3D object from a array of mesh3D objects
        private void SetModel(ArrayList meshs, Material backMaterial)
        {
            int nMeshNo = meshs.Count;
            if (nMeshNo == 0) return;

            MeshGeometry3D triangleMesh = new MeshGeometry3D();
            int nTotalVertNo = 0;
            for (int j = 0; j < nMeshNo; j++)
            {
                Mesh3D mesh = (Mesh3D)meshs[j];
                int nVertNo = mesh.GetVertexNo();
                int nTriNo = mesh.GetTriangleNo();
                if ((nVertNo <= 0) || (nTriNo <= 0)) continue;
                
                double[] vx = new double[nVertNo];
                double[] vy = new double[nVertNo];
                double[] vz = new double[nVertNo];
                for (int i = 0; i < nVertNo; i++)
                {
                    vx[i] = vy[i] = vz[i] = 0;
                }

                // get normal of each vertex
                for (int i = 0; i < nTriNo; i++)
                {
                    Triangle3D tri = mesh.GetTriangle(i);
                    Vector3D vN = mesh.GetTriangleNormal(i);
                    int n0 = tri.n0;
                    int n1 = tri.n1;
                    int n2 = tri.n2;

                    vx[n0] += vN.X;
                    vy[n0] += vN.Y;
                    vz[n0] += vN.Z;
                    vx[n1] += vN.X;
                    vy[n1] += vN.Y;
                    vz[n1] += vN.Z;
                    vx[n2] += vN.X;
                    vy[n2] += vN.Y;
                    vz[n2] += vN.Z;
                }
                for (int i = 0; i < nVertNo; i++)
                {
                    double length = Math.Sqrt(vx[i]*vx[i] + vy[i]*vy[i] + vz[i]*vz[i]);
                    if (length > 1e-20)
                    {
                        vx[i] /= length;
                        vy[i] /= length;
                        vz[i] /= length;
                    }
                    triangleMesh.Positions.Add(mesh.GetPoint(i));
                    Color color = mesh.GetColor(i);
                    Point mapPt = m_mapping.GetMappingPosition(color);
                    triangleMesh.TextureCoordinates.Add(new System.Windows.Point(mapPt.X, mapPt.Y));
                    triangleMesh.Normals.Add(new Vector3D(vx[i], vy[i], vz[i]));
                }

                for (int i = 0; i < nTriNo; i++)
                {
                    Triangle3D tri = mesh.GetTriangle(i);
                    int n0 = tri.n0;
                    int n1 = tri.n1;
                    int n2 = tri.n2;

                    triangleMesh.TriangleIndices.Add(nTotalVertNo + n0);
                    triangleMesh.TriangleIndices.Add(nTotalVertNo + n1);
                    triangleMesh.TriangleIndices.Add(nTotalVertNo + n2);
                 }
                nTotalVertNo += nVertNo;
            }
            //Material material = new DiffuseMaterial(new SolidColorBrush(Colors.Red));
            Material material = m_mapping.m_material;

            GeometryModel3D triangleModel = new GeometryModel3D(triangleMesh, material);
            triangleModel.Transform = new Transform3DGroup();
            if (backMaterial != null) triangleModel.BackMaterial = backMaterial;

            Content = triangleModel;
        }
    
        // get MeshGeometry3D object from Viewport3D
        public static MeshGeometry3D GetGeometry(Viewport3D viewport3d, int nModelIndex)
        {
            if (nModelIndex == -1) return null;
            ModelVisual3D visual3d = (ModelVisual3D)(viewport3d.Children[nModelIndex]);
            if (visual3d.Content == null) return null;
            GeometryModel3D triangleModel = (GeometryModel3D)(visual3d.Content);
            return (MeshGeometry3D)triangleModel.Geometry;
        }

        // update the ModelVisual3D object in "viewport3d" using Mesh3D array "meshs"
        public int UpdateModel(ArrayList meshs, Material backMaterial, int nModelIndex, Viewport3D viewport3d)
        {
            if (nModelIndex >= 0)
            {
                ModelVisual3D m = (ModelVisual3D)viewport3d.Children[nModelIndex];
                viewport3d.Children.Remove(m);
            }

            if(backMaterial==null)
                SetRGBColor();
            else
                SetPsedoColor();

            SetModel(meshs, backMaterial);

            int nModelNo = viewport3d.Children.Count;
            viewport3d.Children.Add(this);

            return nModelNo;
        }

    }
}
