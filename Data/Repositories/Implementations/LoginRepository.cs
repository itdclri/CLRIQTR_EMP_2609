using CLRIQTR_EMP.Models;
using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace CLRIQTR_EMP.Data.Repositories.Implementations
{
    public class LoginRepository
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MySqlConn"].ConnectionString;

        public EmpLogin GetUser(string username, string password)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                string query = "SELECT empno, pwd, lab FROM emplogin WHERE empno = @empno AND pwd = @pwd";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@empno", username);
                    cmd.Parameters.AddWithValue("@pwd", password);

                    conn.Open();

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new EmpLogin
                            {
                                empno = reader["empno"].ToString(),
                                pwd = reader["pwd"].ToString(),
                                lab = reader["lab"].ToString()
                            };
                        }
                    }
                }
            }
            return null;
        }

        public bool InsertUser(string empno, string pwd, string lab)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                string query = "INSERT INTO emplogin (empno, pwd, lab) VALUES (@empno, @pwd, @lab)";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@empno", empno);
                    cmd.Parameters.AddWithValue("@pwd", pwd);
                    cmd.Parameters.AddWithValue("@lab", lab);

                    conn.Open();

                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0;
                }
            }
        }


        public string SendPasswordRecoveryEmail(string employeeNumber)
        {
            // Fetch the email and password by employee number
            string email = GetEmailByEmployeeNumber(employeeNumber);
            string password = GetPasswordByEmployeeNumber(employeeNumber);

            if (string.IsNullOrEmpty(email))
            {
                return "Employee number not found or email address is not available.";
            }

            if (string.IsNullOrEmpty(password))
            {
                return "Password not found for the given employee number.";
            }


            string subject = "Informaation regarding CSIR-CLRI Quarters Application";
            string body = $@"
    <p>Dear Employee,</p>
    <p>We have received a request to retrieve your password. Below are your login details:</p>
    <p><strong>Employee Number:</strong> {employeeNumber}</p>
    <p><strong>Password:</strong> {password}</p>
    <p>Please use this information to log into your account. </p>
    <hr>
    <p><em>This is an automated message, please do not reply.</em></p>
    <p>Thank you,</p>
    <p>CLRI Quarters Admin</p>";


            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Qtrs", "webadmin@clri.org"));
            message.To.Add(new MailboxAddress("User", email));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body
            };
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                try
                {
                    client.Connect("netsol-smtp-oxcs.hostingplatform.com", 465, MailKit.Security.SecureSocketOptions.SslOnConnect);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.Authenticate("webadmin@clri.org", "ITDpro2@10956");

                    client.Send(message);
                    client.Disconnect(true);

                    return "A recovery email has been sent to your address.";
                }
                catch (Exception ex)
                {
                    // Log the exception if needed
                    return "An error occurred while sending the recovery email.";
                }
            }
        }


        public string GetEmailByEmployeeNumber(string employeeNumber)
        {
            string email = null;
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                string query = "SELECT email FROM empmast WHERE empno = @EmpNo";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@EmpNo", employeeNumber);
                    conn.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            email = reader["email"].ToString();
                        }
                    }
                }
            }
            return email;
        }

        public string GetPasswordByEmployeeNumber(string employeeNumber)
        {
            string password = null;
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                string query = "SELECT pwd FROM emplogin WHERE empno = @EmpNo";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@EmpNo", employeeNumber);
                    conn.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            password = reader["pwd"].ToString();
                        }
                    }
                }
            }
            return password;
        }
    }

}
