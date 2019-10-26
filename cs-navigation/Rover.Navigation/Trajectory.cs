using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Rover.Navigation
{
    public class Trajectory : IDisposable, IEnumerable<Point>
    {

        private IList<Point> _points = new List<Point>();
        public CancellationTokenSource CancellationToken { get; private set; }

        public void AddPoint(Point point)
        {
            _points.Add(point);
        }

        /// <summary>
        /// Add a route at the beginning before the other points to target.
        /// </summary>
        /// <param name="avoidCrashPoints"></param>
        public void AddAvoidCollisionPoints(params Point[] avoidCrashPoints)
        {
            foreach (var c in avoidCrashPoints.Reverse())
            {
                _points.Insert(0, c);
            }
        }
        /// <summary>
        /// If point reached, it should be removed.
        /// </summary>
        public void RemovePoint()
        {
            if (_points.Count > 0)
            {
                _points.RemoveAt(0);
            }
        }

        public Point GetCurrentTarget()
        {
            if (_points.Count > 0)
            {
                return _points[0];
            }
            return null;
        }


        public void Start()
        {
            DisposeToken();
            CancellationToken = new CancellationTokenSource();
        }

        public void Complete()
        {
            DisposeToken();
        }

        public void Interrupt()
        {
            CancellationToken?.Cancel(true);
        }

        public void Dispose()
        {
            DisposeToken();
        }

        private void DisposeToken()
        {
            CancellationToken?.Dispose();
        }

        public IEnumerator<Point> GetEnumerator()
        {
            return _points.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _points).GetEnumerator();
        }
    }
}
