using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace FileSystemTest
{
    public partial class IconTextListControl : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(IconTextListControl), new PropertyMetadata(null, OnItemsSourceChanged));

        public event EventHandler<object> MouseLeftButtonDownItemSelected;
        public event EventHandler<object> MouseLeftButtonUpItemSelected;
        public event EventHandler<object> MouseRightButtonUpItemSelected;

        public event EventHandler<object> ItemDrop;
        public event EventHandler<object> ItemDragOver;
        public event EventHandler<object> ItemDragEnter;
        public event EventHandler<object> ItemDragLeave;

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public IconTextListControl()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateItemsWidth();
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (IconTextListControl)d;
            control.ItemsControl.ItemsSource = control.ItemsSource;
            control.UpdateItemsWidth();
        }

        private void UpdateItemsWidth()
        {
            if (ActualWidth > 0)
            {
                double newWidth = ActualWidth - 20;
                ItemsControl.Width = newWidth;
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            UpdateItemsWidth();
        }

        private void Border_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is Border border && border.Tag is DirectoryItem item)
            {
                item.ItemBorder = border;
            }
        }


        private void Item_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext != null)
                MouseLeftButtonDownItemSelected?.Invoke(this, element.DataContext);
        }


        private void Item_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext != null)
                MouseLeftButtonUpItemSelected?.Invoke(this, element.DataContext);
        }


        private void Item_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext != null)
                MouseRightButtonUpItemSelected?.Invoke(this, element.DataContext);
        }


        private void Item_DragEnter(object sender, DragEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext != null)
                ItemDragEnter?.Invoke(this, element.DataContext);
        }

        private void Item_DragLeave(object sender, DragEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext != null)
                ItemDragLeave?.Invoke(this, element.DataContext);
        }

        private void Item_DragOver(object sender, DragEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext != null)
                ItemDragOver?.Invoke(this, element.DataContext);
        }

        private void Item_Drop(object sender, DragEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext != null)
                ItemDrop?.Invoke(this, element.DataContext);
        }
    }
}