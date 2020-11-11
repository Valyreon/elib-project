using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ElibWpf.CustomComponents
{
	[TemplatePart(Name = "PART_ItemsHolder", Type = typeof(Panel))]
	public class TabControlWithCache : TabControl
	{
		private Panel itemsHolderPanel;

		public TabControlWithCache()
		{
			// This is necessary so that we get the initial databound selected item
			ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
		}

		/// <summary>
		///     Get the ItemsHolder and generate any children
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			itemsHolderPanel = GetTemplateChild("PART_ItemsHolder") as Panel;
			UpdateSelectedItem();
		}

		protected TabItem GetSelectedTabItem()
		{
			var selectedItem = SelectedItem;
			if(selectedItem == null)
			{
				return null;
			}

			if(selectedItem is not TabItem item)
			{
				item = ItemContainerGenerator.ContainerFromIndex(SelectedIndex) as TabItem;
			}

			return item;
		}

		/// <summary>
		///     When the items change we remove any generated panel children and add any new ones as necessary
		/// </summary>
		/// <param name="e"></param>
		protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
		{
			base.OnItemsChanged(e);

			if(itemsHolderPanel == null)
			{
				return;
			}

			switch(e.Action)
			{
				case NotifyCollectionChangedAction.Reset:
					itemsHolderPanel.Children.Clear();
					break;

				case NotifyCollectionChangedAction.Add:
				case NotifyCollectionChangedAction.Remove:
					if(e.OldItems != null)
					{
						foreach(var item in e.OldItems)
						{
							var cp = FindChildContentPresenter(item);
							if(cp != null)
							{
								itemsHolderPanel.Children.Remove(cp);
							}
						}
					}

					// Don't do anything with new items because we don't want to
					// create visuals that aren't being shown

					UpdateSelectedItem();
					break;

				case NotifyCollectionChangedAction.Replace:
					throw new NotImplementedException("Replace not implemented yet");
			}
		}

		protected override void OnSelectionChanged(SelectionChangedEventArgs e)
		{
			base.OnSelectionChanged(e);
			UpdateSelectedItem();
		}

		private ContentPresenter CreateChildContentPresenter(object item)
		{
			if(item == null)
			{
				return null;
			}

			var cp = FindChildContentPresenter(item);

			if(cp != null)
			{
				return cp;
			}

			// the actual child to be added.  cp.Tag is a reference to the TabItem
			cp = new ContentPresenter
			{
				Content = item is TabItem tabItem ? tabItem.Content : item,
				ContentTemplate = SelectedContentTemplate,
				ContentTemplateSelector = SelectedContentTemplateSelector,
				ContentStringFormat = SelectedContentStringFormat,
				Visibility = Visibility.Collapsed,
				Tag = item is TabItem ? item : ItemContainerGenerator.ContainerFromItem(item)
			};
			itemsHolderPanel.Children.Add(cp);
			return cp;
		}

		private ContentPresenter FindChildContentPresenter(object data)
		{
			if(data is TabItem item)
			{
				data = item.Content;
			}

			return data == null ? null : (itemsHolderPanel?.Children.Cast<ContentPresenter>().FirstOrDefault(cp => cp.Content == data));
		}

		/// <summary>
		///     If containers are done, generate the selected item
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
		{
			if(ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
			{
				return;
			}

			ItemContainerGenerator.StatusChanged -= ItemContainerGenerator_StatusChanged;
			UpdateSelectedItem();
		}

		private void UpdateSelectedItem()
		{
			if(itemsHolderPanel == null)
			{
				return;
			}

			// Generate a ContentPresenter if necessary
			var item = GetSelectedTabItem();
			if(item != null)
			{
				CreateChildContentPresenter(item);
			}

			// show the right child
			foreach(ContentPresenter child in itemsHolderPanel.Children)
			{
				child.Visibility = ((TabItem)child.Tag).IsSelected ? Visibility.Visible : Visibility.Collapsed;
			}
		}
	}
}
