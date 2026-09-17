using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using WF_CW2_TEST.Infrastructure;

namespace WF_CW2_TEST
{
    public partial class Calander1 : UserControl
    {
        public Calander1()
        {
            InitializeComponent();
        }

        private void btnsubmit_Click(object sender, EventArgs e)
        {
            if (!ValidateChildren(ValidationConstraints.Enabled))
            {
                return;
            }

            try
            {
                using (SqlConnection connection = Database.OpenConnection())
                using (var command = new SqlCommand(
                    "INSERT INTO Report_problem(Summary,Details,[date],Id) VALUES(@Summary,@Details,@Date,@Id)", connection))
                {
                    command.Parameters.Add("@Summary", SqlDbType.VarChar, 100).Value = txtsummary.Text.Trim();
                    command.Parameters.Add("@Details", SqlDbType.VarChar, 500).Value = txtdetails.Text.Trim();
                    command.Parameters.Add("@Date", SqlDbType.DateTime).Value = DateTime.Now;
                    command.Parameters.Add("@Id", SqlDbType.VarChar, 10).Value = Signin1.signinID;
                    command.ExecuteNonQuery();
                }

                MessageBox.Show("Submitted successfully.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtsummary.Clear();
                txtdetails.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("The report could not be submitted. " + ex.Message,
                    "Support", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtsummary_Validating(object sender, CancelEventArgs e)
        {
            ValidateRequired(txtsummary, "Please Enter Your Problem Summary", e);
        }

        private void txtdetails_Validating(object sender, CancelEventArgs e)
        {
            ValidateRequired(txtdetails, "Please Enter Your Problem Details", e);
        }

        private void ValidateRequired(Control control, string message, CancelEventArgs e)
        {
            bool missing = string.IsNullOrWhiteSpace(control.Text);
            e.Cancel = missing;
            errorProvider1.SetError(control, missing ? message : null);
            if (missing) control.Focus();
        }
    }
}
