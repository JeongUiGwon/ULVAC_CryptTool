using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using CryptTool.Views.Pages;

namespace CryptTool.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private ObservableCollection<NavigationItemViewModel> _items;
        private NavigationItemViewModel _selectedItem;
        private Uri _currentPageUri;

        public ObservableCollection<NavigationItemViewModel> Items
        {
            get { return _items; }
            private set { SetProperty(ref _items, value, "Items"); }
        }

        public NavigationItemViewModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (SetProperty(ref _selectedItem, value, "SelectedItem"))
                {
                    Navigate();
                }
            }
        }

        public Uri CurrentPageUri
        {
            get { return _currentPageUri; }
            private set { SetProperty(ref _currentPageUri, value, "CurrentPageUri"); }
        }

        public MainViewModel()
        {
            Items = new ObservableCollection<NavigationItemViewModel>();

            Items.Add(new NavigationItemViewModel
            {
                Title = "Home",
                PageUri = new Uri("/Views/Pages/HomePage.xaml", UriKind.Relative)
            });

            Items.Add(new NavigationItemViewModel
            {
                Title = "File Encrypt",
                PageUri = new Uri("/Views/Pages/FileEncryptPage.xaml", UriKind.Relative)
            });

            Items.Add(new NavigationItemViewModel
            {
                Title = "Folder Encrypt",
                PageUri = new Uri("/Views/Pages/FolderEncryptPage.xaml", UriKind.Relative)
            });

            Items.Add(new NavigationItemViewModel
            {
                Title = "Recipe Encrypt",
                PageUri = new Uri("/Views/Pages/RecipeEncryptPage.xaml", UriKind.Relative)
            });

            if (Items.Count > 0)
            {
                SelectedItem = Items[0];
            }
        }

        private void Navigate()
        {
            if (SelectedItem == null)
            {
                return;
            }

            CurrentPageUri = SelectedItem.PageUri;
        }
    }
}
