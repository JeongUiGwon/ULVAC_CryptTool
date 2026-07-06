using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CryptTool.Behaviors
{
    public static class FileDropBehavior
    {
        public static readonly DependencyProperty FileDropCommandProperty =
            DependencyProperty.RegisterAttached(
                "FileDropCommand",
                typeof(ICommand),
                typeof(FileDropBehavior),
                new PropertyMetadata(null, OnFileDropCommandChanged));

        public static ICommand GetFileDropCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(FileDropCommandProperty);
        }

        public static void SetFileDropCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(FileDropCommandProperty, value);
        }

        private static void OnFileDropCommandChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            TextBox textBox = d as TextBox;

            if (textBox == null)
                return;

            textBox.AllowDrop = true;
            textBox.PreviewDragOver += TextBox_PreviewDragOver;
            textBox.Drop += TextBox_Drop;
        }

        private static void TextBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;

            e.Handled = true;
        }

        private static void TextBox_Drop(object sender, DragEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (textBox == null)
                return;

            string[] files =
                e.Data.GetData(DataFormats.FileDrop) as string[];

            if (files == null || files.Length == 0)
                return;

            ICommand command = GetFileDropCommand(textBox);

            if (command != null && command.CanExecute(files[0]))
            {
                command.Execute(files[0]);
            }
        }
    }
}
