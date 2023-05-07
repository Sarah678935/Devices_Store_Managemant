using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Devices_Store
{
    public partial class Void : Form
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        DBConnect dbcon = new DBConnect();
        SqlDataReader dr;
        CancelOrder cancelOrder;
        public Void(CancelOrder cancel)
        {
            InitializeComponent();
            cn = new SqlConnection(dbcon.myConnection());        
            cancelOrder = cancel;
            txtUsername.Select();

        }

        private void btnVoid_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtUsername.Text.ToLower() == cancelOrder.txtCancelBy.Text.ToLower())
                {
                    MessageBox.Show("Void by name and cancelled by name are the same! Please void the order with another person.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string user;
                cn.Open();
                cm = new SqlCommand("SELECT * FROM tbUser WHERE username = @username AND password = @password", cn);
                cm.Parameters.AddWithValue("@username", txtUsername.Text);
                cm.Parameters.AddWithValue("@password", txtPass.Text);
                dr = cm.ExecuteReader();
                dr.Read();

                if (dr.HasRows)
                {
                    user = dr["username"].ToString();
                    dr.Close();
                    cn.Close();
                    SaveCancelOrder(user);

                    

                // Delete the canceled order from tbCart table
                string canceledOrderId = cancelOrder.txtId.Text;
                    cn.Open();
                    cm = new SqlCommand("DELETE FROM tbCart WHERE id = @canceledOrderId", cn);
                    cm.Parameters.AddWithValue("@canceledOrderId", canceledOrderId);
                    int deletedRows = cm.ExecuteNonQuery();
                    cn.Close();

                    if (deletedRows > 0)
                    {
                        MessageBox.Show("Order transaction successfully cancelled!", "Cancel Order", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Dispose();
                        cancelOrder.ReloadSoldList();
                        cancelOrder.Dispose();
                    }
                    else
                    {
                        MessageBox.Show("Failed to cancel the order.", "Cancel Order Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    dr.Close();
                    cn.Close();
                    MessageBox.Show("Invalid username or password.", "Authentication Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                cn.Close();
                MessageBox.Show(ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void SaveCancelOrder(string user)
        {
            try
            {
                cn.Open();
                cm = new SqlCommand("insert into tbCancel (transno, pcode, price, qty, total, sdate, voidby, cancelledby, reason, action) values (@transno, @pcode, @price, @qty, @total, @sdate, @voidby, @cancelledby, @reason, @action)", cn);
                cm.Parameters.AddWithValue("@transno", cancelOrder.txtTransno.Text);
                cm.Parameters.AddWithValue("@pcode", cancelOrder.txtPcode.Text);
                cm.Parameters.AddWithValue("@price",double.Parse(cancelOrder.txtPrice.Text));
                cm.Parameters.AddWithValue("@qty", int.Parse(cancelOrder.txtQty.Text));
                cm.Parameters.AddWithValue("@total", double.Parse(cancelOrder.txtTotal.Text));
                cm.Parameters.AddWithValue("@sdate", DateTime.Now);
                cm.Parameters.AddWithValue("@voidby", user);
                cm.Parameters.AddWithValue("@cancelledby", cancelOrder.txtCancelBy.Text);
                cm.Parameters.AddWithValue("@reason", cancelOrder.txtReason.Text);
                cm.Parameters.AddWithValue("@action", cancelOrder.cboInventory.Text);
                cm.ExecuteNonQuery();
                cn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }
      





        private void picClose_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void Void_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Dispose();
            }
        }
    }
}
