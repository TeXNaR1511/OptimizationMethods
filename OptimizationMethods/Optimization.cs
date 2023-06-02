﻿using Avalonia.Threading;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using OptimizationMethods.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using OxyPlot;
using OxyPlot.Avalonia;
using DynamicData;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Reflection;
using Microsoft.CodeAnalysis.Scripting;
using System.Text.RegularExpressions;
using Avalonia.Controls.Templates;

namespace OptimizationMethods
{
    public class Optimization : ViewModelBase
    {

        private DispatcherTimer distimer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 1) };

        private string functionAsString = "(x,y)=>x*x+y*y";

        public string FunctionAsString
        {
            get => functionAsString;
            set =>  this.RaiseAndSetIfChanged(ref functionAsString, value);
        }

        private Func<double, double, double> function;

        private double[,] heatMap = new double[,] { };

        public double[,] HeatMap
        {
            get => heatMap;
            set => this.RaiseAndSetIfChanged(ref heatMap, value);
        }

        private string firstBorder = "(-10, -10)";

        public string FirstBorder
        {
            get => firstBorder;
            set => this.RaiseAndSetIfChanged(ref firstBorder, value);
        }

        private string secondBorder = "(10, 10)";


        public string SecondBorder
        {
            get => secondBorder;
            set => this.RaiseAndSetIfChanged(ref secondBorder, value);
        }

        private int linspaceValue = 100;

        public int LinspaceValue
        {
            get => linspaceValue;
            set => this.RaiseAndSetIfChanged(ref linspaceValue, value);
        }

        private PlotModel myPlotModel;

        public PlotModel MyPlotModel
        {
            get => myPlotModel;
            set => this.RaiseAndSetIfChanged(ref myPlotModel, value);
        }

        private List<Point> CurrentPoints = new List<Point>();

        private List<Point> Velocity = new List<Point>();

        private List<Point> BestPoints = new List<Point>();

        private Point BestPoint = new Point();

        private double bestSolution = double.PositiveInfinity;

        public double BestSolution
        {
            get => bestSolution;
            set => this.RaiseAndSetIfChanged(ref bestSolution, value);
        }

        private int numberOfPoints = 10;

        public int NumberOfPoints
        {
            get => numberOfPoints;
            set => this.RaiseAndSetIfChanged(ref numberOfPoints, value);
        }

        private double r_p = 0.5;
        public double R_p
        {
            get => r_p;
            set => this.RaiseAndSetIfChanged(ref r_p, value);
        }

        private double r_g = 0.5;
        public double R_g
        {
            get => r_g;
            set => this.RaiseAndSetIfChanged(ref r_g, value);
        }

        private double inertia = 0.5;
        public double Inertia
        {
            get => inertia;
            set => this.RaiseAndSetIfChanged(ref inertia, value);
        }

        public Optimization()
        {
            initDistimerTick();
            distimer.Tick += (s, e) =>
            {
                distimerTick();
            };
        }

        public PlotModel createPlotModel(List<Point> points)
        {
            var model = new PlotModel()
            {
                PlotAreaBorderThickness = new OxyThickness(2),
                DefaultFontSize = 20,
            };
            Point a = Point.FromString(FirstBorder);
            Point b = Point.FromString(SecondBorder);
            model.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Position = AxisPosition.Bottom,
                MajorTickSize = 10,
                MinorTickSize = 7,
                AbsoluteMaximum = Math.Max(a.X, b.X),
                AbsoluteMinimum = Math.Min(a.X, b.X),
            });
            model.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Position = AxisPosition.Left,
                MajorTickSize = 10,
                MinorTickSize = 7,
                AbsoluteMaximum = Math.Max(a.Y, b.Y),
                AbsoluteMinimum = Math.Min(a.Y, b.Y),
            });
            model.Axes.Add(new OxyPlot.Axes.LinearColorAxis
            {
                //Palette = OxyPalettes.Rainbow(256),
                Palette = OxyPalettes.Magma(256),
                Position = AxisPosition.Top,
                MajorTickSize = 10,
                MinorTickSize = 7,
                TickStyle = TickStyle.Inside
            });
            
            
            var heatMapSeries = new OxyPlot.Series.HeatMapSeries
            {
                X0 = Math.Min(a.X, b.X),
                X1 = Math.Max(a.X, b.X),
                Y0 = Math.Min(a.Y, b.Y),
                Y1 = Math.Max(a.Y, b.Y),
                Interpolate = true,
                RenderMethod = HeatMapRenderMethod.Bitmap,
                Data = HeatMap,
            };
            model.Series.Add(heatMapSeries);

            var scatterSeries = new OxyPlot.Series.ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerFill = OxyColor.FromRgb(0, 157, 255),
            };
            //System.Diagnostics.Debug.WriteLine(points.Count);
            //i < Math.Min(NumberOfPoints, points.Count)
            for (int i = 0; i < points.Count; i++)
            {
                scatterSeries.Points.Add(new ScatterPoint(points[i].X, points[i].Y, 7, 0));
            }

            model.Series.Add(scatterSeries);
            return model;
        }

        public Func<double, double, double> EvaluateFunction(string s)
        {
            s = s.Trim();
            Regex regex = new Regex(@"\(x,[ ]*y\)[ ]*=>[ ]*");
            MatchCollection matches = regex.Matches(s);
            if (s == null || matches.Count != 1)
            {
                s = "(x,y)=>x*x+y*y";
            }
            return CSharpScript.EvaluateAsync<Func<double, double, double>>(s, ScriptOptions.Default.WithImports("System.Math")).Result;
        }

        public void setRandomPoints()
        {
            CurrentPoints = new List<Point>();
            BestPoints = new List<Point>();
            Velocity = new List<Point>();
            //System.Diagnostics.Debug.WriteLine(FirstBorder + SecondBorder);
            //System.Diagnostics.Debug.WriteLine(Point.FromString(FirstBorder));
            //System.Diagnostics.Debug.WriteLine(Point.FromString(SecondBorder));
            for (int i = 0; i < NumberOfPoints; i++)
            {
                Point cur = Point.CreateRandomPoint(Point.FromString(FirstBorder), Point.FromString(SecondBorder));
                //System.Diagnostics.Debug.WriteLine(a);
                Point vel = Point.CreateRandomPoint(-Point.FromString(SecondBorder) + Point.FromString(FirstBorder), Point.FromString(SecondBorder) - Point.FromString(FirstBorder));
                CurrentPoints.Add(cur);
                Velocity.Add(vel);
            }
            BestPoints = CurrentPoints;
            MyPlotModel = createPlotModel(CurrentPoints);
        }

        public void initDistimerTick()
        {
            //System.Diagnostics.Debug.WriteLine("init");
            function = EvaluateFunction(FunctionAsString);
            HeatMap = createHeatMap(function, Point.FromString(FirstBorder), Point.FromString(SecondBorder), LinspaceValue);

            MyPlotModel = createPlotModel(CurrentPoints);

        }

        public void distimerTick()
        {
            function = EvaluateFunction(FunctionAsString);
            HeatMap = createHeatMap(function, Point.FromString(FirstBorder), Point.FromString(SecondBorder), LinspaceValue);
            if (ClassicROI)
            {
                List<List<Point>> a = Methods.ClassicROIIter(CurrentPoints, Velocity, BestPoints, BestPoint, function, NumberOfPoints, Point.CreateRandomPoint(new Point(0, 0), new Point(1, 1)), new Point(R_p, R_g));
                CurrentPoints = a[0];
                Velocity = a[1];
                BestPoints = a[2];
                BestPoint = BestPoints.Select(x => (function(x.X, x.Y), x)).Min().Item2;
                BestSolution = Math.Round(function(BestPoint.X, BestPoint.Y), 7);
            }
            if (InertialROI)
            {
                List<List<Point>> a = Methods.InertialROIIter(CurrentPoints, Velocity, BestPoints, BestPoint, function, NumberOfPoints, Point.CreateRandomPoint(new Point(0, 0), new Point(1, 1)), new Point(R_p, R_g), Inertia);
                CurrentPoints = a[0];
                Velocity = a[1];
                BestPoints = a[2];
                BestPoint = BestPoints.Select(x => (function(x.X, x.Y), x)).Min().Item2;
                BestSolution = Math.Round(function(BestPoint.X, BestPoint.Y), 7);
            }

            MyPlotModel = createPlotModel(CurrentPoints);
        }

        private string startStopButtonName = "Start";

        public string StartStopButtonName
        {
            get => startStopButtonName;
            set => this.RaiseAndSetIfChanged(ref startStopButtonName, value);
        }

        public void distimerStartStop()
        {
            if (distimer.IsEnabled)
            {
                distimer.Stop();
                StartStopButtonName = "Start";
            }
            else
            {
                distimer.Start();
                StartStopButtonName = "Stop";
            }
        }

        public void distimerReset()
        {
            distimer.Stop();
            CurrentPoints = new List<Point>();
            BestPoints = new List<Point>();
            Velocity = new List<Point>();
            BestPoint = new Point();
            BestSolution = double.PositiveInfinity;
            initDistimerTick();
            StartStopButtonName = "Start";
        }

        public double[,] createHeatMap(Func<double, double, double> function, Point a, Point b, int numberOfValues)
        {
            double[,] result = new double[numberOfValues, numberOfValues];
            double[] c = MathNet.Numerics.Generate.LinearSpaced(numberOfValues, Math.Min(a.X, b.X), Math.Max(a.X, b.X));
            double[] d = MathNet.Numerics.Generate.LinearSpaced(numberOfValues, Math.Min(a.Y, b.Y), Math.Max(a.Y, b.Y));
            for (int i = 0; i < numberOfValues; i++)
            {
                for (int j = 0; j < numberOfValues; j++)
                {
                    result[i, j] = function(c[i], d[j]);
                }
            }
            return result;
        }

        private bool classicROI = true;

        public bool ClassicROI
        {
            get => classicROI;
            set => this.RaiseAndSetIfChanged(ref classicROI, value);
        }

        private bool inertialROI = false;

        public bool InertialROI
        {
            get => inertialROI;
            set => this.RaiseAndSetIfChanged(ref inertialROI, value);
        }

        private string indexMethod = "0";

        public string IndexMethod
        {
            get => indexMethod;
            set
            {
                if (value == "0")
                {
                    ClassicROI = true;
                    InertialROI = false;
                }
                else if (value == "1")
                {
                    ClassicROI = false;
                    InertialROI = true;
                }
                this.RaiseAndSetIfChanged(ref indexMethod, value);
            }
        }

        
    }
}
