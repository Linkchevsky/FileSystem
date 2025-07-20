using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FileSystemTest
{
    public partial class MainWindow : Window
    {
        private SolidColorBrush _defaultColor = null;
        private SolidColorBrush _selectedColor = new SolidColorBrush(Color.FromArgb(50, 0, 191, 255));
        private SolidColorBrush _dragSelectedColor = new SolidColorBrush(Color.FromArgb(25, 0, 191, 255));

        public Action<DirectoryItemFile> FileUsed;
        public Action<DirectoryItemFolder> FolderUsed;

        private Dictionary<int, DirectoryItem> _allElements = new Dictionary<int, DirectoryItem>();
        Random random = new Random();

        // Коллекция элементов для отображения
        private ObservableCollection<DirectoryItem> Items { get; } = new ObservableCollection<DirectoryItem>();
        private DirectoryItem _currentOwner;

        public MainWindow()
        {
            InitializeComponent();

            // Инициализация начальной папки
            _currentOwner = new DirectoryItemFolder();

            FileUsed += FileUsage;
            FolderUsed += FolderUsage;

            // Устанавливаем источник данных для элементов
            IconList.ItemsSource = Items;

            LoadSampleData();
        }

        private void LoadSampleData()
        {
            DirectoryItem newItem;
            int id = 0;


            while (!_allElements.ContainsKey(id))
                id = random.Next(100000, 1000000);
            newItem = new DirectoryItemFolder
            {
                Id = id,
                Owner = _currentOwner,
                Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/folder.png")),
                Name = "Пустая папка",
            };
            _allElements.Add(newItem.Id, newItem);
            Items.Add(newItem);


            while (!_allElements.ContainsKey(id))
                id = random.Next(100000, 1000000);
            newItem = new DirectoryItemFolder
            {
                Id = id,
                Owner = _currentOwner,
                Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/folder.png")),
                Name = "Папка с проектами",
            };
            _allElements.Add(newItem.Id, newItem);
            Items.Add(newItem);

            DirectoryItemFolder tempItem = (DirectoryItemFolder)newItem;
            for (int i = 0; i < 10; i++)
            {
                while (!_allElements.ContainsKey(id))
                    id = random.Next(100000, 1000000);
                newItem = new DirectoryItemFile
                {
                    Id = id,
                    Owner = tempItem,
                    Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/file.png")),
                    Name = $"Файл {i} с разным текстом",
                };
                tempItem.ChildElements.Add(newItem);
                _allElements.Add(newItem.Id, newItem);
                Items.Add(newItem);
            }


            for (int i = 0; i < 20; i++)
            {
                Items.Add(new DirectoryItemFile
                {
                    Id = id,
                    Owner = _currentOwner,
                    Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/file.png")),
                    Name = $"Файл {i} с разным текстом",
                });
            }
        }


        private Border _selectedBorder;
        private DateTime _lastClickTime;

        // Нажатие левой кнопкой мыши по элементу
        private void MouseLeftButtonDownItemSelected(object sender, object e)
        {
            if (e is DirectoryItem selectedItem)
            {
                _lastClickTime = DateTime.Now;

                // Новый элемент
                if (_selectedBorder != selectedItem.ItemBorder)
                {
                    if (_selectedBorder != null)
                        _selectedBorder.Background = _defaultColor;

                    selectedItem.ItemBorder = GetElementUnderCursor();

                    _selectedBorder = selectedItem.ItemBorder;
                    _selectedBorder.Background = _selectedColor;
                }
                // Старый элемент
                else
                {
                    selectedItem.ItemBorder = _selectedBorder;

                    if ((DateTime.Now - _lastClickTime).TotalMilliseconds < 500) // Обработка двойного клика
                    {
                        switch(selectedItem.ItemType)
                        {
                            case DirectoryItemType.File:
                                FileUsed?.Invoke((DirectoryItemFile)selectedItem);
                                break;


                            case DirectoryItemType.Folder:
                                FolderUsed?.Invoke((DirectoryItemFolder)selectedItem);
                                break;
                        }
                    }
                }


                if (selectedItem.ItemBorder != null)
                {
                    DataObject data = new DataObject(DataFormats.Serializable, sender);
                    DragDrop.DoDragDrop((DependencyObject)sender, data, DragDropEffects.Move);
                }
            }
        }


        // Отпускание левой кнопкой мыши по элементу
        private void MouseLeftButtonUpItemSelected(object sender, object e)
        {
            if (e is DirectoryItem selectedItem)
                MessageBox.Show($"Выбран элемент:\nID: {selectedItem.Id}\nНазвание: {selectedItem.Name}", "Информация о элементе");
        }


        // Отпускание правой кнопкой мыши по элементу
        private void MouseRightButtonUpItemSelected(object sender, object e)
        {
            if (e is DirectoryItem selectedItem)
                MessageBox.Show($"Выбран элемент:\nID: {selectedItem.Id}\nНазвание: {selectedItem.Name}", "Информация о элементе");
        }



        private Border GetElementUnderCursor()
        {
            Point position = Mouse.GetPosition(this);
            HitTestResult hitTestResult = VisualTreeHelper.HitTest(this, position);

            if (hitTestResult != null)
            {
                DependencyObject visualHit = hitTestResult.VisualHit;

                while (visualHit != null && !(visualHit is Border))
                    visualHit = VisualTreeHelper.GetParent(visualHit);

                Border border = (Border)visualHit;

                if (border != null)
                    return border;
            }

            return null;
        }



        private void ItemDragEnter(object sender, object e)
        {
            if (e is DirectoryItem selectedItem)
            {
                if (_selectedBorder != selectedItem.ItemBorder)
                    if (selectedItem.ItemBorder != null)
                        selectedItem.ItemBorder.Background = _dragSelectedColor;
            }
        }

        private void ItemDragLeave(object sender, object e)
        {
            if (e is DirectoryItem selectedItem)
            {
                if (_selectedBorder != selectedItem.ItemBorder)
                    if (selectedItem.ItemBorder != null)
                        selectedItem.ItemBorder.Background = _defaultColor;
            }
        }

        private void ItemDragOver(object sender, object e)
        {
            
        }

        private void ItemDrop(object sender, object e)
        {
            if (e is DirectoryItem selectedItem)
            {
                if (_selectedBorder != selectedItem.ItemBorder)
                    if (selectedItem.ItemBorder != null)
                        selectedItem.ItemBorder.Background = _defaultColor;

                if (selectedItem.ItemType == DirectoryItemType.Folder)
                {

                }
            }
        }



        private void FileUsage(DirectoryItemFile file)
        {
            MessageBox.Show("Использование файла");
        }


        private void FolderUsage(DirectoryItemFolder file)
        {
            MessageBox.Show("Использование папки");
        }
    }


    public class DirectoryItem
    {
        public int Id { get; set; }
        public Border ItemBorder { get; set; }
        public DirectoryItemType ItemType { get; set; }
        public DirectoryItem Owner;


        public ImageSource Icon { get; set; }
        public string Name { get; set; }
        public Color Color { get; set; }
        public double Opacity { get; set; }
    }

    public class DirectoryItemFile : DirectoryItem
    {
        public DirectoryItemFile()
        {
            ItemType = DirectoryItemType.File;
        }
    }

    public class DirectoryItemFolder : DirectoryItem
    {
        public List<DirectoryItem> ChildElements = new List<DirectoryItem>();

        public DirectoryItemFolder()
        {
            ItemType = DirectoryItemType.Folder;
        }
    }


    public enum DirectoryItemType
    {
        File,
        Folder
    }
}





/*
private Border GetElementUnderCursor()
        {
            Point position = Mouse.GetPosition(this);
            HitTestResult hitTestResult = VisualTreeHelper.HitTest(this, position);

            if (hitTestResult != null)
            {
                DependencyObject visualHit = hitTestResult.VisualHit;

                Border border = FindVisualParent<Border>(visualHit);
                if (border != null)
                    return border;
            }

            return null;
        }

        public static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null && !(child is T))
            {
                child = VisualTreeHelper.GetParent(child);
            }

            return child as T;
        }
*/