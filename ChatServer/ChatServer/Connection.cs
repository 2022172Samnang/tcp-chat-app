using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ChatServer
{

    class Connection
    {
        TcpClient tcpClient;

        private Thread thrSender;
        private StreamReader srReceptor;
        private StreamWriter swEnviador;
        private string usuarioAtual;
        private string strResposta;

        public Connection(TcpClient tcpCon)
        {
            tcpClient = tcpCon;

            thrSender = new Thread(AceitaCliente);
            thrSender.IsBackground = true;

            thrSender.Start();
        }

        private void FechaConexao()
        {

            tcpClient.Close();
            srReceptor.Close();
            swEnviador.Close();
        }


        private void AceitaCliente()
        {
            srReceptor = new StreamReader(tcpClient.GetStream());
            swEnviador = new StreamWriter(tcpClient.GetStream());

            usuarioAtual = srReceptor.ReadLine();

            if (usuarioAtual != "")
            {

                if (Server.htUsuarios.Contains(usuarioAtual) == true)
                {

                    swEnviador.WriteLine("0|Este nome de usuário já existe.");
                    swEnviador.Flush();
                    FechaConexao();
                    return;
                }
                else if (usuarioAtual == "Administrator")
                {

                    swEnviador.WriteLine("0|Este nome de usuário é reservado.");
                    swEnviador.Flush();
                    FechaConexao();
                    return;
                }
                else
                {

                    swEnviador.WriteLine("1");
                    swEnviador.Flush();

                
                    Server.IncluiUsuario(tcpClient, usuarioAtual);
                }
            }
            else
            {
                FechaConexao();
                return;
            }
            
            try
            {

                while ((strResposta = srReceptor.ReadLine()) != "")
                {

                    if (strResposta == null)
                    {
                        Server.RemoveUser(tcpClient);
                    }
                    else
                    {

                        Server.EnviaMensagem(usuarioAtual, strResposta);
                    }
                }
            }
            catch
            {

                Server.RemoveUser(tcpClient);
            }
        }
    }
}
