using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatClient
{
    public partial class Form1 : Form
    {
        private string NomeUsuario;
        private StreamWriter stwEnviador;
        private StreamReader strReceptor;
        private TcpClient tcpServidor;
        private delegate void AtualizaLogCallBack(string strMensagem);
        private delegate void FechaConexaoCallBack(string strMotivo);
        private Thread mensagemThread;
        private IPAddress enderecoIP;
        private int portaHost;
        private bool Conectado;

        public Form1()
        {
            Application.ApplicationExit += new EventHandler(OnApplicationExit);
            InitializeComponent();
        }

        private void btnConectar_Click(object sender, EventArgs e)
        {
            if (!Conectado)
            {
                Connection();
            }
            else 
            {
                Disconnect("Disconnected");
            }
        }

        private void btnEnviar_Click(object sender, EventArgs e)
        {
            Send();
        }

        private void txtMensagem_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                Send();
            }
        }

        private void Connection()
        {
            try
            {
                enderecoIP = IPAddress.Parse(txtServidorIP.Text);
                portaHost = (int)numPortaHost.Value;
                tcpServidor = new TcpClient();
                tcpServidor.Connect(enderecoIP, portaHost);
                Conectado = true;
                NomeUsuario = txtUsuario.Text;
                txtServidorIP.Enabled = false;
                numPortaHost.Enabled = false;
                txtUsuario.Enabled = false;
                txtMensagem.Enabled = true;
                btnEnviar.Enabled = true;
                btnConectar.ForeColor = Color.Red;
                btnConectar.Text = "Disconnect";

                stwEnviador = new StreamWriter(tcpServidor.GetStream());
                stwEnviador.WriteLine(txtUsuario.Text);
                stwEnviador.Flush();

                mensagemThread = new Thread(new ThreadStart(Receive));
                mensagemThread.IsBackground = true;
                mensagemThread.Start();

                labelStatus.Invoke(new Action(() =>
                {
                    labelStatus.ForeColor = Color.Green;
                    labelStatus.Text = $"Connected to {enderecoIP}:{portaHost}";
                }));
            }
            catch (Exception ex)
            {
                labelStatus.Invoke(new Action(()=> 
                {
                    labelStatus.ForeColor = Color.Red;
                    labelStatus.Text = "Error : \n" + ex.Message;
                }));
            }
        }

        private void Receive()
        {
            strReceptor = new StreamReader(tcpServidor.GetStream());
            string ConResposta = strReceptor.ReadLine();
            if (ConResposta[0] == '1')
            {
                this.Invoke(new AtualizaLogCallBack(this.Logs), new object[] { "Connection Success!" });
            }
            else 
            {
                string Motivo = "Not Connected: ";
                Motivo += ConResposta.Substring(2, ConResposta.Length - 2);
                this.Invoke(new FechaConexaoCallBack(this.Disconnect), new object[] { Motivo });
                return;
            }
            while (Conectado)
            {
                this.Invoke(new AtualizaLogCallBack(this.Logs), new object[] { strReceptor.ReadLine() });
            }
        }

        private void Logs(string strMensagem)
        {
            txtLog.AppendText(strMensagem + "\r\n");
        }

        private void Send()
        {
            if (txtMensagem.Lines.Length >= 1)
            {
                stwEnviador.WriteLine(txtMensagem.Text);
                stwEnviador.Flush();
                txtMensagem.Lines = null;
            }
            txtMensagem.Text = "";
        }

        private void Disconnect(string Motivo)
        {
            txtLog.AppendText(Motivo + "\r\n");
            txtServidorIP.Enabled = true;
            numPortaHost.Enabled = true;
            txtUsuario.Enabled = true;
            txtMensagem.Enabled = false;
            btnEnviar.Enabled = false;
            btnConectar.ForeColor = Color.Green;
            btnConectar.Text = "Connect";
            Conectado = false;
            stwEnviador.Close();
            strReceptor.Close();
            tcpServidor.Close();

            labelStatus.Invoke(new Action(() =>
            {
                labelStatus.ForeColor = Color.Green;
                labelStatus.Text = $"Diconnected from chat {enderecoIP}:{portaHost}";
            }));
        }

        public void OnApplicationExit(object sender, EventArgs e)
        {
            if (Conectado == true)
            {
                Conectado = false;
                stwEnviador.Close();
                strReceptor.Close();
                tcpServidor.Close();

                labelStatus.Invoke(new Action(() =>
                {
                    labelStatus.ForeColor = Color.Green;
                    labelStatus.Text = $"Diconnected from chat {enderecoIP}:{portaHost}";
                }));
            }
        }
    }
}
