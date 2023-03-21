using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Net.Http;
using System.Diagnostics.CodeAnalysis;

namespace server
{
    internal class Program
    {
        class server
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8888); //прослушивание
            List<client> clients = new List<client>(); //подключенные
            protected internal async Task start()
            {
                try
                {
                    tcpListener.Start();
                    Console.WriteLine("Server start...");
                    while (true)
                    {
                        TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();

                        client Client = new client(tcpClient, this);
                        clients.Add(Client);
                        Task.Run(()=>Client.Process());
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    Close();
                }
            }
            protected internal async Task SendMessage(string message, string login) //отправка сообщения
            {
                foreach (var item in clients)
                {
                    if (login!=item.login)
                    {
                        await item.writer.WriteAsync(message);
                        await item.writer.FlushAsync();
                        Console.WriteLine("Message send: " + message);
                    }
                    
                }
            }
            protected internal void kick(string login)
            {
                client Client = clients.FirstOrDefault(x => x.login == login);

                if (Client != null) { clients.Remove(Client); }
                Client?.Close();
            }
            protected internal void Close() //выключение сервера
            {
                foreach (var item in clients)
                {
                    item.Close();
                }
                tcpListener.Stop();
            }
        }
        class client
        {
            TcpClient tcpClient;
            server Server;
            protected internal string login { get; } = Guid.NewGuid().ToString();
            protected internal StreamReader reader { get; }
            protected internal StreamWriter writer { get; }


            public client(TcpClient client, server serverObj) //коструктор класса
            {
                tcpClient = client;
                Server = serverObj;
                var a = client.GetStream();
                writer = new StreamWriter(a); //отправка
                reader = new StreamReader(a); //чтение
            }

            public async Task Process() //процесс получения и отправки сообщений
            {
                try
                {
                    string userName = await reader.ReadLineAsync();
                    string message = userName + " join";

                    await Server.SendMessage(message, login);
                    Console.WriteLine(message);
                    
                    while (true)
                    {
                        try
                        {
                            message = await reader.ReadLineAsync();
                            if (message == null)
                            {
                                continue;
                            }
                            message = userName+ ": " + message;
                            Console.WriteLine(message);

                            await Server.SendMessage(message, login);
                        }
                        catch
                        {
                            message = userName + " leave";
                            Console.WriteLine(message);
                            await Server.SendMessage(message, login);
                            break;
                        }
                    
                    }
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    Server.kick(login);
                }
            }
            protected internal void Close() //закрытие всех открытых подключений
            {
                writer.Close();
                reader.Close();
                tcpClient.Close();
            }
        }

            static void Main(string[] args)
            {
                server Server = new server(); //Создаем сервер
                var task = Server.start();
                task.Wait();
            
            }
    }
}
