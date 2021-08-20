using System;

namespace Valyreon.Elib.Wpf.Messages
{
	public class ShowInputDialogMessage
	{
		public ShowInputDialogMessage(string title, string text, Action<string> func)
		{
			Title = title;
			Text = text;
			CallOnResult = func;
		}

		public Action<string> CallOnResult { get; }

		public string Text { get; }

		public string Title { get; }
	}
}
