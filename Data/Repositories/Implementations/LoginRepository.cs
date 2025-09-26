using CLRIQTR_EMP.Models;
using MySql.Data.MySqlClient;
using System.Configuration;

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
    }
}
