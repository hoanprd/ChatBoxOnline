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
using System.Net.Http;
using System.Web.Script.Serialization;
using System.Collections;

namespace TCPChatClientServerGUI
{
    public partial class FrmClient : Form
    {
        public FrmClient()
        {
            InitializeComponent();
        }

        Socket sckClient;
        string lang_first = "auto";
        string lang_second;

        public bool IsTrans2 = false;

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

        private void butKetnoi_Click(object sender, EventArgs e)
        {
            //tao socket
            sckClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //connect
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(txtServerIP.Text), (int)numServerPort.Value);
            sckClient.BeginConnect(ep, new AsyncCallback(xulyketnoi),null);
        }
        void xulyketnoi(IAsyncResult result)
        {
            sckClient.EndConnect(result);
            //Cap nhat trang thai, va bat dau gui nhan du lieu
            //cap nhat trang thai
            lbTrangThai.Invoke(new CapNhatGiaoDien(CapNhatTrangThai), new object[] { "Ket noi thanh cong." });
            //Bat dau nhan du lieu
            if (IsTrans2 == false)
            {
                sckClient.BeginReceive(data, 0, 1024, SocketFlags.None, new AsyncCallback(xulyLang), null);
                //IsTrans2 = true;
            }
            else
            {
                sckClient.BeginReceive(data, 0, 1024, SocketFlags.None, new AsyncCallback(xulydulieunhanduoc), null);
            }
        }
        //Khai bao bo dem de nhan du lieu
        byte[] data = new byte[1024];
        void xulyLang(IAsyncResult result)
        {
            //Goi ham EndReceive
            int size = sckClient.EndReceive(result);
            //Xu ly du lieu nhan duoc trong data[]
            String thongdiep = Encoding.Unicode.GetString(data, 0, size);
            if (thongdiep == "English")
            {
                //lang_second = "en";
                lang_first = "en";
            }
            if (thongdiep == "French")
            {
                //lang_second = "fr";
                lang_first = "fr";
            }
            if (thongdiep == "Vietnamese")
            {
                //lang_second = "vi";
                lang_first = "vi";
            }
            if (thongdiep == "Lao")
            {
                //lang_second = "lo";
                lang_first = "lo";
            }
            if (thongdiep == "Chinese")
            {
                //lang_second = "zh-tw";
                lang_first = "zh-tw";
            }
            if (thongdiep == "Japanese")
            {
                //lang_second = "ja";
                lang_first = "ja";
            }
            //Cho nhan tiep
            sckClient.BeginReceive(data, 0, 1024, SocketFlags.None, new AsyncCallback(xulydulieunhanduoc), null);
        }

        void xulydulieunhanduoc(IAsyncResult result)
        {
            //Goi ham EndReceive
            int size = sckClient.EndReceive(result);
            //Xu ly du lieu nhan duoc trong data[]
            String thongdiep = Encoding.Unicode.GetString(data, 0, size);
            thongdiep = TranslateText(thongdiep);
            //Chen thong diep vao textbox noidungchat
            txtNoidungChat.Invoke(new CapNhatGiaoDien(CapNhatNoiDungChat), new object[] { "Server: " + thongdiep });
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
            CapNhatNoiDungChat("Client: " + txtThongdiep.Text);
            //txtThongdiep.Text = TranslateText(txtThongdiep.Text);
            //sckClient.Send(Encoding.ASCII.GetBytes(txtThongdiep.Text));
            sckClient.Send(Encoding.Unicode.GetBytes(txtThongdiep.Text));
            txtThongdiep.Text = "";
        }

        private void txtThongdiep_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((int)e.KeyChar == 13)
            {
                //goi lai ham gui
                butGui_Click(null, null);
            }
        }

        public void ClientLangComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ClientLangComboBox.Text == "English")
            {
                IsTrans2 = true;
                lang_second = "en";
                sckClient.Send(Encoding.Unicode.GetBytes(ClientLangComboBox.Text));
                MessageBox.Show("You chose " + ClientLangComboBox.Text);
            }
            if (ClientLangComboBox.Text == "French")
            {
                IsTrans2 = true;
                lang_second = "fr";
                sckClient.Send(Encoding.Unicode.GetBytes(ClientLangComboBox.Text));
                MessageBox.Show("You chose " + ClientLangComboBox.Text);
            }
            if (ClientLangComboBox.Text == "Vietnamese")
            {
                IsTrans2 = true;
                lang_second = "vi";
                sckClient.Send(Encoding.Unicode.GetBytes(ClientLangComboBox.Text));
                MessageBox.Show("You chose " + ClientLangComboBox.Text);
            }
            if (ClientLangComboBox.Text == "Lao")
            {
                IsTrans2 = true;
                lang_second = "lo";
                sckClient.Send(Encoding.Unicode.GetBytes(ClientLangComboBox.Text));
                MessageBox.Show("You chose " + ClientLangComboBox.Text);
            }
            if (ClientLangComboBox.Text == "Chinese")
            {
                IsTrans2 = true;
                lang_second = "zh-tw";
                sckClient.Send(Encoding.Unicode.GetBytes(ClientLangComboBox.Text));
                MessageBox.Show("You chose " + ClientLangComboBox.Text);
            }
            if (ClientLangComboBox.Text == "Japanese")
            {
                IsTrans2 = true;
                lang_second = "ja";
                sckClient.Send(Encoding.Unicode.GetBytes(ClientLangComboBox.Text));
                MessageBox.Show("You chose " + ClientLangComboBox.Text);
            }
        }
    }
}
