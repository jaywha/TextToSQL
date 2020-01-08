using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TextToSQL
{
    /// <summary>
    /// Interaction logic for frmLoadQuery.xaml
    /// </summary>
    public partial class frmLoadQuery : Window
    {
        public string SqlServerName { get; }

        List<Saved> saveds = new List<Saved>();

        public frmLoadQuery()
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
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgQueries.SelectedIndex != -1)
            {
                csHolder.selected = saveds[dgQueries.SelectedIndex];
                this.Close();
            }
        }

        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            using (SqlConnection conn = new SqlConnection("Data Source =" + SqlServerName + "; Initial Catalog = EEPT; Persist Security Info = True; User ID = inhouse; Password = Password1"))
            {
                using(SqlCommand cmd = new SqlCommand("SELECT * FROM TextToSql_Queries", conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            saveds.Add(new Saved()
                            {
                                Database = reader["Data_Base"].ToString(),
                                Query = reader["Query"].ToString(),
                                Parameters = reader["Parameters"].ToString()
                            });
                        }
                    }
                    conn.Close();
                }
            }
            dgQueries.ItemsSource = saveds;
        }
    }
}
