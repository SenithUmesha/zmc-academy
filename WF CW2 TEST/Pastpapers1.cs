using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using WF_CW2_TEST.Infrastructure;

namespace WF_CW2_TEST
{
    public partial class Pastpapers1 : UserControl
    {
        public static string P_id;
        public static string Return_date;

        public Pastpapers1()
        {
            InitializeComponent();
        }

        private void Pastpapers1_Load(object sender, EventArgs e)
        {
            panel1.Visible = false;
            txtaddeddate.Text = DateTime.Now.ToLongDateString();
            LoadPapers(null);
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dataGridView1.Rows.Count)
            {
                return;
            }

            object value = dataGridView1.Rows[e.RowIndex].Cells[0].Value;
            if (value == null) return;

            P_id = Convert.ToString(value);
            LoadSelectedPaper(P_id);
        }

        private void txtsearchbar_TextChanged(object sender, EventArgs e)
        {
            LoadPapers(txtsearchbar.Text.Trim());
        }

        private void btnrefresh_Click(object sender, EventArgs e)
        {
            txtsearchbar.Clear();
            P_id = null;
            panel1.Visible = false;
            LoadPapers(null);
        }

        private void btnaddtolist_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(P_id))
            {
                MessageBox.Show("Select a paper first.", "Past papers",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DateTime returnDate = dateTimePicker2.Value.Date;
            if (returnDate < DateTime.Today)
            {
                MessageBox.Show("Return date cannot be in the past.", "Past papers",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using (SqlConnection connection = Database.OpenConnection())
                using (var command = new SqlCommand(
                    "INSERT INTO Pastpaperlist(P_addeddate,P_returndate,P_id,Id) VALUES(@Added,@Return,@PaperId,@StudentId)", connection))
                {
                    command.Parameters.Add("@Added", SqlDbType.Date).Value = DateTime.Today;
                    command.Parameters.Add("@Return", SqlDbType.Date).Value = returnDate;
                    command.Parameters.Add("@PaperId", SqlDbType.VarChar, 10).Value = P_id;
                    command.Parameters.Add("@StudentId", SqlDbType.VarChar, 10).Value = Signin1.signinID;
                    command.ExecuteNonQuery();
                }

                Return_date = returnDate.ToString("yyyy-MM-dd");
                MessageBox.Show("Your paper has been added to the list.", "Past papers",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("The paper could not be added. " + ex.Message,
                    "Past papers", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadPapers(string prefix)
        {
            try
            {
                using (SqlConnection connection = Database.OpenConnection())
                using (var command = new SqlCommand(
                    string.IsNullOrWhiteSpace(prefix)
                        ? "SELECT P_id, Subject, Year FROM Pastpapers ORDER BY Subject, Year DESC"
                        : "SELECT P_id, Subject, Year FROM Pastpapers WHERE Subject LIKE @Prefix ORDER BY Subject, Year DESC",
                    connection))
                using (var adapter = new SqlDataAdapter(command))
                {
                    if (!string.IsNullOrWhiteSpace(prefix))
                    {
                        command.Parameters.Add("@Prefix", SqlDbType.VarChar, 20).Value = prefix + "%";
                    }

                    var table = new DataTable();
                    adapter.Fill(table);
                    dataGridView1.DataSource = table;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Past papers could not be loaded. " + ex.Message,
                    "Past papers", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSelectedPaper(string paperId)
        {
            try
            {
                using (SqlConnection connection = Database.OpenConnection())
                using (var command = new SqlCommand(
                    "SELECT P_id, Subject, Year FROM Pastpapers WHERE P_id = @PaperId", connection))
                {
                    command.Parameters.Add("@PaperId", SqlDbType.VarChar, 10).Value = paperId;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            panel1.Visible = false;
                            return;
                        }

                        txtpaperid.Text = Convert.ToString(reader["P_id"]);
                        txtsubject.Text = Convert.ToString(reader["Subject"]);
                        txtyear.Text = Convert.ToString(reader["Year"]);
                        panel1.Visible = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Paper details could not be loaded. " + ex.Message,
                    "Past papers", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
