using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using WF_CW2_TEST.Infrastructure;
using WF_CW2_TEST.Services;

namespace WF_CW2_TEST
{
    public partial class Resetpass1 : Form
    {
        public static string resetemail;
        public static string randomcode1;

        public Resetpass1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Hide();
            new Signin1().Show();
        }

        private void Resetpass1_Load(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = Database.OpenConnection())
                using (var command = new SqlCommand(
                    "SELECT Email FROM Registration WHERE Id = @Id", connection))
                {
                    command.Parameters.Add("@Id", SqlDbType.VarChar, 10).Value = Signin1.SigninForget;
                    object result = command.ExecuteScalar();
                    resetemail = result == null || result == DBNull.Value ? null : Convert.ToString(result);
                }

                if (string.IsNullOrWhiteSpace(resetemail))
                {
                    throw new InvalidOperationException("No email address is registered for this account.");
                }

                randomcode1 = EmailService.GenerateVerificationCode();
                EmailService.SendVerificationCode(resetemail, randomcode1);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Password reset email could not be sent. " + ex.Message,
                    "Reset Password", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
                new Signin1().Show();
            }
        }

        private void btnsignin_Click(object sender, EventArgs e)
        {
            if (string.Equals(randomcode1, txtcode.Text.Trim(), StringComparison.Ordinal))
            {
                Hide();
                new Resetpass2().ShowDialog();
                return;
            }

            MessageBox.Show("Please enter the valid code.", "Warning",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void pbclose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
