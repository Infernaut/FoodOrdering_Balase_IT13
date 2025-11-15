using System;
using System.Data;
using System.Windows.Forms;

namespace FoodOrdering_Balase_IT13
{
    public partial class ViewOrdersForm : Form
    {
        private readonly DatabaseHelper dbHelper;

        public ViewOrdersForm()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
            LoadOrders();
        }

        private void LoadOrders()
        {
            DataTable orders = dbHelper.GetAllOrders();
            dgvOrders.DataSource = orders;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadOrders();
        }

        private int? GetSelectedOrderId()
        {
            if (dgvOrders.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select an order first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }

            object value = dgvOrders.SelectedRows[0].Cells["OrderID"].Value;
            if (value == null || value == DBNull.Value)
            {
                MessageBox.Show("Invalid order selection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            return Convert.ToInt32(value);
        }

        private void btnVoid_Click(object sender, EventArgs e)
        {
            int? orderId = GetSelectedOrderId();
            if (orderId == null)
            {
                return;
            }

            if (MessageBox.Show("Void this order?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            if (dbHelper.UpdateOrderStatus(orderId.Value, "Cancelled"))
            {
                LoadOrders();
            }
        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            int? orderId = GetSelectedOrderId();
            if (orderId == null)
            {
                return;
            }

            if (MessageBox.Show("Mark this order as completed?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            if (dbHelper.UpdateOrderStatus(orderId.Value, "Completed"))
            {
                LoadOrders();
            }
        }
    }
}
