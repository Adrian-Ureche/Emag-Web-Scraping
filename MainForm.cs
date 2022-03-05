using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Emag
{
    public partial class Emag : Form
    {
        private IWebDriver driver = new ChromeDriver();
        public static List<string> srcList = new List<string>();

        public Emag()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            Thread bgThread = new Thread(new ThreadStart(backgroundThread));
            bgThread.Start();
        }

        private void backgroundThread()
        {
            ///    Open Chrome file    ///
            driver.Url = txtLink.Text;


            ///    Starting to add products to the database    ///

            do
            {
                analyzeProducts();

            } while (goToNextPage());

            MessageBox.Show("The analysis is complete!");
        }

        private void analyzeProducts()
        {
            ReadOnlyCollection<IWebElement> elementsName = driver.FindElements(By.XPath("//a[@class='product-title js-product-url']"));
            ReadOnlyCollection<IWebElement> elementsPrice = driver.FindElements(By.XPath("//p[contains(@class,'product-new-price')]"));
            ReadOnlyCollection<IWebElement> elementsSrc = driver.FindElements(By.XPath("//a[@class='product-title js-product-url']"));


            for (int i = 0; i < elementsName.Count; i++)
            {
                string name = elementsName.ElementAt(i).GetAttribute("innerHTML").Replace('"', ' ').Replace("<span class= text-danger >", "").Replace("</span>", "").Replace("'", " ").Replace("                            ", "");
                string price = elementsPrice.ElementAt(i).GetAttribute("innerHTML").Replace('"', ' ').Replace("<sup>", ",").Replace("</sup> <span>", " ").Replace("</span>", "").Replace("<span class= font-size-sm >", "");
                string src = elementsSrc.ElementAt(i).GetAttribute("href");

                
                Product p = new Product(name, price, src);
            }

        }
        
        private bool goToNextPage()
        {
            ReadOnlyCollection<IWebElement> nextPage = this.driver.FindElements(By.XPath("//span[@aria-hidden='true']"));

            foreach(IWebElement np in nextPage)
                if (np.GetAttribute("innerHTML").CompareTo("Pagina urmatoare") == 0)
                {
                    bool flag = true;
                    while (flag)
                        try
                        {
                            np.Click();
                            flag = false;
                        }
                        catch
                        {
                            System.Threading.Thread.Sleep(500);
                            try
                            {
                                IWebElement cookie = this.driver.FindElement(By.XPath("//button[@class='btn btn-primary js-accept gtm_h76e8zjgoo btn-block']"));
                                cookie.Click();
                            }
                            catch
                            {
                                try
                                {
                                    IWebElement cookie = this.driver.FindElement(By.XPath("//button[@class='btn font-size-ms btn-sm btn-primary accept-btn js-accept btn-block gtm_h76e8zjgoo']"));
                                    cookie.Click();
                                }
                                catch
                                {
                                    System.Threading.Thread.Sleep(100);
                                }
                            }
                            System.Threading.Thread.Sleep(1000);

                            try
                            {
                                IWebElement acountX = this.driver.FindElement(By.XPath("//button[@class='js-dismiss-login-notice-btn dismiss-btn btn btn-link pad-sep-none pad-hrz-none']"));
                                acountX.Click();
                            }
                            catch
                            {
                                System.Threading.Thread.Sleep(100);
                            }
                        }

                    System.Threading.Thread.Sleep(2000);
                    return true;
                }

            System.Threading.Thread.Sleep(2000);
            return false;
        }
        

        private void btnCheck_Click(object sender, EventArgs e)
        {
            lvProducts.Items.Clear();

            string connectionString = "server=127.0.0.1;uid=root;" + "pwd=root;database=emag;";
            MySqlConnection sqlProductsConn = new MySqlConnection(connectionString);
            sqlProductsConn.Open();

            string request = "SELECT * from Products Where NewPrice <> OldPrice";
            MySqlCommand cmd = new MySqlCommand(request, sqlProductsConn);


            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                // Check is the reader has any rows at all before starting to read.
                if (reader.HasRows)
                {
                    // Read advances to the next row.
                    while (reader.Read())
                    {
                        string name = reader.GetString(reader.GetOrdinal("Name"));
                        string oldPrice = reader.GetString(reader.GetOrdinal("OldPrice"));
                        string newPrice = reader.GetString(reader.GetOrdinal("NewPrice"));
                        string src = reader.GetString(reader.GetOrdinal("Src"));

                        string[] row = { name, oldPrice, newPrice };
                        var listViewItem = new ListViewItem(row);
                        lvProducts.Items.Add(listViewItem);

                        srcList.Add(src);
                    }
                }
            }


            sqlProductsConn.Close();
        }

        private void btnGoToPage_Click(object sender, EventArgs e)
        {
            if (lvProducts.SelectedItems.Count > 0)
            {
                int index = lvProducts.Items.IndexOf(lvProducts.SelectedItems[0]);
                driver.Url = srcList.ElementAt(index);
            }
            else
                MessageBox.Show("Select an elemet from list");
        }


        private void btnClearDB_Click(object sender, EventArgs e)
        {
            Product.clearDatabase();
            MessageBox.Show("Database is clear!");
        }
    }
}