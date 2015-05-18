using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Threading;

namespace DLPointParser
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Windows_Loaded(object sender, RoutedEventArgs e)
        {
            tbTarget.Text = "adf.ly/\ndepositfiles.com/files/";
        }

        private void GO_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder builder = new StringBuilder();
            string[] targets = tbTarget.Text.Split(new char[] { '\n' });
            string[] exces = tbException.Text.Split(new char[] { '\n' });
            string url = tbURL.Text;

            Thread thread = new Thread(ThreadWork);
            this.Title = "Parsing";
            thread.Start(new object[] { url, targets, exces, builder });

            tbResult.Text = builder.ToString();
        }

        void ThreadWork(object parameter)
        {
            object[] resource = (object[])parameter;
            string url = (string)resource[0];
            string[] targets = (string[])resource[1];
            string[] exces = (string[])resource[2];
            StringBuilder builder = (StringBuilder)resource[3];
            int urlCount = 0;

            string result = null;
            if (url.IndexOf(@"{0}") != -1)
            {
                int index = 1;
                for (; ; index++)
                {
                    string currentUrl = String.Format(url, index);
                    result = GetWebSourceString(currentUrl);
                    if (result.Length > 0)
                    {
                        int getCount = FindTargetURL(builder, result, targets, exces);
                        urlCount += getCount;
                        tbResult.Dispatcher.Invoke(new Action(() =>
                        {
                            tbResult.Text = builder.ToString();
                        }));
                        labelProgress.Dispatcher.Invoke(new Action(() =>
                        {
                            labelProgress.Text = urlCount.ToString();
                        }));
                    }
                    else { break; }
                }
            }
            else
            {
                result = GetWebSourceString(url);
                if (result.Length > 0)
                {
                    int getCount = FindTargetURL(builder, result, targets, exces);
                    urlCount += getCount;
                    tbResult.Dispatcher.Invoke(new Action(() =>
                    {
                        tbResult.Text = builder.ToString();
                    }));
                }
            }
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.Title = "step by";
            }));
        }

        string GetWebSourceString(string url)
        {
            WebClient client = new WebClient();
            Stream stream;
            try
            {
                stream = client.OpenRead(url);
            }
            catch (WebException ex)
            {
                return String.Empty;
            }
            StreamReader reader = new StreamReader(stream);
            string result = reader.ReadToEnd();
            client.Dispose();
            stream.Dispose();
            reader.Dispose();
            return result;
        }

        int FindTargetURL(StringBuilder colleResult, string content, string[] targets, string[] exces)
        {
            int getCount = 0;
            foreach (string target in targets)
            {
                int findIndex = content.IndexOf(target);
                while (findIndex != -1)
                {
                    int head = 0, tail = 0;
                    for (int i = findIndex; i >= 0; i--)
                    {
                        if (content[i] == '"')
                        {
                            head = i + 1;
                            break;
                        }
                    }
                    for (int i = findIndex; i < content.Length; i++)
                    {
                        if (content[i] == '"')
                        {
                            tail = i;
                            break;
                        }
                    }
                    string result = content.Substring(head, tail - head);
                    bool isElse = false;
                    foreach (string ex in exces)
                    {
                        if (ex == result)
                        { isElse = true; break; }
                    }
                    if (!isElse)
                    {
                        colleResult.AppendLine(result);
                        getCount++;
                    }
                    findIndex = content.IndexOf(target, tail);
                }
            }
            return getCount;
        }
    }
}
