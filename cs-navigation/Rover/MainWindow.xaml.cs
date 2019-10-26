using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Rover.Navigation;
using Point = Rover.Navigation.Point;

namespace Rover
{

	internal class WorldTransform
	{

		private Point _corner1;
		private Point _corner2;
		private double _actualWidth;
		private double _actualHeight;

		public WorldTransform(Point corner1, Point corner2, double actualWidht, double actualHeight)
		{
			_corner1 = corner1;
			_corner2 = corner2;
			_actualWidth = actualWidht;
			_actualHeight = actualHeight;
		}

		public System.Windows.Point Transform(Navigation.Point point)
		{
			float minX = _corner1.X;
			float minY = _corner1.Y;
			float maxX = _corner2.X;
			float maxY = _corner2.Y;

			var transform = new TranslateTransform(0 - minX, 0 - minY);
			var scale = new ScaleTransform((_actualWidth - 10) / Math.Abs(maxX - minX), (_actualHeight - 50) / Math.Abs(maxY - minY));

			return scale.Transform(transform.Transform(
				new System.Windows.Point(point.X, -1 * point.Y)));
		}

	}

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		public void DrawMap(Map map)
		{
			var transform = new WorldTransform(_corner1, _corner2, Panel.ActualWidth, Panel.ActualHeight);
			var drawing = new DrawingGroup();

			var mapGeometryGroup = DrawMapGroup(map, transform);
			drawing.Children.Add(mapGeometryGroup);
			var expensiveEdges = DrawCostlyEdges(map, transform);
			drawing.Children.Add(expensiveEdges);

			var targetShape = DrawTargetShape(transform);
			drawing.Children.Add(targetShape);

			var visitedGroup = DrawVisitedNodes(transform);
			drawing.Children.Add(visitedGroup);

			var routeGroup = DrawPlannedRoute(transform);
			drawing.Children.Add(routeGroup);

			var obstaclesGroup = DrawObstacles(transform);
			drawing.Children.Add(obstaclesGroup);

			var geometryImage = new DrawingImage(drawing);
			geometryImage.Freeze();

			Image anImage = new Image();
			anImage.Source = geometryImage;
			anImage.Stretch = Stretch.None;
			anImage.HorizontalAlignment = HorizontalAlignment.Left;

			Border exampleBorder = new Border();
			exampleBorder.Child = anImage;
			exampleBorder.BorderBrush = Brushes.Gray;
			exampleBorder.BorderThickness = new Thickness(1);
			exampleBorder.HorizontalAlignment = HorizontalAlignment.Left;
			exampleBorder.VerticalAlignment = VerticalAlignment.Top;
			exampleBorder.Margin = new Thickness(10);

			this.Margin = new Thickness(20);
			this.Background = Brushes.White;
			this.Panel.Children.Clear();
			this.Panel.Children.Add(exampleBorder);
		}

		private GeometryDrawing DrawObstacles(WorldTransform transform)
		{
			var obstacleGroup = new GeometryGroup();
			foreach (var obstacle in _obstacles)
			{
				if ((obstacle.X > _corner2.X) ||
				    (obstacle.Y < _corner2.Y) ||
				    (obstacle.X < _corner1.X) ||
				    (obstacle.Y > _corner1.Y))
				{
					continue;
				}

				var obstaclePoint = transform.Transform(obstacle);
				var obstacleShape = new EllipseGeometry(obstaclePoint, 2, 2);
				obstacleGroup.Children.Add(obstacleShape);
			}

			if (_currentPosition != null)
			{
				var directionPoint = transform.Transform(_currentPosition.CalculateReflectionPointInXandYPlane(_currentOrientation, 0.15f));
				var directionPoint1 = transform.Transform(_currentPosition.CalculateReflectionPointInXandYPlane(_currentOrientation + (float)(Math.PI / 2), 0.15f));
				var directionPoint2 = transform.Transform(_currentPosition.CalculateReflectionPointInXandYPlane(_currentOrientation - (float)(Math.PI / 2), 0.15f));

				obstacleGroup.Children.Add(new LineGeometry(directionPoint1, directionPoint));
				obstacleGroup.Children.Add(new LineGeometry(directionPoint, directionPoint2));
				obstacleGroup.Children.Add(new LineGeometry(directionPoint2, directionPoint1));
			}

			return new GeometryDrawing(new SolidColorBrush(
				Colors.DeepPink), new Pen(Brushes.DeepPink, 1), obstacleGroup);
		}

		private GeometryDrawing DrawCostlyEdges(Map map, WorldTransform transform)
		{
			var mapGeometryGroup = new GeometryGroup();
			foreach (var point in map.Nodes)
			{
				var start = transform.Transform(point.Point);

				foreach (var line in point.Connections)
				{
					if (line.Cost < 7)
					{
						continue;
					}
					var end = transform.Transform(line.ConnectedNode.Point);

					mapGeometryGroup.Children.Add(new LineGeometry(start, end));
				}
			}

			return new GeometryDrawing(new SolidColorBrush(
				Colors.Red), new Pen(Brushes.Red, 6), mapGeometryGroup);
		}

