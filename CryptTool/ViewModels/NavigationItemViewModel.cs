using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptTool.ViewModels
{
    public sealed class NavigationItemViewModel : BaseViewModel
    {
        private string _title;
        private Uri _pageUri;

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value, "Title"); }
        }

        public Uri PageUri
        {
            get { return _pageUri; }
            set { SetProperty(ref _pageUri, value, "PageUri"); }
        }
    }
}
