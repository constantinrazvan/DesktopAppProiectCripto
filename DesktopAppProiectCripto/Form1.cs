using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DesktopAppProiectCripto
{
    public partial class Form1 : Form
    {

        private static Collection<String> unEncryptedTextsCollection;
        private int nextId = 1;

        public Form1()
        {

            InitializeComponent();
            textBox2.TextChanged += textBox2_TextChanged;
            label5.Text = "";

            listBox1.Items.Add("Text 1");
            listBox2.Items.Add("Text 2");

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            int byteLength = Encoding.UTF8.GetByteCount(textBox2.Text);
            if (byteLength == 8)
            {
                label5.ForeColor = Color.Green;
                label5.Text = "The key length is exactly 8 bytes";
            }
            else
            {
                label5.ForeColor = Color.Red;
                label5.Text = byteLength > 8 ? "The key length is over 8 bytes" : "The key length is under 8 bytes";
            }

        }

        private string originalText = "";
        
        private void Form1_Load(object sender, EventArgs e)
        {
        
            label4.BackColor = Color.Transparent;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                if (string.IsNullOrEmpty(originalText))
                {
                    originalText = textBox2.Text;
                }
                textBox2.Text = new string('*', textBox2.Text.Length);
            }
            else
            {
                textBox2.Text = originalText;
                originalText = "";
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox3.Checked)
            {
                this.checkBox2.Checked = false;
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox2.Checked)
            {
                this.checkBox3.Checked = false;

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string text = textBox1.Text;
            string key = textBox2.Text;

            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show("You have to fill the TEXT field!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            if (string.IsNullOrEmpty(key))
            {
                MessageBox.Show("You have to fill the KEY field!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (key.Length != 8)
            {
                MessageBox.Show("The KEY must be exactly 8 bytes long.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string encryptedText = "";

            if (checkBox2.Checked)
            {
                RC5 rc5 = new RC5(key);
                encryptedText = rc5.Encrypt(text);

                listBox1.Items.Add($"Original: {textBox1.Text}");
                listBox2.Items.Add($"Encrypted with RC5: {encryptedText}");
            }
            else if (checkBox3.Checked)
            {
                encryptedText = DES.Encrypt(text, key);

                listBox1.Items.Add($"Original: {textBox1.Text}");
                listBox2.Items.Add($"Encrypted with DES: {encryptedText}");
            }
            else
            {
                MessageBox.Show("Please select an encryption method (RC5 or DES).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            nextId++;
            textBox1.Clear();
            textBox2.Clear();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox2.SelectedIndex == -1)
            {
                MessageBox.Show("Please select an item from the encrypted list (listBox2)!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedItem = listBox2.SelectedItem.ToString();
            string encryptedText = selectedItem.Substring(selectedItem.IndexOf(':') + 1).Trim();
            string key = PromptForKey();

            if (string.IsNullOrEmpty(key))
            {
                MessageBox.Show("No key entered. Cannot decrypt.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string decryptedText = "";
            try
            {
                if (selectedItem.StartsWith("Encrypted with RC5: "))
                {
                    RC5 rc5 = new RC5(key);
                    decryptedText = rc5.Decrypt(encryptedText, key);
                }
                else if (selectedItem.StartsWith("Encrypted with DES: "))
                {
                    decryptedText = DES.Decrypt(encryptedText, key);
                }

                if (string.IsNullOrEmpty(decryptedText))
                {
                    throw new Exception("Decryption failed.");
                }

                MessageBox.Show("Decrypted Text: " + decryptedText, "Decrypted", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to decrypt. Check the key and try again.\n" + ex.Message, "Decryption Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private string PromptForKey()
        {
            using (Form prompt = new Form())
            {
                prompt.Width = 500;
                prompt.Height = 150;
                prompt.Text = "Enter Decryption Key";
                Label textLabel = new Label() { Left = 50, Top = 20, Text = "Key:" };
                TextBox textBox = new TextBox() { Left = 50, Top = 50, Width = 400 };
                Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 70, DialogResult = DialogResult.OK };
                confirmation.Click += (sender, e) => { prompt.Close(); };
                prompt.Controls.Add(textBox);
                prompt.Controls.Add(confirmation);
                prompt.Controls.Add(textLabel);
                prompt.AcceptButton = confirmation;

                return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
            }
        }
    }
}