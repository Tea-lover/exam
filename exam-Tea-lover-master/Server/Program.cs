using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        static Server server; //объект сервера
        static Thread listenThread; //поток прослушивания соединения

        static void Main(string[] args)
        {
            try
            {
                server = new Server();//инициализация объекта сервера
                listenThread = new Thread(new ThreadStart(server.Listen));//инициализация потока
                listenThread.Start(); // запуск прослушивания подключений
            }
            catch (Exception ex)//в случае ошибок закрываем сервер и выдаем сообщение об ошибке
            {
                server.Disconnect();
                Console.WriteLine(ex.Message);
            }
        }
    }
}
