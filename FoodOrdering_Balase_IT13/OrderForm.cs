using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace FoodOrdering_Balase_IT13
{
    public partial class OrderForm : Form
    {
        private DatabaseHelper dbHelper;
        private DataTable orderItems;
        private DataTable availableItems;

        public OrderForm()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
            InitializeOrderItemsTable();
            LoadAvailableItems();
        }

        private void InitializeOrderItemsTable()
        {
            orderItems = new DataTable();
            orderItems.Columns.Add("FoodID", typeof(int));
            orderItems.Columns.Add("FoodName", typeof(string));
            orderItems.Columns.Add("UnitPrice", typeof(decimal));
            orderItems.Columns.Add("Quantity", typeof(int));
            orderItems.Columns.Add("TotalPrice", typeof(decimal));
        }

        private void LoadAvailableItems()
        {
            try
            {
                availableItems = dbHelper.ExecuteStoredProcedure("sp_GetAllFoodItems");
                lstAvailableItems.DataSource = availableItems;
                lstAvailableItems.DisplayMember = "FoodName";
                lstAvailableItems.ValueMember = "FoodID";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading food items: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAddToOrder_Click(object sender, EventArgs e)
        {
            if (lstAvailableItems.SelectedItem == null || numQuantity.Value <= 0)
            {
                MessageBox.Show("Please select an item and specify a quantity greater than zero.", 
                    "Invalid Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataRowView selectedRow = (DataRowView)lstAvailableItems.SelectedItem;
            int foodId = (int)selectedRow["FoodID"];
            string foodName = selectedRow["FoodName"].ToString();
            decimal unitPrice = (decimal)selectedRow["Price"];
            int quantity = (int)numQuantity.Value;
            decimal totalPrice = unitPrice * quantity;

            // Check if item already exists in order
            DataRow[] existingRows = orderItems.Select($"FoodID = {foodId}");
            if (existingRows.Length > 0)
            {
                // Update existing item
                existingRows[0]["Quantity"] = (int)existingRows[0]["Quantity"] + quantity;
                existingRows[0]["TotalPrice"] = (decimal)existingRows[0]["UnitPrice"] * (int)existingRows[0]["Quantity"];
            }
            else
            {
                // Add new item
                DataRow newRow = orderItems.NewRow();
                newRow["FoodID"] = foodId;
                newRow["FoodName"] = foodName;
                newRow["UnitPrice"] = unitPrice;
                newRow["Quantity"] = quantity;
                newRow["TotalPrice"] = totalPrice;
                orderItems.Rows.Add(newRow);
            }

            dgvOrderItems.DataSource = orderItems;
            UpdateOrderTotal();
            
            // Reset quantity
            numQuantity.Value = 1;
        }

        private void UpdateOrderTotal()
        {
            decimal total = 0;
            foreach (DataRow row in orderItems.Rows)
            {
                total += (decimal)row["TotalPrice"];
            }
            lblTotal.Text = $"Total: {total:C}";
        }

        private void btnRemoveItem_Click(object sender, EventArgs e)
        {
            if (dgvOrderItems.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dgvOrderItems.SelectedRows)
                {
                    int foodId = (int)row.Cells["FoodID"].Value;
                    DataRow[] rows = orderItems.Select($"FoodID = {foodId}");
                    if (rows.Length > 0)
                    {
                        orderItems.Rows.Remove(rows[0]);
                    }
                }
                UpdateOrderTotal();
            }
        }

        private void btnSubmitOrder_Click(object sender, EventArgs e)
        {
            if (orderItems.Rows.Count == 0)
            {
                MessageBox.Show("Please add items to your order before submitting.", 
                    "Empty Order", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string customerName = txtCustomerName.Text.Trim();
            if (string.IsNullOrEmpty(customerName))
            {
                MessageBox.Show("Please enter your name.", "Customer Name Required", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int orderId = dbHelper.CreateOrder(
                    customerName,
                    DateTime.Now,
                    txtNotes.Text.Trim(),
                    orderItems);

                if (orderId > 0)
                {
                    MessageBox.Show($"Order #{orderId} has been placed successfully!", 
                        "Order Placed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error placing order: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
