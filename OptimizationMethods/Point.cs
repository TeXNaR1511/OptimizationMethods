using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
            return "(" + X + ", " + Y + ")";
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

        public static Point operator *(Point a, double b)
        {
            return new Point(a.X * b, a.Y * b);
        }

        public static Point operator /(Point a, double b)
        {
            return new Point(a.X / b, a.Y / b);
        }

        public static Point FromString(string s)
        {
            s = s.Trim();
            Regex regex = new Regex(@"\(-?\d+(\.\d+)?,[ ]*-?\d+(\.\d+)?\)");
            MatchCollection matches = regex.Matches(s);
            if (s == null || matches.Count != 1)
            {
                return new Point(0, 0);
            }
            else
            {
                s = s.Remove(0, 1);
                s = s.Remove(s.Length - 1);
                string[] strings = s.Split(new char[] { ',' });
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

        public static Point CreateRandomPoint(Point a, Point b)
        {
            var uniform1 = new ContinuousUniform(Math.Min(a.X, b.X), Math.Max(a.X, b.X));
            var uniform2 = new ContinuousUniform(Math.Min(a.Y, b.Y), Math.Max(a.Y, b.Y));
            return new Point(uniform1.Sample(), uniform2.Sample());
        }
    }
}
