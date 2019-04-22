using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LoginForm
{
    public partial class WeightEntries : Form
    {
        private SerialPort _serialPort;         //<-- declares a SerialPort Variable to be used throughout the form
        string cs = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        //String cs = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\saqib\Desktop\WeightingMachine\LoginForm\LoginForm\Database1.mdf;Integrated Security=True";
        public WeightEntries()
        {
            InitializeComponent();
            fillProductsComboBox();
            loadAllWeightEntries();
            loadAllPorts();
            loadBaudRate();
            loadParity();
            loadStopBit();
            loadDataBit();
        }

        private void loadBaudRate()
        {
            comboBox3.Items.Add(9600);
            comboBox3.Items.Add(115200);
            comboBox3.Items.Add(256000);
        }

        private void loadParity()
        {
            comboBox4.Items.Add("None");
            comboBox4.Items.Add("Even");
            comboBox4.Items.Add("Odd");
            comboBox4.Items.Add("Space");
        }

        private void loadStopBit()
        {
            comboBox5.Items.Add("None");
            comboBox5.Items.Add("One");
            comboBox5.Items.Add("One Point Five");
            comboBox5.Items.Add("Two");
        }

        private void loadDataBit()
        {
            
            comboBox6.Items.Add("0");
            comboBox6.Items.Add("1");
            comboBox6.Items.Add("2");
            comboBox6.Items.Add("3");
            comboBox6.Items.Add("4"); 
            comboBox6.Items.Add("5"); 
            comboBox6.Items.Add("6"); 
            comboBox6.Items.Add("7"); 
        }

        private void WeightEntries_Load(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            loadAllWeightEntries();
        }
        private void loadAllWeightEntries()
        {
            SqlConnection myConnection = default(SqlConnection);
            myConnection = new SqlConnection(cs);
            myConnection.Open();
            SqlCommand myCommand = default(SqlCommand);

            myCommand = new SqlCommand("SELECT Product_Name as [Product Name], Weight, Created_By as [Created By],Created_At as [Created At], Loose_Weight [Is Loose Weight] FROM WeightEntries", myConnection);
            DataTable dt = new DataTable();
            SqlDataAdapter sdr = new SqlDataAdapter(myCommand);
            sdr.Fill(dt);
            dataGridView1.DataSource = dt;

            myConnection.Close();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Hide();
            MainForm uf = new MainForm();
            uf.ShowDialog();
        }

        private void fillProductsComboBox()
        {
            SqlConnection myConnection = default(SqlConnection);
            myConnection = new SqlConnection(cs);
            SqlCommand myCommand = default(SqlCommand);

            myCommand = new SqlCommand("SELECT * FROM Products", myConnection);

            SqlDataReader myReader;
            try
            {
                myConnection.Open();
                myReader = myCommand.ExecuteReader();
                while (myReader.Read())
                {
                    string prodName = myReader.GetString(1);
                    comboBox1.Items.Add(prodName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            myConnection.Close();
        }

        private void loadAllPorts()
        {
            try
            {
                string[] portNames = System.IO.Ports.SerialPort.GetPortNames();     //<-- Reads all available comPorts
                foreach (var portName in portNames)
                {
                    comboBox2.Items.Add(portName);                  //<-- Adds Ports to combobox
                }
                comboBox2.Items.Add("COM1");
                try
                {
                    comboBox2.SelectedIndex = 0;
                }
                catch (Exception)
                {
                }
            }
            catch (Exception)
            {

            }
            
        }


        private void SerialPortOnDataReceived(object sender, SerialDataReceivedEventArgs serialDataReceivedEventArgs)
        {
            while (readEntries)
            {
                textBox1.Text += string.Format("{0:X2} ", _serialPort.ReadByte());

                SqlConnection myConnection = default(SqlConnection);
                myConnection = new SqlConnection(cs);
                myConnection.Open();
                SqlCommand myCommand = default(SqlCommand);

                myCommand = new SqlCommand("INSERT INTO WeightEntries (Created_At,Created_By,Is_Deleted,Deleted_Date,Product_Id,Product_Name,Weight,Loose_Weight)"+
                    "VALUES (@Created_At,@Created_By,@Is_Deleted,@Deleted_Date,@Product_Id,@Product_Name,@Weight,@Loose_Weight)", myConnection);
                myCommand.Parameters.AddWithValue("@Product_Name", selectedProduct);
                myCommand.Parameters.AddWithValue("@Created_At", DateTime.Now);
                myCommand.Parameters.AddWithValue("@Created_By", 1);
                myCommand.Parameters.AddWithValue("@Is_Deleted", false);
                myCommand.Parameters.AddWithValue("@Deleted_Date", DateTime.Now);
                myCommand.Parameters.AddWithValue("@Weight", Convert.ToDouble(string.Format("{0:X2} ", _serialPort.ReadByte())));
                myCommand.Parameters.AddWithValue("@Loose_Weight", false);
                myCommand.ExecuteNonQuery();
                myConnection.Close();
            }
        }
        static string selectedProduct = "";
        static bool readEntries = true;
        
        private void button1_Click(object sender, EventArgs e)
        {
            readEntries = true;
            selectedProduct = comboBox1.Text;
            if (string.IsNullOrEmpty(selectedProduct))
            {
                MessageBox.Show("Please Select a Product!");
                comboBox1.Focus();
            }
            else
            {
                try
                {

                    SqlConnection myConnection = default(SqlConnection);
                    myConnection = new SqlConnection(cs);

                    SqlCommand myCommand = default(SqlCommand);

                    myCommand = new SqlCommand("SELECT * FROM Products WHERE Product_Name = @Product_Name", myConnection);

                    SqlParameter pName = new SqlParameter("@Product_Name", SqlDbType.VarChar);

                    pName.Value = selectedProduct;

                    myCommand.Parameters.Add(pName);

                    myCommand.Connection.Open();

                    SqlDataReader myReader = myCommand.ExecuteReader(CommandBehavior.CloseConnection);
                    var BaudRateVal = comboBox3.Text;
                    var ParityText = comboBox4.Text;
                    var StopBitVal = comboBox5.Text;
                    
                    System.IO.Ports.StopBits StopBitValue = StopBitVal == "None" ? StopBits.None :
                       StopBitVal == "One" ? StopBits.One :
                       StopBitVal == "One Point Five" ? StopBits.OnePointFive :
                       StopBitVal == "Two" ? StopBits.Two : StopBits.None;

                    Parity paritySpaceVal = ParityText == "Space" ? Parity.Space :
                        ParityText == "None" ? Parity.None :
                        ParityText == "Even" ? Parity.Even :
                        ParityText == "Odd" ? Parity.Odd : Parity.None;

                    int dataBitsVal = Convert.ToInt32(comboBox6.Text);

                    if (myReader.Read() == true)
                    {
                        if (_serialPort != null && _serialPort.IsOpen)
                            _serialPort.Close();
                        if (_serialPort != null)
                            _serialPort.Dispose();
                        //<-- End of Block

                        _serialPort = new SerialPort(comboBox2.Text, Convert.ToInt32(BaudRateVal), paritySpaceVal, dataBitsVal, StopBitValue);       //<-- Creates new SerialPort using the name selected in the combobox
                        _serialPort.DataReceived += SerialPortOnDataReceived;       //<-- this event happens everytime when new data is received by the ComPort
                        _serialPort.Open();     //<-- make the comport listen
                        textBox1.Text = "Listening on " + _serialPort.PortName + "...\r\n";
                    }
                    else
                    {
                        MessageBox.Show("Something went wrong please try later!");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            readEntries = false;
        }
    }
}
