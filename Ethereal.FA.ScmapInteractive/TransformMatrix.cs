﻿//------------------------------------------------------------------
// (c) Copywrite Jianzhong Zhang
// This code is under The Code Project Open License
// Please read the attached license document before using this class
//------------------------------------------------------------------

// class for a 3d rotation, drag and zoom.
// version 0.1

using System;
using System.Windows.Media.Media3D;
using System.Windows.Input;
using System.Windows;

namespace WPFChart3D
{
    public class TransformMatrix
    {
        private Matrix3D m_viewMatrix = new Matrix3D();
        private Matrix3D m_projMatrix = new Matrix3D();
        public Matrix3D m_totalMatrix = new Matrix3D();

        public double m_scaleFactor = 1.3;                      // sensativity for zoom
        private bool m_mouseDown = false;                       // mouse down
        private Point m_movePoint;                              // previous mouse location

        public void ResetView()
        {
            m_viewMatrix.SetIdentity();
        }

        public void OnLBtnDown(Point pt)
        {
            m_mouseDown = true;
            m_movePoint = pt;
        }

        public void OnMouseMove(Point pt, System.Windows.Controls.Viewport3D viewPort)
        {
            if (!m_mouseDown) return;

            double width = viewPort.ActualWidth;
            double height = viewPort.ActualHeight;

            //OrthographicCamera camera = viewPort.Camera as System.Windows.Media.Media3D.OrthographicCamera;
            //Matrix3D cameraMatrix = camera.Transform.Value;

            if (Keyboard.IsKeyDown(Key.LeftCtrl)|| Keyboard.IsKeyDown(Key.RightCtrl))
            {
            }
            else if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                double shiftX = 2 *(pt.X - m_movePoint.X) /( width);
                double shiftY = -2 *(pt.Y - m_movePoint.Y)/( width);
                m_viewMatrix.Translate(new Vector3D(shiftX, shiftY, 0));
                m_movePoint = pt;
            }
            else
            {
                double aY = 180 * (pt.X - m_movePoint.X) / width;
                double aX = 180 * (pt.Y - m_movePoint.Y) / height;

                m_viewMatrix.Rotate(new Quaternion(new Vector3D(1, 0, 0), aX));
                m_viewMatrix.Rotate(new Quaternion(new Vector3D(0, 1, 0), aY));
                m_movePoint = pt;
            }
            m_totalMatrix = Matrix3D.Multiply(m_projMatrix, m_viewMatrix);
        }
		
        public void OnLBtnUp()
        {
            m_mouseDown = false;
        }

        public void OnKeyDown(System.Windows.Input.KeyEventArgs args)
        {
            switch (args.Key)
            {
                case Key.Home:
                     m_viewMatrix.SetIdentity();
                     break;
                case Key.OemPlus:
                     m_viewMatrix.Scale(new Vector3D(m_scaleFactor, m_scaleFactor, m_scaleFactor));
                     break;
                case Key.OemMinus:
                     m_viewMatrix.Scale(new Vector3D(1/m_scaleFactor, 1/m_scaleFactor, 1/m_scaleFactor));
                     break;
                default:
                     return;
            }
            m_totalMatrix = Matrix3D.Multiply(m_projMatrix, m_viewMatrix);
        }

        // transform input point pt1, (rotate "aX, aZ" and move to "center")
        public static Point3D Transform(Point3D pt1, Point3D center, double aX, double aZ)
        {
 			double angleX = 3.1415926f*aX/180;
			double angleZ = 3.1415926f*aZ/180;

			// rotate from z-axis
            double x2 = pt1.X * Math.Cos(angleZ) + pt1.Z * Math.Sin(angleZ);
            double y2 = pt1.Y;
            double z2 = -pt1.X * Math.Sin(angleZ) + pt1.Z * Math.Cos(angleZ);

            double x3 = center.X + x2 * Math.Cos(angleX) - y2 * Math.Sin(angleX);
            double y3 = center.Y + x2 * Math.Sin(angleX) + y2 * Math.Cos(angleX);
			double z3 = center.Z + z2;

            return new Point3D(x3, y3, z3);
        }

