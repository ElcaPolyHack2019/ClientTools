using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Rover.Navigation;

namespace Rover.Console
{
	class Program
	{
		static void Main(string[] args)
		{
			var bot = new DuckieBot("http://192.168.1.10:5000", "Elcaduck");
			var map = new Map(new Point(-2f, -1.2f), new Point(2f, 1.2f), 0.2f);
			bot.MoveOnMap(map, new Point(-1.9f, -1.1f), new CancellationToken()).GetAwaiter().GetResult();

		}
	}
}
