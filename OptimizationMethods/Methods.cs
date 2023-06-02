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
        public static List<List<Point>> ClassicROIIter(List<Point> points, List<Point> velocity, List<Point> bestPoints, Point best, Func<double, double, double> function, int numberOfPoints, Point Phi, Point R)
        {
            List<Point> newVelocity = new List<Point>();
            for (int i = 0; i < numberOfPoints; i++)
            {
                newVelocity.Add(velocity[i] + (bestPoints[i] - points[i]) * Phi.X * R.X + (best - points[i]) * Phi.Y * R.Y);
            }
            for (int i = 0; i < numberOfPoints; i++)
            {
                points[i] += newVelocity[i];
            }
            List<Point> newBestPoints = new List<Point>();
            for (int i = 0; i < numberOfPoints; i++)
            {
                newBestPoints.Add(function(points[i].X, points[i].Y) <= function(bestPoints[i].X, bestPoints[i].Y) ? points[i] : bestPoints[i]);
            }
            return new List<List<Point>> { points, newVelocity, newBestPoints };
        }

        public static List<List<Point>> InertialROIIter(List<Point> points, List<Point> velocity, List<Point> bestPoints, Point best, Func<double, double, double> function, int numberOfPoints, Point Phi, Point R, double inertia)
        {
            List<Point> newVelocity = new List<Point>();
            for (int i = 0; i < numberOfPoints; i++)
            {
                newVelocity.Add(inertia * velocity[i] + (bestPoints[i] - points[i]) * Phi.X * R.X + (best - points[i]) * Phi.Y * R.Y);
            }
            for (int i = 0; i < numberOfPoints; i++)
            {
                points[i] += newVelocity[i];
            }
            List<Point> newBestPoints = new List<Point>();
            for (int i = 0; i < numberOfPoints; i++)
            {
                newBestPoints.Add(function(points[i].X, points[i].Y) <= function(bestPoints[i].X, bestPoints[i].Y) ? points[i] : bestPoints[i]);
            }
            return new List<List<Point>> { points, newVelocity, newBestPoints };
        }

        public static List<List<Point>> CanonicalROIIter(List<Point> points, List<Point> velocity, List<Point> bestPoints, Point best, Func<double, double, double> function, int numberOfPoints, Point Phi, Point R, double kanon)
        {
            List<Point> newVelocity = new List<Point>();
            var phi0 = Phi.X + Phi.Y;
            var Chi = 2 * kanon / Math.Abs(2 - phi0 - Math.Sqrt(phi0 * phi0 - 4 * phi0));
            for (int i = 0; i < numberOfPoints; i++)
            {
                newVelocity.Add(Chi * (velocity[i] + (bestPoints[i] - points[i]) * Phi.X * R.X + (best - points[i]) * Phi.Y * R.Y));
            }
            for (int i = 0; i < numberOfPoints; i++)
            {
                points[i] += newVelocity[i];
            }
            List<Point> newBestPoints = new List<Point>();
            for (int i = 0; i < numberOfPoints; i++)
            {
                newBestPoints.Add(function(points[i].X, points[i].Y) <= function(bestPoints[i].X, bestPoints[i].Y) ? points[i] : bestPoints[i]);
            }
            return new List<List<Point>> { points, newVelocity, newBestPoints };
        }
    }
}
