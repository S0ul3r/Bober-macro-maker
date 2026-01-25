using System;
using System.Windows;

namespace WWMBoberRotations
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            var dataDir = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, 
                "Data"
            );
            
            if (!System.IO.Directory.Exists(dataDir))
            {
                System.IO.Directory.CreateDirectory(dataDir);
            }
        }
    }
}
