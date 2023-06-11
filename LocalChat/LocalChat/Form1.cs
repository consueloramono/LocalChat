using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace LocalChat
{
    public partial class Form1 : Form
    {
        Socket sck;
        EndPoint epLocal, epRemote;

        public Form1()
        {
            InitializeComponent();
            sck = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sck.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            textLocalIp.Text = GetLocalIP();
            textFriendsIp.Text = GetLocalIP();
            send.Enabled = false;
        }

        // Отримання локальної IP-адреси
        private string GetLocalIP()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }

        // Обробник події кнопки "Зв'язатись"
        private void start_Click(object sender, EventArgs e)
        {
            try
            {
                // Прив'язка сокета до локальної IP-адреси та порту
                epLocal = new IPEndPoint(IPAddress.Parse(textLocalIp.Text), Convert.ToInt32(textLocalPort.Text));
                sck.Bind(epLocal);

                // Встановлення з'єднання з віддаленою IP-адресою та портом
                epRemote = new IPEndPoint(IPAddress.Parse(textFriendsIp.Text), Convert.ToInt32(textFriendsPort.Text));
                sck.Connect(epRemote);

                // Розпочати асинхронне отримання повідомлень
                byte[] buffer = new byte[1500];
                sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);

                start.Text = "Приєднано";
                start.Enabled = false;
                send.Enabled = true;
                textMassage.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        // Обробник асинхронного отримання повідомлень
        private void MessageCallBack(IAsyncResult asyncResult)
        {
            try
            {
                int size = sck.EndReceiveFrom(asyncResult, ref epRemote);
                if (size > 0)
                {
                    byte[] receivedData = new byte[1464];
                    receivedData = (byte[])asyncResult.AsyncState;
                    UTF8Encoding eEncoding = new UTF8Encoding();
                    string receivedMessage = eEncoding.GetString(receivedData);
                    listMessage.Items.Add("Людина 2 " + receivedMessage);
                }
                byte[] buffer = new byte[1500];
                sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }
        }

        // Обробник події кнопки "Відправити"
        private void send_Click(object sender, EventArgs e)
        {
            try
            {
                // Кодування повідомлення та відправка через сокет
                UTF8Encoding enc = new UTF8Encoding();
                byte[] msg = enc.GetBytes(textMassage.Text);
                sck.Send(msg);

                // Додавання повідомлення до списку
                listMessage.Items.Add("Людина 1 " + textMassage.Text);
                textMassage.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
