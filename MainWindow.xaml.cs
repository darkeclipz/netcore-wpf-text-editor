using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace netcore_wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window 
    {
        private static Brush backgroundColor = 
            (SolidColorBrush)(new BrushConverter().ConvertFrom("#111"));
        private static Brush backgroundColorAccent = 
            (SolidColorBrush)(new BrushConverter().ConvertFrom("#222"));
        private static Brush foregroundColor = 
            (SolidColorBrush)(new BrushConverter().ConvertFrom("#999"));
        private static Brush borderColor = 
            (SolidColorBrush)(new BrushConverter().ConvertFrom("#999"));
        private static Brush editorTextColor =
            (SolidColorBrush)(new BrushConverter().ConvertFrom("#eee"));
        private static Brush identifierColor = 
            (SolidColorBrush)(new BrushConverter().ConvertFrom("#227add"));

        private static int menuPaddingWidth = 14;
        private static int menuPaddingHeight = 6;
        private static Thickness menuItemPadding = 
            new Thickness(menuPaddingWidth, menuPaddingHeight, menuPaddingWidth, menuPaddingHeight);
        private static int menuItemMargin = 2;

        private static RichTextBox editor;
        private static StatusBar status;
        private static bool fileHasPendingChanges = false;
        private static string currentFilepath = String.Empty;

        public MainWindow()
        {
            this.WindowStyle = WindowStyle.ThreeDBorderWindow;
            InitializeComponent();
            Grid grid = InitializeGrid();
            InitializeMenu(grid);
            InitializeStatusBar(grid);
            InitializeTree(grid);
            InitializeEditor(grid);
        }    

        private static void InitializeStatusBar(Grid grid)
        {
            // Statusbar
            status = new StatusBar();
            status.Background = backgroundColor;
            status.BorderThickness = new Thickness(0, 2, 0, 0);
            status.BorderBrush = backgroundColorAccent;
            status.Margin = new Thickness(2);
            status.Padding = new Thickness(2);
            Grid.SetRow(status, 2);
            Grid.SetColumnSpan(status, 2);
            var sItem = new StatusBarItem();
            sItem.Foreground = foregroundColor;
            sItem.Content = "Hello";
            status.Items.Add(sItem);
            grid.Children.Add(status);
        }

        private static void EditorCreateBlankDocument(RichTextBox editor) {
            var p = new Paragraph();
            p.Foreground = editorTextColor;
            p.FontSize = 12;
            p.Margin = new Thickness(0);
            editor.Document.Blocks.Clear();
            editor.Document.Blocks.Add(p);
            (status.Items[0] as StatusBarItem).Content = "<new>";
        }

        private static void InitializeEditor(Grid grid)
        {
            // Adding a text editor
            editor = new RichTextBox();
            editor.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            Grid.SetRow(editor, 1);
            Grid.SetColumn(editor, 1);
            editor.HorizontalAlignment = HorizontalAlignment.Stretch;
            editor.VerticalAlignment = VerticalAlignment.Stretch;
            grid.Children.Add(editor);
            editor.Background = backgroundColorAccent;
            editor.Foreground = editorTextColor;
            editor.FontFamily = new FontFamily("Consolas");
            editor.FontSize = 14;
            editor.BorderThickness = new Thickness(0);            
            editor.Margin = new Thickness(2);
            editor.Padding = new Thickness(10);
            editor.TextChanged += TextChangedEvent;
            EditorCreateBlankDocument(editor);
            fileHasPendingChanges = false;
        }

        private static void TextChangedEvent(object sender, EventArgs args)
        {
            fileHasPendingChanges = true;
        }

        private static void ReadDirectory(string filepath, TreeViewItem parent) {
            // Read directories.
            foreach(var directory in Directory.EnumerateDirectories(filepath)) {
                var info = new DirectoryInfo(directory);
                var treeItem = new TreeViewItem();
                treeItem.Foreground = foregroundColor;
                treeItem.Header = info.Name;
                parent.Items.Add(treeItem);
                ReadDirectory(directory, treeItem);
            }
            // Read files
            foreach(var file in Directory.EnumerateFiles(filepath)) {
                var info = new FileInfo(file);
                var treeItem = new TreeViewItem();
                treeItem.Foreground = foregroundColor;
                treeItem.Header = info.Name;
                treeItem.MouseDoubleClick += MenuItemClick;
                treeItem.Tag = file;
                parent.Items.Add(treeItem);
            }
        }

        private static bool CheckForPendingChanges() {
            if(fileHasPendingChanges) {
                var response = MessageBox.Show("Save changes?", 
                    "Unsaved changes", MessageBoxButton.YesNoCancel);
                switch(response) {
                    case MessageBoxResult.Yes:
                        FileSave(null, null);
                        fileHasPendingChanges = false;
                        return true;
                    case MessageBoxResult.No:
                        return true;
                    case MessageBoxResult.Cancel:
                        return false;       
                    default:
                        return false;             
                }
            }
            else {
                return true;
            }
        }

        private static void EditorOpenFile(string filepath) {
            var content = File.ReadAllText(filepath);
            editor.Document.Blocks.Clear();
            var paragraph = new Paragraph();
            paragraph.Inlines.Add(content);
            paragraph.Margin = new Thickness(0);
            paragraph.FontFamily = new FontFamily("Consolas");
            paragraph.FontSize = 12;
            editor.Document.Blocks.Add(paragraph);
            var info = new FileInfo(filepath);
            (status.Items[0] as StatusBarItem).Content = info.Name;
            fileHasPendingChanges = false;
        }

        private static void MenuItemClick(object sender, EventArgs e) {
            if(sender is TreeViewItem tvi) {
                if(CheckForPendingChanges()) {
                    EditorOpenFile(tvi.Tag.ToString());
                }
            }
        }

        private static void InitializeTree(Grid grid)
        {
            // Add a tree view
            var tree = new TreeView();
            Grid.SetRow(tree, 1);
            Grid.SetColumn(tree, 0);
            tree.HorizontalAlignment = HorizontalAlignment.Stretch;
            tree.VerticalAlignment = VerticalAlignment.Stretch;
            tree.Background = backgroundColor;
            tree.BorderThickness = new Thickness(0);
            grid.Children.Add(tree);
            var treeViewItem = new TreeViewItem();
            treeViewItem.Header = "/";
            treeViewItem.Foreground = foregroundColor;
            tree.Items.Add(treeViewItem);
            ReadDirectory(Directory.GetCurrentDirectory(), treeViewItem);            
        }

        private static void FileNew(object sender, EventArgs args) 
        {
            if(CheckForPendingChanges()) {
                EditorCreateBlankDocument(editor);
                fileHasPendingChanges = false;    
            }
        }

        private static void FileOpen(object sender, EventArgs args) 
        {
            if(CheckForPendingChanges()) {
                var dialog = new OpenFileDialog();
                if(dialog.ShowDialog() == true) {
                    EditorOpenFile(dialog.FileName);
                    currentFilepath = dialog.FileName;
                    fileHasPendingChanges = false;
                }
            }
        }

        private static void FileSave(object sender, EventArgs args) 
        {
            if(string.IsNullOrWhiteSpace(currentFilepath)) {
                FileSaveAs(null, null);
            }
            else {
                SaveToFile(currentFilepath);
            }
        }

        private static void SaveToFile(string filepath) {
            var textRange = new TextRange(editor.Document.ContentStart, editor.Document.ContentEnd);
            File.WriteAllText(filepath, textRange.Text);
        }

        private static void FileSaveAs(object sender, EventArgs args) 
        {
            var dialog = new SaveFileDialog();
            if(dialog.ShowDialog() == true) {
                SaveToFile(dialog.FileName);
                currentFilepath = dialog.FileName;
                fileHasPendingChanges = false;
            }
        }

        private static void FileQuit(object sender, EventArgs args) 
        {
            if(CheckForPendingChanges()) {
                MessageBox.Show("Quit!");
            }
        }

        private static void InitializeMenu(Grid grid)
        {
            // Add a menu
            var menu = new Menu();
            menu.Margin = new Thickness(menuItemMargin);
            Grid.SetRow(menu, 0);
            Grid.SetColumnSpan(menu, 2);
            menu.HorizontalAlignment = HorizontalAlignment.Stretch;
            menu.VerticalAlignment = VerticalAlignment.Stretch;
            menu.BorderThickness = new Thickness(0, 0, 0, 2);
            menu.BorderBrush = backgroundColorAccent;
            menu.Background = backgroundColor;
            menu.Margin = new Thickness(2);
            menu.Padding = new Thickness(2);

            grid.Children.Add(menu);

            // File menu
            var item = new MenuItem();
            item.Background = foregroundColor;
            item.Header = "File";
            item.Padding = menuItemPadding;
            item.BorderThickness = new Thickness(0);
            menu.Items.Add(item);

            var itemNew = new MenuItem
            {
                Header = "New..."
            };
            itemNew.Click += FileNew;
            item.Items.Add(itemNew);

            var itemOpen = new MenuItem
            {
                Header = "Open..."
            };
            itemOpen.Click += FileOpen;
            item.Items.Add(itemOpen);

            var itemSave = new MenuItem
            {
                Header = "Save"
            };
            itemSave.Click += FileSave;
            item.Items.Add(itemSave);

            var itemSaveAs = new MenuItem
            {
                Header = "Save as..."
            };
            itemSaveAs.Click += FileSaveAs;
            item.Items.Add(itemSaveAs);

            var itemQuit = new MenuItem
            {
                Header = "Quit"
            };
            itemQuit.Click += FileQuit;
            item.Items.Add(itemQuit);
        }

        private Grid InitializeGrid()
        {
            // Create a new grid
            var grid = new Grid();
            grid.Background = backgroundColor;
            //grid.ShowGridLines = true;
            this.Content = grid;

            // Add three rows
            for (int i = 0; i < 3; i++)
            {
                var row = new RowDefinition();
                switch (i)
                {
                    case 0:
                        row.Height = new GridLength(0, GridUnitType.Auto);
                        break;
                    case 1:
                        row.Height = new GridLength(1, GridUnitType.Star);
                        break;
                    case 2:
                        row.Height = new GridLength(0, GridUnitType.Auto);
                        break;
                }

                grid.RowDefinitions.Add(row);
            }

            // Add columns
            var colTree = new ColumnDefinition();
            colTree.Width = new GridLength(1, GridUnitType.Star);
            grid.ColumnDefinitions.Add(colTree);

            var colEditor = new ColumnDefinition();
            colEditor.Width = new GridLength(3, GridUnitType.Star);
            grid.ColumnDefinitions.Add(colEditor);
            return grid;
        }

    }
}
