﻿using System;
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
            this.ItemContainerGenerator.StatusChanged += this.ItemContainerGenerator_StatusChanged;
        }

        /// <summary>
        ///     Get the ItemsHolder and generate any children
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.itemsHolderPanel = this.GetTemplateChild("PART_ItemsHolder") as Panel;
            this.UpdateSelectedItem();
        }

        protected TabItem GetSelectedTabItem()
        {
            object selectedItem = this.SelectedItem;
            if (selectedItem == null)
            {
                return null;
            }

            if (!(selectedItem is TabItem item))
            {
                item = this.ItemContainerGenerator.ContainerFromIndex(this.SelectedIndex) as TabItem;
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

            if (this.itemsHolderPanel == null)
            {
                return;
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    this.itemsHolderPanel.Children.Clear();
                    break;

                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null)
                    {
                        foreach (object item in e.OldItems)
                        {
                            ContentPresenter cp = this.FindChildContentPresenter(item);
                            if (cp != null)
                            {
                                this.itemsHolderPanel.Children.Remove(cp);
                            }
                        }
                    }

                    // Don't do anything with new items because we don't want to
                    // create visuals that aren't being shown

                    this.UpdateSelectedItem();
                    break;

                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException("Replace not implemented yet");
            }
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            this.UpdateSelectedItem();
        }

        private ContentPresenter CreateChildContentPresenter(object item)
        {
            if (item == null)
            {
                return null;
            }

            ContentPresenter cp = this.FindChildContentPresenter(item);

            if (cp != null)
            {
                return cp;
            }

            // the actual child to be added.  cp.Tag is a reference to the TabItem
            cp = new ContentPresenter
            {
                Content = item is TabItem tabItem ? tabItem.Content : item,
                ContentTemplate = this.SelectedContentTemplate,
                ContentTemplateSelector = this.SelectedContentTemplateSelector,
                ContentStringFormat = this.SelectedContentStringFormat,
                Visibility = Visibility.Collapsed,
                Tag = item is TabItem ? item : this.ItemContainerGenerator.ContainerFromItem(item)
            };
            this.itemsHolderPanel.Children.Add(cp);
            return cp;
        }

        private ContentPresenter FindChildContentPresenter(object data)
        {
            if (data is TabItem item)
            {
                data = item.Content;
            }

            if (data == null)
            {
                return null;
            }

            if (this.itemsHolderPanel == null)
            {
                return null;
            }

            return this.itemsHolderPanel.Children.Cast<ContentPresenter>().FirstOrDefault(cp => cp.Content == data);
        }

        /// <summary>
        ///     If containers are done, generate the selected item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            if (this.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
            {
                return;
            }

            this.ItemContainerGenerator.StatusChanged -= this.ItemContainerGenerator_StatusChanged;
            this.UpdateSelectedItem();
        }

        private void UpdateSelectedItem()
        {
            if (this.itemsHolderPanel == null)
            {
                return;
            }

            // Generate a ContentPresenter if necessary
            TabItem item = this.GetSelectedTabItem();
            if (item != null)
            {
                this.CreateChildContentPresenter(item);
            }

            // show the right child
            foreach (ContentPresenter child in this.itemsHolderPanel.Children)
            {
                child.Visibility = ((TabItem) child.Tag).IsSelected ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}