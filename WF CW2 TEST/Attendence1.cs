using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using WF_CW2_TEST.Infrastructure;

namespace WF_CW2_TEST
{
    public partial class Attendence1 : UserControl
    {
        public Attendence1()
        {
            InitializeComponent();
        }

        private void btnsubmit_Click(object sender, EventArgs e)
        {
            string table = comboBox1.SelectedIndex == 0 ? "Attendence1" : "Attendence2";

            try
            {
                using (SqlConnection connection = Database.OpenConnection())
                using (var command = new SqlCommand(
                    "SELECT Id, [date], Attended FROM " + table + " WHERE Id = @Id ORDER BY [date] DESC", connection))
                using (var adapter = new SqlDataAdapter(command))
                {
                    command.Parameters.Add("@Id", SqlDbType.VarChar, 10).Value = Signin1.signinID;
                    var dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    dataGridView1.DataSource = dataTable;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Attendance could not be loaded. " + ex.Message,
                    "Attendance", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
