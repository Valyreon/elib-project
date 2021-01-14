using Valyreon.Elib.DataLayer;
using System;

namespace Valyreon.Elib.Wpf.BindingItems
{
	public class PaneMainItem
	{
		public PaneMainItem(string paneCaption, Enum faIconName, string viewerCaption, FilterParameters filter)
		{
			Filter = filter;
			PaneCaption = paneCaption;
			Icon = faIconName;
			ViewerCaption = viewerCaption;
		}

		public FilterParameters Filter { get; }
		public Enum Icon { get; }
		public string PaneCaption { get; }
		public string ViewerCaption { get; }
	}
}
