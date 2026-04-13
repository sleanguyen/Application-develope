using System;
using System.Data;
using System.Data.SqlClient; // library to connect and interact with SQL Server
using System.Windows.Forms;

namespace coursework2
{
    public partial class Form1 : Form
    {
        // Connection String: Points directly to SQL Server Express 
        string connectionString = @"Data Source=localhost\SQLEXPRESS;Initial Catalog=EducationDB;Integrated Security=True";

        public Form1()
        {
            InitializeComponent();
            dgvPeople.ReadOnly = true;
            dgvPeople.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        // Event run when open app
        private void Form1_Load(object sender, EventArgs e)
        {
            cbRole.Items.Clear();
            cbRole.Items.Add("Teacher");
            cbRole.Items.Add("Admin");
            cbRole.Items.Add("Student");
            // admin work type
            cbWorkType.Items.Clear();
            cbWorkType.Items.Add("Full-time");
            cbWorkType.Items.Add("Part-time");

            // filter combo box for role, allow user to select a specific role to view in DataGridView
            if (cbFilterRole != null)
            {
                cbFilterRole.Items.Clear();
                cbFilterRole.Items.Add("All");
                cbFilterRole.Items.Add("Teacher");
                cbFilterRole.Items.Add("Admin");
                cbFilterRole.Items.Add("Student");
                cbFilterRole.SelectedIndex = 0; 
            }
            // load data form sql
            LoadData();

            //security make sure all specific fields are hidden until user select a Role
            HideAllSpecificFields();
        }

        // Supplementary function: Load data form Database SQL and put it in DataGridView
        private void LoadData(string roleFilter = "All")
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT * FROM People";
                    // If user choose a specific role to filter, add WHERE clause to SQL query to get only that role's data
                    if (roleFilter != "All")
                    {
                        query += " WHERE Role = @Role";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        if (roleFilter != "All")
                        {
                            cmd.Parameters.AddWithValue("@Role", roleFilter);
                        }

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvPeople.DataSource = dt; // input data into DataGridView
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error when load data: " + ex.Message);
            }
        }

        // Supplementary function: hide all specific fields when user change Role selection
        private void HideAllSpecificFields()
        {
            txtSalary.Visible = false;
            txtSubject1.Visible = false;
            txtSubject2.Visible = false;
            txtSubject3.Visible = false;
            cbWorkType.Visible = false;
            txtWorkingHours.Visible = false;
        }

        // UI processing: auto show or hide specific fields based on Role selection
        private void cbRole_SelectedIndexChanged(object sender, EventArgs e)
        {
            HideAllSpecificFields(); // hide all specific fields first to reset the form before showing the relevant ones
            string role = cbRole.SelectedItem?.ToString();

            if (role == "Teacher")
            {
                txtSalary.Visible = true;
                txtSubject1.Visible = true;
                txtSubject2.Visible = true;
            }
            else if (role == "Admin")
            {
                txtSalary.Visible = true;
                cbWorkType.Visible = true;
                txtWorkingHours.Visible = true;
            }
            else if (role == "Student")
            {
                txtSubject1.Visible = true;
                txtSubject2.Visible = true;
                txtSubject3.Visible = true;
            }
        }

