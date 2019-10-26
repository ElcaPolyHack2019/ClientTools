using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Rover.Navigation.Test
{
	[TestFixture()]
	public class LidarTest
	{
		private LidarData[] _data;

		[SetUp]
		public void SetUp()
		{
			var dataPath = Path.GetDirectoryName(new Uri(typeof(LidarData).Assembly.CodeBase).AbsolutePath);

			using (var testDataStream = new FileStream(Path.Combine(dataPath, "LidarTestData.txt"), FileMode.Open))
			{
				var reader = new StreamReader(testDataStream);
				var data = reader.ReadToEnd();

				_data = JsonConvert.DeserializeObject<LidarData[]>(data);
				_data.Should().NotBeNull();
			}
		}

		[Test]
		public void TransformLidar_EverythingNull_NoCollisionPoints()
		{
			for (int i = 0; i < _data[0].Ranges.Length; i++)
			{
				_data[0].Ranges[i] = null;
			}

			var points = _data[0].TransformDetectedPointsToStandardSystem(0.5f);
			points.Should().BeEmpty();
		}

		[Test]
		public void TransformLidar_SpecificPointIsOk()
		{
			for (int i = 0; i < _data[0].Ranges.Length; i++)
			{
				_data[0].Ranges[i] = null;
			}

			_data[0].Ranges[_data[0].Ranges.Length / 2] = 1;

			var points = _data[0].TransformDetectedPointsToStandardSystem(0f);
			points[0].Should().Be(new Point(1, 0));
		}

		[Test]
		public void TransformLidar_SpecificPointRotatedIsOk()
		{
			for (int i = 0; i < _data[0].Ranges.Length; i++)
			{
				_data[0].Ranges[i] = null;
			}

			_data[0].Ranges[_data[0].Ranges.Length / 2] = 1;

			var points = _data[0].TransformDetectedPointsToStandardSystem((float)(Math.PI / 2));
			points[0].Y.Should().Be(-1);
			points[0].X.Should().BeApproximately(0, 0.01f);
		}

	}
}
