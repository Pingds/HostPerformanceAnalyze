using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Win32;

namespace HostPerformanceAnalyze
{
    public class ExcelHelper
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        public static string SaveCSV(List<BaseDataHandler> dt)
        {
            try
            {
                string fileFullName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", $"{ DateTime.Now.ToString("HH-mm-ss")}.csv");
                FileInfo fi = new FileInfo(fileFullName);
                if (!fi.Directory.Exists)
                {
                    fi.Directory.Create();
                }
                FileStream fs = new FileStream(fileFullName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                //StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default);
                StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
                //Time	RoomsHost GPU	All GPU	RoomsHost CPU	RoomsHost Memory	Time	RoomsHost Handle	RoomsHost Thread	dwm GPU

                string data = "Time,";
                //column header
                for (int i = 0; i < dt.Count; i++)
                {
                    if (i == 1)
                    {
                        data += "All GPU,";
                    }
                    if (i == 3)
                    {
                        data += "Time,";
                    }
                    data += dt[i].ProcessName + " " + dt[i].DataType.ToString() + ",";
                }
                data = data.Substring(0, data.Length - 1);
                sw.WriteLine(data);

                //write row data
                int dataRowCount = dt[0].Data.Count;
                for (int i = 0; i < dataRowCount; i++)//no need calculate value 0 case
                {
                    data = "";
                    for (int j = 0; j < dt.Count; j++)
                    {
                        if (j == 0)
                        {
                            data += dt[j].Data[i].Time.ToString("HH:mm:ss") + ",";
                        }
                        if (j == 1)
                        {
                            data += dt[0].Data[i].Value + dt[5].Data[i].Value + ",";//all GPU
                        }
                        if (j == 3)
                        {
                            data += dt[j].Data[i].Time.ToString("HH:mm:ss") + ",";
                        }
                        data += dt[j].Data[i].Value + ",";
                    }
                    data = data.Substring(0, data.Length - 1);
                    sw.WriteLine(data);
                }
                sw.Close();
                fs.Close();

                MessageBox.Show("Store successly!");
                return fileFullName;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Store failed! " + ex.ToString());
                return string.Empty;
            }
        }

        /// <summary>
        /// 将DataTable中数据写入到CSV文件中
        /// </summary>
        /// <param name="dt">提供保存数据的DataTable</param>
        /// <param name="fileName">CSV的文件路径</param>
        public static void SaveCSV(DataTable dt)
        {
            SaveFileDialog objSFD = new SaveFileDialog() { DefaultExt = "csv", Filter = "CSV Files (*.csv)|*.csv|Excel XML (*.xml)|*.xml|All files (*.*)|*.*", FilterIndex = 1 };
            if (objSFD.ShowDialog() == true)
            {
                string strFormat = objSFD.FileName;
                FileInfo fi = new FileInfo(strFormat);
                if (!fi.Directory.Exists)
                {
                    fi.Directory.Create();
                }
                FileStream fs = new FileStream(strFormat, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                //StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default);
                StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
                string data = "";
                //写出列名称
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    data += dt.Columns[i].ColumnName.ToString();
                    if (i < dt.Columns.Count - 1)
                    {
                        data += ",";
                    }
                }
                sw.WriteLine(data);
                //写出各行数据
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    data = "";
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        string str = dt.Rows[i][j].ToString();
                        str = str.Replace("\"", "\"\"");//替换英文冒号 英文冒号需要换成两个冒号
                        if (str.Contains(',') || str.Contains('"')
|| str.Contains('\r') || str.Contains('\n')) //含逗号 冒号 换行符的需要放到引号中
                        {
                            str = string.Format("\"{0}\"", str);
                        }

                        data += str;
                        if (j < dt.Columns.Count - 1)
                        {
                            data += ",";
                        }
                    }
                    sw.WriteLine(data);
                }
                sw.Close();
                fs.Close();
            }

        }

        #region wpf客户端 导出DataGrid数据到Excel

        /// <summary>
        /// CSV格式化
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>格式化数据</returns>
        private static string FormatCsvField(string data)
        {
            return String.Format("\"{0}\"", data.Replace("\"", "\"\"\"").Replace("\n", "").Replace("\r", ""));
        }



        /// <summary>
        /// 导出DataGrid数据到Excel
        /// </summary>
        /// <param name="withHeaders">是否需要表头</param>
        /// <param name="grid">DataGrid</param>
        /// <param name="dataBind"></param>
        /// <returns>Excel内容字符串</returns>
        public static string ExportDataGrid(bool withHeaders, System.Windows.Controls.DataGrid grid, bool dataBind)
        {
            try
            {
                var strBuilder = new System.Text.StringBuilder();
                var source = (grid.ItemsSource as System.Collections.IList);
                if (source == null) return "";
                var headers = new List<string>();
                List<string> bt = new List<string>();

                foreach (var hr in grid.Columns)
                {
                    // DataGridTextColumn textcol = hr. as DataGridTextColumn;
                    headers.Add(hr.Header.ToString());
                    if (hr is DataGridTextColumn)//列绑定数据
                    {
                        DataGridTextColumn textcol = hr as DataGridTextColumn;
                        if (textcol != null)
                            bt.Add((textcol.Binding as Binding).Path.Path.ToString()); //获取绑定源

                    }
                    else if (hr is DataGridTemplateColumn)
                    {
                        if (hr.Header.Equals("操作"))
                            bt.Add("Id");
                    }
                    else
                    {

                    }
                }
                strBuilder.Append(String.Join(",", headers.ToArray())).Append("\r\n");
                foreach (var data in source)
                {
                    var csvRow = new List<string>();
                    foreach (var ab in bt)
                    {
                        string s = ReflectionUtil.GetProperty(data, ab).ToString();
                        if (s != null)
                        {
                            csvRow.Add(FormatCsvField(s));
                        }
                        else
                        {
                            csvRow.Add("\t");
                        }
                    }
                    strBuilder.Append(String.Join(",", csvRow.ToArray())).Append("\r\n");
                    // strBuilder.Append(String.Join(",", csvRow.ToArray())).Append("\t");
                }
                return strBuilder.ToString();
            }
            catch (Exception ex)
            {
                // LogHelper.Error(ex);
                return "";
            }
        }
        public class ReflectionUtil
        {
            public static object GetProperty(object obj, string propertyName)
            {
                PropertyInfo info = obj.GetType().GetProperty(propertyName);
                if (info == null && propertyName.Split('.').Count() > 0)
                {
                    object o = ReflectionUtil.GetProperty(obj, propertyName.Split('.')[0]);
                    int index = propertyName.IndexOf('.');
                    string end = propertyName.Substring(index + 1, propertyName.Length - index - 1);
                    return ReflectionUtil.GetProperty(o, end);
                }
                object result = null;
                try
                {
                    result = info.GetValue(obj, null);
                }
                catch (TargetException)
                {
                    return "";
                }
                return result == null ? "" : result;
            }
        }
        /// <summary>
        /// 导出DataGrid数据到Excel为CVS文件
        /// 使用utf8编码 中文是乱码 改用Unicode编码
        ///
        /// </summary>
        /// <param name="withHeaders">是否带列头</param>
        /// <param name="grid">DataGrid</param>
        public static void ExportDataGridSaveAs(bool withHeaders, System.Windows.Controls.DataGrid grid)
        {
            try
            {
                string data = ExportDataGrid(true, grid, true);
                var sfd = new Microsoft.Win32.SaveFileDialog
                {
                    DefaultExt = "csv",
                    Filter = "CSV Files (*.csv)|*.csv|All files (*.*)|*.*",
                    FilterIndex = 1
                };
                if (sfd.ShowDialog() == true)
                {
                    using (Stream stream = sfd.OpenFile())
                    {
                        using (var writer = new StreamWriter(stream, System.Text.Encoding.Unicode))
                        {
                            data = data.Replace(",", "\t");
                            writer.Write(data);
                            writer.Close();
                        }
                        stream.Close();
                    }
                }
                MessageBox.Show("导出成功！");
            }
            catch (Exception ex)
            {
                // LogHelper.Error(ex);
            }
        }

        #endregion 导出DataGrid数据到Excel
    }
}
