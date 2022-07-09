using beta.Infrastructure.Behaviors;
using beta.Models.API.MapsVault;
using beta.ViewModels;
using System;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;

namespace beta.Views
{
    /// <summary>
    /// Interaction logic for MapDetailsView.xaml
    /// </summary>
    public partial class MapDetailsView : UserControl
    {
        public MapDetailsView() => InitializeComponent();
        public MapDetailsView(int id, NavigationService nav = null) : this() =>
            DataContext = new MapViewModel(id, nav);
        public MapDetailsView(string name, NavigationService nav = null) : this() =>
            DataContext = new MapViewModel(name, nav);
        public MapDetailsView(ApiMapModel selected, ApiMapModel[] similar, NavigationService nav = null) : this() =>
            DataContext = new MapViewModel(selected, similar, nav);
        private void ListBox_Initialized(object sender, System.EventArgs e)
        {
            ((ListBox)sender).ItemsSource = Enumerable.Range(0, 20);
        }

        private void ScrollViewer_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var scroll = (ScrollViewer)sender;
            var property = scroll.GetType().GetProperty("ScrollInfo", BindingFlags.NonPublic | BindingFlags.Instance);
            property.SetValue(scroll, new ScrollInfoAdapter((IScrollInfo)property.GetValue(scroll)));
        }

        private void ListBox_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //var scroll = Tools.FindChild<ScrollViewer>((ListBox)sender);
            //var property = scroll.GetType().GetProperty("ScrollInfo", BindingFlags.NonPublic | BindingFlags.Instance);
            //property.SetValue(scroll, new ScrollInfoAdapter((IScrollInfo)property.GetValue(scroll)));
        }

		#region 3D Viewport

		/**
		 * <summary>
		 * Method that zoom in and out on mouse wheel. Reference Code: https://www.codeproject.com/Articles/23332/WPF-D-Primer
		 * </summary>
		 *
		 * <param name="sender">sender object</param>
		 * <param name="e">arguments</param>
		 */
		private void _Viewport3DMouseWheel(object sender, MouseWheelEventArgs e)
		{
			var viewport = (Viewport3D)sender;
			var cam = (PerspectiveCamera)viewport.Camera;
			cam.Position = new Point3D(cam.Position.X, cam.Position.Y, cam.Position.Z - e.Delta / 2D);
		}

		/**
		 * <summary>
		 * variable to control the viewport rotation through the mouse
		 * </summary>
		 */
		private bool _MouseDownFlag;

		/**
		 * <summary>
		 * variable to control the viewport rotation through the mouse
		 * </summary>
		 */
		private System.Windows.Point _MouseLastPos;

		/**
		 * <summary>
		 * Method to control the viewport rotation through the mouse. Reference Code: https://www.codeproject.com/Articles/23332/WPF-D-Primer
		 * </summary>
		 *
		 * <param name="sender">sender object</param>
		 * <param name="e">arguments</param>
		 */
		private void _Viewport3DMouseUp(object sender, MouseButtonEventArgs e)
		{
			_MouseDownFlag = false;
		}

		/**
		 * <summary>
		 * Method to control the viewport rotation through the mouse. Reference Code: https://www.codeproject.com/Articles/23332/WPF-D-Primer
		 * </summary>
		 *
		 * <param name="sender">sender object</param>
		 * <param name="e">arguments</param>
		 */
		private void _Viewport3DMouseDown(object sender, MouseButtonEventArgs e)
		{
			var viewport = (Viewport3D)sender;
			if (e.LeftButton != MouseButtonState.Pressed) return;
			_MouseDownFlag = true;
			System.Windows.Point pos = Mouse.GetPosition(viewport);
			_MouseLastPos = new System.Windows.Point(pos.X - viewport.ActualWidth / 2, viewport.ActualHeight / 2 - pos.Y);
		}

		/**
		 * <summary>
		 * Method to control the viewport rotation through the mouse. Reference Code: https://www.codeproject.com/Articles/23332/WPF-D-Primer
		 * </summary>
		 *
		 * <param name="sender">sender object</param>
		 * <param name="e">arguments</param>
		 */
		private void _Viewport3DMouseMove(object sender, MouseEventArgs e)
		{
			var viewport = (Viewport3D)sender;
			if (!_MouseDownFlag) return;
			System.Windows.Point pos = Mouse.GetPosition(viewport);
			System.Windows.Point actualPos = new System.Windows.Point(pos.X - viewport.ActualWidth / 2, viewport.ActualHeight / 2 - pos.Y);
			double dx = actualPos.X - _MouseLastPos.X;
			double dy = actualPos.Y - _MouseLastPos.Y;
			double mouseAngle = 0;

			if (dx != 0 && dy != 0)
			{
				mouseAngle = Math.Asin(Math.Abs(dy) / Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2)));

				if (dx < 0 && dy > 0) mouseAngle += Math.PI / 2;
				else if (dx < 0 && dy < 0) mouseAngle += Math.PI;
				else if (dx > 0 && dy < 0) mouseAngle += Math.PI * 1.5;
			}
			else if (dx == 0 && dy != 0)
			{
				mouseAngle = Math.Sign(dy) > 0 ? Math.PI / 2 : Math.PI * 1.5;
			}
			else if (dx != 0 && dy == 0)
			{
				mouseAngle = Math.Sign(dx) > 0 ? 0 : Math.PI;
			}

			double axisAngle = mouseAngle + Math.PI / 2;

			Vector3D axis = new Vector3D(Math.Cos(axisAngle) * 4, Math.Sin(axisAngle) * 4, 0);

			double rotation = 0.02 * Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));

			Transform3DGroup group = viewport.Children[0].Transform as Transform3DGroup;

			if (group == null)
			{
				group = new Transform3DGroup();
				viewport.Children[0].Transform = group;
			}

			QuaternionRotation3D r =
				 new QuaternionRotation3D(
				 new Quaternion(axis, rotation * 180 / Math.PI));
			group.Children.Add(new RotateTransform3D(r));

			_MouseLastPos = actualPos;
		}
		#endregion
	}
}
