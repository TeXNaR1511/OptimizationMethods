using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationMethods
{
    public class Methods
    {

        public Methods()
        {

        }

        /// <summary>
        /// Iteration of classic ROI optimization method.
        /// </summary>
        /// <param name="points">Points of current iteration.</param>
        /// <param name="previous">Points of previous iteration.</param>
        /// <param name="velocity">Previous velocities of point</param>
        /// <param name="function"></param>
        /// <param name="numberOfPoints"></param>
        /// <param name="Phi"></param>
        /// <param name="R"></param>
        public static List<List<Point>> ClassicROIIter(List<Point> points, List<Point> previous, List<Point> velocity, Func<double, double, double> function, int numberOfPoints, List<double> Phi, List<double> R)
        {
            List<Point> new_points = new List<Point>();
            for (int i = 0; i < numberOfPoints; i++)
            {
                new_points.Add(function(points[i].X, points[i].Y) <= function(previous[i].X, previous[i].Y) ? points[i] : previous[i]);
            }

            Point best = new_points.Select(x => (function(x.X, x.Y), x)).Min().Item2;

            List<Point> new_velocity = new List<Point>();
            for (int i = 0; i < numberOfPoints; i++)
            {
                new_velocity.Add(velocity[i] + (new_points[i] - points[i]) * Phi[0] * R[0] + (best - points[i]) * Phi[1] * R[1]);
            }

            for (int i = 0; i < numberOfPoints; i++)
            {
                points[i] += new_velocity[i];
            }

            return new List<List<Point>> { points, new_velocity };
        }
    }
}