        // Add function:save new data from form into SQL Database
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (cbRole.SelectedItem == null)
            {
                MessageBox.Show("Please select a Role before adding one!");
                return;
            }
            // Security: Do not leave Name, Phone Number, and Email fields blank
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtPhone.Text) || string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Security Alert: Name, phone number, and email address cannot be left blank!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Stop, do not execute the code that is being saved to SQL.
            }

            // Security: Check that the email format includes the @ symbol
            if (!txtEmail.Text.Contains("@"))
            {
                MessageBox.Show("Security Alert: Invalid email!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Security: Prevent text from being entered into the Salary field (Only applies if Role is Teacher or Admin)
            string role = cbRole.SelectedItem.ToString();
            if (role == "Teacher" || role == "Admin")
            {
                decimal parsedSalary;
                // If the Salary field is not empty AND you enter an invalid number
                if (!string.IsNullOrWhiteSpace(txtSalary.Text) && !decimal.TryParse(txtSalary.Text, out parsedSalary))
                {
                    MessageBox.Show("Security Alert: The salary must be a valid number!", "Data error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
                try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "INSERT INTO People (Name, Phone, Email, Role, Salary, WorkType, WorkingHours, Subject1, Subject2, Subject3) " +
                                   "VALUES (@Name, @Phone, @Email, @Role, @Salary, @WorkType, @WorkingHours, @Sub1, @Sub2, @Sub3)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Assign common values
                        cmd.Parameters.AddWithValue("@Name", txtName.Text);
                        cmd.Parameters.AddWithValue("@Phone", txtPhone.Text);
                        cmd.Parameters.AddWithValue("@Email", txtEmail.Text);
                        cmd.Parameters.AddWithValue("@Role", cbRole.SelectedItem.ToString());

                        // Handle unique values 
                        cmd.Parameters.AddWithValue("@Salary", string.IsNullOrEmpty(txtSalary.Text) ? (object)DBNull.Value : decimal.Parse(txtSalary.Text));
                        cmd.Parameters.AddWithValue("@WorkType", cbWorkType.SelectedItem == null ? (object)DBNull.Value : cbWorkType.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@WorkingHours", string.IsNullOrEmpty(txtWorkingHours.Text) ? (object)DBNull.Value : int.Parse(txtWorkingHours.Text));
                        cmd.Parameters.AddWithValue("@Sub1", string.IsNullOrEmpty(txtSubject1.Text) ? (object)DBNull.Value : txtSubject1.Text);
                        cmd.Parameters.AddWithValue("@Sub2", string.IsNullOrEmpty(txtSubject2.Text) ? (object)DBNull.Value : txtSubject2.Text);
                        cmd.Parameters.AddWithValue("@Sub3", string.IsNullOrEmpty(txtSubject3.Text) ? (object)DBNull.Value : txtSubject3.Text);

                        con.Open();
                        cmd.ExecuteNonQuery(); // run SQL command
                        con.Close();
                    }
                }
                LoadData(); // Renew data in DataGridView after adding new record
                MessageBox.Show("Record added successfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error when add: " + ex.Message);
            }
        }

        // Edit function
        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvPeople.CurrentRow == null) return;

            
            int id = Convert.ToInt32(dgvPeople.CurrentRow.Cells["ID"].Value);
            string role = dgvPeople.CurrentRow.Cells["Role"].Value.ToString();

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    
                    string query = "UPDATE People SET Name=@Name, Phone=@Phone, Email=@Email ";

                    
                    if (role == "Teacher")
                        query += ", Salary=@Salary, Subject1=@Sub1, Subject2=@Sub2 WHERE ID=@ID";
                    else if (role == "Admin")
                        query += ", Salary=@Salary, WorkType=@WorkType, WorkingHours=@WorkingHours WHERE ID=@ID";
                    else if (role == "Student")
                        query += ", Subject1=@Sub1, Subject2=@Sub2, Subject3=@Sub3 WHERE ID=@ID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        
                        cmd.Parameters.AddWithValue("@ID", id);
                        cmd.Parameters.AddWithValue("@Name", txtName.Text);
                        cmd.Parameters.AddWithValue("@Phone", txtPhone.Text);
                        cmd.Parameters.AddWithValue("@Email", txtEmail.Text);

                        
                        if (role == "Teacher" || role == "Admin")
                            cmd.Parameters.AddWithValue("@Salary", string.IsNullOrEmpty(txtSalary.Text) ? (object)DBNull.Value : decimal.Parse(txtSalary.Text));

                        if (role == "Admin")
                        {
                            cmd.Parameters.AddWithValue("@WorkType", cbWorkType.Text);
                            cmd.Parameters.AddWithValue("@WorkingHours", string.IsNullOrEmpty(txtWorkingHours.Text) ? (object)DBNull.Value : int.Parse(txtWorkingHours.Text));
                        }

                        if (role == "Teacher" || role == "Student")
                        {
                            cmd.Parameters.AddWithValue("@Sub1", txtSubject1.Text);
                            cmd.Parameters.AddWithValue("@Sub2", txtSubject2.Text);
                        }

                        if (role == "Student")
                            cmd.Parameters.AddWithValue("@Sub3", txtSubject3.Text);

                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            LoadData(); // Cập nhật lại Grid
                            MessageBox.Show("Security Alert: Data updated successfully!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Data Alert: Update failed. " + ex.Message);
            }
        
    }

        // Delete function: Delete the selected record in DataGridView and SQL Database
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvPeople.CurrentRow != null)
            {
                // Ask for confirmation before deleting (Increases system security)
                DialogResult result = MessageBox.Show("Are you sure you want to delete?", "Yes", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    int id = Convert.ToInt32(dgvPeople.CurrentRow.Cells["ID"].Value);
                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        string query = "DELETE FROM People WHERE ID = @ID";
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@ID", id);
                            con.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }
                    LoadData(); // renew data in DataGridView after deleting record
                }
            }
        }

        // View function: load all data from SQL Database and show in DataGridView (without filter)
        private void btnView_Click(object sender, EventArgs e)
        {
            LoadData("All");
        }

        // filter by role function: view data in DataGridView based on the role selected in the filter combo box 
        private void btnViewRole_Click(object sender, EventArgs e)
        {
            if (cbFilterRole.SelectedItem != null)
            {
                LoadData(cbFilterRole.SelectedItem.ToString());
            }
            else
            {
                MessageBox.Show("Please select role to view");
            }
        }

        private void dgvPeople_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvPeople.Rows[e.RowIndex];
                txtName.Text = row.Cells["Name"].Value.ToString();
                txtPhone.Text = row.Cells["Phone"].Value.ToString();
                txtEmail.Text = row.Cells["Email"].Value.ToString();
                cbRole.Text = row.Cells["Role"].Value.ToString();

                
                txtSalary.Text = row.Cells["Salary"].Value.ToString();
                txtWorkingHours.Text = row.Cells["WorkingHours"].Value.ToString();
                txtSubject1.Text = row.Cells["Subject1"].Value.ToString();
                txtSubject2.Text = row.Cells["Subject2"].Value.ToString();
                txtSubject3.Text = row.Cells["Subject3"].Value.ToString();
                cbWorkType.Text = row.Cells["WorkType"].Value.ToString();
            }
        }

        private void cbFilterRole_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbFilterRole.SelectedItem != null)
            {
                string selectedRole = cbFilterRole.SelectedItem.ToString();

                LoadData(selectedRole);
            }
        }
    }
}