using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace WF_CW2_TEST
{
    public partial class Signup7 : Form
    {
        public static string method;
        public static string type;
        public static string expiredate;
        public static string cvc;
        public static string cardno;

        public Signup7()
        {
            InitializeComponent();
        }

        private void btnback_Click(object sender, EventArgs e)
        {
            Close();
            new Signup6().Show();
        }

        private void btnnext_Click(object sender, EventArgs e)
        {
            if (!ValidateChildren(ValidationConstraints.Enabled))
            {
                return;
            }

            if (cmbmethod.SelectedItem == null || cmbtype.SelectedItem == null)
            {
                MessageBox.Show("Please select a payment method and plan.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string normalizedCard = new string(txtcardno.Text.Where(char.IsDigit).ToArray());
            string normalizedCvc = new string(txtcvc.Text.Where(char.IsDigit).ToArray());

            if (normalizedCard.Length < 12 || normalizedCard.Length > 19)
            {
                MessageBox.Show("Please enter a valid card number.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (normalizedCvc.Length < 3 || normalizedCvc.Length > 4)
            {
                MessageBox.Show("Please enter a valid CVC.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            method = cmbmethod.Text;
            type = cmbtype.Text;
            expiredate = dateTimePicker1.Value.ToString("yyyy-MM");
            cardno = normalizedCard;
            cvc = normalizedCvc;

            if (MessageBox.Show(
                    "By creating a ZMC Academy account, you are agreeing to be bound by the terms of use.",
                    "Terms of use", MessageBoxButtons.YesNo, MessageBoxIcon.Information) != DialogResult.Yes)
            {
                return;
            }

            Hide();
            new Signup9().ShowDialog();
        }

        private void pbclose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void txtcardno_Validating(object sender, CancelEventArgs e)
        {
            ValidateRequired(txtcardno, "Please Enter Your Card Number", e);
        }

        private void txtcvc_Validating(object sender, CancelEventArgs e)
        {
            ValidateRequired(txtcvc, "Please Enter Your CVC", e);
        }

        private void ValidateRequired(Control control, string message, CancelEventArgs e)
        {
            bool missing = string.IsNullOrWhiteSpace(control.Text);
            e.Cancel = missing;
            errorProvider1.SetError(control, missing ? message : null);
            if (missing) control.Focus();
        }

        private void txtcardno_KeyPress(object sender, KeyPressEventArgs e)
        {
            RestrictToDigits(e);
        }

        private void txtcvc_KeyPress(object sender, KeyPressEventArgs e)
        {
            RestrictToDigits(e);
        }

        private void RestrictToDigits(KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != 8)
            {
                e.Handled = true;
            }
        }
    }
}
