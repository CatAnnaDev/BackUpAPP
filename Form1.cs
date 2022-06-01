using BackUpAPP.Config;
using BackUpAPP.CopyProcess;
using BackUpAPP.GetDirectory;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace BackUpAPP
{
    public partial class Form1 : Form
    {
        private static ConfigInit? _configinit;
        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            // Init Config File
            _configinit = new ConfigInit();
            await _configinit.InitializeAsync();
            
            // Add known Folders in config file
            ConfigInit.Config.Path = KnownFolders.GetPath().ToArray();
            
            // Add folders in listbox
            foreach (var data in ConfigInit.Config.Path)
            {
                var matchingvalues = listBox1.Items.Contains(data);
                if (!matchingvalues)
                    listBox1.Items.Add(data);
            }
            
        }

        // Update config file with new path or removed
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            UpdateConfig.UpdateConfigFile(listBox1);
        }

        // Detect delete keycode to remove item in listbox
        private void Form1_KeyPress(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Delete)
            {
                if (listBox1.SelectedIndex != -1)
                {
                    listBox1.Items.RemoveAt(listBox1.SelectedIndex);
                    Thread.Sleep(50);
                }
            }
        }

        // add item
        private void button2_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.InitialDirectory = @"C:\";
                fbd.ShowNewFolderButton = true;

                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    var matchingvalues = listBox1.Items.Contains(fbd.SelectedPath);
                    if(!matchingvalues)
                        listBox1.Items.Add(fbd.SelectedPath);
                    else
                        MessageBox.Show("Path exist already");

                }
            }
        }

        // delete item
        private void button3_Click(object sender, EventArgs e)
        {
            if(listBox1.SelectedIndex != -1)
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
        }


        // Ask to start backup
        public static string backupfolderPath = "";
        private void button1_Click(object sender, EventArgs e)
        {
            const string message = "Do you want start the backup process ?";
            const string caption = "Starting Backup";

            if (listBox1.Items.Count == 0)
                MessageBox.Show("Please select folders befor starting");
            else
            {

                using (var fbd = new FolderBrowserDialog())
                {
                    fbd.InitialDirectory = @"C:\";
                    fbd.ShowNewFolderButton = true;
                    DialogResult result = fbd.ShowDialog();

                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        Regex PathValidation = new Regex("^([a-zA-Z]:)?(\\\\[^<>:\"/\\\\|?*]+)+\\\\?$");
                        if (PathValidation.IsMatch(fbd.SelectedPath))
                        {
                            backupfolderPath = fbd.SelectedPath;
                            var msg = MessageBox.Show(message, caption,MessageBoxButtons.YesNo,MessageBoxIcon.Question);

                            if (msg == DialogResult.Yes)
                            {
                                startbachup();                            
                            }
                        }
                        else
                            MessageBox.Show("Invalid path !");
                    }
                }

            }
        }

        // Backup process
        private async void startbachup()
        {
            List<string> tmp = new();
            for (int i = 0; i < listBox1.Items.Count; i++)
                tmp.Add(listBox1.Items[i].ToString());
            await DataCopy.FFCopy(tmp.ToArray());
        }
    }
}