using System;
using System.Text;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Threading;

namespace SomeProject.Library.Client
{
    public class Client
    {
        public TcpClient tcpClient;
        public OperationResult SendMessageToServer(string message)
        {
            try
            {

                //инициализация
                tcpClient = new TcpClient("127.0.0.1", 8080);


                NetworkStream stream = tcpClient.GetStream();
                byte[] data = System.Text.Encoding.UTF8.GetBytes(message);

                //отправка сообщения
                stream.Write(data, 0, data.Length);

                stream.Close();
                tcpClient.Close();
                return new OperationResult(Result.OK, "") ;
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        }

        public OperationResult SendFileToServer(string path)
        {
            try
            {
                tcpClient = new TcpClient("127.0.0.1", 8080);
                NetworkStream writerStream = tcpClient.GetStream(); 
                BinaryFormatter format = new BinaryFormatter();

                byte[] buf = new byte[1024];
                int count;

                FileStream fs = new FileStream(path, FileMode.Open);
                BinaryReader br = new BinaryReader(fs);

                string ext = Path.GetExtension(path);
                format.Serialize(writerStream, ext);//Вначале передаём формат

                while ((count = br.Read(buf, 0, 1024)) > 0)
                {
                    format.Serialize(writerStream, buf);//Передаем файл в цикле по 1024 байта
                }

                fs.Close();
                writerStream.Close();
                tcpClient.Close();

                return new OperationResult(Result.OK, "");
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        }


        public OperationResult ReceiveMessageFromServer()
        {
            try
            {
                tcpClient = new TcpClient("127.0.0.1", 8080);
                StringBuilder recievedMessage = new StringBuilder();
                byte[] data = new byte[256];
                NetworkStream stream = tcpClient.GetStream();

                do
                {
                    int bytes = stream.Read(data, 0, data.Length);
                    recievedMessage.Append(Encoding.UTF8.GetString(data, 0, bytes));
                }
                while (stream.DataAvailable);
                stream.Close();
                tcpClient.Close();
                

                return new OperationResult(Result.OK, recievedMessage.ToString());
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.ToString());
            }
        }
    }
}
