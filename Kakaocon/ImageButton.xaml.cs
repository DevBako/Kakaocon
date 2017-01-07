using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Kakaocon {
	/// <summary>
	/// ImageButton.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class ImageButton : UserControl {
		public ImageButton() {
			InitializeComponent();

			this.RenderTransformOrigin = new Point(0.5, 0.5);
		}

		#region Source
		public string Source {
			get { return (string)GetValue(SourceProperty); }
			set { SetValue(SourceProperty, value); }
		}

		public static readonly DependencyProperty SourceProperty
			= DependencyProperty.Register("Source", typeof(string), typeof(ImageButton),
			new FrameworkPropertyMetadata(
				null,
				FrameworkPropertyMetadataOptions.AffectsRender,
				SourcePropertyChanged));

		private static void SourcePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
			ImageButton button = obj as ImageButton;

			string uri = string.Format("pack://application:,,,/Kakaocon;component/{0}", e.NewValue);
			button.image.Source = new BitmapImage(new Uri(uri));
		}
		#endregion

		#region ViewMode
		public enum Mode { Visible, Disable, Hidden };

		public Mode ViewMode {
			get { return (Mode)GetValue(EnableProperty); }
			set { SetValue(EnableProperty, value); }
		}
		public static readonly DependencyProperty EnableProperty
			= DependencyProperty.Register("ViewMode", typeof(Mode), typeof(ImageButton),
			new FrameworkPropertyMetadata(
				Mode.Visible,
				FrameworkPropertyMetadataOptions.AffectsRender,
				EnablePropertyChanged));

		private static void EnablePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
			ImageButton button = obj as ImageButton;
			button.AnimateControl((Mode)e.OldValue, (Mode)e.NewValue);
		}
		#endregion

		#region Selected
		public enum SelectedMode { True, False, None };

		public SelectedMode Selected {
			get { return (SelectedMode)GetValue(SelectedProperty); }
			set { SetValue(SelectedProperty, value); }
		}
		public static readonly DependencyProperty SelectedProperty
			= DependencyProperty.Register("Selected", typeof(SelectedMode), typeof(ImageButton),
			new FrameworkPropertyMetadata(
				SelectedMode.None,
				FrameworkPropertyMetadataOptions.AffectsRender,
				SelectedPropertyChanged));

		private static void SelectedPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
			ImageButton button = obj as ImageButton;
			Rectangle select = button.select;

			switch ((SelectedMode)e.NewValue) {
				case SelectedMode.None:
					select.Visibility = Visibility.Collapsed;
					button.Opacity = 1;
					break;

				case SelectedMode.True:
					select.Visibility = Visibility.Visible;
					button.Opacity = 1;
					break;

				case SelectedMode.False:
					select.Visibility = Visibility.Collapsed;
					button.Opacity = 0.5;
					break;
			}
		}
		#endregion

		#region InnerMargin
		public double InnerMargin {
			get { return (double)GetValue(InnerMarginProperty); }
			set {
				SetValue(InnerMarginProperty, value);
			}
		}
		public static readonly DependencyProperty InnerMarginProperty = DependencyProperty.Register(
			"InnerMargin",
			typeof(double),
			typeof(ImageButton),
			new FrameworkPropertyMetadata(
				Double.NaN,
				FrameworkPropertyMetadataOptions.AffectsRender,
				InnerMarginPropertyChanged));

		private static void InnerMarginPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
			(obj as ImageButton).image.Margin = new Thickness((double)e.NewValue);
		}
		#endregion

		#region Size
		public double Size {
			get { return (double)GetValue(SizeProperty); }
			set {
				SetValue(SizeProperty, value);
			}
		}
		public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
			"Size",
			typeof(double),
			typeof(ImageButton),
			new FrameworkPropertyMetadata(
				Double.NaN,
				FrameworkPropertyMetadataOptions.AffectsRender,
				SizePropertyChanged));

		private static void SizePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
			(obj as ImageButton).Width = (double)e.NewValue;
			(obj as ImageButton).Height = (double)e.NewValue;

			(obj as ImageButton).circle.Width = (obj as ImageButton).circle.Height = (double)e.NewValue - 10;
		}
		#endregion

		public string Type { get; set; }
		public event EventHandler<CustomButtonEventArgs> Response;
		private void Button_Click(object sender, RoutedEventArgs e) {
			if (ViewMode == Mode.Hidden || ViewMode == Mode.Disable) { return; }

			//AnimateCircle();

			if (Response != null) {
				Response(this, new CustomButtonEventArgs("Click", Type, ""));
			}
		}

		public event EventHandler<CustomButtonEventArgs> EnterLeave;
		private void gridMain_MouseEnter(object sender, MouseEventArgs e) {
			if (EnterLeave != null) {
				EnterLeave(this, new CustomButtonEventArgs("Enter", Type, ""));
			}
		}

		private void gridMain_MouseLeave(object sender, MouseEventArgs e) {
			if (EnterLeave != null) {
				EnterLeave(this, new CustomButtonEventArgs("Leave", "", ""));
			}
		}

		private void Button_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
			AnimateCirclePressed();
		}

		private void Button_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
			AnimateCircleReleased();
		}

		private void AnimateCirclePressed() {
			Storyboard sb = new Storyboard();

			circle.RenderTransformOrigin = new Point(0.5, 0.5);
			circle.RenderTransform = new ScaleTransform(0.2, 0.2);

			DoubleAnimation opacity = new DoubleAnimation(0, 0.2, TimeSpan.FromMilliseconds(150));
			DoubleAnimation uiopacity = new DoubleAnimation(1, 0.6, TimeSpan.FromMilliseconds(150));
			Storyboard.SetTarget(opacity, circle);
			Storyboard.SetTarget(uiopacity, image);
			Storyboard.SetTargetProperty(opacity, new PropertyPath(Ellipse.OpacityProperty));
			Storyboard.SetTargetProperty(uiopacity, new PropertyPath(Image.OpacityProperty));

			DoubleAnimation scalex = new DoubleAnimation(0.5, TimeSpan.FromMilliseconds(250));
			DoubleAnimation scaley = new DoubleAnimation(0.5, TimeSpan.FromMilliseconds(250));
			Storyboard.SetTarget(scalex, circle);
			Storyboard.SetTarget(scaley, circle);
			Storyboard.SetTargetProperty(scalex, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
			Storyboard.SetTargetProperty(scaley, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));

			sb.Children.Add(opacity);
			sb.Children.Add(uiopacity);
			sb.Children.Add(scalex);
			sb.Children.Add(scaley);

			sb.Begin(this);
		}

		private void AnimateCircleReleased() {
			Storyboard sb = new Storyboard();

			circle.RenderTransformOrigin = new Point(0.5, 0.5);
			circle.RenderTransform = new ScaleTransform(0.2, 0.2);

			DoubleAnimation opacity = new DoubleAnimation(0.6, 0, TimeSpan.FromMilliseconds(250));
			DoubleAnimation uiopacity = new DoubleAnimation(1, TimeSpan.FromMilliseconds(500));
			Storyboard.SetTarget(opacity, circle);
			Storyboard.SetTarget(uiopacity, image);
			Storyboard.SetTargetProperty(opacity, new PropertyPath(Ellipse.OpacityProperty));
			Storyboard.SetTargetProperty(uiopacity, new PropertyPath(Image.OpacityProperty));

			DoubleAnimation scalex = new DoubleAnimation(0.5, 1, TimeSpan.FromMilliseconds(250));
			DoubleAnimation scaley = new DoubleAnimation(0.5, 1, TimeSpan.FromMilliseconds(250));
			Storyboard.SetTarget(scalex, circle);
			Storyboard.SetTarget(scaley, circle);
			Storyboard.SetTargetProperty(scalex, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
			Storyboard.SetTargetProperty(scaley, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));

			sb.Children.Add(opacity);
			sb.Children.Add(uiopacity);
			sb.Children.Add(scalex);
			sb.Children.Add(scaley);

			sb.Begin(this);
		}

		private void AnimateControl(Mode o, Mode n) {
			Storyboard sb = new Storyboard();

			double from = o == Mode.Hidden ? 0 : 1;
			double to = n == Mode.Hidden ? 0 : 1;
			double op = 1;

			switch (n) {
				case Mode.Disable:
					op = 0.3;
					break;
				case Mode.Hidden:
					op = 0;
					break;
				case Mode.Visible:
					op = 1;
					break;
			}

			this.IsHitTestVisible = n == Mode.Visible ? true : false;

			this.RenderTransform = new ScaleTransform(from, from);
			this.RenderTransformOrigin = new Point(0.5, 0.5);

			DoubleAnimation opacity = new DoubleAnimation(op, TimeSpan.FromMilliseconds(250));
			DoubleAnimation scalex = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(250)) {
				EasingFunction = new CubicEase() {
					EasingMode = EasingMode.EaseOut,
				}
			};
			DoubleAnimation scaley = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(250)) {
				EasingFunction = new CubicEase() {
					EasingMode = EasingMode.EaseOut,
				}
			};

			Storyboard.SetTarget(opacity, this);
			Storyboard.SetTarget(scalex, this);
			Storyboard.SetTarget(scaley, this);

			Storyboard.SetTargetProperty(opacity, new PropertyPath(UIElement.OpacityProperty));
			Storyboard.SetTargetProperty(scalex, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
			Storyboard.SetTargetProperty(scaley, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));

			sb.Children.Add(opacity);
			sb.Children.Add(scalex);
			sb.Children.Add(scaley);

			sb.Begin(this);
		}

		Storyboard sb;
		public void StartAnimateImage(int direction = -1) {
			this.IsHitTestVisible = false;

			if (sb == null) {
				sb = new Storyboard() {
					RepeatBehavior = RepeatBehavior.Forever,
				};

				image.RenderTransformOrigin = new Point(0.5, 0.5);
				image.RenderTransform = new RotateTransform(0);

				DoubleAnimation rotate = new DoubleAnimation(0, 360 * direction, TimeSpan.FromMilliseconds(1000));
				Storyboard.SetTarget(rotate, image);
				Storyboard.SetTargetProperty(rotate, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));

				sb.Children.Add(rotate);

				/*
				DoubleAnimation da = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(500));
				Storyboard.SetTarget(da, image);
				Storyboard.SetTargetProperty(da, new PropertyPath(Image.OpacityProperty));
				sb.Children.Add(da);
				 */

			}

			sb.Begin(this, true);
		}

		public void StopAnimateImage() {
			this.IsHitTestVisible = true;

			if (sb != null) {
				sb.Stop(this);
			}
		}
	}

	public class CustomButtonEventArgs : EventArgs {
		public string ActionType { get; internal set; }
		public string Main { get; internal set; }
		public string Detail { get; internal set; }

		public CustomButtonEventArgs(string actiontype, string type, string detail) {
			this.ActionType = actiontype;
			this.Main = type;
			this.Detail = detail;
		}
	}
}
