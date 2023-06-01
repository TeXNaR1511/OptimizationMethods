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
            model.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Position = AxisPosition.Bottom,
                MajorTickSize = 10,
                MinorTickSize = 7,
            });
            model.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Position = AxisPosition.Left,
                MajorTickSize = 10,
                MinorTickSize = 7,
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
            
            var a = Point.FromString(FirstBorder);
            var b = Point.FromString(SecondBorder);
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
                MarkerFill = OxyColors.Red,

            };

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

        public void initDistimerTick()
        {
            //System.Diagnostics.Debug.WriteLine("init");
            function = EvaluateFunction(FunctionAsString);
            if (ClassicROI)
            {
                //HeatMap = new double[,] { { 1, 2, 3}, { 4, 5, 6} };
                HeatMap = createHeatMap(function, Point.FromString(FirstBorder), Point.FromString(SecondBorder), LinspaceValue);
            }
            if (InertialROI)
            {
                HeatMap = createHeatMap(function, Point.FromString(FirstBorder), Point.FromString(SecondBorder), LinspaceValue);
            }

            MyPlotModel = createPlotModel(new List<Point> { new Point(0, 0), new Point(1, 1)});
        }

        public void distimerTick()
        {
            //System.Diagnostics.Debug.WriteLine("tick");
            function = EvaluateFunction(FunctionAsString);
            if (ClassicROI)
            {
                HeatMap = createHeatMap(function, Point.FromString(FirstBorder), Point.FromString(SecondBorder), LinspaceValue);
            }
            if (InertialROI)
            {
                HeatMap = createHeatMap(function, Point.FromString(FirstBorder), Point.FromString(SecondBorder), LinspaceValue);
            }

            MyPlotModel = createPlotModel(new List<Point> { new Point(0, 0), new Point(1, 1) });
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