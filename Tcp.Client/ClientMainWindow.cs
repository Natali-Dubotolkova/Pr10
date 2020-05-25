using System;
using System.Windows.Forms;
using SomeProject.Library.Client;
using SomeProject.Library;
using System.IO;
using System.Threading;

namespace SomeProject.TcpClient
{
    public partial class ClientMainWindow : Form
    {
        public ClientMainWindow()
        {
            InitializeComponent();

        }

        string path;

        /// <summary>
        /// Обработчик кнопки загрузки файла
        /// </summary>
        private void OpenFileBtnClick(object sender, EventArgs e)
        {
            openFileDialog1.DefaultExt = ".*;";
            openFileDialog1.Filter = "All files (*.*)|*.*";
            openFileDialog1.Title = "Выберите файл";
            openFileDialog1.FileName = "";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                path = openFileDialog1.FileName;
                textBox2.Text = path;
                try
                {
                    using (StreamReader sr = new StreamReader(path))
                    {
                        textBox.Text = sr.ReadToEnd();
                    }
                }
                catch (Exception exp)
                {
                    Console.WriteLine(exp.Message);
                }
            }
        }

        /// <summary>
        /// Обработчик кнопки отправки сообщения на сервер
        /// </summary>
        private void OnMsgBtnClick(object sender, EventArgs e)
        {
            Client client = new Client();
            OperationResult OPres = client.SendMessageToServer(textBox.Text);
            Result res = OPres.Result;
            string mes = OPres.Message;
             if (res == Result.OK)
            {
                textBox.Text = "All right!";
                labelRes.Text = "Message was sent succesfully!";
            }
            else
            {
                textBox.Text = mes;
                labelRes.Text = "Cannot send the message to the server.";
            }
            timer.Interval = 3000;
            timer.Start();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            labelRes.Text = "";
            timer.Stop();
        }
        /// <summary>
        /// Обработчик кнопки отправки файла на сервер
        /// </summary>
        private void OnFileBtnClick(object sender, EventArgs e)
        {
            Client client = new Client();
            OperationResult OPres = client.SendFileToServer(textBox2.Text);
            Result res = OPres.Result;
            string mes = OPres.Message;

            if (res == Result.OK)
            {
                textBox.Text = "All right!";
                labelRes.Text = "File was sent succesfully!";
            }
            else
            {
                textBox.Text = mes;
                labelRes.Text = "Cannot send the file to the server.";
            }
            timer.Interval = 3000;
            timer.Start();
        }

        private void ClientMainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }
    }
}
