using Domain;
using ElibWpf.ViewModels.Controls;
using System;

namespace ElibWpf.Components
{
    public class PaneMainItem
    {
        public string PaneCaption { get; }
        public string FaIconName { get; }
        public BookViewerViewModel ViewModel { get; }

        public PaneMainItem(string paneCaption, string faIconName, BookViewerViewModel viewModel)
        {
            this.PaneCaption = paneCaption;
            this.FaIconName = faIconName;
            this.ViewModel = viewModel;
        }
    }
}
