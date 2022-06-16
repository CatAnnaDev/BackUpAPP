namespace BackUpAPP.Logger
{
    internal class RichLogger
    {
        public static void Log(string message)
        {
            RichTextBox? t = Application.OpenForms["Form1"].Controls["richTextBox1"] as RichTextBox;
            t.Text += message + "\n";
        }
    }
}
