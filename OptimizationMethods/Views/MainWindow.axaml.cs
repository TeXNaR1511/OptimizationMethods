using Avalonia.Controls;
using Avalonia.Logging;
using Avalonia.Markup.Xaml;
using OxyPlot.Series;

namespace OptimizationMethods.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Logger.TryGet(LogEventLevel.Fatal, LogArea.Control)?.Log(this, "Avalonia Infrastructure");
            System.Diagnostics.Debug.WriteLine("System Diagnostics Debug");
            this.WindowState = WindowState.Maximized;

            //AvaloniaXamlLoader.Load(this);
            //var plot = this.FindControl<OxyPlot.Avalonia.Plot>("MyPlot");
            //plot.Title = "qwe";
            //plot.RenderingDecorator.Method.Name = HeatMapRenderMethod.Rectangles;
        }
    }
}