using ADONETHWOne.Models;
using System.Data.SqlClient;
using System.Diagnostics;

namespace ADONETHWOne.Queries
{
    public class DataAccess
    {
        SqlConnection? conn = null;
        public DataAccess()
        {
            var builder = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            string connectionString = builder.Build().GetConnectionString("SqlServerLibary");

            try
            {
                conn = new SqlConnection(connectionString);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public List<Category> ReadDataFromCatagories()
        {
            SqlDataReader? reader = null;
            List<Category> categories = new();

            try
            {
                conn?.Open();

                using SqlCommand cmd = new SqlCommand("SELECT * FROM Categories AS [cgs]", conn);
                reader = cmd.ExecuteReader();


                while (reader.Read())
                {
                    categories.Add(new Category { Id = (int)reader["Id"], Name = reader["Name"].ToString() });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            finally
            {
                conn?.Close();
                reader?.Close();
            }
            return categories;
        }

        public int ExecCategoryId(string categoryName)
        {
            int id;

            try
            {
                conn?.Open();

                using SqlCommand cmd = new SqlCommand("SELECT Id FROM Categories AS [cgs] WHERE [cgs].Name=@p", conn);
                cmd.Parameters.AddWithValue("@p", categoryName);
                id = int.Parse(cmd.ExecuteScalar().ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return -1;
            }
            finally
            {
                conn?.Close();
            }
            return id;
        }
        public int ExecAuthorId(string authorName)
        {
            int id;

            try
            {
                conn?.Open();

                using SqlCommand cmd = new SqlCommand("SELECT Id FROM Authors AS [ath] WHERE [ath].FirstName=@p", conn);
                cmd.Parameters.AddWithValue("@p", authorName);
                id = int.Parse(cmd.ExecuteScalar().ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return -1;
            }
            finally
            {
                conn?.Close();
            }
            return id;
        }

        public List<Author> ReadDataFromAuthors(string categoryName)
        {
            SqlDataReader? reader = null;
            List<Author> authorsNames = new();
            int id = ExecCategoryId(categoryName);
            if (id == -1)
                return null;

            try
            {
                conn?.Open();

                using SqlCommand cmd = new SqlCommand("SELECT DISTINCT * FROM Authors AS [ath] WHERE [ath].Id IN (SELECT DISTINCT Id_Author FROM Books AS [bs] WHERE [bs].Id_Category = @p)", conn);
                cmd.Parameters.AddWithValue("@p", id.ToString());
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    authorsNames.Add(new() { Id = (int)reader["Id"], FirstName = reader["FirstName"].ToString(), LastName = reader["LastName"].ToString() });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            finally
            {
                conn?.Close();
                reader?.Close();
            }

            return authorsNames;
        }

        public List<Book> ReadDataFromBooks(string authorName)
        {
            SqlDataReader? reader = null;
            List<Book> books = new();
            int id = ExecAuthorId(authorName);
            if (id == -1)
                return null;

            try
            {
                conn?.Open();

                using SqlCommand cmd = new SqlCommand("SELECT * FROM Books AS [bs] WHERE [bs].Id_Author = @p", conn);
                cmd.Parameters.AddWithValue("@p", id);
                reader = cmd.ExecuteReader();


                while (reader.Read())
                {
                    books.Add(
                        new Book
                        {
                            Id = (int)reader["Id"],
                            Name = reader["Name"].ToString(),
                            Comment = reader["Comment"].ToString(),
                            Pages = (int)reader["Pages"],
                            YearPress = (int)reader["YearPress"],
                            Quantity = (int)reader["Quantity"],
                            Id_Author = (int)reader["Id_Author"],
                            Id_Category = (int)reader["Id_Category"],
                            Id_Press = (int)reader["Id_Press"],
                            Id_Themes = (int)reader["Id_Themes"]
                        }
                        );
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return null;
            }
            finally
            {
                conn?.Close();
                reader?.Close();
            }
            return books;
        }

        public bool RemoveBook(int id)
        {
            try
            {
                conn?.Open();

                using SqlCommand cmd = new SqlCommand("DELETE FROM Books WHERE Books.Id = @p;", conn);
                cmd.Parameters.AddWithValue("@p", id);
                Debug.WriteLine(cmd.ExecuteScalar());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
            finally
            {
                conn?.Close();
            }
            return true;
        }
        public bool UpdateBook(Book book)
        {
            try
            {
                conn?.Open();

                using SqlCommand cmd = new SqlCommand("UPDATE Books SET [Name] = @p1, Pages = @p2, YearPress = @p3, Comment = @p4, Quantity = @p5  WHERE Books.Id = @p;", conn);
                cmd.Parameters.AddWithValue("@p", book.Id);
                cmd.Parameters.AddWithValue("@p1", book.Name);
                cmd.Parameters.AddWithValue("@p2", book.Pages);
                cmd.Parameters.AddWithValue("@p3", book.YearPress);
                cmd.Parameters.AddWithValue("@p4", book.Comment);
                cmd.Parameters.AddWithValue("@p5", book.Quantity);
                Debug.WriteLine(cmd.ExecuteScalar());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
            finally
            {
                conn?.Close();
            }
            return true;
        }

        public int ReturnPossibleBookId()
        {

            SqlDataReader? reader = null;
            List<int> ids = new();

            try
            {
                conn?.Open();

                using SqlCommand cmd = new SqlCommand("SELECT Id FROM Books AS [bs]", conn);
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    ids.Add((int)reader[0]);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            finally
            {
                conn?.Close();
                reader?.Close();
            }
            int i = 1;
            while(true)
            {
                if (!ids.Exists(id => id == i))
                    return i;
                i++;
            }
        }
        public bool AddBook(Book book)
        {
            try
            {
                conn?.Open();

                using SqlCommand cmd = new SqlCommand("INSERT INTO Books(Id, [Name], Pages, YearPress, Id_Themes, Id_Category, Id_Author, Id_Press, Comment, Quantity) VALUES(@p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10)", conn);

                cmd.Parameters.AddWithValue("@p1", book.Id);
                cmd.Parameters.AddWithValue("@p2", book.Name);
                cmd.Parameters.AddWithValue("@p3", book.Pages);
                cmd.Parameters.AddWithValue("@p4", book.YearPress);
                cmd.Parameters.AddWithValue("@p5", book.Id_Themes);
                cmd.Parameters.AddWithValue("@p6", book.Id_Category);
                cmd.Parameters.AddWithValue("@p7", book.Id_Author);
                cmd.Parameters.AddWithValue("@p8", book.Id_Press);
                cmd.Parameters.AddWithValue("@p9", book.Comment);
                cmd.Parameters.AddWithValue("@p10", book.Quantity);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return false;
            }
            finally
            {
                conn?.Close();
            }
            return true;
        }
    }
}
