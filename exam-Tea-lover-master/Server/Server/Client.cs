using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    //Класс, который определяет очередного подключенного клиента
    public class Client
    {
        public string Id;//идентификатор
        NetworkStream Stream;//поток клиента
        private TcpClient client;//объект клиент
        private Server server;//Объект сервера

        //конструктор, который инициализирует клиента и сервер
        public Client(TcpClient tcpClient, Server serverObject)
        {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverObject;
        }

        //прослушивание входящих данных
        public void Process()
        {
            try
            {
                Stream = client.GetStream();//получаем поток

                //запускаем бесконечный цикл прослушивания
                while (true)
                {
                    try
                    {
                        string connection_msg = String.Empty;//обнуляем строку с данными

                        byte[] data = new byte[100];//готовим буффер, куда будут загружаться байты
                        StringBuilder builder = new StringBuilder();//билдер, для создания полноценной строки из байт
                        int bytes = 0;//переменна для определения длины доступных для чтения байт
                        
                        //старт чтения данных, даже если их и нет они не считаются, т.к. для чтения задана длина в 0 байт
                        do
                        {
                            bytes = Stream.Read(data, 0, data.Length);//получаем кол-во считанных байт, и одновеременно считываем байты в буффер
                            builder.Append(Encoding.Unicode.GetString(data, 0, bytes));//присоединяем полученные данные к билдеру
                        } while (Stream.DataAvailable);//цикл работает, пока в потоке есть данные

                        connection_msg = builder.ToString();//преобразовавываеем из билдера в полноценную строку

                        Thread.Sleep(20);

                        //если строка не пустая, то обрабатываем сообщение
                        if (!String.IsNullOrEmpty(connection_msg))
                        {
                            Console.WriteLine(connection_msg);
                            ExecuteMessage(connection_msg);
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            finally
            {
                Close();//если мы вышли из цикла прослушивания сообщений, то обрубаем соединение
            }
        }

        //обработчик сообщения
        private void ExecuteMessage(string message)
        {
            //команда имеет вид имя-данные, но данные могут отсутствовать

            string text = String.Empty;//имя
            string data = String.Empty;//данных

            //если в команде есть разделитель "-", то первый разделитель отделяет имя команды от данных
            if (message.IndexOf(":") > -1)
            {
                text = message.Substring(0, message.IndexOf(":"));//получаем имя команды
                data = message.Substring(message.IndexOf(":") + 1);//получаем данные
            }
            else//иначе, сообщение и есть команда
            {
                text = message;
            }

            //обрабатываем полученную команду

            if (text == "4")//команда которая записывает результат игры(счет) в бд
            {
                FileStream aFile;
                StreamWriter sw;
                aFile = new FileStream(server.database_name, FileMode.Append, FileAccess.Write);//инициализируем файл, для записи в конец файла
                StreamWriter writer = new StreamWriter(aFile);//инициализируем стрим райтер этим файлом

                string move = message.Substring(2, 5);
                string move_num = message.Substring(message.LastIndexOf(" ")+1);
                string player = message.Substring(message.IndexOf(" ")+1, 1);
                string fen = message.Substring(8);


                writer.WriteLine("\"" + Id + "\","+ "\"" + player + "\","+ "\"" + move_num + "\"," + "\"" + move + "\"," + "\"" + fen + "\"");

                writer.Close();//закрываем райтер
                aFile.Close();//закрываем файл
            }
            else if (text == "5") //команда, которая отправляет клиенту результаты игр других
            {
                OleDbConnection connectionDB = new OleDbConnection(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" +
                                                                   AppDomain.CurrentDomain.BaseDirectory +
                                                                   ";Extended Properties=text"); //инициируем соединение
                string Select1 = "SELECT * FROM [" + server.database_name + "]"; //записываем команду для выборки данных
                OleDbCommand comand1 = new OleDbCommand(Select1, connectionDB); //соединяем команду и соединение
                OleDbDataAdapter adapter1 = new OleDbDataAdapter(comand1); //создаем адаптер, для заполнения дата сета
                DataSet AllTables = new DataSet(); //создаем дата сет
                connectionDB.Open(); //выполняем соединение
                adapter1.Fill(AllTables); //загружаем в дата сет данные, полученные командой
                connectionDB.Close(); //закрываем соединение

                Console.WriteLine("Всего строк: {0}", AllTables.Tables[0].Rows.Count);

                string result = String.Empty;

                for (int i = 0; i < AllTables.Tables[0].Rows.Count; i++)//данные, которые хранятся в колонке RESULT из дата сета в массив
                {
                    if (AllTables.Tables[0].Rows[i]["ID"].ToString() == data)
                    {
                        result += AllTables.Tables[0].Rows[i]["PLAYER"].ToString() + "|" +
                                 AllTables.Tables[0].Rows[i]["MOVE_NUM"].ToString() + "|" +
                                 AllTables.Tables[0].Rows[i]["MOVE"].ToString() + "|" +
                                 AllTables.Tables[0].Rows[i]["FEN"].ToString() + "?";
                    }
                }

                Console.WriteLine(result);
                SendMessage(result);
            }
        }

        //процедура отправки ответа
        public void SendMessage(string message)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);//переводим строку в байты
            Stream.Write(data, 0, data.Length);//записываем байты в поток
        }

        //процедура закрытия соединения
        public void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (client != null)
                client.Close();
        }
    }
}
