﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace LoginForm
{
    public partial class frmLogin : Form
    {
                string cs = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public frmLogin()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (txtUserName.Text == "")
            {
                MessageBox.Show("Please enter user name", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtUserName.Focus();
                return;
            }
            if (txtPassword.Text == "")
            {
                MessageBox.Show("Please enter password", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPassword.Focus();
                return;
            }
            try
            {
                SqlConnection myConnection = default(SqlConnection);
                myConnection = new SqlConnection(cs);

                SqlCommand myCommand = default(SqlCommand);

                myCommand = new SqlCommand("SELECT Username,Password FROM Users WHERE Username = @Username AND Password = @Password", myConnection);

                SqlParameter uName = new SqlParameter("@Username", SqlDbType.VarChar);
                SqlParameter uPassword = new SqlParameter("@Password", SqlDbType.VarChar);

                uName.Value = txtUserName.Text;
                uPassword.Value = txtPassword.Text;

                myCommand.Parameters.Add(uName);
                myCommand.Parameters.Add(uPassword);

                myCommand.Connection.Open();

                SqlDataReader myReader = myCommand.ExecuteReader(CommandBehavior.CloseConnection);

                if (myReader.Read() == true)
                {
                    MessageBox.Show("You have logged in successfully " + txtUserName.Text);
                    //Hide the login form
                    this.Hide();
                }


                else
                {
                    MessageBox.Show("Login Failed...Try again !", "Login Denied", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    txtUserName.Clear();
                    txtPassword.Clear();
                    txtUserName.Focus();

                }
                if (myConnection.State == ConnectionState.Open)
                {
                    myConnection.Dispose();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        
    }
}
