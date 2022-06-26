using BackUpAPP.Config;
using BackUpAPP.CopyProcess;
using BackUpAPP.GetDirectory;
using BackUpAPP.GetDirectorySize;
using BackUpAPP.Logger;
using System.Diagnostics;
using System.Net;
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

            // Add Folders from db file
            _configinit.ReadConfig();
            if(_configinit.TMPPath.Count == 0)
                _configinit.TMPPath = KnownFolders.GetPath().ToList();
            

            foreach (var data in _configinit.TMPPath)
            {
                _configinit.WritePathConfig(data);
            }

            // Add folders in listbox
            foreach (var data in _configinit.TMPPath)
            {
                var matchingvalues = listBox1.Items.Contains(data);
                if (!matchingvalues)
                    listBox1.Items.Add(data);
            }

            // get backup size at launch
            label2.Text = $"BackupSize: {BackupSize()}";

        }

        // Update config file with new path or removed
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            UpdateConfig.UpdateConfigFile(listBox1);
        }

        // Detect delete keycode to remove item in listbox
        private void Form1_KeyPress(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (listBox1.SelectedIndex != -1)
                {
                    string RemoveItem = listBox1.GetItemText(listBox1.SelectedItem);
                    _configinit.UpdatePathConfig(RemoveItem);
                    listBox1.Items.RemoveAt(listBox1.SelectedIndex);
                    Thread.Sleep(50);
                }
                label2.Text = $"BackupSize: {BackupSize()}";
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
                    if (!matchingvalues)
                        listBox1.Items.Add(fbd.SelectedPath);
                    else
                        MessageBox.Show("Path exist already");

                }
                label2.Text = $"BackupSize: {BackupSize()}";
            }
        }

        // delete item
        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                string RemoveItem = listBox1.GetItemText(listBox1.SelectedItem);
                _configinit.UpdatePathConfig(RemoveItem);
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
                
            }
                
            label2.Text = $"BackupSize: {BackupSize()}";
        }

        // Ask to start backup
        public static string backupfolderPath = "";
        private void button1_Click(object sender, EventArgs e)
        {
            List<string> tmp = new();
            for (int i = 0; i < listBox1.Items.Count; i++)
                tmp.Add(listBox1.Items[i].ToString());
            var total = DataCopy.GetSize(tmp.ToArray());
            var cmp = DataCopy.GetSize(tmp.ToArray(), false);

            DriveInfo dDrive;

            if (listBox1.Items.Count == 0)
            {
                MessageBox.Show("Please select folders befor starting");
                return;
            }
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
                            UpdateConfig.UpdatePath(fbd.SelectedPath); // WIP

                            dDrive = new DriveInfo(backupfolderPath.Substring(0, 1));

                            string message = $"Do you want start the backup process ?\nBe sure you've enough space \nBackup Size {BackupSize()} Free Space {DirSize.SizeSuffix(dDrive.TotalFreeSpace)}";
                            string caption = "Starting Backup";

                            var msg = MessageBox.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            label1.Text = backupfolderPath;
                            if (msg == DialogResult.Yes && long.Parse(cmp) < dDrive.TotalFreeSpace)
                            {
                                startbachup();
                            }
                            else
                            {
                                MessageBox.Show("Not enough space !");
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
            RichLogger.Log("Done !");
        }

        // Winget Backup
        private async void button4_Click(object sender, EventArgs e)
        {
            if (backupfolderPath == string.Empty)
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
                        }
                        else
                            MessageBox.Show("Invalid path !");
                    }
                }
            }
            await Exec("cmd.exe", $"winget export -o {backupfolderPath}\\WinGet.json");
        }

        // Chocolatey Backup
        private async void button5_Click(object sender, EventArgs e)
        {
            if (backupfolderPath == string.Empty)
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
                        }
                        else
                            MessageBox.Show("Invalid path !");
                    }
                }
            }

            // Chocolatey upgrade
            await Exec("cmd.exe", $"choco upgrade chocolatey -y");

            // Make a Chocolatey export 
            await Exec("cmd.exe", $"choco export --output-file-path=\"'{backupfolderPath}\\ChocolateyBackup.config'\"");
        }

        // Winget Import .JSON
        private async void button6_Click(object sender, EventArgs e)
        {
            var filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "json files (*.json)|*.json";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;
                    await Exec("cmd.exe", $"winget -i {filePath} --ignore-unavailable --ignore-versions --accept-package-agreements --accept-source-agreements --verbose-logs");
                }
            }
        }

        // Chocolatey Import .config
        private async void button7_Click(object sender, EventArgs e)
        {
            var filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "config files (*.config)|*.config";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;
                    await Exec("powershell.exe", $"choco install {filePath} -y -n");
                }
            }
        }

        // Chocolatey install 
        private async void button9_Click(object sender, EventArgs e)
        {
            await Exec("powershell.exe", "Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iwr https://community.chocolatey.org/install.ps1 -UseBasicParsing | iex");
        }

        // winget install
        private async void button8_Click(object sender, EventArgs e)
        {
            await Exec("cmd.exe", $"start ms-windows-store://pdp/?ProductId=9NBLGGH4NNS1");
        }

        // Download Win11 ISO 
        private async void button11_Click(object sender, EventArgs e)
        {
            // set lang && set edition
            // https://www.microsoft.com/fr-fr/software-download/windows11
            // MediaCreationToolW11.exe /Eula Accept /Retail /MediaArch x64 /MediaLangCode en-US /MediaEdition Pro

            string remoteUri = "https://go.microsoft.com/fwlink/?linkid=2156295";
            string fileName = "MediaCreationToolW11.exe";
            WebClient myWebClient = new WebClient();
            RichLogger.Log($"Downloading File {fileName}");
            myWebClient.DownloadFile(remoteUri, fileName);
            RichLogger.Log($"Successfully Downloaded File {fileName}");

            await Exec("cmd", $"MediaCreationToolW10.exe");
        }

        // Download Win10 ISO 
        private async void button10_Click(object sender, EventArgs e)
        {
            // set lang && set edition
            // https://www.microsoft.com/fr-fr/software-download/windows10
            // MediaCreationTool21H2.exe /Eula Accept /Retail /MediaArch x64 /MediaLangCode en-US /MediaEdition Pro

            string remoteUri = "https://go.microsoft.com/fwlink/?LinkId=691209";
            string fileName = "MediaCreationTool21H2.exe";
            WebClient myWebClient = new WebClient();
            RichLogger.Log($"Downloading File {fileName}");
            myWebClient.DownloadFile(remoteUri, fileName);
            RichLogger.Log($"Successfully Downloaded File {fileName}");

            await Exec("cmd", $"MediaCreationTool21H2.exe");
        }

        // Exec CMD
        private static Task Exec(string filename, string cmd)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            startInfo.FileName = filename;
            startInfo.Arguments = $"/C " + cmd;
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            return Task.CompletedTask;
        }

        private string BackupSize()
        {
            List<string> tmp = new();
            for (int i = 0; i < listBox1.Items.Count; i++)
                tmp.Add(listBox1.Items[i].ToString());
            return DataCopy.GetSize(tmp.ToArray());
        }

        private void listBox1_DragDrop(object sender, DragEventArgs e)
        {
            List<string> filepaths = new List<string>();
            foreach (var s in (string[])e.Data.GetData(DataFormats.FileDrop, false))
            {
                if (Directory.Exists(s))
                {
                    //Add files from folder
                    filepaths.AddRange(Directory.GetFiles(s));
                }
                else
                {
                    //Add filepath
                    filepaths.Add(s);
                }
            }
        }

    }
}