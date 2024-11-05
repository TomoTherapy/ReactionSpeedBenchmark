using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ReactionSpeedBenchmark
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private static int MIN = 2000;
		private static int MAX = 4000;
		private Stopwatch watch;
		private AutoResetEvent autoResetEvent;
		private List<long> times;
		private bool isResponsing;
		private Task task;
		private bool isCheated;
		private bool isRunning;

		public MainWindow()
		{
			InitializeComponent();
			watch = new Stopwatch();
			times = new List<long>();
		}

		private void Response_Rectangle_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			Reaction();
		}

		private void Response_Rectangle_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key != Key.Space) return;
			Reaction();
		}

		private void Reaction()
		{
			if (!isRunning) { return; }

			watch.Stop();

			if (!isResponsing)
			{
				isCheated = true;
				isResponsing = false;
				isRunning = false;
				Start_Button.IsEnabled = true;
				Response_Rectangle.Fill = Brushes.Gray;
				MessageBox.Show("You, Fucking cheater!", "Die!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return;
			}

			long time = watch.ElapsedMilliseconds;
			watch.Reset();
			string timeStr = time.ToString() + "ms";
			ElapsedMilliseconds_TextBlock.Text = timeStr;
			TestResults_StackPanel.Children.Add(new TextBlock() { Text = timeStr });
			times.Add(time);

			Response_Rectangle.Fill = Brushes.LimeGreen;
			autoResetEvent.Set();
			isResponsing = false;
		}

		private void Start_Button_Click(object sender, RoutedEventArgs e)
		{
			TestResults_StackPanel.Children.Clear();
			Response_Rectangle.Focus();
			watch.Reset();
			Response_Rectangle.Fill = Brushes.LimeGreen;
			times.Clear();
			autoResetEvent?.Dispose();
			autoResetEvent = new AutoResetEvent(false);
			isResponsing = false;
			isCheated = false;
			isRunning = true;
			Start_Button.IsEnabled = false;

			task = Task.Run(() =>
			{
				Random random = new Random();

				for (int i = 0; i < 5; ++i)
				{
					int tick = random.Next(MIN, MAX);
					Thread.Sleep(tick);

					if (isCheated)
					{
						return;
					}

					Dispatcher.Invoke(() =>
					{
						Response_Rectangle.Fill = Brushes.Red;
					});
					watch.Start();
					isResponsing = true;
					autoResetEvent.WaitOne();
				}

				double ave = times.Average();
				isRunning = false;
				MessageBox.Show("Average : " + ave.ToString("0.0") + "ms");
				Dispatcher.Invoke(() =>
				{
					Response_Rectangle.Fill = Brushes.Gray;
					Start_Button.IsEnabled = true;
				});
			});
		}

		private void Exit_Button_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
		{
			DragMove();
		}
	}
}
