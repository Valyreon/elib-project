using Domain;
using ElibWpf.ViewModels;
using ElibWpf.ViewModels.Controls;
using Microsoft.Xaml.Behaviors.Core;
using Models;
using Models.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            return new BookViewerViewModel(Caption, Filter, selector);
        }

        public static ViewerState ToState(IViewer viewModel)
        {
            return new ViewerState { Filter = viewModel.Filter, Caption = viewModel.Caption };
        }
    }
}
