using CLRIQTR_EMP.Models;
using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using NLog;
using Google.Protobuf.Collections;

namespace CLRIQTR_EMP.Data.Repositories.Implementations
{
    public class LoginRepository
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MySqlConn"].ConnectionString;

        public EmpLogin GetUser(string username, string password)
        {
            try
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
                                logger.Debug("GetUser: found user for EmpNo {0}", username);
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

                logger.Debug("GetUser: no user found for EmpNo {0}", username);
                return null;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "GetUser failed for EmpNo {0}", username);
                throw;
            }
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
                logger.Warn("Password recovery failed – no email for EmpNo {0}", employeeNumber);
                return "Employee number not found or email address is not available.";
            }

            if (string.IsNullOrEmpty(password))
            {
                logger.Warn("Password recovery failed – no password for EmpNo {0}", employeeNumber);
                return "Password not found for the given employee number.";
            }

            logger.Info("Sending password recovery email to {0} for EmpNo {1}", email, employeeNumber);


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

                    logger.Info("Password recovery email sent successfully for EmpNo {0}", employeeNumber);
                    return "A recovery email has been sent to your address.";
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error while sending recovery email for EmpNo {0}", employeeNumber);
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
                            logger.Info("Email fetched for EmpNo {0}: {1}", employeeNumber, email);
                        }
                        else
                        {
                            logger.Warn("No email found in empmast for EmpNo {0}", employeeNumber);
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
                            logger.Info("Password fetched for EmpNo {0}", employeeNumber);
                        }
                        else
                        {
                            logger.Warn("No password found in emplogin for EmpNo {0}", employeeNumber);
                        }
                    }
                }
            }
            return password;
        }

        // Inside your LoginRepository.cs class

        // (You'll need a using statement for MySql.Data.MySqlClient)
        // (I'm assuming you have a _connectionString field/property in this class)

        // Inside your LoginRepository.cs class
        // (Make sure you have 'using MySql.Data.MySqlClient;')

        public bool UserExists(string empno)
        {
            // Assuming your connection string is _connectionString
            // and your table is emp_login
            string sql = "SELECT COUNT(1) FROM emplogin WHERE empno = @empno";

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@empno", empno);
                        connection.Open();
                        int count = Convert.ToInt32(command.ExecuteScalar());
                        bool exists = count > 0;

                        logger.Info("UserExists check for EmpNo: {0} | Exists: {1}", empno, exists);

                        return exists;

                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error in UserExists() for EmpNo: {0}", empno);
                return false;
            }
        }
    }

}
