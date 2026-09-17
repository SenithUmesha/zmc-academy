using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using WF_CW2_TEST.Infrastructure;
using WF_CW2_TEST.Security;

namespace WF_CW2_TEST
{
    public partial class Resetpass2 : Form
    {
        public Resetpass2()
        {
            InitializeComponent();
        }

        private void btnsignin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtnewpass.Text))
            {
                MessageBox.Show("Please enter a new password.", "Reset Password",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (txtnewpass.Text != txtconfirmnewpass.Text)
            {
                MessageBox.Show("Passwords don't match.", "Reset Password",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (SqlConnection connection = Database.OpenConnection())
                using (var command = new SqlCommand(
                    "UPDATE Registration SET Password = @Password WHERE Id = @Id", connection))
                {
                    command.Parameters.Add("@Password", SqlDbType.VarChar, 255)
                        .Value = PasswordHasher.Hash(txtconfirmnewpass.Text);
                    command.Parameters.Add("@Id", SqlDbType.VarChar, 10)
                        .Value = Signin1.SigninForget;

                    if (command.ExecuteNonQuery() != 1)
                    {
                        throw new InvalidOperationException("The account could not be found.");
                    }
                }

                MessageBox.Show("Password reset successfully.", "Reset Password",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Hide();
                new Signin1().ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Password reset failed. " + ex.Message,
                    "Reset Password", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pbclose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
