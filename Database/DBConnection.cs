using System;
using MySql.Data.MySqlClient;

namespace MotorPartsInventory.Database
{
    public class DBConnection
    {
        // Connection string
        private string connectionString = "server=localhost;port=3306;database=MotorPartsInventory;uid=root;pwd=;";

        // MySQL connection object
        private MySqlConnection conn;

        public DBConnection()
        {
            conn = new MySqlConnection(connectionString);
        }

        // Open connection
        public MySqlConnection GetConnection()
        {
            try
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                {
                    conn.Open();
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Database connection error: " + ex.Message);
            }
            return conn;
        }

        // Close connection
        public void CloseConnection()
        {
            if (conn.State == System.Data.ConnectionState.Open)
            {
                conn.Close();
            }
        }
    }
}
