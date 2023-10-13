using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelDataReader;

namespace EcoMonitoringIS.Classes
{
    class ExcelControl
    {
        public static DataTableCollection ExelToTableColl(string fileName)
        {
            DataTableCollection tableCollection =  null;
            //FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read);
            FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);////
            IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream);
            DataSet db = reader.AsDataSet(new ExcelDataSetConfiguration()
            {
                ConfigureDataTable = (x) => new ExcelDataTableConfiguration()
                {
                    UseHeaderRow = true
                }
            });
            tableCollection = db.Tables;
            return tableCollection;
            //DataTable table = tableCollection[0];

        }
    }
}
