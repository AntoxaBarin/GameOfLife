using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace GameOfLife.DB;

public class Database
{
    public SQLiteConnection connection;

    public Database(string dbPath)
    {
        connection = new SQLiteConnection("Data Source=" + dbPath);
    }

    public void Open()
    {
        connection.Open();
    }

    public void Close()
    {
        connection.Close();
    }

    public void Create()
    {
        string commandText = "CREATE TABLE IF NOT EXISTS fields (id INTEGER PRIMARY KEY, field TEXT)";
        SQLiteCommand command = new SQLiteCommand(commandText, connection);
        command.ExecuteNonQuery();
    }

    public void AddField(string field)
    {
        string commandText = "INSERT INTO fields (field) VALUES (@field)";
        SQLiteCommand command = new SQLiteCommand(commandText, connection);
        command.Parameters.AddWithValue("@field", field);
        command.ExecuteNonQuery();
    }

    public async Task<string?> GetField(int ID_)
    {
        List<string> fields = new List<string>();
        string commandText = $"SELECT field FROM fields WHERE ID=={ID_}";
        SQLiteCommand command = new SQLiteCommand(commandText, connection);
        SQLiteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            string field = reader["field"].ToString();
            if (field == "")
            {
                return null;
            }
            fields.Add(field);
        }
        
        return fields[0];
    }
}