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
        DialogResult file;
        IProgress<int> p;
        int screenshotNum = 0;
        int timerDoing = 0;
        int progressValue = 0;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DeviceData device = null;
                try
                {
                    device = AdbClient.Instance.GetDevices().First();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No device connected or no adb server\n\nError: " + ex.Message);
                    Application.Exit();
                }
            
            
            


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
            screenshotNum++;
            var device = AdbClient.Instance.GetDevices().First();
            var receiver = new ConsoleOutputReceiver();
            AdbClient.Instance.ExecuteRemoteCommand("screencap -p /data/local/tmp/screenshot.png", device, receiver);
            using (SyncService service = new SyncService(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)), device))
            using (Stream stream = File.OpenWrite(Path.GetTempPath() + "\\screenshot" + screenshotNum + ".png"))
            {
                service.Pull("/data/local/tmp/screenshot.png", stream, null, CancellationToken.None);
                stream.Dispose();
                stream.Close();
            }
            pictureBox1.Image = Image.FromFile(Path.GetTempPath() + "\\screenshot" + screenshotNum + ".png");
            label5.Visible = true;
            label5.Text = "Saved screenshot " + '"' + "screenshot" + screenshotNum + ".png" + '"';
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
            push();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            pull();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var device = AdbClient.Instance.GetDevices().First();
            var file = openFileDialog1.ShowDialog();
            if (file == DialogResult.OK)
            {
                String filePath = openFileDialog1.InitialDirectory + openFileDialog1.FileName;
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

        private void Progress_Bar(int num)
        {
            progressValue = num;
        }
        private void push()
        {
            timerDoing = 0;
            openFileDialog2.ShowDialog();
            if (file == DialogResult.OK)
            {
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void reallyPush()
        {
            var device = AdbClient.Instance.GetDevices().First();
            p = new Progress<int>(Progress_Bar);
            String newPath = textBox2.Text;
            String filePath = openFileDialog2.InitialDirectory + openFileDialog2.FileName;
            using (SyncService service = new SyncService(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)), device))
            using (Stream stream = File.OpenRead(filePath))
            {
                service.Push(stream, newPath, 444, DateTime.Now, p, CancellationToken.None);
            }
        }

        private void pull()
        {
            timerDoing = 0;
            var file = saveFileDialog1.ShowDialog();
            if (file == DialogResult.OK)
            {
                backgroundWorker2.RunWorkerAsync();
            }
        }

        private void reallyPull()
        {
            var device = AdbClient.Instance.GetDevices().First();
            p = new Progress<int>(Progress_Bar);
            String newPath = textBox2.Text;
            String filePath = saveFileDialog1.InitialDirectory + saveFileDialog1.FileName;
            using (SyncService service = new SyncService(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)), device))
            using (Stream stream = File.OpenWrite(filePath))
            {
                service.Pull(newPath, stream, p, CancellationToken.None);

            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (progressValue != progressBar1.Value)
            {
                progressBar1.Value = progressValue;
            }
            if (progressBar1.Value == 100)
            {
                progressBar1.Value = 0;
            }

        }

        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            reallyPush();
        }

        private void BackgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            reallyPull();
        }

        private void Button8_Click(object sender, EventArgs e)
        {
            var device = AdbClient.Instance.GetDevices().First();
            var receiver = new ConsoleOutputReceiver();
            DialogResult dialogResult = MessageBox.Show("Be sure to execute only trusted apps. Run?", "Run", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                AdbClient.Instance.ExecuteRemoteCommand("monkey -p " + textBox3.Text + " -c android.intent.category.LAUNCHER 1", device, receiver);
                MessageBox.Show(device.Name + " says:\n \n" + receiver.ToString());
            }
        }

        private void Button9_Click(object sender, EventArgs e)
        {
            var device = AdbClient.Instance.GetDevices().First();
            var receiver = new ConsoleOutputReceiver();
            DialogResult dialogResult = MessageBox.Show("Be sure to know what you are doing. Kill?", "Run", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                AdbClient.Instance.ExecuteRemoteCommand("am force-stop --user 0 " + textBox3.Text, device, receiver);
                MessageBox.Show(device.Name + " says:\n \n" + receiver.ToString());
            }
        }
    }

}
