using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static int  CountFile = 0;
        static void Main(string[] args)
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
                //Путь пока не создан... 
                try
                {
                    //Пытаемся создать папку:
                    Directory.CreateDirectory(newpath);
                }
                catch (IOException ex)
                {
                    //В случае ошибок ввода-вывода выдаем сообщение об ошибке
                    Console.WriteLine(ex.Message);
                }
                catch (UnauthorizedAccessException ex)
                {
                    //В случае ошибки с нехваткой прав вновь выдаем сообщение:
                    Console.WriteLine(ex.Message);
                }
            }


            int newCountFile = Interlocked.Increment(ref CountFile);
            string filename = Path.Combine(newpath, "File" + Convert.ToString(newCountFile) + ".txt");

            Console.WriteLine(filename);

            byte[] bytes = { 0, 0, 0, 25 };

            File.WriteAllBytes(filename, bytes);

            Console.ReadLine();
        }
    }
}
