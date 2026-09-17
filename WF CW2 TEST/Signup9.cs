using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using WF_CW2_TEST.Infrastructure;
using WF_CW2_TEST.Security;
using WF_CW2_TEST.Services;

namespace WF_CW2_TEST
{
    public partial class Signup9 : Form
    {
        public static string randomcode2;

        public Signup9()
        {
            InitializeComponent();
        }

        private void pbclose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Hide();
            new Signup7().Show();
        }

        private void btnsignin_Click(object sender, EventArgs e)
        {
            if (!string.Equals(randomcode2, txtverificode.Text.Trim(), StringComparison.Ordinal))
            {
                MessageBox.Show("Please enter the valid code.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (SqlConnection connection = Database.OpenConnection())
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        Execute(connection, transaction,
                            "INSERT INTO Registration(Name,Id,Password,Address,Contact_number,Birth_of_date,Email) " +
                            "VALUES(@Name,@Id,@Password,@Address,@Contact,@BirthDate,@Email)",
                            new SqlParameter("@Name", Signup1.name),
                            new SqlParameter("@Id", Signup1.id),
                            new SqlParameter("@Password", PasswordHasher.Hash(Signup1.password)),
                            new SqlParameter("@Address", Signup1.address),
                            new SqlParameter("@Contact", Signup1.phonenum),
                            new SqlParameter("@BirthDate", DateTime.Parse(Signup1.dob)),
                            new SqlParameter("@Email", Signup1.email));

                        Execute(connection, transaction,
                            "INSERT INTO Ol_Results(Year,Mathematics,Science,Sinhala,English,History,Religion,Bucket_1,Bucket_2,Bucket_3,Id) " +
                            "VALUES(@Year,@Mathematics,@Science,@Sinhala,@English,@History,@Religion,@Bucket1,@Bucket2,@Bucket3,@Id)",
                            new SqlParameter("@Year", Signup2.OLyear),
                            new SqlParameter("@Mathematics", Signup2.maths),
                            new SqlParameter("@Science", Signup2.science),
                            new SqlParameter("@Sinhala", Signup2.sinhala),
                            new SqlParameter("@English", Signup2.OLenglish),
                            new SqlParameter("@History", Signup2.history),
                            new SqlParameter("@Religion", Signup2.religion),
                            new SqlParameter("@Bucket1", Signup2.OLbucket1),
                            new SqlParameter("@Bucket2", Signup2.OLbucket2),
                            new SqlParameter("@Bucket3", Signup2.OLbucket3),
                            new SqlParameter("@Id", Signup1.id));

                        Execute(connection, transaction,
                            "INSERT INTO Al_Results(Year,Stream,Bucket_1,Bucket_2,Bucket_3,English,Id) " +
                            "VALUES(@Year,@Stream,@Bucket1,@Bucket2,@Bucket3,@English,@Id)",
                            new SqlParameter("@Year", Signup3.ALyear),
                            new SqlParameter("@Stream", Signup3.ALstream),
                            new SqlParameter("@Bucket1", Signup3.ALbucket1),
                            new SqlParameter("@Bucket2", Signup3.ALbucket2),
                            new SqlParameter("@Bucket3", Signup3.ALbucket3),
                            new SqlParameter("@English", Signup3.ALenglish),
                            new SqlParameter("@Id", Signup1.id));

                        Execute(connection, transaction,
                            "INSERT INTO Other_Qualifications(Category,Name,Reason,Year,Id) VALUES(@Category,@Name,@Reason,@Year,@Id)",
                            new SqlParameter("@Category", Signup4.OQcategory1),
                            new SqlParameter("@Name", Signup4.OQname1),
                            new SqlParameter("@Reason", Signup4.OQreason1),
                            new SqlParameter("@Year", Signup4.OQyear1),
                            new SqlParameter("@Id", Signup1.id));

                        Execute(connection, transaction,
                            "INSERT INTO Other_Qualifications1(Category,Name,Reason,Year,Id) VALUES(@Category,@Name,@Reason,@Year,@Id)",
                            new SqlParameter("@Category", Signup5.OQcategory2),
                            new SqlParameter("@Name", Signup5.OQname2),
                            new SqlParameter("@Reason", Signup5.OQreason2),
                            new SqlParameter("@Year", Signup5.OQyear2),
                            new SqlParameter("@Id", Signup1.id));

                        Execute(connection, transaction,
                            "INSERT INTO Course(Course_school,Course_name,Id) VALUES(@School,@Course,@Id)",
                            new SqlParameter("@School", Signup6.Courseschool),
                            new SqlParameter("@Course", Signup6.Coursecourse),
                            new SqlParameter("@Id", Signup1.id));

                        string last4 = Signup7.cardno.Length <= 4
                            ? Signup7.cardno
                            : Signup7.cardno.Substring(Signup7.cardno.Length - 4);

                        Execute(connection, transaction,
                            "INSERT INTO Payment(Method,Type,Card_last4,Expire_date,Id) VALUES(@Method,@Type,@Last4,@ExpireDate,@Id)",
                            new SqlParameter("@Method", Signup7.method),
                            new SqlParameter("@Type", Signup7.type),
                            new SqlParameter("@Last4", last4),
                            new SqlParameter("@ExpireDate", Signup7.expiredate),
                            new SqlParameter("@Id", Signup1.id));

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }

                // Card verification values are only needed for the signup screen; never persist the CVC.
                Signup7.cardno = null;
                Signup7.cvc = null;

                Hide();
                new Signup8().ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not create the account. " + ex.Message,
                    "Registration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void Execute(
            SqlConnection connection,
            SqlTransaction transaction,
            string sql,
            params SqlParameter[] parameters)
        {
            using (var command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.AddRange(parameters);
                command.ExecuteNonQuery();
            }
        }

        private void Signup9_Load(object sender, EventArgs e)
        {
            randomcode2 = EmailService.GenerateVerificationCode();

            try
            {
                EmailService.SendVerificationCode(Signup1.email, randomcode2);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Verification email could not be sent. Configure SMTP with the ZMC_SMTP_* environment variables.\n\n" + ex.Message,
                    "Email Configuration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
