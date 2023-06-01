using Avalonia.Markup.Xaml;
using ReactiveUI;

namespace OptimizationMethods.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {

        public MainWindowViewModel() 
        {
            
        }

        private Optimization optimization = new Optimization();

        public Optimization Optimization
        {
            get => optimization;
            set => this.RaiseAndSetIfChanged(ref optimization, value);
        }
    }
}