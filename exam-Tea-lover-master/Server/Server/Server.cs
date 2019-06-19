using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class Server
    {
        TcpListener tcpListener; //слушатель соединения
        public List<Client> clients = new List<Client>(); //лист с клиентами
        public string database_name = "Database.txt";//файл базы данных, который лежит рядом с сервером

        //конструктор сервера
        public Server()
        {
            //проверяет, лежит ли рядом файл бд, если нет, то создает его
            if (!File.Exists(database_name))
            {
                StreamWriter writer = new StreamWriter(database_name);//создаем райтер
                writer.WriteLine("\"ID\",\"PLAYER\",\"MOVE_NUM\",\"MOVE\",\"FEN\"");//записываем строку с именами столбцов
                writer.Close();//закрываем райтер
            }
        }

        //добавление соединения к листу
        public void AddConnection(Client clientObject)
        {
            clients.Add(clientObject);
        }

        //удаления соединения из листа
        public void RemoveConnection(string id)
        {
            
            Client client = clients.FirstOrDefault(c => c.Id == id);//получаем клиента из листа клиентов с нужным Id
            if (client != null)
                clients.Remove(client);//закрываем соединение
        }

        //слушатель подключений
        public void Listen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 6589);//инициализируем слушатель подключений
                tcpListener.Start();//запускаем его

                //бесконечный цикл создания подключений, если такие поступают
                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();//если есть соединение
                    Client clientObject = new Client(tcpClient, this);//создаем клиента
                    AddConnection(clientObject);//добавляем его в лист клиентом

                    Task.Run(() => clientObject.Process());//запускаем новом потоке прослушку сообщений от клиента
                }
            }
            catch (Exception ex)
            {
                Disconnect();//отключение сервера в случае проблем
            }
        }

        //отключение сервера
        public void Disconnect()
        {
            tcpListener.Stop();//остановка слушателя
            for (int i = 0; i < clients.Count; i++)//отключение всех клиентов
            {
                clients[i].Close();
            }
            Environment.Exit(0);//выход из программы
        }
    }
}
