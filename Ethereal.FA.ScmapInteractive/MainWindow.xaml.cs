using Ethereal.FA.Vault;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using WPFChart3D;
using Model3D = WPFChart3D.Model3D;
using Point = System.Windows.Point;

namespace Ethereal.FA.ScmapInteractive
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // transform class object for rotate the 3d model
        public TransformMatrix m_transformMatrix = new TransformMatrix();

        // ***************************** 3d chart ***************************
        private Chart3D m_3dChart;       // data for 3d chart
        public int m_nChartModelIndex = -1;         // model index in the Viewport3d
        public int m_nSurfaceChartGridNo = 1000;     // surface chart grid no. in each axis
        public int m_nScatterPlotDataNo = 5000;     // total data number of the scatter plot

        // ***************************** selection rect ***************************
        ViewportRect m_selectRect = new ViewportRect();
        public int m_nRectModelIndex = -1;



        public MainWindow()
        {
            InitializeComponent();

            // selection rect
            m_selectRect.SetRect(new Point(-0.5, -0.5), new Point(-0.5, -0.5));
            Model3D model3d = new Model3D();
            ArrayList meshs = m_selectRect.GetMeshes();
            m_nRectModelIndex = model3d.UpdateModel(meshs, null, m_nRectModelIndex, this.mainViewport);

            // display the 3d chart data no.
            gridNo.Text = String.Format("{0:d}", m_nSurfaceChartGridNo);
            dataNo.Text = String.Format("{0:d}", m_nScatterPlotDataNo);

            // display surface chart
            TestScatterPlot(1000);
            TransformChart();
        }

        // function for testing surface chart
        public void TestSurfacePlot(int nGridNo)
        {
            int nXNo = nGridNo;
            int nYNo = nGridNo;
            // 1. set the surface grid
            m_3dChart = new UniformSurfaceChart3D();
            ((UniformSurfaceChart3D)m_3dChart).SetGrid(nXNo, nYNo, -100, 100, -100, 100);

            // 2. set surface chart z value
            double xC = m_3dChart.XCenter();
            double yC = m_3dChart.YCenter();
            int nVertNo = m_3dChart.GetDataNo();
            double zV;
            for (int i = 0; i < nVertNo; i++)
            {
                Vertex3D vert = m_3dChart[i];

                double r = 0.15 * Math.Sqrt((vert.x - xC) * (vert.x - xC) + (vert.y - yC) * (vert.y - yC));
                if (r < 1e-10) zV = 1;
                else zV = Math.Sin(r) / r;

                m_3dChart[i].z = (float)zV;
            }
            m_3dChart.GetDataRange();

            // 3. set the surface chart color according to z vaule
            double zMin = m_3dChart.ZMin();
            double zMax = m_3dChart.ZMax();
            for (int i = 0; i < nVertNo; i++)
            {
                Vertex3D vert = m_3dChart[i];
                double h = (vert.z - zMin) / (zMax - zMin);

                Color color = WPFChart3D.TextureMapping.PseudoColor(h);
                m_3dChart[i].color = color;
            }

            // 4. Get the Mesh3D array from surface chart
            ArrayList meshs = ((UniformSurfaceChart3D)m_3dChart).GetMeshes();

            // 5. display vertex no and triangle no of this surface chart
            UpdateModelSizeInfo(meshs);

            // 6. Set the model display of surface chart
            Model3D model3d = new Model3D();
            Material backMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Gray));
            m_nChartModelIndex = model3d.UpdateModel(meshs, backMaterial, m_nChartModelIndex, this.mainViewport);

            // 7. set projection matrix, so the data is in the display region
            float xMin = m_3dChart.XMin();
            float xMax = m_3dChart.XMax();
            m_transformMatrix.CalculateProjectionMatrix(xMin, xMax, xMin, xMax, zMin, zMax, 0.5);
            TransformChart();
        }

        // function for testing 3d scatter plot
        public void TestScatterPlot(int nDotNo)
        {
            // 1. set scatter chart data no.
            m_3dChart = new ScatterChart3D();
            m_3dChart.SetDataNo(nDotNo);

            // 2. set property of each dot (size, position, shape, color)
            Random randomObject = new Random();
            int nDataRange = 200;
            for (int i = 0; i < nDotNo; i++)
            {
                ScatterPlotItem plotItem = new ScatterPlotItem();

                plotItem.w = 4;
                plotItem.h = 6;

                plotItem.x = (float)randomObject.Next(nDataRange);
                plotItem.y = (float)randomObject.Next(nDataRange);
                plotItem.z = (float)randomObject.Next(nDataRange);

                plotItem.shape = randomObject.Next(4);

                Byte nR = (Byte)randomObject.Next(256);
                Byte nG = (Byte)randomObject.Next(256);
                Byte nB = (Byte)randomObject.Next(256);

                plotItem.color = Color.FromRgb(nR, nG, nB);
                ((ScatterChart3D)m_3dChart).SetVertex(i, plotItem);
            }

            // 3. set the axes
            m_3dChart.GetDataRange();
            m_3dChart.SetAxes();

            // 4. get Mesh3D array from the scatter plot
            ArrayList meshs = ((ScatterChart3D)m_3dChart).GetMeshes();

            // 5. display model vertex no and triangle no
            UpdateModelSizeInfo(meshs);

            // 6. display scatter plot in Viewport3D
            Model3D model3d = new Model3D();
            m_nChartModelIndex = model3d.UpdateModel(meshs, null, m_nChartModelIndex, this.mainViewport);

            // 7. set projection matrix
            float viewRange = (float)nDataRange;
            m_transformMatrix.CalculateProjectionMatrix(0, viewRange, 0, viewRange, 0, viewRange, 0.5);
            TransformChart();
        }

        // function for set a scatter plot, every dot is just a simple pyramid.
        public void TestSimpleScatterPlot(int nDotNo)
        {
            // 1. set the scatter plot size
            m_3dChart = new ScatterChart3D();
            m_3dChart.SetDataNo(nDotNo);

            // 2. set the properties of each dot
            Random randomObject = new Random();
            int nDataRange = 200;
            for (int i = 0; i < nDotNo; i++)
            {
                ScatterPlotItem plotItem = new ScatterPlotItem();

                plotItem.w = 2;
                plotItem.h = 2;

                plotItem.x = (float)randomObject.Next(nDataRange);
                plotItem.y = (float)randomObject.Next(nDataRange);
                plotItem.z = (float)randomObject.Next(nDataRange);

                plotItem.shape = (int)Chart3D.SHAPE.PYRAMID;

                Byte nR = (Byte)randomObject.Next(256);
                Byte nG = (Byte)randomObject.Next(256);
                Byte nB = (Byte)randomObject.Next(256);

                plotItem.color = Color.FromRgb(nR, nG, nB);
                ((ScatterChart3D)m_3dChart).SetVertex(i, plotItem);
            }
            // 3. set axes
            m_3dChart.GetDataRange();
            m_3dChart.SetAxes();

            // 4. Get Mesh3D array from scatter plot
            ArrayList meshs = ((ScatterChart3D)m_3dChart).GetMeshes();

            // 5. display vertex no and triangle no.
            UpdateModelSizeInfo(meshs);

            // 6. show 3D scatter plot in Viewport3d
            Model3D model3d = new Model3D();
            m_nChartModelIndex = model3d.UpdateModel(meshs, null, m_nChartModelIndex, this.mainViewport);

            // 7. set projection matrix
            float viewRange = (float)nDataRange;
            m_transformMatrix.CalculateProjectionMatrix(0, viewRange, 0, viewRange, 0, viewRange, 0.5);
            TransformChart();
        }


        public void OnViewportMouseDown(object sender, MouseButtonEventArgs args)
        {
            Point pt = args.GetPosition(mainViewport);
            if (args.ChangedButton == MouseButton.Left)         // rotate or drag 3d model
            {
                m_transformMatrix.OnLBtnDown(pt);
            }
            else if (args.ChangedButton == MouseButton.Right)   // select rect
            {
                m_selectRect.OnMouseDown(pt, mainViewport, m_nRectModelIndex);
            }
        }

        public void OnViewportMouseMove(object sender, MouseEventArgs args)
        {
            Point pt = args.GetPosition(mainViewport);

            if (args.LeftButton == MouseButtonState.Pressed)                // rotate or drag 3d model
            {
                m_transformMatrix.OnMouseMove(pt, mainViewport);

                TransformChart();
            }
            else if (args.RightButton == MouseButtonState.Pressed)          // select rect
            {
                m_selectRect.OnMouseMove(pt, mainViewport, m_nRectModelIndex);
            }
            else
            {
                /*
                String s1;
                Point pt2 = m_transformMatrix.VertexToScreenPt(new Point3D(0.5, 0.5, 0.3), mainViewport);
                s1 = string.Format("Screen:({0:d},{1:d}), Predicated: ({2:d}, H:{3:d})", 
                    (int)pt.X, (int)pt.Y, (int)pt2.X, (int)pt2.Y);
                this.statusPane.Text = s1;
                */
            }
        }

        public void OnViewportMouseUp(object sender, MouseButtonEventArgs args)
        {
            Point pt = args.GetPosition(mainViewport);
            if (args.ChangedButton == MouseButton.Left)
            {
                m_transformMatrix.OnLBtnUp();
            }
            else if (args.ChangedButton == MouseButton.Right)
            {
                if (m_nChartModelIndex == -1) return;
                // 1. get the mesh structure related to the selection rect
                MeshGeometry3D meshGeometry = Model3D.GetGeometry(mainViewport, m_nChartModelIndex);
                if (meshGeometry == null) return;

                // 2. set selection in 3d chart
                m_3dChart.Select(m_selectRect, m_transformMatrix, mainViewport);

                // 3. update selection display
                m_3dChart.HighlightSelection(meshGeometry, Color.FromRgb(200, 200, 200));
            }
        }
        public static float Sigmoid(double value)
        {
            float k = (float)Math.Exp(value);
            return k / (1.0f + k);
        }
        // zoom in 3d display
        public void OnKeyDown(object sender, KeyEventArgs args)
        {
            m_transformMatrix.OnKeyDown(args);
            TransformChart();
        }


        float Lerp(float firstFloat, float secondFloat, float by)
        {
            return firstFloat * (1 - by) + secondFloat * by;
        }
        private void SelectScmap(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            var selected = dialog.ShowDialog();
            if (selected == true)
            {
                GC.Collect(3);
                GC.WaitForFullGCComplete();

                var scmap = Scmap.FromFile(dialog.FileName);

                int nXNo = scmap.Width;
                int nYNo = scmap.Height;
                // 1. set the surface grid
                m_3dChart = new UniformSurfaceChart3D();
                ((UniformSurfaceChart3D)m_3dChart).SetGrid(nXNo/3, nYNo/3, 0, scmap.Width, 0, scmap.Height);

                // 2. set surface chart z value
                double xC = m_3dChart.XCenter();
                double yC = m_3dChart.YCenter();
                int nVertNo = m_3dChart.GetDataNo();
                double z = 0;

                var min = 0.0;
                var max = 0.0;
                float HeightWidthMultiply = scmap.Height / (float)scmap.Width;
                var scale = scmap.HeightScale;
                for (int i = 0; i < nVertNo; i++)
                {
                    Vertex3D vert = m_3dChart[i];

                    //double r = 0.15 * Math.Sqrt(Scmap.GetHeight((int)vert.x, (int)vert.y, map.Heightmap, map.Width) * map.HeigthScale);
                    //if (r < 1e-10) zV = 1;
                    //else zV = Math.Sin(r) / r;

                    var h = Scmap.GetHeight((int)vert.x, (int)vert.y, scmap.Heightmap, scmap.Width);
                    //z = Lerp(h, (float)z, 0.5f);
                    //if (h > 1) h-= Math.Pow(h,3);
                    z = h * (float)scmap.HeightScale;
                    //if (i == 0) z = 0;
                    //if (i == nVertNo - 1) z = 255;

                    //if (HeightWidthMultiply == 0.5f && y > 0 && y % 2f == 0)
                    //{
                    //	heights[y - 1, x] = Lerp(heights[y, x], heights[y - 2, x], 0.5f);
                    //}
                    //if (z > h) z+= h * map.HeigthScale/3;
                    //if (z < h) z-= h * map.HeigthScale/3;

                    if (min > z) min = z;
                    if (max < z) max = z;

                    m_3dChart[i].z = (float)z;
                    z = h;
                }
                var avg = (max + min) / 2;
                var k = (float)500 * scmap.HeightScale;
                for (int i = 0; i < nVertNo; i++)
                {
                    Vertex3D vert = m_3dChart[i];

                    if (vert.z > avg)
                    {
                        vert.z -= (float)k * (float)0.95;
                    }
                    else
                    {
                        vert.z += (float)k * (float)0.15;
                    }
                    //m_3dChart[i].z = (float)z;
                }
                m_3dChart.GetDataRange();

                // 3. set the surface chart color according to z vaule
                double zMin = m_3dChart.ZMin();
                double zMax = m_3dChart.ZMax();
                //for (int i = 0; i < nVertNo; i++)
                //{
                //    Vertex3D vert = m_3dChart[i];
                //    double h = (vert.z - zMin) / (zMax - zMin);

                //    Color color = WPFChart3D.TextureMapping.PseudoColor(h);
                //    m_3dChart[i].color = color;
                //}

                // 4. Get the Mesh3D array from surface chart
                ArrayList meshs = ((UniformSurfaceChart3D)m_3dChart).GetMeshes();

                // 5. display vertex no and triangle no of this surface chart
                UpdateModelSizeInfo(meshs);

                // 6. Set the model display of surface chart
                Model3D model3d = new Model3D();

                BitmapImage bitmap = new BitmapImage(new Uri(dialog.FileName.Replace(".scmap",".png")));
                ImageBrush imageBrush = new ImageBrush(bitmap);
                Material backMaterial = new DiffuseMaterial(imageBrush);
                m_nChartModelIndex = model3d.UpdateModel(meshs, backMaterial, m_nChartModelIndex, this.mainViewport);

                // 7. set projection matrix, so the data is in the display region
                float xMin = m_3dChart.XMin();
                float xMax = m_3dChart.XMax();
                m_transformMatrix.CalculateProjectionMatrix(xMin, xMax, xMin, xMax, zMin, zMax, scmap.HeightScale, 0.6);
                TransformChart();
            }
        }
        private void surfaceButton_Click(object sender, RoutedEventArgs e)
        {
            int nGridNo = Int32.Parse(gridNo.Text);
            if (nGridNo < 2) return;
            if (nGridNo > 500)
            {
                MessageBox.Show("too many data");
                return;
            }
            TestSurfacePlot(nGridNo);
        }

        private void scatterButton_Click(object sender, RoutedEventArgs e)
        {
            int nDataNo = Int32.Parse(dataNo.Text);
            if (nDataNo < 3) return;

            if ((bool)checkBoxShape.IsChecked)
            {
                if (nDataNo > 10000)
                {
                    MessageBox.Show("too many data");
                    return;
                }
                TestScatterPlot(nDataNo);
            }
            else
            {
                if (nDataNo > 100000)
                {
                    MessageBox.Show("too many data");
                    return;
                }
                TestSimpleScatterPlot(nDataNo);
            }
        }

        private void UpdateModelSizeInfo(ArrayList meshs)
        {
            int nMeshNo = meshs.Count;
            int nChartVertNo = 0;
            int nChartTriangelNo = 0;
            for (int i = 0; i < nMeshNo; i++)
            {
                nChartVertNo += ((Mesh3D)meshs[i]).GetVertexNo();
                nChartTriangelNo += ((Mesh3D)meshs[i]).GetTriangleNo();
            }
            labelVertNo.Content = String.Format("Vertex No: {0:d}", nChartVertNo);
            labelTriNo.Content = String.Format("Triangle No: {0:d}", nChartTriangelNo);
        }

        // this function is used to rotate, drag and zoom the 3d chart
        private void TransformChart()
        {
            if (m_nChartModelIndex == -1) return;
            ModelVisual3D visual3d = (ModelVisual3D)(this.mainViewport.Children[m_nChartModelIndex]);
            if (visual3d.Content == null) return;
            Transform3DGroup group1 = visual3d.Content.Transform as Transform3DGroup;
            group1.Children.Clear();
            group1.Children.Add(new MatrixTransform3D(m_transformMatrix.m_totalMatrix));
        }
    }
}
