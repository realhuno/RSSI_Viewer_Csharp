using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.IO.Ports;
using ZedGraph;
using System.Runtime.InteropServices;
using WindowsFormsApplication4.Ini;
using WindowsFormsApplication4.file;
using System.Diagnostics;
using System.Threading;



//Not is Use MYSQL Database
//using dbconnect;
//using MySql.Data.MySqlClient;
namespace WindowsFormsApplication4
{
    public partial class Form1 : Form
    {
        public ZedGraph.ZedGraphControl zedGraphControl1;
        public GraphPane myPane;
        public int counter;
        public PointPairList spl1 = new PointPairList();
        public PointPairList spl2 = new PointPairList();
        public PointPairList spl3 = new PointPairList();
        public PointPairList spl4 = new PointPairList();
        private IniFile Ini;
        public string iniip="10.0.0.65";
        public string iniport="1234";
        public string request="NULL";
        public string log;
        public string timer1interval="1";
        public string timer2interval= "1";
        public string comport;
        public string baud;
      

        public int p1, p2;
        public int step = 0;

       public string log_filename;


        public bool runner = false;
        Stopwatch stopwatch = new Stopwatch();



        public Form1()
        {
            this.Size = new Size(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);
            this.WindowState = FormWindowState.Maximized;


          


            InitializeComponent();
            zedGraphControl1.GraphPane.Title.Text = "RAW RSSI";
            double[] x = new double[100];
            double[] y = new double[100];
            zedGraphControl1.Location = new Point(0, 100);


            zedGraphControl1.Size = new Size(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height - 100);
            GraphPane myPane = zedGraphControl1.GraphPane;
            //myPane.XAxis.Type = AxisType.Date;
            //myPane.XAxis.Scale.Format = "mm:ss:fff";
            myPane.YAxis.Title.Text = "RSSI";
            myPane.XAxis.Title.Text = "Milliseconds";
            /*
            myPane.XAxis.Scale.FontSpec.Angle = 60;
            myPane.XAxis.Scale.FontSpec.Size = 12;
            myPane.XAxis.Scale.MajorUnit = DateUnit.Millisecond;
            myPane.XAxis.Scale.MajorStep = 500;
            myPane.XAxis.Scale.MinorUnit = DateUnit.Millisecond;
            myPane.XAxis.Scale.MinorStep = 250;

            */
            LineItem myCurve1 = myPane.AddCurve("RSSI1", spl1, Color.Blue, SymbolType.None);
            LineItem myCurve2 = myPane.AddCurve("RSSI2", spl2, Color.Red, SymbolType.None);
            LineItem myCurve3 = myPane.AddCurve("RSSI3", spl3, Color.Yellow, SymbolType.None);
            LineItem myCurve4 = myPane.AddCurve("RSSI4", spl4, Color.Green, SymbolType.None);
            stopToolStripMenuItem.Enabled = false;
            startToolStripMenuItem.Enabled = false;

            //Mouselcick Event Function on the Graph
            zedGraphControl1.MouseClick += zedGraphControl1_MouseClick;


          

            System.Reflection.Assembly a = System.Reflection.Assembly.GetEntryAssembly();
            string baseDir = System.IO.Path.GetDirectoryName(a.Location);

            
            
            
            Ini=new Ini.IniFile (baseDir + "\\conf.ini");
            iniip = Ini.IniReadValue("settings", "ip");
            iniport = Ini.IniReadValue("settings", "port");
            request = Ini.IniReadValue("settings", "request");
            log = Ini.IniReadValue("settings", "log");
            timer1interval = Ini.IniReadValue("settings", "timer1interval");
            timer2interval = Ini.IniReadValue("settings", "timer2interval");

            comport = Ini.IniReadValue("settings", "comport");
            baud = Ini.IniReadValue("settings", "baud");
           
            serialPort1.BaudRate = Convert.ToInt32(baud);
            timer1.Interval = Convert.ToInt16(timer1interval);
            timer2.Interval = Convert.ToInt16(timer2interval);

            serialPort1.BaudRate = Convert.ToInt32(baud);
            serialPort1.PortName = comport.ToString();

           
           





        }

