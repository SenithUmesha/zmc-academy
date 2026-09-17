using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using WF_CW2_TEST.Infrastructure;

namespace WF_CW2_TEST
{
    public partial class Signup1 : Form
    {
        public static string name;
        public static string id;
        public static string address;
        public static string password;
        public static string phonenum;
        public static string email;
        public static string dob;

        public Signup1()
        {
            InitializeComponent();
        }

        private void pbclose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnaboutus_Click(object sender, EventArgs e)
        {
            Close();
            new home().Show();
        }

        private void btnnext_Click(object sender, EventArgs e)
        {
            if (!ValidateChildren(ValidationConstraints.Enabled))
            {
                return;
            }

            name = txtname.Text.Trim();
            id = txtid.Text.Trim();
            address = txtaddress.Text.Trim();
            password = txtpass.Text;
            phonenum = txtcn.Text.Trim();
            email = txtemail.Text.Trim();
            dob = dateTimePicker1.Value.ToString("yyyy-MM-dd");

            try
            {
                using (SqlConnection connection = Database.OpenConnection())
                using (var command = new SqlCommand(
                    "SELECT COUNT(1) FROM Registration WHERE Id = @Id OR Email = @Email", connection))
                {
                    command.Parameters.Add("@Id", SqlDbType.VarChar, 10).Value = id;
                    command.Parameters.Add("@Email", SqlDbType.VarChar, 254).Value = email;

                    int matches = Convert.ToInt32(command.ExecuteScalar());
                    if (matches > 0)
                    {
                        MessageBox.Show("This ID or email has already been registered.", "Warning",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                Hide();
                new Signup2().ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not check registration details. " + ex.Message,
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtname_Validating(object sender, CancelEventArgs e)
        {
            ValidateRequired(txtname, "Please Enter Your Name", e);
        }

        private void txtid_Validating(object sender, CancelEventArgs e)
        {
            ValidateRequired(txtid, "Please Enter Your ID", e);
        }

        private void txtaddress_Validating(object sender, CancelEventArgs e)
        {
            ValidateRequired(txtaddress, "Please Enter Your Address", e);
        }

        private void txtcn_Validating(object sender, CancelEventArgs e)
        {
            ValidateRequired(txtcn, "Please Enter Your Contact Number", e);
        }

        private void txtschool_Validating(object sender, CancelEventArgs e)
        {
            ValidateRequired(txtemail, "Please Enter Your Email", e);
        }

        private void txtpass_Validating(object sender, CancelEventArgs e)
        {
            ValidateRequired(txtpass, "Please Enter Your Password", e);
        }

        private void ValidateRequired(Control control, string message, CancelEventArgs e)
        {
            bool missing = string.IsNullOrWhiteSpace(control.Text);
            e.Cancel = missing;
            errorProvider1.SetError(control, missing ? message : null);
            if (missing) control.Focus();
        }

        private void txtcn_KeyPress(object sender, KeyPressEventArgs e)
        {
            char chr = e.KeyChar;
            if (!char.IsDigit(chr) && chr != 8 && chr != '+')
            {
                e.Handled = true;
                MessageBox.Show("Please Enter A Valid Value", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Signup1_Load(object sender, EventArgs e)
        {
        }
    }
}
