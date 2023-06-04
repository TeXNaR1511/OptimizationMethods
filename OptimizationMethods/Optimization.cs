using Avalonia.Animation;
using Avalonia.Threading;
using DynamicData;
using MathNet.Numerics.Distributions;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using OptimizationMethods.ViewModels;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace OptimizationMethods
{
    public class Optimization : ViewModelBase
    {

        private DispatcherTimer distimer = new DispatcherTimer();

        private string functionAsString = "(x,y)=>x*x+y*y";

        public string FunctionAsString
        {
            get => functionAsString;
            set => this.RaiseAndSetIfChanged(ref functionAsString, value);
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

        private Point bestPoint = new Point(double.PositiveInfinity, double.PositiveInfinity);
        public Point BestPoint
        {
            get => bestPoint;
            set => this.RaiseAndSetIfChanged(ref bestPoint, value);
        }

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

        private double kanon = 0.5;
        public double Kanon
        {
            get => kanon;
            set => this.RaiseAndSetIfChanged(ref kanon, value);
        }

        private int kNeighbours = 3;
        public int KNeighbours
        {
            get => kNeighbours;
            set => this.RaiseAndSetIfChanged(ref kNeighbours, value);
        }

        private string rKNN = "0.1,0.1,0.5,0.5,0.5";

        public string RKNN
        {
            get => rKNN;
            set => this.RaiseAndSetIfChanged(ref rKNN, value);
        }

        private int updateFrequency = 1000;

        public int UpdateFrequency
        {
            get => updateFrequency;
            set => this.RaiseAndSetIfChanged(ref updateFrequency, value);
        }

        private int livingPoints = 0;

        public int LivingPoints
        {
            get => livingPoints;
            set => this.RaiseAndSetIfChanged(ref livingPoints, value);
        }

        private int iterationCount = 0;

        public int IterationCount
        {
            get => iterationCount;
            set => this.RaiseAndSetIfChanged(ref iterationCount, value);
        }

        private double initTemperature = 1.0;

        public double InitTemperature
        {
            get => initTemperature;
            set => this.RaiseAndSetIfChanged(ref initTemperature, value);
        }

        private double temperature;

        public double Temperature
        {
            get => temperature;
            set => this.RaiseAndSetIfChanged(ref temperature, value);
        }

        private double cooling = 0.99;

        public double Cooling
        {
            get => cooling;
            set => this.RaiseAndSetIfChanged(ref cooling, value);
        }

        private int iterBeforeCool = 10;

        public int IterBeforeCool
        {
            get => iterBeforeCool;
            set => this.RaiseAndSetIfChanged(ref iterBeforeCool, value);
        }

        private int kChild = 20;
        public int KChild
        {
            get => kChild;
            set => this.RaiseAndSetIfChanged(ref kChild, value);
        }

        private double birthRate = 0.1;
        public double BirthRate
        {
            get => birthRate;
            set => this.RaiseAndSetIfChanged(ref birthRate, value);
        }



        public Optimization()
        {

            //var b = new List<Point> { new Point(1, 1), new Point(2, 2), new Point(3, 3), new Point(4, 4) };
            //var a = Methods.findKNN(b, 3);

            //System.Diagnostics.Debug.WriteLine(a.Count);

            //for (int i = 0; i < b.Count; i++)
            //{
            //    System.Diagnostics.Debug.Write(b[i]);
            //}
            //System.Diagnostics.Debug.WriteLine(" ");
            //for (int i = 0; i < a.Count; i++)
            //{
            //    for (int j = 0; j < a[i].Count; j++)
            //    {
            //        System.Diagnostics.Debug.Write(a[i][j]);
            //    }
            //    System.Diagnostics.Debug.WriteLine(" ");
            //}
            distimer.Interval = new TimeSpan(0, 0, 0, 0, UpdateFrequency);
            initDistimerTick();
            distimer.Tick += (s, e) =>
            {
                distimer.Interval = new TimeSpan(0, 0, 0, 0, UpdateFrequency);
                distimerTick();
            };
            //distimer.Interval = new TimeSpan
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

            //var scatterSeries = new OxyPlot.Series.ScatterSeries
            //{
            //    MarkerType = MarkerType.Circle,
            //    MarkerFill = OxyColor.FromRgb(0, 157, 255),
            //};
            ////System.Diagnostics.Debug.WriteLine(points.Count);
            ////i < Math.Min(NumberOfPoints, points.Count)
            //for (int i = 0; i < points.Count; i++)
            //{
            //    var p = new ScatterPoint(points[i].X, points[i].Y, 7, 0);
            //    scatterSeries.Points.Add(p);
            //    scatterSeries.LabelFormatString = i.ToString();
            //    //scatterSeries.Title = i.ToString();
            //    //scatterSeries.TrackerFormatString = scatterSeries.TrackerFormatString + "kek";
            //}
            //
            //model.Series.Add(scatterSeries);

            OxyPlot.Series.ScatterSeries pt;

            for (int i = 0; i < points.Count; i++)
            {
                pt = new OxyPlot.Series.ScatterSeries
                {
                    MarkerType = MarkerType.Circle,
                    MarkerFill = OxyColor.FromRgb(0, 157, 255),
                    //TextColor = OxyColors.White,
                    //FontSize = 10,
                    //LabelMargin = -5,
                    
                };
                var p = new ScatterPoint(points[i].X, points[i].Y, 7, 0);
                pt.Points.Add(p);
                if (Labeled)
                {
                    pt.LabelFormatString = i.ToString();
                    pt.TextColor = OxyColors.White;
                }
                model.Series.Add(pt);
            }

            var scatterPoint = new OxyPlot.Series.ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerFill = OxyColor.FromRgb(255, 0, 0),
                //TextColor = OxyColors.Gold,
                //FontSize = 10,
                //LabelMargin = 10,
            };
            scatterPoint.Points.Add(new ScatterPoint(BestPoint.X, BestPoint.Y, 7, 0));
            if (Labeled)
            {
                scatterPoint.LabelFormatString = "best";
                scatterPoint.TextColor = OxyColors.Gold;
            }
            model.Series.Add(scatterPoint);

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
                BestPoints.Add(cur - vel);
            }

            //BestPoints = CurrentPoints;
            LivingPoints = CurrentPoints.Count;
            BestPoint = BestPoints.Select(x => (function(x.X, x.Y), x)).Min().Item2;
            BestSolution = function(BestPoint.X, BestPoint.Y);
            MyPlotModel = createPlotModel(CurrentPoints);
        }

        public void initDistimerTick()
        {
            IterationCount = 0;
            Temperature = InitTemperature;
            //System.Diagnostics.Debug.WriteLine("init");
            function = EvaluateFunction(FunctionAsString);
            HeatMap = createHeatMap(function, Point.FromString(FirstBorder), Point.FromString(SecondBorder), LinspaceValue);

            MyPlotModel = createPlotModel(CurrentPoints);
        }

        public void distimerTick()
        {
            IterationCount++;
            function = EvaluateFunction(FunctionAsString);
            HeatMap = createHeatMap(function, Point.FromString(FirstBorder), Point.FromString(SecondBorder), LinspaceValue);
            if (ClassicROI)
            {
                var phi = Point.CreateRandomPoint(new Point(0, 0), new Point(1, 1));
                List<List<Point>> a = Methods.ClassicROIIter(CurrentPoints, Velocity, BestPoints, BestPoint, function, phi, new Point(R_p, R_g));
                CurrentPoints = a[0];
                LivingPoints = CurrentPoints.Count;
                Velocity = a[1];
                BestPoints = a[2];
                BestPoint = BestPoints.Select(x => (function(x.X, x.Y), x)).Min().Item2;
                BestSolution = function(BestPoint.X, BestPoint.Y);
            }
            else if (InertialROI)
            {
                var phi = Point.CreateRandomPoint(new Point(0, 0), new Point(1, 1));
                List<List<Point>> a = Methods.InertialROIIter(CurrentPoints, Velocity, BestPoints, BestPoint, function, phi, new Point(R_p, R_g), Inertia);
                CurrentPoints = a[0];
                LivingPoints = CurrentPoints.Count;
                Velocity = a[1];
                BestPoints = a[2];
                BestPoint = BestPoints.Select(x => (function(x.X, x.Y), x)).Min().Item2;
                BestSolution = function(BestPoint.X, BestPoint.Y);
            }
            else if (CanonicalROI)
            {
                var phi = Point.CreateRandomPoint(new Point(2, 2), new Point(4, 4));
                List<List<Point>> a = Methods.CanonicalROIIter(CurrentPoints, Velocity, BestPoints, BestPoint, function, phi, new Point(R_p, R_g), Kanon);
                CurrentPoints = a[0];
                LivingPoints = CurrentPoints.Count;
                Velocity = a[1];
                BestPoints = a[2];
                BestPoint = BestPoints.Select(x => (function(x.X, x.Y), x)).Min().Item2;
                BestSolution = function(BestPoint.X, BestPoint.Y);
            }
            else if (KNNROI)
            {
                List<double> r = ListDoubleFromString(RKNN);
                var uniform1 = new ContinuousUniform(0, 1);
                List<double> phi = uniform1.Samples().Take(KNeighbours + 2).ToList();
                //List<double> phi = new List<double> { uniform1.Sample(), uniform1.Sample(), uniform1.Sample() };
                List<List<Point>> a = Methods.KNNROIIter(CurrentPoints, Velocity, BestPoints, BestPoint, KNeighbours, function, phi, r, Inertia);
                CurrentPoints = a[0];
                LivingPoints = CurrentPoints.Count;
                Velocity = a[1];
                BestPoints = a[2];
                BestPoint = BestPoints.Select(x => (function(x.X, x.Y), x)).Min().Item2;
                BestSolution = function(BestPoint.X, BestPoint.Y);
            }
            else if (ExtinctionROI)
            {
                //System.Diagnostics.Debug.WriteLine("check");
                var phi = Point.CreateRandomPoint(new Point(0, 0), new Point(1, 1));
                List<List<Point>> a = Methods.ExtinctionROIIter(CurrentPoints, Velocity, BestPoints, BestPoint, function, phi, new Point(R_p, R_g), Inertia);
                CurrentPoints = a[0];
                LivingPoints = CurrentPoints.Count;
                //for (int i = 0; i < CurrentPoints.Count; i++)
                //{
                //    System.Diagnostics.Debug.WriteLine(CurrentPoints[i]);
                //}
                Velocity = a[1];
                BestPoints = a[2];
                BestPoint = BestPoints.Select(x => (function(x.X, x.Y), x)).Min().Item2;
                BestSolution = function(BestPoint.X, BestPoint.Y);
            }
            else if (SimulatedAnnealing)
            {
                if (IterationCount % IterBeforeCool == 0) Temperature *= Cooling;
                List<List<Point>> a = Methods.SimulatedAnnealingIter(CurrentPoints, BestPoints, function, Temperature, Inertia);
                CurrentPoints = a[0];
                LivingPoints = CurrentPoints.Count;
                BestPoints = a[1];
                BestPoint = BestPoints.Select(x => (function(x.X, x.Y), x)).Min().Item2;
                BestSolution = function(BestPoint.X, BestPoint.Y);
            }
            else if (SimpleGenetic)
            {
                var phi = Point.CreateRandomPoint(new Point(0, 0), new Point(1, 1));
                List<List<Point>> a = Methods.SimpleGeneticIter(CurrentPoints, BestPoints, BestPoint, function, KNeighbours, new Point(R_p, R_g), phi, KChild, BirthRate);
                CurrentPoints = a[0];
                LivingPoints = CurrentPoints.Count;
                BestPoints = a[1];
                BestPoint = BestPoints.Select(x => (function(x.X, x.Y), x)).Min().Item2;
                BestSolution = function(BestPoint.X, BestPoint.Y);
            }

            if (NoBounds)
            {
                //nothing
            }
            else if (Projection)
            {
                for (int i = 0; i < CurrentPoints.Count; i++)
                {
                    CurrentPoints[i] = Point.GetProjection(CurrentPoints[i], Point.FromString(FirstBorder), Point.FromString(SecondBorder));
                }
            }
            else if (Random)
            {
                for (int i = 0; i < CurrentPoints.Count; i++)
                {
                    if (!Point.InsideBounds(CurrentPoints[i], Point.FromString(FirstBorder), Point.FromString(SecondBorder)))
                    {
                        CurrentPoints[i] = Point.CreateRandomPoint(Point.FromString(FirstBorder), Point.FromString(SecondBorder));
                    }
                }
            }
            else if (Rebound)
            {
                for (int i = 0; i < CurrentPoints.Count; i++)
                {
                    CurrentPoints[i] = Point.GetRebound(CurrentPoints[i], Point.FromString(FirstBorder), Point.FromString(SecondBorder));
                }
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
            BestPoint = new Point(double.PositiveInfinity, double.PositiveInfinity);
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

        private bool canonicalROI = false;

        public bool CanonicalROI
        {
            get => canonicalROI;
            set => this.RaiseAndSetIfChanged(ref canonicalROI, value);
        }

        private bool knnROI = false;

        public bool KNNROI
        {
            get => knnROI;
            set => this.RaiseAndSetIfChanged(ref knnROI, value);
        }

        private bool extinctionROI = false;

        public bool ExtinctionROI
        {
            get => extinctionROI;
            set => this.RaiseAndSetIfChanged(ref extinctionROI, value);
        }

        private bool simulatedAnnealing = false;

        public bool SimulatedAnnealing
        {
            get => simulatedAnnealing;
            set => this.RaiseAndSetIfChanged(ref simulatedAnnealing, value);
        }

        private bool simpleGenetic = false;

        public bool SimpleGenetic
        {
            get => simpleGenetic;
            set => this.RaiseAndSetIfChanged(ref simpleGenetic, value);
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
                    CanonicalROI = false;
                    KNNROI = false;
                    ExtinctionROI = false;
                    SimulatedAnnealing = false;
                    SimpleGenetic = false;
                }
                else if (value == "1")
                {
                    ClassicROI = false;
                    InertialROI = true;
                    CanonicalROI = false;
                    KNNROI = false;
                    ExtinctionROI = false;
                    SimulatedAnnealing = false;
                    SimpleGenetic = false;
                }
                else if (value == "2")
                {
                    ClassicROI = false;
                    InertialROI = false;
                    CanonicalROI = true;
                    KNNROI = false;
                    ExtinctionROI = false;
                    SimulatedAnnealing = false;
                    SimpleGenetic = false;
                }
                else if (value == "3")
                {
                    ClassicROI = false;
                    InertialROI = false;
                    CanonicalROI = false;
                    KNNROI = true;
                    ExtinctionROI = false;
                    SimulatedAnnealing = false;
                    SimpleGenetic = false;
                }
                else if (value == "4")
                {
                    ClassicROI = false;
                    InertialROI = false;
                    CanonicalROI = false;
                    KNNROI = false;
                    ExtinctionROI = true;
                    SimulatedAnnealing = false;
                    SimpleGenetic = false;
                }
                else if (value == "5")
                {
                    ClassicROI = false;
                    InertialROI = false;
                    CanonicalROI = false;
                    KNNROI = false;
                    ExtinctionROI = false;
                    SimulatedAnnealing = true;
                    SimpleGenetic = false;
                }
                else if (value == "6")
                {
                    ClassicROI = false;
                    InertialROI = false;
                    CanonicalROI = false;
                    KNNROI = false;
                    ExtinctionROI = false;
                    SimulatedAnnealing = false;
                    SimpleGenetic = true;
                }
                this.RaiseAndSetIfChanged(ref indexMethod, value);
            }
        }

        public List<double> ListDoubleFromString(string s)
        {
            s = s.Trim();
            List<double> result = new List<double>();
            int count = s.Count(f => f == ',');
            if (count != KNeighbours + 1)
            {
                for (int i = 0; i < KNeighbours + 2; i++)
                {
                    result.Add(0.5);
                }
            }
            else
            {
                string[] splt = s.Split(',');
                for (int i = 0; i < splt.Length; i++)
                {
                    result.Add(Convert.ToDouble(splt[i].Replace('.', ',')));
                }
            }
            return result;
        }

        private bool noBounds = true;
        public bool NoBounds
        {
            get => noBounds;
            set => this.RaiseAndSetIfChanged(ref noBounds, value);
        }

        private bool projection = false;
        public bool Projection
        {
            get => projection;
            set => this.RaiseAndSetIfChanged(ref projection, value);
        }

        private bool random = false;
        public bool Random
        {
            get => random;
            set => this.RaiseAndSetIfChanged(ref random, value);
        }

        private bool rebound = false;
        public bool Rebound
        {
            get => rebound;
            set => this.RaiseAndSetIfChanged(ref rebound, value);
        }

        private string indexBound = "0";

        public string IndexBound
        {
            get => indexBound;
            set
            {
                if (value == "0")
                {
                    NoBounds = true;
                    Projection = false;
                    Random = false;
                    Rebound = false;
                }
                else if (value == "1")
                {
                    NoBounds = false;
                    Projection = true;
                    Random = false;
                    Rebound = false;
                }
                else if (value == "2")
                {
                    NoBounds = false;
                    Projection = false;
                    Random = true;
                    Rebound = false;
                }
                else if (value == "3")
                {
                    NoBounds = false;
                    Projection = false;
                    Random = false;
                    Rebound = true;
                }
                this.RaiseAndSetIfChanged(ref indexBound, value);
            }
        }

        private bool noLabeled = true;
        public bool NoLabeled
        {
            get => noLabeled;
            set => this.RaiseAndSetIfChanged(ref noLabeled, value);
        }

        private bool labeled = false;
        public bool Labeled
        {
            get => labeled;
            set => this.RaiseAndSetIfChanged(ref labeled, value);
        }

        private string indexLabeled = "0";

        public string IndexLabeled
        {
            get => indexLabeled;
            set
            {
                if (value == "0")
                {
                    NoLabeled = true;
                    Labeled = false;
                }
                else if (value == "1")
                {
                    NoLabeled = false;
                    Labeled = true;
                }
                this.RaiseAndSetIfChanged(ref indexLabeled, value);
            }
        }
    }
}
