using Valyreon.Elib.Mvvm;

namespace Valyreon.Elib.Wpf.Models
{
    public class SourcePathObservable : ObservableObject
    {
        private readonly SourcePath path;

        public SourcePathObservable(SourcePath path)
        {
            this.path = path;
        }

        public string Path
        {
            get => path.Path;
            set
            {
                path.Path = value;
                RaisePropertyChanged(nameof(Path));
            }
        }

        public bool RecursiveScan
        {
            get => path.RecursiveScan;
            set
            {
                path.RecursiveScan = value;
                RaisePropertyChanged(nameof(RecursiveScan));
            }
        }

        public SourcePath GetSourcePath()
        {
            return path;
        }
    }
}