        private void zedGraphControl1_MouseClick(object sender, MouseEventArgs e)
        {
            object nearestObject;
            int index;
            this.zedGraphControl1.GraphPane.FindNearestObject(new PointF(e.X, e.Y), this.CreateGraphics(), out nearestObject, out index);
           
            if (nearestObject != null  && nearestObject.GetType() == typeof(LineItem))
            {

                // Manually get Laptimes ... mouseclick on first peak
                if (step == 0)
                {
                    Console.WriteLine("step0");
                    LineItem LineItem = (LineItem)nearestObject;
                    string point_data_a;
                    string[] string_data_a;


                    point_data_a = LineItem[index].X.ToString();
                    string_data_a = point_data_a.Split(',');
                
                    p1 = Convert.ToInt32(string_data_a[0]);
                    Console.WriteLine("P1:"+p1);
                    step++;

                    return;
                }

                // The Secondary peak ... and calculate times
                if (step == 1)
                {
                    Console.WriteLine("step1");
                    LineItem LineItem = (LineItem)nearestObject;
                    string point_data_b;
                    string[] string_data_b;
                    point_data_b = LineItem[index].X.ToString();
                    string_data_b = point_data_b.Split(',');
                    p2 = Convert.ToInt32(string_data_b[0]);
                    Console.WriteLine("P2:"+p2);
                    step = 0;
                    label2.Text = ((p2 - p1)).ToString();
                    
                }





                           //test

                    // Generate a black line with "Curve 4" in the legend
                    /*
                    GraphPane myPane = zedGraphControl1.GraphPane;

                    LineObj line = new LineObj(Color.Black,
                   myPane.XAxis.Scale.Min, 100, myPane.XAxis.Scale.Max, 100);
                    line.Location.CoordinateFrame = CoordType.AxisXYScale;
                    line.Location.AlignH = AlignH.Left;
                    line.Location.Y = LineItem[index].Y;

                    line.ZOrder = ZOrder.A_InFront;
                    myPane.GraphObjList.Add(line);
                    //test

                     */


                
                

                
            }
        }


        private void UpdateWindowSize(string width, string height)
        {
            zedGraphControl1.Size = new Size(this.Width, this.Height);

        }
        public void command(string cmd)
        {


            byte[] data = new byte[1024];
            string input, stringData;
            IPEndPoint ipep = new IPEndPoint(
                            IPAddress.Parse(iniip), Convert.ToInt32(iniport));


            Socket server = new Socket(AddressFamily.InterNetwork,
                           SocketType.Stream, ProtocolType.Tcp);

            try
            {
                server.Connect(ipep);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Unable to connect to server.");
                Console.WriteLine(ex.ToString());
                return;
            }
            server.Send(Encoding.ASCII.GetBytes(cmd + "\r\n"));
            data = new byte[1024];
            int recv = server.Receive(data);
            stringData = Encoding.ASCII.GetString(data, 0, recv);

            server.Shutdown(SocketShutdown.Both);
            server.Close();
        }
        // Currently not is use!!!
        private void timer1_Tick(object sender, EventArgs e)
        {
            string data = "sdf";
            string[] stringData2 = data.Split(';');


            /*
            string[] hexValuesSplit = stringData.Split(' ');
            int adcv1 = Convert.ToInt32(hexValuesSplit[4], 16);
            int adcv2 = Convert.ToInt32(hexValuesSplit[5], 16);
            int adcv3 = Convert.ToInt32(hexValuesSplit[6], 16);
            int adcv4 = Convert.ToInt32(hexValuesSplit[7], 16);
            
            int adcv1 = Convert.ToInt32(hexValuesSplit[1], 16);
            int adcv2 = Convert.ToInt32(hexValuesSplit[2], 16);
            int adcv3 = Convert.ToInt32(hexValuesSplit[3], 16);
            int adcv4 = Convert.ToInt32(hexValuesSplit[4], 16);

            int adcv1 = Convert.ToInt32(stringData2[0]);
            int adcv2 = Convert.ToInt32(stringData2[0]);
            int adcv3 = Convert.ToInt32(stringData2[0]);
            int adcv4 = Convert.ToInt32(stringData2[0]);
 *             */

            int adcv1=0;
            int adcv2=0;
            int adcv3=0;
            int adcv4=0;
            string stringData = "123";
            if (stringData.Length > 3)
            {
            adcv1 = Convert.ToInt32(stringData2[0]);

            }
            else {
            adcv1 = 200;
            adcv2 = 253;
            adcv3 = 251;
            adcv4 = 250;
            }





            //sqlinsert(adcv1, adcv2, adcv3, adcv4);

            label1.Text = Convert.ToString(adcv1 + " " + adcv2 + " " + adcv3 + " " + adcv4);
            counter = counter + 1;

            if (this.log == "true")
            {
                //filewrite file = new filewrite();
                //file.write(counter + "," + adcv1 + "," + adcv2 + "," + adcv3 + "," + adcv4);
            }

            spl1.Add(counter, adcv1);
            spl2.Add(counter, adcv2);
            spl3.Add(counter, adcv3);
            spl4.Add(counter, adcv4);


            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
            zedGraphControl1.Refresh();
            Invalidate();
            /*
            
            stringData = Encoding.ASCII.GetString(data, 0, recv);
            Console.WriteLine(stringData);

            while (true)
            {
                input = Console.ReadLine();
                if (input == "exit")
                    break;
                server.Send(Encoding.ASCII.GetBytes(input));
                data = new byte[1024];
                recv = server.Receive(data);
                stringData = Encoding.ASCII.GetString(data, 0, recv);
                Console.WriteLine(stringData);
            }
             /**/

            //server.Shutdown(SocketShutdown.Both);
            //server.Close();
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            startToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.Enabled = true;
            runner = true;
            udps();
          
        }

