using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Web.Script.Serialization;
using System.Net.Http;
using System.Collections;

namespace TCPChatClientServerGUI
{
    public partial class FrmServer : Form
    {
        public FrmServer()
        {
            InitializeComponent();
        }
        
        //Khai bao 2 sockets
        Socket sckServer, sckClient;
        //sckServer: cho ket noi den tu client
        //sckClient: truyen nhan du lieu voi client

        string lang_first = "auto";
        string lang_second = "auto";

        public string TranslateText(string input)
        {
            string url = String.Format
            ("https://translate.googleapis.com/translate_a/single?client=gtx&sl={0}&tl={1}&dt=t&q={2}",
             lang_first, lang_second, Uri.EscapeUriString(input));
            HttpClient httpClient = new HttpClient();
            string result = httpClient.GetStringAsync(url).Result;
            var jsonData = new JavaScriptSerializer().Deserialize<List<dynamic>>(result);
            var translationItems = jsonData[0];
            string translation = "";
            foreach (object item in translationItems)
            {
                IEnumerable translationLineObject = item as IEnumerable;
                IEnumerator translationLineString = translationLineObject.GetEnumerator();
                translationLineString.MoveNext();
                translation += string.Format(" {0}", Convert.ToString(translationLineString.Current));
            }
            if (translation.Length > 1) { translation = translation.Substring(1); };
            return translation;
        }

        private void butKhoitao_Click(object sender, EventArgs e)
        {
            //Tao socket
            sckServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //Bind, Listen
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, (int)numServerPort.Value);
            sckServer.Bind(ep);
            sckServer.Listen(5);
            //Accept bat dong bo
            sckServer.BeginAccept(new AsyncCallback(xulyketnoi), null);
            lbTrangThai.Text = "Dang cho ket noi ....";
        }
        void xulyketnoi(IAsyncResult result)
        {
            sckClient = sckServer.EndAccept(result);
            //cap nhat trang thai
            lbTrangThai.Invoke(new CapNhatGiaoDien(CapNhatTrangThai), new object[] { "Ket noi thanh cong." });
            //Bat dau nhan du lieu
            sckClient.BeginReceive(data,0,1024,SocketFlags.None, new AsyncCallback(xulydulieunhanduoc),null);
        }
        //Khai bao bo dem de nhan du lieu
        byte[] data = new byte[1024];
        void xulydulieunhanduoc(IAsyncResult result)
        {
            //Goi ham EndReceive
            int size = sckClient.EndReceive(result);
            //Xu ly du lieu nhan duoc trong data[]
            //String thongdiep = Encoding.ASCII.GetString(data, 0, size);
            String thongdiep = Encoding.Unicode.GetString(data, 0, size);
            //Chen thong diep vao textbox noidungchat
            txtNoidungChat.Invoke(new CapNhatGiaoDien(CapNhatNoiDungChat), new object[] { "Client: " + thongdiep });
            //Cho nhan tiep
            sckClient.BeginReceive(data, 0, 1024, SocketFlags.None, new AsyncCallback(xulydulieunhanduoc), null);
        }
        delegate void CapNhatGiaoDien(string s);
        void CapNhatTrangThai(string s)
        {
            lbTrangThai.Text = s;
        }
        void CapNhatNoiDungChat(string s)
        {
            txtNoidungChat.Text += s + "\r\n";
        }

        private void butGui_Click(object sender, EventArgs e)
        {
            CapNhatNoiDungChat("Server: " + txtThongdiep.Text);
            txtThongdiep.Text = TranslateText(txtThongdiep.Text);
            //sckClient.Send(Encoding.ASCII.GetBytes(txtThongdiep.Text));
            sckClient.Send(Encoding.Unicode.GetBytes(txtThongdiep.Text));
            txtThongdiep.Text = "";
        }

        public void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SeverLangComboBox.Text == "English")
            {
                lang_second = "en";
            }
            if (SeverLangComboBox.Text == "French")
            {
                lang_second = "fr";
            }
            if (SeverLangComboBox.Text == "Vietnamese")
            {
                lang_second = "vi";
            }
            if (SeverLangComboBox.Text == "Lao")
            {
                lang_second = "lo";
            }
            if (SeverLangComboBox.Text == "Chinese")
            {
                lang_second = "zh-tw";
            }
            if (SeverLangComboBox.Text == "Japanese")
            {
                lang_second = "ja";
            }
        }

        private void txtThongdiep_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((int)e.KeyChar == 13)
            {
                //goi lai ham gui
                butGui_Click(null, null);
            }
        }
    }
}
