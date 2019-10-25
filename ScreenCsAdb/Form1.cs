using SharpAdbClient;
using SharpAdbClient.DeviceCommands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenCsAdb
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button1.Click += new EventHandler(button1_Click);
            Controls.Add(button1);

            button2.Click += new EventHandler(button2_Click);
            Controls.Add(button2);

            button3.Click += new EventHandler(button3_Click);
            Controls.Add(button3);

            button4.Click += new EventHandler(button4_Click);
            Controls.Add(button4);

            button5.Click += new EventHandler(button5_Click);
            Controls.Add(button5);

            button6.Click += new EventHandler(button6_Click);
            Controls.Add(button6);

            button7.Click += new EventHandler(button7_Click);
            Controls.Add(button7);

            openFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(OpenFileDialog1_FileOk);

            var device = AdbClient.Instance.GetDevices().First();




            label1.Text = "Device connected: " + device.Name;
             progressBar1.Value = 0;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            var device = AdbClient.Instance.GetDevices().First();
            var receiver = new ConsoleOutputReceiver();
            DialogResult dialogResult = MessageBox.Show("This will reboot your device. Are you sure?", "Reboot", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                if (radioButton1.Checked)
                {
                    AdbClient.Instance.ExecuteRemoteCommand("reboot", device, receiver);
                }
                else if (radioButton2.Checked)
                {
                    AdbClient.Instance.ExecuteRemoteCommand("reboot recovery", device, receiver);
                }
                else if (radioButton3.Checked)
                {
                    AdbClient.Instance.ExecuteRemoteCommand("reboot bootloader", device, receiver);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var device = AdbClient.Instance.GetDevices().First();
            var receiver = new ConsoleOutputReceiver();
            AdbClient.Instance.ExecuteRemoteCommand("screencap -p /data/local/tmp/screenshot.png", device, receiver);
            using (SyncService service = new SyncService(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)), device))
            using (Stream stream = File.OpenWrite(Path.GetTempPath() + "\\screenshot.png"))
            {
                service.Pull("/data/local/tmp/screenshot.png", stream, null, CancellationToken.None);
                //TODO: Close the stream so you can take another screenshot
            }
            pictureBox1.Image = Image.FromFile(Path.GetTempPath() + "\\screenshot.png");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var device = AdbClient.Instance.GetDevices().First();
            var receiver = new ConsoleOutputReceiver();
            DialogResult dialogResult = MessageBox.Show("Be sure to execute only trusted commands. Run?", "Shell", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                AdbClient.Instance.ExecuteRemoteCommand(textBox1.Text, device, receiver);
                MessageBox.Show(device.Name + " says:\n \n" + receiver.ToString());
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            var device = AdbClient.Instance.GetDevices().First();
            var file = openFileDialog1.ShowDialog();
            if (file == DialogResult.OK)
            {
                String filePath = openFileDialog1.InitialDirectory + openFileDialog1.FileName;
                MessageBox.Show("the file is: " + filePath);
                PackageManager manager = new PackageManager(device);
                manager.InstallPackage(filePath, reinstall: false);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {

        }

        private void OpenFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            if (!openFileDialog1.FileName.ToLower().EndsWith(".apk"))
            {
                e.Cancel = true;
            }
        }
    }
}
