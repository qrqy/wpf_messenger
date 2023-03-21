using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace messengerB
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string userName = "B";
        StreamReader reader = null;
        StreamWriter writer = null;
        public MainWindow()
        {
            InitializeComponent();
            string ip = "127.0.0.1";
            int port = 8888;
            TcpClient client = new TcpClient();

            try
            {
                client.Connect(ip, port);
                reader = new StreamReader(client.GetStream());
                writer = new StreamWriter(client.GetStream());
                if (writer is null || reader is null)
                {
                    return;
                }
                Task task = writer.WriteLineAsync(userName);
                task.Wait();
                task = writer.FlushAsync();
                task.Wait();
                Task.Run(()=> GetMessage(reader));
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Task task = SendMessage(writer);
            TextBoxInput.Focus();
        }
        async Task GetMessage(StreamReader Reader)
        {
            try
            {
                while (true)
                {
                    string message = await Reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(message))
                    {
                        continue;
                    }
                    else
                    {
                        TextBoxOutput.Text = TextBoxOutput.Text + "\n" + message;
                        TextBoxOutput.ScrollToEnd();
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            

        }
        async Task SendMessage(StreamWriter Writer)
        {
            string message = TextBoxInput.Text;
            if (!string.IsNullOrEmpty(message))
            {
                await Writer.WriteLineAsync(message);
                await Writer.FlushAsync();
                TextBoxOutput.Text = TextBoxOutput.Text + "\n" + userName + ": " + message;
                TextBoxOutput.ScrollToEnd();
                TextBoxInput.Clear();
            }
        }

        private void TextBoxInput_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Task task = SendMessage(writer);
                TextBoxInput.Focus();
            }
        }
    }
}