        // transform input point pt1, (rotate "aX, aZ" and move to "center")
        public static void Transform(Mesh3D model, Point3D center, double aX, double aZ)
        {
 			double angleX = 3.1415926f*aX/180;
			double angleZ = 3.1415926f*aZ/180;

            int nVertNo = model.GetVertexNo();
            for (int i = 0; i < nVertNo; i++)
            {
                Point3D pt1 = model.GetPoint(i);
                // rotate from z-axis
                double x2 = pt1.X * Math.Cos(angleZ) + pt1.Z * Math.Sin(angleZ);
                double y2 = pt1.Y;
                double z2 = -pt1.X * Math.Sin(angleZ) + pt1.Z * Math.Cos(angleZ);

                double x3 = center.X + x2 * Math.Cos(angleX) - y2 * Math.Sin(angleX);
                double y3 = center.Y + x2 * Math.Sin(angleX) + y2 * Math.Cos(angleX);
                double z3 = center.Z + z2;

                model.SetPoint(i, x3, y3, z3);
            }
        }

        // set the projection matrix
        public void CalculateProjectionMatrix(WPFChart3D.Mesh3D mesh, double scaleFactor)
        {
            CalculateProjectionMatrix(mesh.m_xMin, mesh.m_xMax, mesh.m_yMin, mesh.m_yMax, mesh.m_zMin, mesh.m_zMax, scaleFactor);
        }

        public void CalculateProjectionMatrix(double min, double max, double scaleFactor)
        {
            CalculateProjectionMatrix(min, max, min, max, min, max, scaleFactor);
        }

        public void CalculateProjectionMatrix(double xMin, double xMax, double yMin, double yMax, double zMin, double zMax, double scaleFactor)
        {
            double xC = (xMin + xMax) / 2;
            double yC = (yMin + yMax) / 2;
            double zC = (zMin + zMax) / 2;

            double xRange = (xMax - xMin) / 2;
            double yRange = (yMax - yMin) / 2;
            double zRange = (zMax - zMin) / 2;

            m_projMatrix.SetIdentity();
            m_projMatrix.Translate(new Vector3D(-xC, -yC, -zC));

            if (xRange < 1e-10) return;

            double sX = scaleFactor/xRange;
            double sY = scaleFactor/yRange;
            double sZ = scaleFactor/zRange;
            m_projMatrix.Scale(new Vector3D(sX, sY, sZ));

            m_totalMatrix = Matrix3D.Multiply(m_projMatrix, m_viewMatrix);
        }
        public void CalculateProjectionMatrix(double xMin, double xMax, double yMin, double yMax, double zMin, double zMax, double zScale, double scaleFactor)
        {
            double xC = (xMin + xMax) / 2;
            double yC = (yMin + yMax) / 2;
            double zC = (zMin + zMax) / 2;

            double xRange = (xMax - xMin) / 2;
            double yRange = (yMax - yMin) / 2;
            //double zRange = (zMax - zMin) / 2;

            m_projMatrix.SetIdentity();
            m_projMatrix.Translate(new Vector3D(-xC, -yC, -zC));

            if (xRange < 1e-10) return;

            double sX = scaleFactor/xRange;
            double sY = scaleFactor/yRange;
            //double sZ = scaleFactor/zRange;
            m_projMatrix.Scale(new Vector3D(sX, sY, zScale));

            m_totalMatrix = Matrix3D.Multiply(m_projMatrix, m_viewMatrix);
        }

        // get the screen position from original vertex
        public Point VertexToScreenPt(Point3D point, System.Windows.Controls.Viewport3D viewPort)
        {
            Point3D pt2 = m_totalMatrix.Transform(point);

            double width = viewPort.ActualWidth;
            double height = viewPort.ActualHeight;

            double x3 = width / 2 + (pt2.X) * width / 2;
            double y3 = height / 2 - (pt2.Y) * width / 2;

            return new Point(x3, y3);
        }

        public static Point ScreenPtToViewportPt(Point point, System.Windows.Controls.Viewport3D viewPort)
        {
            double width = viewPort.ActualWidth;
            double height = viewPort.ActualHeight;

            double x3 = (double)point.X;
            double y3 = (double)point.Y;
            double x2 = (x3 - width / 2)*2/width;
            double y2 = (height / 2 - y3)*2/width;

            return new Point(x2, y2);
        }

        public Point VertexToViewportPt(Point3D point, System.Windows.Controls.Viewport3D viewPort)
        {
            Point3D pt2 = m_totalMatrix.Transform(point);
            return new Point(pt2.X, pt2.Y);
        }
    }
}
