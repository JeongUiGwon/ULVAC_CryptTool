using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace CryptTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_DispatcherUnhandledException( object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Exception ex = e.Exception;

            string exceptionMessage = ex.Message;
            string exceptionType = ex.GetType().Name;

            string message =
               "An unexpected error has occurred.\n\n" +
               "[Error Type]\n" + exceptionType + "\n\n" +
               "[Message]\n" + exceptionMessage + "\n\n" +
               "Do you want to continue running the application?\n\n" +
               "Click 'Yes' to continue, or 'No' to exit.";

            MessageBoxResult result = MessageBox.Show(
               message,
               "Application Error",
               MessageBoxButton.YesNo,
               MessageBoxImage.Error);

            if (result == MessageBoxResult.Yes)
            {
                e.Handled = true;
            }
            else
            {
                e.Handled = false;
                Shutdown();
            }
        }
    }
}
