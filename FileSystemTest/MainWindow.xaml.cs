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

        public DirectoryItemFolder MainFolder = new DirectoryItemFolder
        {
            Id = 100000,
            Owner = null,
            Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/folder.png")),
            Name = "Основной раздел",
        };
        private DirectoryItem _currentOwner;

        public MainWindow()
        {
            InitializeComponent();

            FileUsed += FileUsage;
            FolderUsed += FolderUsage;

            // Установка источника данных для списка
            IconList.ItemsSource = Items;

            LoadSampleData();
        }


        // Отладочный метод
        private void LoadSampleData()
        {
            DirectoryItem newItem;
            int id = 0;


            while (true)
            {
                id = random.Next(100000, 1000000);
                if (!_allElements.ContainsKey(id))
                    break;
            }
            newItem = new DirectoryItemFolder
            {
                Id = id,
                Owner = _currentOwner,
                Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/folder.png")),
                Name = "Пустая папка",
            };
            _allElements.Add(newItem.Id, newItem);
            MainFolder.AddChild(newItem);


            while (true)
            {
                id = random.Next(100000, 1000000);
                if (!_allElements.ContainsKey(id))
                    break;
            }
            newItem = new DirectoryItemFolder
            {
                Id = id,
                Owner = _currentOwner,
                Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/folder.png")),
                Name = "Папка с проектами",
            };
            _allElements.Add(newItem.Id, newItem);
            MainFolder.AddChild(newItem);

            DirectoryItemFolder tempItem = (DirectoryItemFolder)_allElements[id];
            for (int i = 0; i < 10; i++)
            {
                while (true)
                {
                    id = random.Next(100000, 1000000);
                    if (!_allElements.ContainsKey(id))
                        break;
                }
                newItem = new DirectoryItemFile
                {
                    Id = id,
                    Owner = tempItem,
                    Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/file.png")),
                    Name = $"Файл {i} с разным текстом",
                };
                _allElements.Add(newItem.Id, newItem);
                tempItem.AddChild(newItem);
            }


            for (int i = 0; i < 20; i++)
            {
                while (true)
                {
                    id = random.Next(100000, 1000000);
                    if (!_allElements.ContainsKey(id))
                        break;
                }
                newItem = new DirectoryItemFile
                {
                    Id = id,
                    Owner = _currentOwner,
                    Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/file.png")),
                    Name = $"Файл {i} с разным текстом",
                };
                _allElements.Add(newItem.Id, newItem);
                MainFolder.AddChild(newItem);
            }

            LoadElements(MainFolder);
        }


        private Border _selectedBorder;
        private DateTime _lastClickTime;

        // Нажатие левой кнопкой мыши по элементу
        private void MouseLeftButtonDownItemSelected(object sender, object e)
        {
            if (e is DirectoryItem selectedItem)
            {
                // Новый элемент
                if (_selectedBorder != selectedItem.ItemBorder)
                {
                    if (_selectedBorder != null)
                        _selectedBorder.Background = _defaultColor;

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

                _lastClickTime = DateTime.Now;

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



        private void FileUsage(DirectoryItemFile file) => MessageBox.Show("Использование файла");


        private void FolderUsage(DirectoryItemFolder folder) => LoadElements(folder);



        private void LoadElements(DirectoryItemFolder folder)
        {
            Items.Clear();
            _currentOwner = folder;

            string path = folder.Name;
            DirectoryItem owner = folder.Owner;
            while (true) 
            {
                if (owner != null)
                {
                    path = owner.Name + '\\' + path;
                    owner = owner.Owner;
                }
                else
                    break;
            }

            PathTextBlock.Text = path;

            foreach(DirectoryItem element in folder.GetChild())
                Items.Add(element);
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            DirectoryItemFolder newOwner = (DirectoryItemFolder)_currentOwner.Owner;
            if (newOwner != null)
                LoadElements(newOwner);
        }
    }


    public abstract class DirectoryItem
    {
        public int Id { get; set; }
        public Border ItemBorder { get; set; }
        public DirectoryItemType ItemType { get; set; }
        public DirectoryItem Owner { get; set; }


        public ImageSource Icon { get; set; }
        public string Name { get; set; }
        public Color Color { get; set; }
        public double Opacity { get; set; }


        public virtual void Open() => Console.WriteLine($"Opening {ItemType}: {Name}");

        public virtual void Delete() => Console.WriteLine($"Deleting {ItemType}: {Name}");

        public virtual string GetInfo()
        {
            return $"{ItemType} | ID: {Id} | Name: {Name}";
        }
    }

    public class DirectoryItemFile : DirectoryItem
    {
        public DirectoryItemFile()
        {
            ItemType = DirectoryItemType.File;
        }


        public override void Open()
        {
            base.Open();
            MessageBox.Show(Name);
        }

        public override void Delete()
        {
            Console.WriteLine($"Удаление файла {Name}");
        }
    }

    public class DirectoryItemFolder : DirectoryItem
    {
        private List<DirectoryItem> _childElements = new List<DirectoryItem>();

        public DirectoryItemFolder()
        {
            ItemType = DirectoryItemType.Folder;
        }


        public override void Open()
        {
            base.Open();
            MessageBox.Show(Name);
        }

        public override void Delete()
        {
            Console.WriteLine($"Удаление папки {Name}");
        }

        public void AddChild(DirectoryItem item)
        {
            item.Owner = this;
            _childElements.Add(item);
        }

        public List<DirectoryItem> GetChild() { return _childElements; }
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

                while (visualHit != null && !(visualHit is Border))
                    visualHit = VisualTreeHelper.GetParent(visualHit);

                Border border = (Border)visualHit;

                if (border != null)
                    return border;
            }

            return null;
        }
*/