using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;

namespace Rover.Navigation
{

	public delegate void NavPointReachedHandler(object sender, EventArgs args);
	public delegate void RouteCalculatedHandler(object sender, RouteArgs args);

	public delegate void ObstaclesDetectedHanlder(object sender, ObstaclesDetectedArgs args);

	public class RouteArgs : EventArgs
	{
		public RouteArgs(List<Node> nodes, Point nextPoint)
		{
			Route = nodes;
			NextPoint = nextPoint;
		}

		public List<Node> Route { get; set; }
		public Point NextPoint { get; set; }
	}

	public class ObstaclesDetectedArgs : EventArgs
	{
		public ObstaclesDetectedArgs(List<Point> obstacles, Point currentPosition, float currentOrientation)
		{
			Obstacles = obstacles;
			CurrentPosition = currentPosition;
			CurrentOrientation = currentOrientation;
		}

		public List<Point> Obstacles { get; set; }
		public Point CurrentPosition { get; set; }

		public float CurrentOrientation { get; set; }
	
	}

	public class DuckieBot
    {

        private static ILog _log = LogManager.GetLogger(typeof(DuckieBot));

        private string _baseUrl;

        public string DuckieId { get; }

        public event NavPointReachedHandler NavPointReached;
        public event RouteCalculatedHandler RouteCalculated;
        public event ObstaclesDetectedHanlder ObstaclesDetected;


        // CurrentX, CurrentY will be updated by the timer which queries current status.
		// It will call UpdateStatus.

		public float CurrentX { get; set; }
        public float CurrentY { get; set; }

		/// <summary>
		/// the orientation of the nose of the duckie (in radians)
		/// </summary>
		public float CurrentOrientation { get; set; }

		public DuckieBot(string serverUrl, string duckieId)
        {
            _baseUrl = $"{serverUrl}/api/{duckieId}";
            DuckieId = duckieId;
        }

        public async Task<DuckieStatus> GetStatus()
        {
	        await Task.Delay(TimeSpan.FromMilliseconds(850));

	        var client = new HttpClient();
            var result = await client.GetAsync($"{_baseUrl}");
            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new ApplicationException("GetStatus failed: " + result.StatusCode);
            }

			var content = await result.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<DuckieStatus>(content);
		}

		/// <summary>
		/// forward with the current angle and the current duration from the current position with the given duration.
		/// </summary>
		public async Task Forward(TimeSpan duration)
		{
			_log.InfoFormat($"Forward {DuckieId} from {CurrentX}/{CurrentY} in direction {CurrentOrientation} for {duration}");
			var client = new HttpClient();
			var result = await client.GetAsync($"{_baseUrl}/forward?duration={duration.TotalSeconds}&pow=0.7");
			if (result.StatusCode != System.Net.HttpStatusCode.OK)
			{
				throw new ApplicationException("Forward failed: " + result.StatusCode);
			}

			var content = await result.Content.ReadAsStringAsync();
			await Task.Delay(duration);
		}

		/// <summary>
		/// backward with the current angle and the current duration from the current position with the given duration.
		/// </summary>
		public async Task Backward(TimeSpan duration)
		{
			_log.InfoFormat($"Backward {DuckieId} from {CurrentX}/{CurrentY} in direction {CurrentOrientation} for {duration}");
			var client = new HttpClient();
			var result = await client.GetAsync($"{_baseUrl}/backward?duration={duration.TotalSeconds}&pow=0.7");
			if (result.StatusCode != System.Net.HttpStatusCode.OK)
			{
				throw new ApplicationException("Backward failed: " + result.StatusCode);
			}

			var content = await result.Content.ReadAsStringAsync();
			await Task.Delay(duration);
		}

		/// <summary>
		/// rotate left with the given duration.
		/// </summary>
		public async Task RotateLeft(TimeSpan duration)
		{
			_log.InfoFormat($"Rotate left {DuckieId} from {CurrentX}/{CurrentY} direction {CurrentOrientation} for {duration}");
			var client = new HttpClient();
			var result = await client.GetAsync($"{_baseUrl}/rotate?duration={duration.TotalSeconds}&direction=left&pow=0.7");
			if (result.StatusCode != System.Net.HttpStatusCode.OK)
			{
				throw new ApplicationException("Rotate Left failed: " + result.StatusCode);
			}

			var content = await result.Content.ReadAsStringAsync();
			await Task.Delay(duration);
		}

		public async Task RotateRight(TimeSpan duration)
		{
			_log.InfoFormat($"Rotate right {DuckieId} from {CurrentX}/{CurrentY} direction {CurrentOrientation} for {duration}");
			var client = new HttpClient();
			var result = await client.GetAsync($"{_baseUrl}/rotate?duration={duration.TotalSeconds}&direction=right&pow=0.7");
			if (result.StatusCode != System.Net.HttpStatusCode.OK)
			{
				throw new ApplicationException("Rotate Right failed: " + result.StatusCode);
			}

			var content = await result.Content.ReadAsStringAsync();
			await Task.Delay(duration);
		}

		public async Task<LidarData> Lidar()
		{
			_log.InfoFormat($"GetLidar {DuckieId}");
			var client = new HttpClient();
			var result = await client.GetAsync($"{_baseUrl}/lidar");
			if (result.StatusCode != System.Net.HttpStatusCode.OK)
			{
				throw new ApplicationException("Lidar failed: " + result.StatusCode);
			}

			var content = await result.Content.ReadAsStringAsync();

			return JsonConvert.DeserializeObject<LidarData>(content);
		}

