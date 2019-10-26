using Newtonsoft.Json;

namespace Rover.Navigation
{
    public class DuckieStatus
    {

        public DuckieStatus()
        {
        }

        public DuckieStatus(string id, float x, float y, float orientationRad)
        {
            Id = id;
            X = x;
            Y = y;
            OrientationRad = orientationRad;
        }

        [JsonProperty("rover_id")]
        public string Id { get; private set; }

        [JsonProperty("gps_x")]
        public float X { get; private set; }

        [JsonProperty("gps_y")]
        public float Y { get; private set; }

        [JsonProperty("gps_orientation_rad")]
        public float OrientationRad { get; private set; }

        public float CalculateDistanceInXandYPlane(DuckieStatus other)
        {
            return new Point(X, Y).CalculateDistanceInXandYPlane(new Point(other.X, other.Y));
        }

	}

}
