using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoMonitoringIS.Classes
{
    internal class FileControl
    {
        public static string? OpenFileNameDialog()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Excel documents (.xlsx)|*.xlsx";
            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                string filename = dialog.FileName;
                return filename;
            }
            else
                return null;
        }
    }
}
