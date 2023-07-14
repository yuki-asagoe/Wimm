using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Wimm.Logging;

namespace Wimm
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            CrashReport.Log("Unhandled Exception Occurs in the UI thread.",e.Exception);
            MessageBox.Show($"[{e.Exception.GetType().Name}]\n詳しくはログフォルダのクラッシュレポートをご覧ください", "UIスレッドで未処理の例外が発生しました。");
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += (o, eh) =>
            {
                var exception = eh.ExceptionObject as Exception;
                if(exception is not null)
                {
                    CrashReport.Log("Unhandled Exception Occurs in the Worker thread.", exception);
                    MessageBox.Show($"[{exception?.GetType().Name}]\n詳しくはログフォルダのクラッシュレポートをご覧ください", "ワーカースレッドで未処理の例外が発生しました。");
                }

                Environment.Exit(-1);
            };
        }
    }
}
