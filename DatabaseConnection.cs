using MySql.Data.MySqlClient;
using System;
using System.Data;

namespace LibrarySystem.Database
{
    /// <summary>
    /// Public class for MySQL database connection management.
    /// Provides centralized connection handling for the Library Information System.
    /// </summary>
    public class DatabaseConnection
    {
        // ==========================================
        // CONNECTION SETTINGS — update to match your MySQL server
        // ==========================================
        private static readonly string Server   = "localhost";
        private static readonly string Database = "library_system";
        private static readonly string Username = "root";
        private static readonly string Password = "";          // change if needed
        private static readonly uint   Port     = 3306;

        private static readonly string ConnectionString =
            $"Server={Server};Port={Port};Database={Database};" +
            $"Uid={Username};Pwd={Password};CharSet=utf8;";

        // Singleton
        private static DatabaseConnection? _instance;
        public static DatabaseConnection Instance =>
            _instance ??= new DatabaseConnection();

        private DatabaseConnection() { }

        public MySqlConnection GetConnection()
        {
            var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        public bool TestConnection()
        {
            try
            {
                using var conn = GetConnection();
                return conn.State == ConnectionState.Open;
            }
            catch { return false; }
        }

        public int ExecuteNonQuery(string sql, params MySqlParameter[] parameters)
        {
            using var conn = GetConnection();
            using var cmd  = new MySqlCommand(sql, conn);
            cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteNonQuery();
        }

        public object? ExecuteScalar(string sql, params MySqlParameter[] parameters)
        {
            using var conn = GetConnection();
            using var cmd  = new MySqlCommand(sql, conn);
            cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteScalar();
        }

        public MySqlDataReader ExecuteReader(MySqlConnection conn,
                                             string sql,
                                             params MySqlParameter[] parameters)
        {
            var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteReader();
        }

        public DataTable FillDataTable(string sql, params MySqlParameter[] parameters)
        {
            using var conn    = GetConnection();
            using var cmd     = new MySqlCommand(sql, conn);
            cmd.Parameters.AddRange(parameters);
            using var adapter = new MySqlDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }
    }
}
