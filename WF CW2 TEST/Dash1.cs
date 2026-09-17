using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using WF_CW2_TEST.Infrastructure;

namespace WF_CW2_TEST
{
    public partial class Dash1 : Form
    {
        public Dash1()
        {
            InitializeComponent();
            lbltime.Text = DateTime.Now.ToLongTimeString();
            LoadDisplayName();
        }

        private void LoadDisplayName()
        {
            try
            {
                using (SqlConnection connection = Database.OpenConnection())
                using (var command = new SqlCommand(
                    "SELECT Name FROM Registration WHERE Id = @Id", connection))
                {
                    command.Parameters.Add("@Id", SqlDbType.VarChar, 10).Value = Signin1.signinID;
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        txtnameshow.Text = "Welcome, " + Convert.ToString(result);
                    }
                }
            }
            catch (Exception ex)
            {
                txtnameshow.Text = "Welcome";
                MessageBox.Show("Profile details could not be loaded. " + ex.Message,
                    "Dashboard", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void pbclose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Dash1_Load(object sender, EventArgs e)
        {
            panel7.Height = button1.Height;
            panel7.Top = button1.Top;
            ShowSection(news11);
        }

        private void button3_Click(object sender, EventArgs e) { ShowSection(calander11); }
        private void button4_Click(object sender, EventArgs e) { ShowSection(news11); }

        private void button6_Click(object sender, EventArgs e)
        {
            panel7.Height = button6.Height;
            panel7.Top = button6.Top;
            ShowSection(library11);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            panel7.Height = button1.Height;
            panel7.Top = button1.Top;
            ShowSection(news11);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            panel7.Height = button7.Height;
            panel7.Top = button7.Top;
            ShowSection(pastpapers11);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            panel7.Height = button8.Height;
            panel7.Top = button8.Top;
            ShowSection(attendence11);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            panel7.Height = button2.Height;
            panel7.Top = button2.Top;

            if (MessageBox.Show("Do you want to sign out?", "User Sign Out",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                Signin1.signinID = null;
                Hide();
                new home().Show();
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            panel7.Height = button9.Height;
            panel7.Top = button9.Top;
            ShowSection(courses11);
        }

        private void ShowSection(Control active)
        {
            calander11.Hide();
            library11.Hide();
            attendence11.Hide();
            pastpapers11.Hide();
            courses11.Hide();
            news11.Hide();
            active.Show();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lbltime.Text = DateTime.Now.ToLongTimeString();
        }

        private void courses11_Load(object sender, EventArgs e)
        {
        }
    }
}
