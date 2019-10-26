using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Rover.Navigation.Test
{
	[TestFixture()]
	public class NavigationTest
	{

		[Test]
		public void TestNavPoints()
		{
			var map = new Map(new Point(-2, -2), new Point(2, 2), 0.2f);
			map.StartNode = map.Nodes.Find(x => x.Name == $"Node_1_1");
			map.EndNode = map.Nodes.Find(x => x.Name == $"Node_3_3");

			var searchEngine = new SearchEngine(map);
			var navPoints = searchEngine.GetShortestPathDijikstra();

			navPoints.Count.Should().Be(5);
		}

		[Test]
		public void TestNavPoints_AfterAppliedSuspectUnreachable()
		{
			var map = new Map(new Point(-2, -2), new Point(2, 2), 0.2f);
			map.StartNode = map.Nodes.Find(x => x.Name == $"Node_1_1");
			map.EndNode = map.Nodes.Find(x => x.Name == $"Node_3_3");
			map.ApplySuspectUnreachable(new List<Node>()
			{
				map.Nodes.First(x => x.Name == "Node_2_1"),
				map.Nodes.First(x => x.Name == "Node_1_3"),
				map.Nodes.First(x => x.Name == "Node_3_2")
			});

			var searchEngine = new SearchEngine(map);
			var navPoints = searchEngine.GetShortestPathDijikstra();

			navPoints.Count.Should().Be(5);
			AssertPath(navPoints, "Node_1_1", "Node_1_2", "Node_2_2", "Node_2_3", "Node_3_3");
		}

		[Test]
		public void TestNavPoints_AfterAppliedSuspectUnreachableNearby()
		{
			var map = new Map(new Point(-2, 2), new Point(2, -2), 0.2f);
			map.StartNode = map.Nodes.Find(x => x.Name == $"Node_1_1");
			map.EndNode = map.Nodes.Find(x => x.Name == $"Node_3_3");
			map.ApplySuspectUnreachable(new List<Point>()
			{
				new Point(-1.5f, 1.6f)
			}, 0.2f);

			var searchEngine = new SearchEngine(map);
			var navPoints = searchEngine.GetShortestPathDijikstra();

			navPoints.Count.Should().Be(5);
			AssertPath(navPoints, "Node_1_1", "Node_1_2", "Node_1_3", "Node_2_3", "Node_3_3");
		}

		public void AssertPath(List<Node> path, params string[] names)
		{
			path.Count.Should().Be(names.Length);
			for (int i = 0; i < names.Length; i++)
			{
				path[i].Name.Should().Be(names[i]);
			}
		}

	}
}