		public async Task GoTo(float x, float y, CancellationToken cancellationToken)
        {
            _log.InfoFormat($"GoTo {DuckieId} from {CurrentX}/{CurrentY} with orientation {CurrentOrientation} to {x}/{y}");
            
            if (Math.Abs(CurrentX - x) > 0.2)
            {
	            await MoveParallelX(x);
            }

			cancellationToken.ThrowIfCancellationRequested();

            if (Math.Abs(CurrentY - y) > 0.2)
            {
	            await MoveParallelY(y);
            }
        }

		private async Task MoveParallelX(float x)
		{
			while (true)
			{
				// rotate first to move parallel to x direction
				// radians = 0 means x direction
				while (Math.Abs(CurrentOrientation) > 0.3)
				{
					if (CurrentOrientation < 0)
					{
						await RotateLeft(TimeSpan.FromMilliseconds(250));
					}
					else
					{
						await RotateRight(TimeSpan.FromMilliseconds(250));
					}

					await RefreshStatus();
				}

				while (Math.Abs(CurrentX - x) > 0.2)
				{
					if (Math.Abs(CurrentOrientation) > 0.3)
					{
						// need to check orientation again.
						break;
					}
					if (CurrentX < x)
					{
						await Forward(TimeSpan.FromMilliseconds(400));
					}
					else
					{
						await Backward(TimeSpan.FromMilliseconds(400));
					}

					await RefreshStatus();
				}

				if (Math.Abs(CurrentX - x) <= 0.2)
				{
					return;
				}
			}
		}

		private async Task MoveParallelY(float y)
		{
			while (true)
			{
				// rotate first to move parallel to y direction
				// radians = Pi/2 means y direction
				while (Math.Abs(CurrentOrientation - (Math.PI / 2)) > 0.3)
				{
					if (CurrentOrientation - (Math.PI / 2) < 0)
					{
						await RotateLeft(TimeSpan.FromMilliseconds(250));
					}
					else
					{
						await RotateRight(TimeSpan.FromMilliseconds(250));
					}

					await RefreshStatus();
				}

				while (Math.Abs(CurrentY - y) > 0.2)
				{
					if (Math.Abs(CurrentOrientation - (Math.PI / 2)) > 0.3)
					{
						break; // need to check orientation again.
					}
					if (CurrentY < y)
					{
						await Forward(TimeSpan.FromMilliseconds(400));
					}
					else
					{
						await Backward(TimeSpan.FromMilliseconds(400));
					}

					await RefreshStatus();
				}

				if (Math.Abs(CurrentY - y) <= 0.2)
				{
					return;
				}
			}
		}

		public async Task RefreshStatus()
		{
			var status = await GetStatus();
			UpdateStatus(status);
		}
        private void UpdateStatus(DuckieStatus s)
        {
            CurrentX = s.X;
            CurrentY = s.Y;
            CurrentOrientation = s.OrientationRad;
        }

        public async Task Stop()
        {
            var client = new HttpClient();
            var result = await client.GetAsync($"{_baseUrl}/stop");
            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new ApplicationException("stop failed: " + result.StatusCode);
            }
        }

        public async Task MoveOnMap(Map map, Point destination, CancellationToken cancellationToken)
        {
	        map.EndNode = map.FindKnownLocationsInRange(destination, 0.3f).First();

			while (true)
	        {
		        await RefreshStatus();

				var lidarData = await Lidar();
				if (lidarData != null)
				{
					var suspectUnreachable = lidarData.TransformDetectedPointsToStandardSystem(CurrentOrientation, new Point(CurrentX, CurrentY));
					ObstaclesDetected?.Invoke(this, new ObstaclesDetectedArgs(suspectUnreachable, new Point(CurrentX, CurrentY), CurrentOrientation));
					map.ApplySuspectUnreachable(suspectUnreachable, 0.2f);
				}

				//continue;

				if (destination.CalculateDistanceInXandYPlane(new Point(CurrentX, CurrentY)) < 0.2f)
		        {
			        break;
		        }

		        map.StartNode = map.FindKnownLocationsInRange(new Point(CurrentX, CurrentY), 0.3f).First();

		        var searchEngine = new SearchEngine(map);
		        var navPoints = searchEngine.GetShortestPathDijikstra();

		        var gotoPoint = FindNext(navPoints);

				RouteCalculated?.Invoke(this, new RouteArgs(navPoints, gotoPoint));

		        if (gotoPoint != null)
		        {
			        await MoveToNextNavPoint(gotoPoint, cancellationToken);

			        NavPointReached?.Invoke(this, EventArgs.Empty);
		        }
	        }
        }

		private Point FindNext(List<Node> navPoints)
		{
			foreach (var nextCandidate in navPoints.Skip(1)) // skip start node
			{
				if (Math.Abs(nextCandidate.Point.X - CurrentX) > 0.2f || Math.Abs(nextCandidate.Point.Y - CurrentY) > 0.2f)
				{
					return nextCandidate.Point;
				}
			}

			return null;
		}

		private async Task MoveToNextNavPoint(Point point, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await GoTo(point.X, point.Y, cancellationToken);
        }
    }
}
