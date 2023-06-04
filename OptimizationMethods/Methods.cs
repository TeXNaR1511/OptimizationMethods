using MathNet.Numerics.Distributions;
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
        /// <param name="Phi"></param>
        /// <param name="R"></param>
        public static List<List<Point>> ClassicROIIter(List<Point> points, List<Point> velocity, List<Point> bestPoints, Point best, Func<double, double, double> function, Point Phi, Point R)
        {
            List<Point> newVelocity = new List<Point>();
            for (int i = 0; i < points.Count; i++)
            {
                newVelocity.Add(velocity[i] + (bestPoints[i] - points[i]) * Phi.X * R.X + (best - points[i]) * Phi.Y * R.Y);
            }
            for (int i = 0; i < points.Count; i++)
            {
                points[i] += newVelocity[i];
            }
            List<Point> newBestPoints = new List<Point>();
            for (int i = 0; i < points.Count; i++)
            {
                newBestPoints.Add(function(points[i].X, points[i].Y) <= function(bestPoints[i].X, bestPoints[i].Y) ? points[i] : bestPoints[i]);
            }
            return new List<List<Point>> { points, newVelocity, newBestPoints };
        }

        public static List<List<Point>> InertialROIIter(List<Point> points, List<Point> velocity, List<Point> bestPoints, Point best, Func<double, double, double> function, Point Phi, Point R, double inertia)
        {
            List<Point> newVelocity = new List<Point>();
            for (int i = 0; i < points.Count; i++)
            {
                newVelocity.Add(inertia * velocity[i] + (bestPoints[i] - points[i]) * Phi.X * R.X + (best - points[i]) * Phi.Y * R.Y);
            }
            for (int i = 0; i < points.Count; i++)
            {
                points[i] += newVelocity[i];
            }
            List<Point> newBestPoints = new List<Point>();
            for (int i = 0; i < points.Count; i++)
            {
                newBestPoints.Add(function(points[i].X, points[i].Y) <= function(bestPoints[i].X, bestPoints[i].Y) ? points[i] : bestPoints[i]);
            }
            return new List<List<Point>> { points, newVelocity, newBestPoints };
        }

        public static List<List<Point>> CanonicalROIIter(List<Point> points, List<Point> velocity, List<Point> bestPoints, Point best, Func<double, double, double> function, Point Phi, Point R, double kanon)
        {
            List<Point> newVelocity = new List<Point>();
            var phi0 = Phi.X + Phi.Y;
            var Chi = 2 * kanon / Math.Abs(2 - phi0 - Math.Sqrt(phi0 * phi0 - 4 * phi0));
            for (int i = 0; i < points.Count; i++)
            {
                newVelocity.Add(Chi * (velocity[i] + (bestPoints[i] - points[i]) * Phi.X * R.X + (best - points[i]) * Phi.Y * R.Y));
            }
            for (int i = 0; i < points.Count; i++)
            {
                points[i] += newVelocity[i];
            }
            List<Point> newBestPoints = new List<Point>();
            for (int i = 0; i < points.Count; i++)
            {
                newBestPoints.Add(function(points[i].X, points[i].Y) <= function(bestPoints[i].X, bestPoints[i].Y) ? points[i] : bestPoints[i]);
            }
            return new List<List<Point>> { points, newVelocity, newBestPoints };
        }

        public static List<List<Point>> findKNN(List<Point> points, int kNeighbours)
        {
            List<List<Point>> result = new List<List<Point>>();
            for (int i = 0; i < points.Count; i++)
            {
                List<Point> temp = points.ToList();
                temp.Sort((x, y) => Point.Distance(x, points[i]).CompareTo(Point.Distance(y, points[i])));
                List<Point> sub = temp.GetRange(1, kNeighbours);
                result.Add(sub);
            }
            return result;
        }

        public static List<List<Point>> KNNROIIter(List<Point> points, List<Point> velocity, List<Point> bestPoints, Point best, int kNeighbours, Func<double, double, double> function, List<double> Phi, List<double> R, double inertia)
        {
            List<Point> newVelocity = new List<Point>();
            var listWithNeighbors = findKNN(points, kNeighbours);
            for (int i = 0; i < points.Count; i++)
            {
                Point temp = new Point();
                temp += inertia * velocity[i] + R[0] * Phi[0] * (bestPoints[i] - points[i]) + R[1] * Phi[1] * (best - points[i]);
                for (int j = 2; j < listWithNeighbors[i].Count; j++)
                {
                    temp += R[j] * Phi[j] * (listWithNeighbors[i][j] - points[i]);
                }
                newVelocity.Add(temp);
            }
            for (int i = 0; i < points.Count; i++)
            {
                points[i] += newVelocity[i];
            }

            List<Point> newBestPoints = new List<Point>();
            for (int i = 0; i < points.Count; i++)
            {
                newBestPoints.Add(function(points[i].X, points[i].Y) <= function(bestPoints[i].X, bestPoints[i].Y) ? points[i] : bestPoints[i]);
            }

            return new List<List<Point>> { points, newVelocity, newBestPoints };
        }

        public static List<List<Point>> ExtinctionROIIter(List<Point> points, List<Point> velocity, List<Point> bestPoints, Point best, Func<double, double, double> function, Point Phi, Point R, double inertia)
        {
            List<Point> newVelocity = new List<Point>();
            for (int i = 0; i < points.Count; i++)
            {
                newVelocity.Add(inertia * velocity[i] + (bestPoints[i] - points[i]) * Phi.X * R.X + (best - points[i]) * Phi.Y * R.Y);
            }
            for (int i = 0; i < points.Count; i++)
            {
                points[i] += newVelocity[i];
            }

            //kill worse point
            //Point worsepoint = points.Select(x => (function(x.X, x.Y), x)).Max().Item2;

            List<Point> badPoints = new List<Point>();

            for (int i = 0; i < points.Count; i++)
            {
                if (function(points[i].X, points[i].Y) > function(bestPoints[i].X, bestPoints[i].Y))
                {
                    badPoints.Add(points[i]);
                }
            }
            //System.Diagnostics.Debug.WriteLine(badPoints.Count);
            
            if (badPoints.Count > 0)
            {
                Point worsePoint = badPoints.Select(x => (function(x.X, x.Y), x)).Max().Item2;

                int worsePointIndex = 0;

                for (int i = 0; i < points.Count; i++)
                {
                    if (points[i] == worsePoint)
                    {
                        worsePointIndex = i;
                        break;
                    }
                }

                points.RemoveAt(worsePointIndex);
                newVelocity.RemoveAt(worsePointIndex);
                bestPoints.RemoveAt(worsePointIndex);
            }

            //

            List<Point> newBestPoints = new List<Point>();
            for (int i = 0; i < points.Count; i++)
            {
                newBestPoints.Add(function(points[i].X, points[i].Y) <= function(bestPoints[i].X, bestPoints[i].Y) ? points[i] : bestPoints[i]);
            }
            return new List<List<Point>> { points, newVelocity, newBestPoints };
        }

        public static List<List<Point>> SimulatedAnnealingIter(List<Point> points, List<Point> bestPoints, Func<double, double, double> function, double temperature, double inertia)
        {
            List<Point> newPoints = new List<Point>();
            var uniform = new ContinuousUniform(0, 1);
            for (int i = 0; i < points.Count; i++)
            {
                newPoints.Add(points[i] + inertia * temperature * (new Point(uniform.Sample(), uniform.Sample()) - new Point(0.5, 0.5)));
            }

            for (int i = 0; i < points.Count; i++)
            {
                var Delta = function(newPoints[i].X, newPoints[i].Y) - function(points[i].X, points[i].Y);
                if (Delta < 0 || uniform.Sample() < Math.Exp(-Delta / temperature))
                {
                    points[i] = newPoints[i];
                }
            }

            List<Point> newBestPoints = new List<Point>();
            for (int i = 0; i < points.Count; i++)
            {
                newBestPoints.Add(function(points[i].X, points[i].Y) <= function(bestPoints[i].X, bestPoints[i].Y) ? points[i] : bestPoints[i]);
            }

            return new List<List<Point>> { points, newBestPoints };
            
        }
    }
}
