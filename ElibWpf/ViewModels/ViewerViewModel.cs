using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElibWpf.ViewModels
{
    public abstract class ViewerViewModel : ViewModelBase, ICloneable
    {
        public abstract object Clone();

        public event Action Refresh;

        protected void RaiseRefreshEvent()
        {
            Refresh?.Invoke();
        }
    }
}
