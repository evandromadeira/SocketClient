using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System;

namespace SocketClient
{
    public partial class Client : Form
    {
        TcpClient tcpClient;
        NetworkStream networkStream;
        Thread thInteraction;

        public Client()
        {
            InitializeComponent();
        }

        private void Connect()
        {
            tcpClient = new TcpClient();
            setMsg("## Estabelecendo conexão...");
            tcpClient.Connect("192.168.1.2", 8000);
        }

        private void Disconnect()
        {
            if (thInteraction != null)
            {
                if (thInteraction.ThreadState == ThreadState.Running)
                {
                    thInteraction.Abort();
                }
            }

            tcpClient.Close();
        }

        private void EnviarMsg(string mensagem)
        {
            if (networkStream.CanWrite)
            {
                byte[] sendBytes = Encoding.ASCII.GetBytes(mensagem);
                networkStream.Write(sendBytes, 0, sendBytes.Length);
            }
        }

        delegate void delSetMsg(string mensagem);
        private void setMsg(string mensagem)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new delSetMsg(setMsg), mensagem);
            }
            else
            {
                rtbConversa.Text += "\nEu: " + mensagem;
            }
        }

        delegate void delGetMsg(string mensagem);
        private void getMsg(string mensagem)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new delGetMsg(getMsg), mensagem);
            }
            else
            {
                rtbConversa.Text += "\nServer: " + mensagem;
            }
        }

        private void RtbMensagem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (networkStream.CanWrite)
                {
                    string mensagem = rtbMensagem.Text;
                    EnviarMsg(mensagem);
                    setMsg(mensagem);
                }
                else
                {
                    setMsg("## Não é possível enviar dados deste stream...");
                    Disconnect();
                }
            }
        }

        private void RtbMensagem_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                rtbMensagem.Clear();
            }
        }

        private void interaction()
        {
            try
            {
                do
                {
                    networkStream = tcpClient.GetStream();

                    if (networkStream.CanRead)
                    {
                        byte[] bytes = new byte[tcpClient.ReceiveBufferSize];
                        networkStream.Read(bytes, 0, Convert.ToInt32(tcpClient.ReceiveBufferSize));

                        string returnData = Encoding.ASCII.GetString(bytes);

                        if (!returnData.Replace("\0", "").Trim().Equals(""))
                        {
                            getMsg(returnData);
                        }
                        else
                        {
                            tcpClient = null;
                        }
                    }
                    else
                    {
                        setMsg("## Não é possível ler dados para estre stream...");
                        Disconnect();
                    }
                } while (tcpClient.Connected);
            }
            catch
            {

            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Connect();

            thInteraction = new Thread(new ThreadStart(interaction));
            thInteraction.IsBackground = true;
            thInteraction.Priority = ThreadPriority.Highest;
            thInteraction.Name = "thInteraction";
            thInteraction.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Disconnect();
        }
    }
}
