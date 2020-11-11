﻿using Domain;
using MVVMLibrary.Messaging;

namespace ElibWpf.Messages
{
	public class SeriesSelectedMessage : MessageBase
	{
		public SeriesSelectedMessage(BookSeries series)
		{
			Series = series;
		}

		public BookSeries Series { get; }
	}
}