        // Currently not is use!!! But the UDP Server
        public void udps()
        {

            int adcv1 = 0;
            int adcv2 = 0;
            int adcv3 = 0;
            int adcv4 = 0;
      

            byte[] data = new byte[1024];
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 1236);
            UdpClient newsock = new UdpClient(ipep);

            Console.WriteLine("Waiting for a client...");

            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);

            data = newsock.Receive(ref sender);

            Console.WriteLine("Message received from {0}:", sender.ToString());
            Console.WriteLine(Encoding.ASCII.GetString(data, 0, data.Length));

            //string welcome = "Welcome to my test server";
            //data = Encoding.ASCII.GetBytes(welcome);
            //newsock.Send(data, data.Length, sender);
            string blubi;
            string[] stringData2;

            while (runner)
            {
                
                Application.DoEvents(); //Updates the Form's UI
                data = newsock.Receive(ref sender);


                
                blubi = Encoding.ASCII.GetString(data, 0, data.Length);
                stringData2 = blubi.Split(' ');
              
                adcv1 = Convert.ToInt32(stringData2[0]);
                adcv2 = Convert.ToInt32(stringData2[1]);
                adcv3 = Convert.ToInt32(stringData2[2]);
                adcv4 = Convert.ToInt32(stringData2[3]);






                //sqlinsert(adcv1, adcv2, adcv3, adcv4);
            
                label1.Text = Convert.ToString(adcv1 + " " + adcv2 + " " + adcv3 + " " + adcv4);
                counter = counter + 1;

                if (this.log == "true")
                {
                   //filewrite file = new filewrite();
                   // file.write(counter + "," + adcv1 + "," + adcv2 + "," + adcv3 + "," + adcv4);
                }

                spl1.Add(counter, adcv1);
                spl2.Add(counter, adcv2);
                spl3.Add(counter, adcv3);
                spl4.Add(counter, adcv4);


                zedGraphControl1.AxisChange();
                zedGraphControl1.Invalidate();
                zedGraphControl1.Refresh();
                Invalidate();



             












                //Console.WriteLine(Encoding.ASCII.GetString(data, 0, data.Length));
                //newsock.Send(data, data.Length, sender);
            }
            newsock.Close();
        }


        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            stopToolStripMenuItem.Enabled = false;
            startToolStripMenuItem.Enabled = true;
            
            //timer1.Enabled = false;
            //timer2.Enabled = false;
            
            runner = false;
            //startToolStripMenuItem.Enabled = true;
            //stopToolStripMenuItem.Enabled = false;
        }


        //Other commands from other Project i use
        private void onToolStripMenuItem_Click(object sender, EventArgs e)
        {
            command("pin set p1 on");
        }

        private void offToolStripMenuItem_Click(object sender, EventArgs e)
        {
            command("pin set p1 off");
        }

        private void oNEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            command("1w get 10189c290208004d");
        }

        private void tWOToolStripMenuItem_Click(object sender, EventArgs e)
        {
            command("1w get 10eb8b290208004b");
        }

        private void convertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            command("1w convert");
        }

        private void lISTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            command("1w list");
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            zedGraphControl1.Size = new Size(this.Width, this.Height - 150);

        }

        private void loadLogToolStripMenuItem_Click(object sender, EventArgs e)
        {


            string readcontent;
            readcontent = null;
            //TextWriter tw = new StreamWriter("logs/log.txt");
            using (StreamReader sr = new StreamReader("logs/log.txt"))
            {
                while (sr.Peek() >= 0)
                {
                    readcontent = sr.ReadLine();

                    string[] data = readcontent.Split(',');
                    spl1.Add(Convert.ToDouble(data[0]), Convert.ToDouble(data[1]));
                    spl2.Add(Convert.ToDouble(data[0]), Convert.ToDouble(data[2]));
                    spl3.Add(Convert.ToDouble(data[0]), Convert.ToDouble(data[3]));
                    spl4.Add(Convert.ToDouble(data[0]), Convert.ToDouble(data[4]));
                }
            }


            //string[] datavalue = data.Split(",");




            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
            zedGraphControl1.Refresh();
            Invalidate();

        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            counter = 0;
            GraphPane myPane = zedGraphControl1.GraphPane;


            zedGraphControl1.GraphPane.CurveList.Clear();
            myPane.GraphObjList.Clear();
            zedGraphControl1.Refresh();
            spl1.Clear();
            spl2.Clear();
            spl3.Clear();
            spl4.Clear();

            LineItem myCurve1 = myPane.AddCurve("ADC1", spl1, Color.Blue, SymbolType.None);
            LineItem myCurve2 = myPane.AddCurve("ADC2", spl2, Color.Red, SymbolType.None);
            LineItem myCurve3 = myPane.AddCurve("ADC3", spl3, Color.Yellow, SymbolType.None);
            LineItem myCurve4 = myPane.AddCurve("ADC4", spl4, Color.Green, SymbolType.None);


        }
        /*
        private void sqlReadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataSet myData = new DataSet();
            

            MySql.Data.MySqlClient.MySqlConnection conn;
            MySql.Data.MySqlClient.MySqlCommand cmd;
            MySql.Data.MySqlClient.MySqlDataAdapter myAdapter;
           
            
            conn = new MySql.Data.MySqlClient.MySqlConnection();
            cmd = new MySql.Data.MySqlClient.MySqlCommand();
            myAdapter = new MySql.Data.MySqlClient.MySqlDataAdapter();

            conn.ConnectionString = "server=127.0.0.1;uid=newuser;" +
              "pwd=newuser;database=test;";

            try
            {
                cmd.CommandText = "SELECT * from adcdata where id=1";
                cmd.Connection = conn;
                //Create Command
                
                //Create a data reader and Execute the command
                MySqlDataReader dataReader = cmd.ExecuteReader();
                
                while (dataReader.Read())
                {
                    //label2.Text=dataReader["temp1"] + "";
                   
                }
                //rdr=cmd.ExecuteReader();

                //myAdapter.SelectCommand = cmd;
                //myAdapter.Fill(myData);


                

                //myData.WriteXml(@"C:\dataset.xml", XmlWriteMode.WriteSchema);
               
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show(ex.Message, "Report could not be created",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

 
        }

      

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DBConnect dbconnect = new DBConnect();
            
            string query = "SELECT * FROM temperatur";

            //Create a list to store the result
            //List<string>[] list = new List<string>[3];
            //list[0] = new List<string>();
            //list[1] = new List<string>();
            //list[2] = new List<string>();

            //Open connection
            if (dbconnect.OpenConnection() == true)
            {
                //Create Command
                MySqlCommand cmd = new MySqlCommand(query, dbconnect.connection);
                //Create a data reader and Execute the command
                MySqlDataReader dataReader = cmd.ExecuteReader();

                //Read the data and store them in the list
                while (dataReader.Read())
                {
                    //label2.Text=dataReader["temp1"] + "";
                    spl1.Add(Convert.ToDouble(dataReader["id"]), Convert.ToDouble(dataReader["temp1"]));
                    spl2.Add(Convert.ToDouble(dataReader["id"]), Convert.ToDouble(dataReader["temp2"]));
                    spl3.Add(Convert.ToDouble(dataReader["id"]), Convert.ToDouble(dataReader["temp3"]));
                    spl4.Add(Convert.ToDouble(dataReader["id"]), Convert.ToDouble(dataReader["temp4"]));

                }

                //close Data Reader
                dataReader.Close();

                //close Connection

                zedGraphControl1.AxisChange();
                zedGraphControl1.Invalidate();
                zedGraphControl1.Refresh();
                Invalidate();
            }
        }
        */
       


  
        /*
        private void serialToolStripMenuItem_Click(object sender, EventArgs e)
        {

            
            serialPort1.Open();

            timer2.Enabled = true;
            stopToolStripMenuItem.Enabled = true;

               
         
            
          
        }

    /*
        public void sqlinsert(int adc1, int adc2, int adc3, int adc4)
        {

            DataSet myData = new DataSet();
            String queryStr;

            MySql.Data.MySqlClient.MySqlConnection conn;
            MySql.Data.MySqlClient.MySqlCommand cmd;

            MySql.Data.MySqlClient.MySqlDataAdapter myAdapter;


            conn = new MySql.Data.MySqlClient.MySqlConnection();

            conn.ConnectionString = "server=127.0.0.1;uid=newuser;" +
         "pwd=newuser;database=test;";
            //cmd = new MySql.Data.MySqlClient.MySqlCommand();
            myAdapter = new MySql.Data.MySqlClient.MySqlDataAdapter();

            queryStr = "INSERT INTO adcdata (adc1,adc2,adc3,adc4) VALUES (@adc1,@adc2,@adc3,@adc4)";

            string blub = "1234";

            using (cmd = new MySqlCommand(queryStr, conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@adc1", adc1);
                cmd.Parameters.AddWithValue("@adc2", adc2);
                cmd.Parameters.AddWithValue("@adc3", adc3);
                cmd.Parameters.AddWithValue("@adc4", adc4);



                int rowAdded = cmd.ExecuteNonQuery();
            }



        }

        */
        private void label3_Click(object sender, EventArgs e)
        {

        }

        
        public long millis()
        {
            
            return (stopwatch.ElapsedMilliseconds);
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
           
            /*
            string stringData;


            stringData = serialPort1.ReadLine();


            string[] testa;
            testa = stringData.Split(' ');
            if (testa.Length < 4)
            {
                stringData = "1 2 3 4 5 6 7 8";
                testa[0] = null;
                testa[1] = null;
                testa[2] = null;
                testa[3] = null;

            }
            else
            {
                stringData = serialPort1.ReadLine();

            }*/
            string stringData;


            serialPort1.NewLine = "\n";
            stringData = serialPort1.ReadLine();

           
            string[] testa;
            testa = stringData.Split(' ');

           
            Console.WriteLine();
            label2.Text = millis().ToString();

            if (testa.Length > 4 & testa[0]=="r" & testa[0].Length==1)
            {

                int adcv1 = Convert.ToInt32(testa[1]);
                int adcv2 = Convert.ToInt32(testa[2]);
                int adcv3 = Convert.ToInt32(testa[3]);
                int adcv4 = Convert.ToInt32(testa[4]);

                Console.WriteLine(testa[0]);


                label1.Text = Convert.ToString(adcv1 + " " + adcv2 + " " + adcv3 + " " + adcv4);
                counter = counter + 1;

                if (this.log == "true")
                {
                    filewrite file = new filewrite();
                    file.write(millis() + "," + adcv1 + "," + adcv2 + "," + adcv3 + "," + adcv4,textBox1.Text);
                }

 

                spl1.Add(millis(), adcv1);
                spl2.Add(millis(), adcv2);
                spl3.Add(millis(), adcv3);
                spl4.Add(millis(), adcv4);


                zedGraphControl1.AxisChange();
                zedGraphControl1.Invalidate();
                zedGraphControl1.Refresh();
                Invalidate();
                serialPort1.DiscardInBuffer();
                
       

                stringData = null;
                this.zedGraphControl1.PointValueEvent += new ZedGraph.ZedGraphControl.PointValueHandler(this.zedGraphControl1_PointValueEvent);

            }


         
        }


        private string zedGraphControl1_PointValueEvent(ZedGraph.ZedGraphControl sender, ZedGraph.GraphPane pane, ZedGraph.CurveItem curve, int iPt)
        {
            //return curve.Label.Text + " - " + curve.Points[iPt].ToString();
            return curve.Points[iPt].ToString();

        }


        private void testToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //System.IO.StreamReader sr = new
                //   System.IO.StreamReader(openFileDialog1.FileName);
                //RESET

                counter = 0;
                GraphPane myPane = zedGraphControl1.GraphPane;


                zedGraphControl1.GraphPane.CurveList.Clear();
                myPane.GraphObjList.Clear();
                zedGraphControl1.Refresh();
                spl1.Clear();
                spl2.Clear();
                spl3.Clear();
                spl4.Clear();

                LineItem myCurve1 = myPane.AddCurve("ADC1", spl1, Color.Blue, SymbolType.None);
                LineItem myCurve2 = myPane.AddCurve("ADC2", spl2, Color.Red, SymbolType.None);
                LineItem myCurve3 = myPane.AddCurve("ADC3", spl3, Color.Yellow, SymbolType.None);
                LineItem myCurve4 = myPane.AddCurve("ADC4", spl4, Color.Green, SymbolType.None);



                //RESET

                string readcontent;
                readcontent = null;
                //TextWriter tw = new StreamWriter("logs/log.txt");
                using (StreamReader sr = new StreamReader(openFileDialog1.FileName))
                {
                    while (sr.Peek() >= 0)
                    {
                        readcontent = sr.ReadLine();

                        string[] data = readcontent.Split(',');
                        spl1.Add(Convert.ToDouble(data[0]), Convert.ToDouble(data[1]));
                        spl2.Add(Convert.ToDouble(data[0]), Convert.ToDouble(data[2]));
                        spl3.Add(Convert.ToDouble(data[0]), Convert.ToDouble(data[3]));
                        spl4.Add(Convert.ToDouble(data[0]), Convert.ToDouble(data[4]));
                    }
                }


                //string[] datavalue = data.Split(",");




                zedGraphControl1.AxisChange();
                zedGraphControl1.Invalidate();
                zedGraphControl1.Refresh();
                Invalidate();





                // MessageBox.Show(sr.ReadToEnd());
                //sr.Close();
            }
        }

        private void networkSQLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //sqlinsert(123, 123, 123, 123);
        }

        private void serialToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void zedGraphControl1_Load(object sender, EventArgs e)
        {

        }



        private void startSerialToolStripMenuItem_Click(object sender, EventArgs e)
        {
     


        }

        private void startToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            serialPort1.Open();
            serialPort1.Write("f0 5880\r\n");
            serialPort1.Write("f1 5880\r\n");
            serialPort1.Write("f2 5880\r\n");
            serialPort1.Write("f3 5880\r\n");
            serialPort1.Write("r\r\n");
            var filename = $"{DateTime.Now:yyyy.dd.M HH-mm-ss}";
            textBox1.Text = filename;
            timer2.Start();
            counter = 0;
            stopwatch.Start();
            

        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            counter++;
        }

        private void stopToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            timer2.Stop();
            serialPort1.Close();
          
            stopwatch.Stop();
            stopwatch.Reset(); 
        }
    }




    //}


    namespace Ini
    {
        /// <summary>
        /// Create a New INI file to store or load data
        /// </summary>
        public class IniFile
        {
            public string path;

            [DllImport("kernel32")]
            private static extern long WritePrivateProfileString(string section,
                string key, string val, string filePath);
            [DllImport("kernel32")]
            private static extern int GetPrivateProfileString(string section,
                     string key, string def, StringBuilder retVal,
                int size, string filePath);

            /// <summary>
            /// INIFile Constructor.
            /// </summary>
            /// <PARAM name="INIPath"></PARAM>
            public IniFile(string INIPath)
            {
                path = INIPath;
            }
            /// <summary>
            /// Write Data to the INI File
            /// </summary>
            /// <PARAM name="Section"></PARAM>
            /// Section name
            /// <PARAM name="Key"></PARAM>
            /// Key Name
            /// <PARAM name="Value"></PARAM>
            /// Value Name
            public void IniWriteValue(string Section, string Key, string Value)
            {
                WritePrivateProfileString(Section, Key, Value, this.path);
            }

            /// <summary>
            /// Read Data Value From the Ini File
            /// </summary>
            /// <PARAM name="Section"></PARAM>
            /// <PARAM name="Key"></PARAM>
            /// <PARAM name="Path"></PARAM>
            /// <returns></returns>
            public string IniReadValue(string Section, string Key)
            {
                StringBuilder temp = new StringBuilder(255);
                int i = GetPrivateProfileString(Section, Key, "", temp,
                                                255, this.path);
                return temp.ToString();

            }
        }
    }

    namespace file
    {
        public class filewrite
        {

           
            
           
            public void write(string inhalt,string file)
            {


              

                //TextWriter tw = new StreamWriter("logs/log.txt");
                StreamWriter sw = File.AppendText("logs/"+ file+".txt");

                // write a line of text to the file
                sw.Write(inhalt + "\r\n");

                // close the stream
                sw.Close();
            }

            public string read()
            {
                // create a writer and open the file
                //string blub = DateTime.Today.ToString("yyyyMMddhhmmss");



                string readcontent;
                readcontent = null;
                //TextWriter tw = new StreamWriter("logs/log.txt");
                using (StreamReader sr = new StreamReader("logs/log.txt"))
                {
                    while (sr.Peek() >= 0)
                    {
                        readcontent = sr.ReadLine();
                    }
                }
                // close the stream


                return readcontent;
            }

            public string readbak()
            {
                // create a writer and open the file
                //string blub = DateTime.Today.ToString("yyyyMMddhhmmss");
                string readcontent;
                //TextWriter tw = new StreamWriter("logs/log.txt");
                StreamReader sw = new StreamReader("logs/log.txt");





                // write a line of text to the file
                readcontent = sw.ReadToEnd();

                // close the stream
                sw.Close();

                return readcontent;
            }

        }

    }

}
    /*
namespace dbconnect
{

    public class DBConnect
    {
        public MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;

        //Constructor
        public DBConnect()
        {
            Initialize();
        }

        //Initialize values
        private void Initialize()
        {
            server = "localhost";
            database = "temp";
            uid = "newuser";
            password = "newuser";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

            connection = new MySqlConnection(connectionString);
        }

        //open connection to database
        public bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                //When handling errors, you can your application's response based 
                //on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                switch (ex.Number)
                {
                    case 0:
                        MessageBox.Show("Cannot connect to server.  Contact administrator");
                        break;

                    case 1045:
                        MessageBox.Show("Invalid username/password, please try again");
                        break;
                }
                return false;
            }
        }

        //Close connection
        public bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        //Insert statement
        public void Insert()
        {
        }

        //Update statement
        public void Update()
        {
        }

        //Delete statement
        public void Delete()
        {
        }



    }
}

    */
      