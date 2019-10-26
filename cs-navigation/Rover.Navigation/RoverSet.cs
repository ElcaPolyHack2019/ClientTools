using System;
using System.Collections.Generic;
using System.Threading;

namespace Rover.Navigation
{
	public class RoverSet
	{
		private List<DuckieBot> _duckieBots = new List<DuckieBot>();
		private Timer _timer;

		
		private void Upadate(object state)
		{
			foreach (var rover in _duckieBots)
			{
				rover.RefreshStatus().GetAwaiter().GetResult();
			}
		}
	

		public void AddRover(DuckieBot bot)
		{
			_duckieBots.Add(bot);
		}

		public void StartUpdate()
		{
			_timer = new Timer(Upadate, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
		}

		public void StopUpdate()
		{
			_timer.Dispose();
		}

	}
}
