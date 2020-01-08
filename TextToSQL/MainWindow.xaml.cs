using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace TextToSQL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Brush[] colors = new Brush[50];
        string[] param = new string[0];
        List<List<string>> items = new List<List<string>>();
        string currFileName = "";
        string selectedFile = "";
        string currDateTime = "";

        public string SqlServerName { get; }

        public MainWindow()
        {
            InitializeComponent();

            using (StreamReader sr = new StreamReader(@"\\joi\eu\Collaboration\EEPT\SQL Server Information.txt"))
            {
                string sServerInformation = sr.ReadToEnd();
                string[] sa;
                char[] delimiterChars = { '\r', '\n' };
                sa = sServerInformation.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < sa.Length; i++)
                {
                    if (sa[i].Contains("Server Name"))
                    {
                        SqlServerName = sa[i].Replace("\r", "").Replace("\n", "").Replace("Server Name = ", "").Trim();

                        if (!string.IsNullOrEmpty(SqlServerName))
                        {
                            break;
                        }
                    }
                }
            }

            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\HB Factory Programs\\TextToSql"))
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\HB Factory Programs\\TextToSql");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog() { Filter = "Text Files (*.txt) | *.txt;" };
                ofd.ShowDialog();
                if (string.IsNullOrWhiteSpace(ofd.FileName)) return;

                currDateTime = DateTime.Now.ToString("yyyyMMddHHmmssFFF");
                currFileName = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\HB Factory Programs\\TextToSql\\" + currDateTime + ".txt";
                if (!File.Exists(currFileName)) File.Create(currFileName).Close();

                lblFilePath.Content = ofd.FileName;
                rtbOutput.AppendText(">> Clearing items list... ");
                items.Clear();
                rtbOutput.AppendText(Environment.NewLine);
                System.IO.StreamReader file = new System.IO.StreamReader(ofd.FileName);
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    bool isEmpty = string.IsNullOrWhiteSpace(tbParams.Text);
                    List<string> numParams = line.Split(',').ToList();
                    if (numParams.Count == 0)
                    {
                        rtbOutput.AppendText(">> Skipping blank line...\n");
                        WriteToTxt(">> Skipping blank line...\n");
                    }
                    else
                    {
                        if (isEmpty)
                        {
                            for (int i = 1; i <= numParams.Count; i++)
                            {

                                tbParams.Text += "@param" + i;
                                if (i != numParams.Count) tbParams.Text += ",";

                            }
                            rtbOutput.AppendText(">> " + param.Length + " paramater(s) detected!\n");
                            WriteToTxt(">> " + param.Length + " paramater(s) detected!\n");
                            rtbOutput.AppendText(">> Parameters: " + tbParams.Text + "\n");
                            WriteToTxt(">> Parameters: " + tbParams.Text + "\n");
                        }
                        string message = "";
                        message += ">> Adding ";

                        foreach (string s in numParams)
                        {
                            message += s + ", ";
                        }
                        message = message.Substring(0, message.Length - 2);
                        rtbOutput.AppendText(message + "\n");
                        WriteToTxt(message + "\n");
                        items.Add(numParams);

                    }
                }
                file.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR!\n" + ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void TbParams_TextChanged(object sender, TextChangedEventArgs e)
        {
            lbParams.Items.Clear();
            param = tbParams.Text.Split(',');
            foreach (string s in param)
            {
                lbParams.Items.Add(s.Trim());
            }
        }

        public static IEnumerable<TextRange> GetAllWordRanges(FlowDocument document)
        {
            string pattern = @"[^\W\d](\w|[-']{1,2}(?=\w))*";
            TextPointer pointer = document.ContentStart;
            while (pointer != null)
            {
                if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string textRun = pointer.GetTextInRun(LogicalDirection.Forward);
                    MatchCollection matches = Regex.Matches(textRun, pattern);
                    foreach (Match match in matches)
                    {
                        int startIndex = match.Index;
                        int length = match.Length;
                        TextPointer start = pointer.GetPositionAtOffset(startIndex);
                        TextPointer end = start.GetPositionAtOffset(length);
                        yield return new TextRange(start, end);
                    }
                }

                pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
            }
        }

        private void RtbQuery_TextChanged(object sender, TextChangedEventArgs e)
        {
            //rtbQuery.IsEnabled = false;
            //IEnumerable<TextRange> wordRanges = GetAllWordRanges(rtbQuery.Document);

            //foreach (TextRange wordRange in wordRanges)
            //{
            //    for(int i = 0; i<param.Length; i++)
            //    {
            //        if (("@"+wordRange.Text.Trim()).Contains(param[i]))
            //        {
            //            wordRange.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);
            //        }
            //        else wordRange.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
            //    }
            //}
            //    rtbQuery.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
            //rtbQuery.IsEnabled = true;
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            rtbOutput.AppendText(">> Connection string: Data Source =" + SqlServerName + "; Initial Catalog = " + tbDatabase.Text + "; Persist Security Info = True; User ID = inhouse; Password = Password1\n");
            WriteToTxt(">> Connection string: Data Source =" + SqlServerName + "; Initial Catalog = " + tbDatabase.Text + "; Persist Security Info = True; User ID = inhouse; Password = Password1\n");
            for (int i = 0; i < items.Count; i++)
            {
                using (SqlConnection conn = new SqlConnection("Data Source =" + SqlServerName + "; Initial Catalog = " + tbDatabase.Text + "; Persist Security Info = True; User ID = inhouse; Password = Password1"))
                {
                    using (SqlCommand cmd = new SqlCommand(new TextRange(rtbQuery.Document.ContentStart, rtbQuery.Document.ContentEnd).Text, conn))
                    {
                        string command = new TextRange(rtbQuery.Document.ContentStart, rtbQuery.Document.ContentEnd).Text;

                        for (int j = 0; j < param.Length; j++)
                        {
                            command = command.Replace(param[j], items[i][j]);
                            cmd.Parameters.AddWithValue(param[j], items[i][j]);
                        }
                        rtbOutput.AppendText(">> Query: " + command + "\n");
                        WriteToTxt(">> Query: " + command + "\n");
                        try
                        {
                            conn.Open();
                            int num = 0;
                            num = cmd.ExecuteNonQuery();
                            rtbOutput.AppendText(">> Rows Effected: " + num + "\n");
                            WriteToTxt(">> Rows Effected: " + num + "\n");
                            conn.Close();
                        }
                        catch (Exception ex) { 
                            if (MessageBox.Show("Error! " + ex.Message + "\nWould you like to continue?", "ERROR", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.No)
                            {
                                return;
                            }
                        }
                    }
                }
            }
            if (MessageBox.Show("Would you like to move output and serial numbers to logs folder?", "Log?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                rtbOutput.AppendText(">> Completed!\n");
                WriteToTxt(">> Completed!\n");
                try
                {
                    string s = @"\\joi\eu\Public\EE Process Test\Logs\TextToSql\SerialNumbers\" + System.IO.Path.GetFileNameWithoutExtension((new FileInfo(lblFilePath.Content.ToString()).Name)) + "_" + currDateTime + ".txt";
                    File.Move(lblFilePath.Content.ToString(),s);
                    File.Move(currFileName, @"\\joi\eu\Public\EE Process Test\Logs\TextToSql\Logs\Log_" + currDateTime+".txt");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error copying files!\n" + ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                rtbOutput.AppendText(">> Completed!\n");
                WriteToTxt(">> Completed!\n");
            }
        }

        private void RtbOutput_TextChanged(object sender, TextChangedEventArgs e)
        {
            rtbOutput.ScrollToEnd();
        }

        private void WriteToTxt(string s)
        {
            using (StreamWriter outputFile = File.AppendText(currFileName))
            {
                outputFile.WriteLine(s);
            }
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            csHolder.selected = null;
            new frmLoadQuery().ShowDialog();
            if(csHolder.selected != null)
            {
                tbDatabase.Text = csHolder.selected.Database;
                rtbQuery.Document.Blocks.Clear();
                rtbQuery.AppendText(csHolder.selected.Query);
                tbParams.Text = csHolder.selected.Parameters;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            using (SqlConnection conn = new SqlConnection("Data Source =" + SqlServerName + "; Initial Catalog = EEPT; Persist Security Info = True; User ID = inhouse; Password = Password1"))
            {
                using (SqlCommand cmd = new SqlCommand("INSERT INTO TextToSql_Queries VALUES(@query, @database, @parameters)", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@query", new TextRange(rtbQuery.Document.ContentStart, rtbQuery.Document.ContentEnd).Text);
                    cmd.Parameters.AddWithValue("@database", tbDatabase.Text);
                    cmd.Parameters.AddWithValue("@parameters", tbParams.Text);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
            MessageBox.Show("Query Saved!");
        }

        private void label_Copy1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string folderPath = @"\\joi\eu\Public\EE Process Test\Logs\TextToSql\";
            if (Directory.Exists(folderPath))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    Arguments = folderPath,
                    FileName = "explorer.exe"
                };

                Process.Start(startInfo);
            }
            else
            {
                MessageBox.Show(string.Format("{0} Directory does not exist!", folderPath));
            }
        }
    }

    public class Saved
    {
        public string Query { get; set; }
        public string Database { get; set; }
        public string Parameters { get; set; }
    }
}