		private GeometryDrawing DrawTargetShape(WorldTransform transform)
		{
			var targetGroup = new GeometryGroup();
			
			var targetPoint = transform.Transform(_target);
			var roverShape = new RectangleGeometry(
				new Rect(targetPoint, new Size(20, 20)));
			targetGroup.Children.Add(roverShape);

			return new GeometryDrawing(new SolidColorBrush(
				Colors.Green), new Pen(Brushes.Green, 4), targetGroup);
		}

		private GeometryDrawing DrawVisitedNodes(WorldTransform transform)
		{
			var visitedGroup = new GeometryGroup();
			foreach (var visited in _visitedNodes)
			{
				var visitedPoint = transform.Transform(visited);
				var roverShape = new RectangleGeometry(
					new Rect(visitedPoint, new Size(10, 10)));
				visitedGroup.Children.Add(roverShape);
			}

			return new GeometryDrawing(new SolidColorBrush(
				Colors.Coral), new Pen(Brushes.Coral, 4), visitedGroup);
		}

		private GeometryDrawing DrawPlannedRoute(WorldTransform transform)
		{
			var routeGroup = new GeometryGroup();
			for (int i = 1; i < _plannedRoute.Count; i++)
			{
				var startPoint = transform.Transform(
					_plannedRoute[i-1].Point);
				var endPoint = transform.Transform(_plannedRoute[i].Point);

				var routeLine = new LineGeometry(
					startPoint, endPoint);
				routeGroup.Children.Add(routeLine);
			}

			if (_plannedNext != null)
			{
				var nextPoint = transform.Transform(_plannedNext);
				routeGroup.Children.Add(new RectangleGeometry(
					new Rect(nextPoint, new Size(20, 20))));
			}

			return new GeometryDrawing(new SolidColorBrush(
				Colors.Aquamarine), new Pen(Brushes.Aquamarine, 7), routeGroup);
		}

		private static GeometryDrawing DrawMapGroup(Map map, WorldTransform transform)
		{
			var mapGeometryGroup = new GeometryGroup();
			foreach (var point in map.Nodes)
			{
				var start = transform.Transform(point.Point);
				mapGeometryGroup.Children.Add(new EllipseGeometry(start, 3, 3));

				foreach (var line in point.Connections)
				{
					var end = transform.Transform(line.ConnectedNode.Point);

					mapGeometryGroup.Children.Add(new LineGeometry(start, end));
				}
			}

			return new GeometryDrawing(new SolidColorBrush(
				Colors.Black), new Pen(Brushes.Black, 4), mapGeometryGroup); 
		}

		private Map _map;
		private Navigation.Point _corner1;
		private Navigation.Point _corner2;
		private List<Navigation.Point> _visitedNodes = new List<Point>();
		private Navigation.Point _target;
		private List<Navigation.Node> _plannedRoute = new List<Node>();
		private Navigation.Point _plannedNext;
		private List<Navigation.Point> _obstacles = new List<Point>();
		private Navigation.Point _currentPosition;
		private float _currentOrientation;

		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			var x1 = float.Parse(Corner1X.Text);
			var y1 = float.Parse(Corner1Y.Text);
			_corner1 = new Navigation.Point(x1, y1);
			var x2 = float.Parse(Corner2X.Text);
			var y2 = float.Parse(Corner2Y.Text);
			_corner2 = new Navigation.Point(x2, y2);
			_map = new Map(_corner1, _corner2, 0.4f);

			_visitedNodes.Clear();

			var destX = float.Parse(DestX.Text);
			var destY = float.Parse(DestY.Text);
			_target = new Navigation.Point(destX, destY);

			DrawMap(_map);

			//await Task.Delay(TimeSpan.FromSeconds(4));
			//_visitedNodes.Add(new Navigation.Point(0, 0));
			//_visitedNodes.Add(new Navigation.Point(0.2f, 0f));

			//DrawMap(_map);

			var bot = new DuckieBot(RoverAdress.Text, RoverId.Text);
			//bot.NavPointReached += Bot_NavPointReached;
			bot.RouteCalculated += Bot_RouteCalculated;
			bot.ObstaclesDetected += Bot_ObstaclesDetected;
			await bot.MoveOnMap(_map, _target, new CancellationToken());
		}

		private void Bot_ObstaclesDetected(object sender, ObstaclesDetectedArgs args)
		{
			_obstacles = args.Obstacles;
			_currentPosition = args.CurrentPosition;
			_currentOrientation = args.CurrentOrientation;
			var rover = (DuckieBot)sender;
			Application.Current.Dispatcher.Invoke(() => {
				DrawMap(_map);
			});
		}

		private void Bot_RouteCalculated(object sender, RouteArgs args)
		{
			var rover = (DuckieBot)sender;
			_visitedNodes.Add(new Navigation.Point(rover.CurrentX, rover.CurrentY));
			_plannedRoute = args.Route;
			_plannedNext = args.NextPoint;
			Application.Current.Dispatcher.Invoke(() => {
				DrawMap(_map);
			});
		}

		private void Bot_NavPointReached(object sender, EventArgs args)
		{
			var rover = (DuckieBot) sender;
			_visitedNodes.Add(new Navigation.Point(rover.CurrentX, rover.CurrentY));
			Application.Current.Dispatcher.Invoke(()  => {
				DrawMap(_map);
			});
		}
	}
}
