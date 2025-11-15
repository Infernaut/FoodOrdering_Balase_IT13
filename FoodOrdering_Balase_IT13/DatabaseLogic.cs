using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace FoodOrdering_Balase_IT13
{
    public class DatabaseHelper
    {
        private const string ConnectionString = "Data Source=THADDEUS\\SQLEXPRESS;Initial Catalog=DB_FoodOrdering_Balase_IT13;Integrated Security=True;TrustServerCertificate=True";

        public DataTable ExecuteStoredProcedure(string procedureName, SqlParameter[]? parameters = null)
        {
            DataTable table = new DataTable();

            try
            {
                using SqlConnection conn = new SqlConnection(ConnectionString);
                using SqlCommand cmd = new SqlCommand(procedureName, conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                using SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(table);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return table;
        }

        public bool UpdateOrderStatus(int orderId, string status)
        {
            try
            {
                using SqlConnection conn = new SqlConnection(ConnectionString);
                using SqlCommand cmd = new SqlCommand("UPDATE Orders SET Status = @Status WHERE OrderID = @OrderID", conn);

                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@OrderID", orderId);

                conn.Open();
                int affected = cmd.ExecuteNonQuery();
                return affected > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public object? ExecuteScalar(string procedureName, SqlParameter[]? parameters = null)
        {
            try
            {
                using SqlConnection conn = new SqlConnection(ConnectionString);
                using SqlCommand cmd = new SqlCommand(procedureName, conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                conn.Open();
                return cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        public int ExecuteNonQuery(string procedureName, SqlParameter[] parameters)
        {
            try
            {
                using SqlConnection conn = new SqlConnection(ConnectionString);
                using SqlCommand cmd = new SqlCommand(procedureName, conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddRange(parameters);

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0;
            }
        }

        public DataTable GetAllOrders()
        {
            DataTable table = new DataTable();

            try
            {
                using SqlConnection conn = new SqlConnection(ConnectionString);
                using SqlCommand cmd = new SqlCommand("SELECT OrderID, OrderDate, CustomerName, TotalAmount, Status, Notes FROM Orders ORDER BY OrderDate DESC, OrderID DESC", conn);

                using SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(table);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return table;
        }

        public int CreateOrder(string customerName, DateTime orderDate, string? notes, DataTable orderItems)
        {
            if (orderItems == null || orderItems.Rows.Count == 0)
            {
                MessageBox.Show("Add at least one item to the order.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return 0;
            }

            try
            {
                using SqlConnection conn = new SqlConnection(ConnectionString);
                conn.Open();

                using SqlTransaction tx = conn.BeginTransaction();

                try
                {
                    decimal totalAmount = 0m;
                    foreach (DataRow row in orderItems.Rows)
                    {
                        if (row["TotalPrice"] != DBNull.Value)
                        {
                            totalAmount += Convert.ToDecimal(row["TotalPrice"]);
                        }
                    }

                    using SqlCommand cmdOrder = new SqlCommand(@"INSERT INTO Orders (OrderDate, CustomerName, TotalAmount, Status, Notes)
VALUES (@OrderDate, @CustomerName, @TotalAmount, @Status, @Notes);
SELECT CAST(SCOPE_IDENTITY() AS int);", conn, tx);

                    cmdOrder.Parameters.AddWithValue("@OrderDate", orderDate);
                    cmdOrder.Parameters.AddWithValue("@CustomerName", customerName);
                    cmdOrder.Parameters.AddWithValue("@TotalAmount", totalAmount);
                    cmdOrder.Parameters.AddWithValue("@Status", "Pending");
                    cmdOrder.Parameters.AddWithValue("@Notes", (object?)notes ?? DBNull.Value);

                    int orderId = (int)cmdOrder.ExecuteScalar();

                    foreach (DataRow row in orderItems.Rows)
                    {
                        int foodId = Convert.ToInt32(row["FoodID"]);
                        int quantity = Convert.ToInt32(row["Quantity"]);
                        decimal unitPrice = Convert.ToDecimal(row["UnitPrice"]);
                        decimal lineTotal = Convert.ToDecimal(row["TotalPrice"]);

                        using SqlCommand cmdDetail = new SqlCommand(@"INSERT INTO OrderDetails (OrderID, FoodID, Quantity, UnitPrice, TotalPrice)
VALUES (@OrderID, @FoodID, @Quantity, @UnitPrice, @TotalPrice);", conn, tx);

                        cmdDetail.Parameters.AddWithValue("@OrderID", orderId);
                        cmdDetail.Parameters.AddWithValue("@FoodID", foodId);
                        cmdDetail.Parameters.AddWithValue("@Quantity", quantity);
                        cmdDetail.Parameters.AddWithValue("@UnitPrice", unitPrice);
                        cmdDetail.Parameters.AddWithValue("@TotalPrice", lineTotal);

                        cmdDetail.ExecuteNonQuery();
                    }

                    tx.Commit();
                    return orderId;
                }
                catch (Exception)
                {
                    tx.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0;
            }
        }
    }
}
