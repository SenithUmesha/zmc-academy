using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using WF_CW2_TEST.Infrastructure;

namespace WF_CW2_TEST
{
    public partial class News1 : UserControl
    {
        public News1()
        {
            InitializeComponent();
        }

        private void News1_Load(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = Database.OpenConnection())
                using (var command = new SqlCommand(
                    "SELECT News_id, News_name, News_date FROM News WHERE News_id IN ('A','B','C','D') ORDER BY News_id", connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string id = Convert.ToString(reader["News_id"]);
                        string headline = Convert.ToString(reader["News_name"]);
                        string date = Convert.ToString(reader["News_date"]);

                        switch (id)
                        {
                            case "A": txtnews1.Text = headline; txtnewsdate1.Text = date; break;
                            case "B": txtnews2.Text = headline; txtnewsdate2.Text = date; break;
                            case "C": txtnews3.Text = headline; txtnewsdate3.Text = date; break;
                            case "D": txtnews4.Text = headline; txtnewsdate4.Text = date; break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("News could not be loaded. " + ex.Message,
                    "News", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
