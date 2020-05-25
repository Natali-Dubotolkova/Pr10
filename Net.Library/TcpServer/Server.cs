using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SomeProject.Library.Server
{
    public class Server
    {
        TcpListener serverListener;
        int CountFile = 0;
        int CountClient = 0;
        int CountMax = 2;

        byte[] buf;

        public Server()
        {
            serverListener = new TcpListener(IPAddress.Loopback, 8080);
        }

        /// <summary>
        /// Создает директорию, если не была создана и файл с именем "File(Номер полученного файла).(Расширение файла);"
        /// </summary>
        /// <returns>Путь до файла</returns>
        public string CreateFile(string ext)
        {
            //Получаем текущую дату в кратком виде с учетом формата даты системы
            string directorydate = DateTime.Now.ToString("yyyy.MM.dd");

            //Путь к нужному месту:
            string basepath = @"G:\Политех\C#\Practice10\";

            //Получаем новый путь, состоящий из пути к исполняемому файлу + папка с текущей датой:
            string newpath = Path.Combine(basepath, directorydate);

            //Проверяем, не создан ли данный путь в предыдущие запуски программы.
            if (!Directory.Exists(newpath))
            {
                //Пытаемся создать папку:
                Directory.CreateDirectory(newpath);

            }

            int newCountFile = Interlocked.Increment(ref CountFile);
            string filename = Path.Combine(newpath, "File" + Convert.ToString(newCountFile) + ext);
            return filename;
        }


        public bool TurnOffListener()
        {
            try
            {
                if (serverListener != null)
                    serverListener.Stop();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot turn off listener: " + e.Message);
                return false;
            }
        }

        /// <summary>
        /// Слушатель для получения файла
        /// </summary>
        /// <returns></returns>
        public async Task TurnOnListener2()
        {
            try
            {
                if (serverListener != null)
                {
                    serverListener.Start();
                }
                    
                
                while (true)
                {
                    OperationResult result = await ReceiveFileFromClient();
                    if (result.Result == Result.Fail)
                        Console.WriteLine("Unexpected error: " + result.Message);
                    else
                        Console.WriteLine("File was sent succesfully");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot turn on listener: " + e.Message);
            }
        }

        /// <summary>
        /// Слушатель для получения сообщения
        /// </summary>
        /// <returns></returns>
        public async Task TurnOnListener()
        {
            try
            {
                if (serverListener != null)
                    serverListener.Start();
                while (true)
                {
                    OperationResult result = await ReceiveMessageFromClient();
                    if (result.Result == Result.Fail)
                        Console.WriteLine("Unexpected error: " + result.Message);
                    else if (result.Result == Result.Toomuch)
                        Console.WriteLine("Toomuch clients: " + result.Message);
                    else Console.WriteLine("New message from client: " + result.Message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot turn on listener: " + e.Message);
            }
        }

        public async Task<OperationResult> ReceiveMessageFromClient()
        {
            try
            {
                Console.WriteLine("Waiting for connections...");

                //принимаем новых клиентов
                TcpClient client = serverListener.AcceptTcpClient();

                int newCountClient = Interlocked.Increment(ref CountClient);
                if (newCountClient >= CountMax)
                {
                    client.Close();
                    return new OperationResult(Result.Toomuch, "Stop!");
                }


                byte[] data = new byte[256];
                StringBuilder recievedMessage = new StringBuilder();
                NetworkStream stream = client.GetStream();
                
                do
                {
                    int bytes = stream.Read(data, 0, data.Length);
                    recievedMessage.Append(Encoding.UTF8.GetString(data, 0, bytes));
                }
                while (stream.DataAvailable);

                stream.Close();
                client.Close();

                return new OperationResult(Result.OK, recievedMessage.ToString());
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        }

        public async Task<OperationResult> ReceiveFileFromClient()
        {
            try
            {
                Console.WriteLine("Waiting for file...");
                TcpClient client = serverListener.AcceptTcpClient();

                NetworkStream readerStream = client.GetStream();
                BinaryFormatter outformat = new BinaryFormatter();

                string filename;

                string ext;
                ext = outformat.Deserialize(readerStream).ToString();//Получаем формат файла

                try
                {
                    //Пытаемся создать папку и файл:
                    filename = CreateFile(ext);
                }
                catch (IOException ex)
                {
                    //В случае ошибок ввода-вывода выдаем сообщение об ошибке
                    return new OperationResult(Result.Fail, ex.Message);
                }
                catch (UnauthorizedAccessException ex)
                {
                    //В случае ошибки с нехваткой прав вновь выдаем сообщение:
                    return new OperationResult(Result.Fail, ex.Message);
                }

                do
                {
                    //Читаем из потока и записываем в массив
                     buf = (byte[])(outformat.Deserialize(readerStream));
                }
                while (readerStream.DataAvailable);


                //записываем массив данных в файл
                File.WriteAllBytes(filename, buf);

                
                readerStream.Close();
                client.Close();

                return new OperationResult(Result.OK, "");
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        }

        public OperationResult SendMessageToClient(string message)
        {
            try
            {
                //инициализация
                TcpClient client = serverListener.AcceptTcpClient();
                NetworkStream stream = client.GetStream();
                byte[] data = Encoding.UTF8.GetBytes(message);

                //отправка сообщения
                stream.Write(data, 0, data.Length);
                
                stream.Close();
                client.Close();
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
            return new OperationResult(Result.OK, "");
        }


    }
}