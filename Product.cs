using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Emag
{
    class Product
    {

        public Product(string name, string price, string src)
        {
            string connectionString = "server=127.0.0.1;uid=root;" + "pwd=root;database=emag;";
            MySqlConnection sqlProductsConn = new MySqlConnection(connectionString);
            sqlProductsConn.Open();


            if (isInDatabase(name, sqlProductsConn))
            {
                string request = "SELECT NewPrice from products where name like '" + name + "'";
                MySqlCommand cmd = new MySqlCommand(request, sqlProductsConn);
                var newPrice = cmd.ExecuteScalar();

                if (newPrice.ToString().CompareTo(price) != 0)
                {
                    request = "UPDATE Products SET OldPrice = '" + newPrice.ToString() + "', NewPrice = '" + price + "' Where Name = '" + name + "'";
                    cmd = new MySqlCommand(request, sqlProductsConn);
                    try { cmd.ExecuteNonQuery(); }
                    catch { MessageBox.Show(request); }
                }
            }
            else
            {
                string request = "INSERT Products (Name, OldPrice, NewPrice, Src) VALUES ('" + name + "', '" + price + "', '" + price + "', '" + src + "')";
                MySqlCommand cmd = new MySqlCommand(request, sqlProductsConn);
                try { cmd.ExecuteNonQuery(); }
                catch { MessageBox.Show(request); }
            }

            sqlProductsConn.Close();
        }

        private bool isInDatabase(string name, MySqlConnection sqlProductsConn)
        {
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) from products where name like '" + name + "'", sqlProductsConn); //command to execute
            var count = cmd.ExecuteScalar();   //execute command and return the result


            if (Convert.ToInt32(count) > 0)
                return true;
            else
                return false;
        }

        public static void clearDatabase()
        {
            string connectionString = "server=127.0.0.1;uid=root;" + "pwd=root;database=emag;";
            MySqlConnection sqlProductsConn = new MySqlConnection(connectionString);
            sqlProductsConn.Open();

            MySqlCommand cmd = new MySqlCommand("DELETE from products", sqlProductsConn);
            cmd.ExecuteNonQuery();

            sqlProductsConn.Close();
        }
    }
}
