using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace ToDoItems.Models
{
    public class ToDoItemsManager
    {
        private string _connectionString;

        public ToDoItemsManager(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<ToDoItem> GetIncompletedItems()
        {
            return GetInternal(false);
        }

        public List<ToDoItem> GetCompleted()
        {
            return GetInternal(true);
        }

        private List<ToDoItem> GetInternal(bool completed)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = @"SELECT tdi.*, c.Name FROM Categories c
                                    JOIN ToDoItems tdi
                                    ON c.Id = tdi.CategoryId
                                    WHERE tdi.CompletedDate IS ";
            if (completed)
            {
                cmd.CommandText += "NOT ";
            }

            cmd.CommandText += "NULL";
            connection.Open();
            List<ToDoItem> items = new List<ToDoItem>();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                items.Add(FromReader(reader));
            }

            return items;
        }

        public List<ToDoItem> GetByCategory(int id)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = @"SELECT tdi.*, c.Name FROM Categories c
                                    JOIN ToDoItems tdi
                                    ON c.Id = tdi.CategoryId
                                    WHERE c.Id = @id";

            cmd.Parameters.AddWithValue("@id", id);
            connection.Open();
            List<ToDoItem> items = new List<ToDoItem>();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                items.Add(FromReader(reader));
            }

            return items;
        }

        private ToDoItem FromReader(SqlDataReader reader)
        {
            var item = new ToDoItem()
            {
                Id = (int)reader["Id"],
                CategoryName = (string)reader["Name"],
                DueDate = (DateTime)reader["DueDate"],
                CategoryId = (int)reader["CategoryId"],
                Title = (string)reader["Title"],
                CompletedDate = reader.GetOrNull<DateTime?>("CompletedDate")
            };

            return item;
        }

        public void AddToDoItem(ToDoItem item)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO ToDoItems (Title, DueDate, CategoryId) " +
                              "VALUES(@title, @dueDate, @categoryId)";
            cmd.Parameters.AddWithValue("@title", item.Title);
            cmd.Parameters.AddWithValue("@dueDate", item.DueDate);
            cmd.Parameters.AddWithValue("@categoryId", item.CategoryId);
            connection.Open();
            cmd.ExecuteNonQuery();
        }

        public List<Category> GetCategories()
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Categories";
            connection.Open();
            List<Category> categories = new();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                categories.Add(new()
                {
                    Id = (int)reader["Id"],
                    Name = (string)reader["Name"]
                });
            }

            return categories;
        }

        public void AddCategory(string name)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO Categories (Name) " +
                              "VALUES(@name)";
            cmd.Parameters.AddWithValue("@name", name);
            connection.Open();
            cmd.ExecuteNonQuery();
        }

        public void MarkAsCompleted(int toDoItemId)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = "UPDATE ToDoItems SET CompletedDate = @date WHERE Id = @id";
            cmd.Parameters.AddWithValue("@date", DateTime.Now);
            cmd.Parameters.AddWithValue("@id", toDoItemId);
            connection.Open();
            cmd.ExecuteNonQuery();
        }

        public void UpdateCategory(Category category)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = "UPDATE Categories SET Name = @name WHERE Id = @id";
            cmd.Parameters.AddWithValue("@name", category.Name);
            cmd.Parameters.AddWithValue("@id", category.Id);
            connection.Open();
            cmd.ExecuteNonQuery();
        }

        public Category GetCategory(int id)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Categories WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            connection.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }
            Category category = new Category
            {
                Id = (int)reader["Id"],
                Name = (string)reader["Name"]
            };

            return category;
        }

    }
}