using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Rover.Navigation
{

	public class LidarData
	{

		[JsonProperty("time_increment")]
		public float TimeIncrement { get; set; }

		[JsonProperty("angle_min")]
		public float AngleMin { get; set; }

		[JsonProperty("angle_max")]
		public float AngleMax { get; set; }

		[JsonProperty("range_min")]
		public float RangeMin { get; set; }

		[JsonProperty("range_max")]
		public float RangeMax { get; set; }

		[JsonProperty("angle_increment")]
		public float AngleIncrement { get; set; }

		[JsonProperty("scan_time")]
		public float ScanTime { get; set; }

		[JsonProperty("ranges")]
		public float?[] Ranges { get; set; }

		[JsonProperty("intensities")]
		public float?[] Intensities { get; set; }


		/// <summary>
		/// The 0 point of the lidar always looks in the direction of the rover.
		/// based on the rover orientation, adapt the ranges and intensities to start at -Pi
		/// seen from the rover nose and ends with +Pi from the rover nose.
		///
		/// Note: Orientation 0 means that the rover looks in positive x direction.
		/// Max Orientation < 2 Pi
		/// </summary>
		public List<Point> TransformDetectedPointsToStandardSystem(float orientation, Point currentLocation)
		{
			var resultsInStandardSystem = new List<Point>();
			float angle = (float)Math.PI + orientation;
			for (int i = 0; i < Ranges.Length / 2; i++)
			{
				// start at -Pi move towards 0. To transfer it to positive angles in 0..2Pi, add 2Pi
				// Therefore the first value is at PI, the last at 2 PI

				angle += AngleIncrement;
				if (Ranges[i] != null)
				{
					resultsInStandardSystem.Add(currentLocation.CalculateReflectionPointInXandYPlane(angle, 
						Ranges[i].Value ));
				}
			}

			angle = orientation; // for the second half, start at orientation.
			for (int i = Ranges.Length / 2; i < Ranges.Length; i++)
			{
				// start at 0 move towards PI.
				if (Ranges[i] != null)
				{
					resultsInStandardSystem.Add(currentLocation.CalculateReflectionPointInXandYPlane(angle,
						Ranges[i].Value));
				}
				angle += AngleIncrement;
			}

			return resultsInStandardSystem;
			// rotate -orientation in radians, i.e. -orientation + 2Pi

			//var results = new List<Point>();
			//var rotationAngle = (-1 * orientation) + (2 * (float) Math.PI);
			//foreach (var point in resultsInLidarSystem)
			//{
			//	results.Add(new Point(
			//		(float)(point.X * Math.Cos(rotationAngle) - point.Y* Math.Sin(rotationAngle)),
			//		(float)(point.X * Math.Sin(rotationAngle) - point.Y * Math.Cos(rotationAngle))));
			//}

			//return results;
		}



	}
}
