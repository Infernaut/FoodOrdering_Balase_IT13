using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace FoodOrdering_Balase_IT13
{
    public partial class MainForm : Form
    {
        private readonly DatabaseHelper dbHelper;
        private DataTable? foodItemsTable;

        public MainForm()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
        }

        private Button? btnNewOrder;

        private void MainForm_Load(object sender, EventArgs e)
        {
            InitializeDataGridView();
            LoadFoodItems();
            InitializeComboBoxes();
            SetFormStyles();
            AddNewOrderButton();
        }

        private void AddNewOrderButton()
        {
            int padding = 20;
            int spacing = 10;

            var btnNewOrder = new Button
            {
                Text = "+ New Order",
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point),
                Size = new Size(150, 45),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Margin = new Padding(10),
                TabStop = false
            };

            var btnViewOrders = new Button
            {
                Text = "View Orders",
                BackColor = Color.FromArgb(52, 58, 64),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
                Size = new Size(150, 45),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Margin = new Padding(10),
                TabStop = false
            };

            void PositionButtons()
            {
                btnNewOrder.Location = new Point(
                    ClientSize.Width - padding - btnNewOrder.Width,
                    ClientSize.Height - padding - btnNewOrder.Height
                );

                btnViewOrders.Location = new Point(
                    btnNewOrder.Left - spacing - btnViewOrders.Width,
                    btnNewOrder.Top
                );
            }

            PositionButtons();

            Resize += (s, e) => PositionButtons();

            btnNewOrder.Click += BtnNewOrder_Click;
            btnViewOrders.Click += (s, e) =>
            {
                using (var viewForm = new ViewOrdersForm())
                {
                    viewForm.ShowDialog();
                }
            };

            Controls.Add(btnNewOrder);
            Controls.Add(btnViewOrders);

            btnNewOrder.BringToFront();
            btnViewOrders.BringToFront();
        }

        private void BtnNewOrder_Click(object? sender, EventArgs e)
        {
            using (var orderForm = new OrderForm())
            {
                if (orderForm.ShowDialog() == DialogResult.OK)
                {
                    // Refresh food items if needed after order is placed
                    LoadFoodItems();
                }
            }
        }

        private void InitializeDataGridView()
        {
            dgvFoodItems.AutoGenerateColumns = false;
            dgvFoodItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvFoodItems.MultiSelect = false;
            dgvFoodItems.ReadOnly = true;
            dgvFoodItems.AllowUserToAddRows = false;
            dgvFoodItems.AllowUserToDeleteRows = false;
            dgvFoodItems.RowHeadersVisible = false;
            dgvFoodItems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvFoodItems.SelectionChanged += DgvFoodItems_SelectionChanged;

            dgvFoodItems.Columns.Clear();
            dgvFoodItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "FoodID", DataPropertyName = "FoodID", HeaderText = "ID", Visible = false });
            dgvFoodItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "FoodName", DataPropertyName = "FoodName", HeaderText = "Food Name" });
            dgvFoodItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Description", DataPropertyName = "Description", HeaderText = "Description" });
            dgvFoodItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Price", DataPropertyName = "Price", HeaderText = "Price", DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            dgvFoodItems.Columns.Add(new DataGridViewCheckBoxColumn { Name = "IsAvailable", DataPropertyName = "IsAvailable", HeaderText = "Available" });
            dgvFoodItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "CategoryID", DataPropertyName = "CategoryID", HeaderText = "CategoryID", Visible = false });
            dgvFoodItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "CategoryName", DataPropertyName = "CategoryName", HeaderText = "Category" });
        }

        private void LoadFoodItems()
        {
            foodItemsTable = dbHelper.ExecuteStoredProcedure("sp_GetAllFoodItems");
            dgvFoodItems.DataSource = foodItemsTable;
        }

        private void InitializeComboBoxes()
        {
            DataTable categories = dbHelper.ExecuteStoredProcedure("sp_GetAllCategories");

            cboCategory.DataSource = categories.Copy();
            cboCategory.DisplayMember = "CategoryName";
            cboCategory.ValueMember = "CategoryID";
            cboCategory.SelectedIndex = -1;

            cboCategoryFilter.DataSource = categories;
            cboCategoryFilter.DisplayMember = "CategoryName";
            cboCategoryFilter.ValueMember = "CategoryID";
            cboCategoryFilter.SelectedIndex = -1;
        }

        private void SetFormStyles()
        {
            BackColor = Color.FromArgb(240, 240, 240);
            Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);

            foreach (Control control in Controls)
            {
                if (control is Button button)
                {
                    button.BackColor = Color.FromArgb(0, 123, 255);
                    button.ForeColor = Color.White;
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderSize = 0;
                    button.Cursor = Cursors.Hand;
                    button.Padding = new Padding(10, 5, 10, 5);
                }
            }

            dgvFoodItems.BackgroundColor = Color.White;
            dgvFoodItems.BorderStyle = BorderStyle.None;
            dgvFoodItems.EnableHeadersVisualStyles = false;
            dgvFoodItems.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(40, 40, 40);
            dgvFoodItems.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvFoodItems.ColumnHeadersDefaultCellStyle.Font = new Font(dgvFoodItems.Font, FontStyle.Bold);
            dgvFoodItems.RowTemplate.Height = 30;
            dgvFoodItems.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
        }

        private void DgvFoodItems_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvFoodItems.SelectedRows.Count == 0)
            {
                return;
            }

            DataGridViewRow row = dgvFoodItems.SelectedRows[0];
            txtFoodID.Text = Convert.ToString(row.Cells["FoodID"].Value);
            txtFoodName.Text = Convert.ToString(row.Cells["FoodName"].Value);
            txtDescription.Text = Convert.ToString(row.Cells["Description"].Value);
            txtPrice.Text = Convert.ToDecimal(row.Cells["Price"].Value).ToString("0.00");
            chkAvailable.Checked = Convert.ToBoolean(row.Cells["IsAvailable"].Value);

            if (row.Cells["CategoryID"].Value != null && row.Cells["CategoryID"].Value != DBNull.Value)
            {
                cboCategory.SelectedValue = Convert.ToInt32(row.Cells["CategoryID"].Value);
            }
            else
            {
                cboCategory.SelectedIndex = -1;
            }
        }

        private void btnAdd_Click(object? sender, EventArgs e)
        {
            if (!ValidateForm())
            {
                return;
            }

            SqlParameter[] parameters =
            {
                new SqlParameter("@FoodName", txtFoodName.Text.Trim()),
                new SqlParameter("@Description", string.IsNullOrWhiteSpace(txtDescription.Text) ? (object)DBNull.Value : txtDescription.Text.Trim()),
                new SqlParameter("@Price", decimal.Parse(txtPrice.Text)),
                new SqlParameter("@CategoryID", cboCategory.SelectedIndex >= 0 ? (object)cboCategory.SelectedValue! : DBNull.Value)
            };

            object? result = dbHelper.ExecuteScalar("sp_AddFoodItem", parameters);
            if (result != null && int.TryParse(result.ToString(), out _))
            {
                MessageBox.Show("Food item added.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                LoadFoodItems();
            }
        }

        private void btnUpdate_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFoodID.Text))
            {
                MessageBox.Show("Select a record to update.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!ValidateForm())
            {
                return;
            }

            SqlParameter[] parameters =
            {
                new SqlParameter("@FoodID", int.Parse(txtFoodID.Text)),
                new SqlParameter("@FoodName", txtFoodName.Text.Trim()),
                new SqlParameter("@Description", string.IsNullOrWhiteSpace(txtDescription.Text) ? (object)DBNull.Value : txtDescription.Text.Trim()),
                new SqlParameter("@Price", decimal.Parse(txtPrice.Text)),
                new SqlParameter("@CategoryID", cboCategory.SelectedIndex >= 0 ? (object)cboCategory.SelectedValue! : DBNull.Value),
                new SqlParameter("@IsAvailable", chkAvailable.Checked)
            };

            int affected = dbHelper.ExecuteNonQuery("sp_UpdateFoodItem", parameters);
            if (affected > 0)
            {
                MessageBox.Show("Food item updated.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                LoadFoodItems();
            }
        }

        private void btnDelete_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFoodID.Text))
            {
                MessageBox.Show("Select a record to delete.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("Delete this record?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            SqlParameter[] parameters =
            {
                new SqlParameter("@FoodID", int.Parse(txtFoodID.Text))
            };

            int affected = dbHelper.ExecuteNonQuery("sp_DeleteFoodItem", parameters);
            if (affected > 0)
            {
                MessageBox.Show("Food item deleted.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                LoadFoodItems();
            }
        }

        private void btnClear_Click(object? sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            txtFoodID.Clear();
            txtFoodName.Clear();
            txtDescription.Clear();
            txtPrice.Clear();
            chkAvailable.Checked = true;
            cboCategory.SelectedIndex = -1;
            dgvFoodItems.ClearSelection();
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtFoodName.Text))
            {
                MessageBox.Show("Enter food name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFoodName.Focus();
                return false;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Enter a valid price.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrice.Focus();
                return false;
            }

            return true;
        }

        private void btnSearch_Click(object? sender, EventArgs e)
        {
            string term = txtSearch.Text.Trim();

            if (string.IsNullOrEmpty(term))
            {
                LoadFoodItems();
            }
            else
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter("@SearchTerm", term)
                };

                DataTable table = dbHelper.ExecuteStoredProcedure("sp_SearchFoodItems", parameters);
                dgvFoodItems.DataSource = table;
            }

            if (cboCategoryFilter.SelectedIndex >= 0 && dgvFoodItems.DataSource is DataTable dt)
            {
                object? selected = cboCategoryFilter.SelectedValue;

                if (selected is DataRowView drv)
                {
                    selected = drv["CategoryID"];
                }

                if (selected != null && int.TryParse(selected.ToString(), out int categoryId))
                {
                    DataView view = new DataView(dt)
                    {
                        RowFilter = $"CategoryID = {categoryId}"
                    };
                    dgvFoodItems.DataSource = view.ToTable();
                }
            }
        }

        private void txtSearch_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                btnSearch_Click(sender, e);
                e.Handled = true;
            }
        }
    }
}
