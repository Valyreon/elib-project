using DataLayer;
using ElibWpf.ViewModels;
using ElibWpf.ViewModels.Controls;
using Models;

namespace ElibWpf.BindingItems
{
    public class ViewerState
    {
        public Filter Filter { get; private set; }
        public string Caption { get; private set; }

        private ViewerState()
        {

        }

        public BookViewerViewModel GetViewModel(Selector selector)
        {
            return new BookViewerViewModel(Caption, Filter.Clone, selector);
        }

        public static ViewerState ToState(IViewer viewModel)
        {
            return new ViewerState { Filter = viewModel.Filter.Clone, Caption = viewModel.Caption };
        }
    }
}
