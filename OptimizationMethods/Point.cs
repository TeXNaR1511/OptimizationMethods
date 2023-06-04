using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Animation.Animators;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;

namespace OptimizationMethods
{
    public class Point
    {
        public double X;
        public double Y;

        //Constructors
        public Point()
        {
            X = 0.0;
            Y = 0.0;
        }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static double Norm(Point a)
        {
            return Math.Sqrt(a.X * a.X + a.Y * a.Y);
        }

        public static double Distance(Point a, Point b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        public override string ToString()
        {
            return "(" + Math.Round(X, 3).ToString().Replace(',', '.') + "," + Math.Round(Y, 3).ToString().Replace(',', '.') + ")";
        }

        public static bool operator ==(Point a, Point b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Point a, Point b)
        {
            return a.X != b.X || a.Y != b.Y;
        }

        public static Point operator +(Point a, Point b)
        {
            return new Point(a.X + b.X, a.Y + b.Y);
        }

        public static Point operator -(Point a, Point b)
        {
            return new Point(a.X - b.X, a.Y - b.Y);
        }

        public static Point operator -(Point a)
        {
            return new Point(-a.X, -a.Y);
        }

        public static Point operator *(Point a, double b)
        {
            return new Point(a.X * b, a.Y * b);
        }

        public static Point operator *(double a, Point b)
        {
            return new Point(a * b.X, a * b.Y);
        }

        public static Point operator /(Point a, double b)
        {
            return new Point(a.X / b, a.Y / b);
        }

        public static Point FromString(string s)
        {
            s = s.Trim();
            Regex regex = new Regex(@"\(-?\d+(\.\d+)?\,[ ]*-?\d+(\.\d+)?\)");
            MatchCollection matches = regex.Matches(s);
            if (s == null || matches.Count != 1)
            {
                return new Point(0, 0);
            }
            else
            {
                s = s.Remove(0, 1);
                s = s.Remove(s.Length - 1);
                //Console.WriteLine(s);
                string[] strings = s.Split(new char[] { ',' });
                strings[0] = strings[0].Replace('.', ',');
                strings[1] = strings[1].Replace('.', ',');
                return new Point(Convert.ToDouble(strings[0]), Convert.ToDouble(strings[1]));
            }
        }

        private static object CreateByTypeName(string typeName)
        {
            // scan for the class type
            var type = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                        from t in assembly.GetTypes()
                        where t.Name == typeName  // you could use the t.FullName as well
                        select t).FirstOrDefault();

            if (type == null)
                throw new InvalidOperationException("Type not found");

            return Activator.CreateInstance(type);
        }

        //public static Point CreateRandomPoint<TYPE>(Point a, Point b, Type type, IContinuousDistribution d) where TYPE : new()
        //{
        //    var distr1 = System.Activator.CreateInstance(type, args) as chto_to;
        //    var distr2 = d.Sample();
        //    var distr3 = new TYPE();
        //    var uniform1 = new ContinuousUniform(Math.Min(a.X, b.X), Math.Max(a.X, b.X));
        //    var uniform2 = new ContinuousUniform(Math.Min(a.Y, b.Y), Math.Max(a.Y, b.Y));
        //    return new Point(uniform1.Sample(), uniform2.Sample());
        //}

        /// <summary>
        /// Create random Point from Uniform Distribution inside ractangle on points a and b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Point CreateRandomPoint(Point a, Point b)
        {
            var uniform1 = new ContinuousUniform(Math.Min(a.X, b.X), Math.Max(a.X, b.X));
            var uniform2 = new ContinuousUniform(Math.Min(a.Y, b.Y), Math.Max(a.Y, b.Y));
            return new Point(uniform1.Sample(), uniform2.Sample());
        }

        /// <summary>
        /// Checks if Point a is inside rectangle on points b and c
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool InsideBounds(Point a, Point b, Point c)
        {
            return Math.Min(b.X, c.X) <= a.X && a.X <= Math.Max(b.X, c.X) && Math.Min(b.Y, c.Y) <= a.Y && a.Y <= Math.Max(b.Y, c.Y);
        }

        /// <summary>
        /// Get projection of Point a on rectangle on points b and c
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Point GetProjection(Point p, Point a, Point b)
        {
            double minX = Math.Min(a.X, b.X);
            double maxX = Math.Max(a.X, b.X);
            double minY = Math.Min(a.Y, b.Y);
            double maxY = Math.Max(a.Y, b.Y);

            double newX;
            double newY;

            if (p.X < minX) newX = minX;
            else if (p.X > maxX) newX = maxX;
            else newX = p.X;
            
            if (p.Y < minY) newY = minY;
            else if (p.Y > maxY) newY = maxY;
            else newY = p.Y;

            return new Point(newX, newY);
        }
        /// <summary>
        /// Get rebound of Point p on rectangle on points a and b
        /// </summary>
        /// <param name="p"></param>
        /// <param name="v"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Point GetRebound(Point p, Point a, Point b)
        {
            double minX = Math.Min(a.X, b.X);
            double maxX = Math.Max(a.X, b.X);
            double minY = Math.Min(a.Y, b.Y);
            double maxY = Math.Max(a.Y, b.Y);

            double newX;
            double newY;

            if (p.X < minX) newX = 2 * minX - p.X;
            else if (p.X > maxX) newX = 2 * maxX - p.X;
            else newX = p.X;

            if (p.Y < minY) newY = 2 * minY - p.Y;
            else if (p.Y > maxY) newY = 2 * maxY - p.Y;
            else newY = p.Y;

            Point result = new Point(newX, newY);

            if (Point.InsideBounds(result, a, b)) return result;
            else return Point.GetRebound(result, a, b);
        }
    }
}
