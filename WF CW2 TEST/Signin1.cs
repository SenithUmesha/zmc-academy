using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using WF_CW2_TEST.Infrastructure;
using WF_CW2_TEST.Security;

namespace WF_CW2_TEST
{
    public partial class Signin1 : Form
    {
        public static string signinID;
        public static string SigninForget;

        public Signin1()
        {
            InitializeComponent();
        }

        private void txtsigninid_Enter(object sender, EventArgs e)
        {
            if (txtsigninid.Text.Equals("     ID")) txtsigninid.Text = "";
        }

        private void txtsigninid_Leave(object sender, EventArgs e)
        {
            if (txtsigninid.Text.Equals("")) txtsigninid.Text = "     ID";
        }

        private void txtsinginpassword_Enter(object sender, EventArgs e)
        {
            if (txtsinginpassword.Text.Equals("     Password")) txtsinginpassword.Text = "";
        }

        private void txtsinginpassword_Leave(object sender, EventArgs e)
        {
            if (txtsinginpassword.Text.Equals("")) txtsinginpassword.Text = "     Password";
        }

        private void pbclose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
            new home().Show();
        }

        private void btnsignin_Click(object sender, EventArgs e)
        {
            string id = txtsigninid.Text.Trim();
            string password = txtsinginpassword.Text;

            if (string.IsNullOrWhiteSpace(id) || id == "ID" ||
                string.IsNullOrWhiteSpace(password) || password.Trim() == "Password")
            {
                MessageBox.Show("Please enter your ID and password.", "User Login",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                string storedPassword = null;

                using (SqlConnection connection = Database.OpenConnection())
                using (var command = new SqlCommand(
                    "SELECT Password FROM Registration WHERE Id = @Id", connection))
                {
                    command.Parameters.Add("@Id", SqlDbType.VarChar, 10).Value = id;
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        storedPassword = Convert.ToString(result);
                    }
                }

                if (!PasswordHasher.Verify(password, storedPassword))
                {
                    MessageBox.Show("Either your ID or password is incorrect.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                signinID = id;
                MessageBox.Show("Login successful.", "User Login",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Hide();
                new Dash1().ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not sign in. " + ex.Message, "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnforget_Click(object sender, EventArgs e)
        {
            string id = txtsigninid.Text.Trim();
            if (string.IsNullOrWhiteSpace(id) || id == "ID")
            {
                MessageBox.Show("Please enter your ID.", "Reset Password",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using (SqlConnection connection = Database.OpenConnection())
                using (var command = new SqlCommand(
                    "SELECT COUNT(1) FROM Registration WHERE Id = @Id", connection))
                {
                    command.Parameters.Add("@Id", SqlDbType.VarChar, 10).Value = id;
                    bool exists = Convert.ToInt32(command.ExecuteScalar()) > 0;
                    if (!exists)
                    {
                        MessageBox.Show("Please enter a valid ID.", "Reset Password",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }

                SigninForget = id;
                Hide();
                new Resetpass1().ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not start password reset. " + ex.Message,
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
