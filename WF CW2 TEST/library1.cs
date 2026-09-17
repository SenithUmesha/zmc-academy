using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using WF_CW2_TEST.Infrastructure;

namespace WF_CW2_TEST
{
    public partial class library1 : UserControl
    {
        public static string B_id;
        public static string Return_date;

        public library1()
        {
            InitializeComponent();
        }

        private void btnaddtolist_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(B_id))
            {
                MessageBox.Show("Select a book first.", "Library",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DateTime returnDate = dateTimePicker2.Value.Date;
            if (returnDate < DateTime.Today)
            {
                MessageBox.Show("Return date cannot be in the past.", "Library",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using (SqlConnection connection = Database.OpenConnection())
                using (var command = new SqlCommand(
                    "INSERT INTO Booklist(B_addeddate,B_returndate,B_id,Id) VALUES(@Added,@Return,@BookId,@StudentId)", connection))
                {
                    command.Parameters.Add("@Added", SqlDbType.Date).Value = DateTime.Today;
                    command.Parameters.Add("@Return", SqlDbType.Date).Value = returnDate;
                    command.Parameters.Add("@BookId", SqlDbType.VarChar, 10).Value = B_id;
                    command.Parameters.Add("@StudentId", SqlDbType.VarChar, 10).Value = Signin1.signinID;
                    command.ExecuteNonQuery();
                }

                Return_date = returnDate.ToString("yyyy-MM-dd");
                MessageBox.Show("Your book has been added to the list.", "Library",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("The book could not be added. " + ex.Message,
                    "Library", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void library1_Load(object sender, EventArgs e)
        {
            panel1.Visible = false;
            txtaddeddate.Text = DateTime.Now.ToLongDateString();
            LoadBooks(null);
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dataGridView1.Rows.Count)
            {
                return;
            }

            object value = dataGridView1.Rows[e.RowIndex].Cells[0].Value;
            if (value == null)
            {
                return;
            }

            B_id = Convert.ToString(value);
            LoadSelectedBook(B_id);
        }

        private void txtsearchbar_TextChanged(object sender, EventArgs e)
        {
            LoadBooks(txtsearchbar.Text.Trim());
        }

        private void LoadBooks(string prefix)
        {
            try
            {
                using (SqlConnection connection = Database.OpenConnection())
                using (var command = new SqlCommand(
                    string.IsNullOrWhiteSpace(prefix)
                        ? "SELECT B_id, B_name, B_author FROM Books ORDER BY B_name"
                        : "SELECT B_id, B_name, B_author FROM Books WHERE B_name LIKE @Prefix ORDER BY B_name",
                    connection))
                using (var adapter = new SqlDataAdapter(command))
                {
                    if (!string.IsNullOrWhiteSpace(prefix))
                    {
                        command.Parameters.Add("@Prefix", SqlDbType.VarChar, 30).Value = prefix + "%";
                    }

                    var table = new DataTable();
                    adapter.Fill(table);
                    dataGridView1.DataSource = table;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Books could not be loaded. " + ex.Message,
                    "Library", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSelectedBook(string bookId)
        {
            try
            {
                using (SqlConnection connection = Database.OpenConnection())
                using (var command = new SqlCommand(
                    "SELECT B_id, B_name, B_author FROM Books WHERE B_id = @BookId", connection))
                {
                    command.Parameters.Add("@BookId", SqlDbType.VarChar, 10).Value = bookId;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            panel1.Visible = false;
                            return;
                        }

                        txtbid.Text = Convert.ToString(reader["B_id"]);
                        txtbname.Text = Convert.ToString(reader["B_name"]);
                        txtbauthor.Text = Convert.ToString(reader["B_author"]);
                        panel1.Visible = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Book details could not be loaded. " + ex.Message,
                    "Library", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnrefresh_Click(object sender, EventArgs e)
        {
            txtsearchbar.Clear();
            B_id = null;
            panel1.Visible = false;
            LoadBooks(null);
        }
    }
}
