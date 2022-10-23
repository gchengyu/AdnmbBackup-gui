using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;

namespace AdnmbBackup_gui
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            if (!Directory.Exists("cache"))
                Directory.CreateDirectory("cache");
            if (!Directory.Exists("po"))
                Directory.CreateDirectory("po");
            if (!Directory.Exists("output"))
                Directory.CreateDirectory("output");
            if (!Directory.Exists("output\\po"))
                Directory.CreateDirectory("output\\po");
            if (!Directory.Exists("output\\all"))
                Directory.CreateDirectory("output\\all");
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.coldthunder11.com/index.php/2020/03/19/%e5%a6%82%e4%bd%95%e8%8e%b7%e5%8f%96a%e5%b2%9b%e7%9a%84%e9%a5%bc%e5%b9%b2/");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string id = textBox1.Text;
            if (id == string.Empty) return;
            if (!File.Exists("cookie.txt"))
            {
                MessageBox.Show("请先放好小饼干");
                return;
            }
            string path = Path.Combine("cache", id + ".json");
            string po = Path.Combine("po", id + ".txt");
            if (File.Exists(po))
                {
                    string poid = File.ReadAllText(po);
                }
            try
            {
                string url = "https://aweidao1.com/api/v1/thread?";
                var cookie = File.ReadAllText("cookie.txt");
                CookieContainer cookieContainer = new CookieContainer();
                cookieContainer.Add(new Cookie("userhash", cookie, "/", "aweidao1.com"));
                HttpClientHandler handler = new HttpClientHandler() { UseCookies = true };
                handler.CookieContainer = cookieContainer;
                HttpClient http = new HttpClient(handler);
                http.DefaultRequestHeaders.Add("Host", "aweidao1.com");
                http.DefaultRequestHeaders.Add("Accept", "application/json");
                http.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
                http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.0.0 Safari/537.36");
                label4.Text = "正在获取第1页";
                var t = http.GetAsync(url + "?id=" + id + "&page=1");
                t.Wait();
                var result = t.Result;
                var t2 = result.Content.ReadAsByteArrayAsync();
                t2.Wait();
                var bytes = t2.Result;
                var str = ReadGzip(bytes);
                label4.Text = str;
                var fpjson = JsonConvert.DeserializeObject<JObject>(str);
                var replyCount = int.Parse(fpjson["ReplyCount"].ToString());
                int pageCount = replyCount / 19;
                if (replyCount % pageCount != 0) pageCount++;
                JArray contentJA = fpjson["replys"].ToObject<JArray>();
                for (var page = 2; page <= pageCount; page++)
                {
                    label4.Text = "正在获取第" + page + "页";
                    t = http.GetAsync(url + "?id=" + id + "&page=" + page);
                    t.Wait();
                    result = t.Result;
                    t2 = result.Content.ReadAsByteArrayAsync();
                    t2.Wait();
                    bytes = t2.Result;
                    str = ReadGzip(bytes);
                    var jo = JsonConvert.DeserializeObject<JObject>(str);
                    JArray ja = jo["replys"].ToObject<JArray>();
                    var rpcount = ja.Count;
                    for (int j = 0; j < rpcount; j++)
                    {
                        contentJA.Add(ja[j]);
                    }
                }
                for (var index = 0; index < contentJA.Count; index++)
                {
                    if (contentJA[index]["userid"].ToString() == "Tips")
                    {
                        contentJA.RemoveAt(index);
                        index--;
                    }
                }
                label4.Text = "完成";
                fpjson["replys"].Replace(contentJA);
                var fjsonstr = JsonConvert.SerializeObject(fpjson, Formatting.Indented);
                File.WriteAllText(path, fjsonstr);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            ConvertToText(path);
            ConvertToTextPoOnly(path);
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Ovler-Young/AdnmbBackup-gui");
        }
        static void ConvertToText(string path)
        {
            var jo = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(path));
            var sb = new StringBuilder();
            sb.Append(jo["userid"].ToString()); sb.Append("  "); sb.Append(jo["now"].ToString());
            sb.Append("  No."); sb.Append(jo["id"].ToString()); sb.Append(Environment.NewLine);
            if (jo["title"].ToString() != "无标题")
            {
                sb.Append("标题:"); sb.Append(jo["title"].ToString()); sb.Append(Environment.NewLine);
            }
            sb.Append(ContentProcess(jo["content"].ToString())); sb.Append(Environment.NewLine);
            var ja = jo["replys"].ToObject<JArray>();
            for (int i = 0; i < ja.Count; i++)
            {
                sb.Append("------------------------------------"); sb.Append(Environment.NewLine);
                sb.Append(ja[i]["userid"].ToString()); sb.Append("  "); sb.Append(ja[i]["now"].ToString());
                sb.Append("  No."); sb.Append(ja[i]["id"].ToString()); sb.Append(Environment.NewLine);
                sb.Append(ContentProcess(ja[i]["content"].ToString())); sb.Append(Environment.NewLine);
            }
            File.WriteAllText(path.Replace("json", "txt").Replace("cache","output"), sb.ToString(), System.Text.Encoding.GetEncoding("GB2312"));
            File.WriteAllText(path.Replace("json", "txt").Replace("cache","output\\all"), sb.ToString(), System.Text.Encoding.GetEncoding("GB2312"));
        }
        static void ConvertToTextPoOnly(string path)
        {
            var po_path = path.Replace("cache", "po").Replace("json", "txt");
            var jo = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(path));
            var sb = new StringBuilder();
            sb.Append(jo["userid"].ToString()); sb.Append("  "); sb.Append(jo["now"].ToString());
            sb.Append("  No."); sb.Append(jo["id"].ToString()); sb.Append(Environment.NewLine);
            if (jo["title"].ToString() != "无标题")
            {
                sb.Append("标题:"); sb.Append(jo["title"].ToString()); sb.Append(Environment.NewLine);
            }
            sb.Append(ContentProcess(jo["content"].ToString())); sb.Append(Environment.NewLine);
            var ja = jo["replys"].ToObject<JArray>();
            var poid = new HashSet<string>();
            poid.Add(jo["userid"].ToString());
            if (File.Exists(po_path) && File.ReadAllText(po_path) != "")
            {
                // read poid line by line
                var lines = File.ReadAllLines(po_path);
                foreach (var line in lines)
                {
                    poid.Add(line.Split(' ')[0]);
                }
            }
            for (int i = 0; i < ja.Count; i++)
            {
                if (poid.Contains(ja[i]["userid"].ToString()))
                {
                    sb.Append("------------------------------------"); sb.Append(Environment.NewLine);
                    sb.Append(ja[i]["userid"].ToString()); sb.Append("  "); sb.Append(ja[i]["now"].ToString());
                    sb.Append("No."); sb.Append(ja[i]["id"].ToString()); sb.Append(Environment.NewLine);
                    sb.Append(ContentProcess(ja[i]["content"].ToString())); sb.Append(Environment.NewLine);
                }
            }
            File.WriteAllText(path.Replace(".json", "_po_only.txt").Replace("cache","output"), sb.ToString(), System.Text.Encoding.GetEncoding("GB2312"));
            File.WriteAllText(path.Replace(".json", "_po_only.txt").Replace("cache","output\\po"), sb.ToString(), System.Text.Encoding.GetEncoding("GB2312"));
        }
        static string ContentProcess(string content)
        {
            return content.Replace("<font color=\"#789922\">&gt;&gt;", ">>").Replace("</font><br />", Environment.NewLine)
                .Replace("</font>", Environment.NewLine)
                .Replace("<br />\r\n", Environment.NewLine).Replace("<br />\n", Environment.NewLine);
        }
        static string ReadGzip(byte[] bytes)
        {
            string result = string.Empty;
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                using (GZipStream decompressedStream = new GZipStream(ms, CompressionMode.Decompress))
                {
                    using (StreamReader sr = new StreamReader(decompressedStream, Encoding.UTF8))
                    {
                        result = sr.ReadToEnd();
                    }
                }
            }
            return result;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (File.Exists("AtuobBackupList.txt"))
            {
                if (!File.Exists("cookie.txt"))
                {
                    MessageBox.Show("请先放好小饼干");
                    return;
                }
                int errCount = 0;
                var cookie = File.ReadAllText("cookie.txt");
                var ids = File.ReadAllLines("AtuobBackupList.txt");
                foreach (var id in ids)
                {
                    try
                    {
                        string path = Path.Combine("cache", id + ".json");
                        if (File.Exists(path)) continue;
                        string url = "https://aweidao1.com/api/v1/thread?";
                        CookieContainer cookieContainer = new CookieContainer();
                        cookieContainer.Add(new Cookie("userhash", cookie, "/", "aweidao1.com"));
                        HttpClientHandler handler = new HttpClientHandler() { UseCookies = true };
                        handler.CookieContainer = cookieContainer;
                        HttpClient http = new HttpClient(handler);
                        http.DefaultRequestHeaders.Add("Host", "aweidao1.com");
                        http.DefaultRequestHeaders.Add("Accept", "application/json");
                        http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.70 Safari/537.36 HavfunClient-AdnmbBackup");
                        label4.Text = ">>" + id + " 正在获取第1页";
                        var t = http.GetAsync(url + "?id=" + id + "&page=1");
                        t.Wait();
                        var result = t.Result;
                        var t2 = result.Content.ReadAsByteArrayAsync();
                        t2.Wait();
                        var bytes = t2.Result;
                        var str = ReadGzip(bytes);
                        var fpjson = JsonConvert.DeserializeObject<JObject>(str);
                        var replyCount = int.Parse(fpjson["ReplyCount"].ToString());
                        int pageCount = replyCount / 19;
                        if (replyCount % pageCount != 0) pageCount++;
                        JArray contentJA = fpjson["replys"].ToObject<JArray>();
                        for (var page = 2; page <= pageCount; page++)
                        {
                            label4.Text = ">>" + id + " 正在获取第" + page + "页";
                            t = http.GetAsync(url + "?id=" + id + "&page=" + page);
                            t.Wait();
                            result = t.Result;
                            t2 = result.Content.ReadAsByteArrayAsync();
                            t2.Wait();
                            bytes = t2.Result;
                            str = ReadGzip(bytes);
                            var jo = JsonConvert.DeserializeObject<JObject>(str);
                            JArray ja = jo["replys"].ToObject<JArray>();
                            var rpcount = ja.Count;
                            for (int j = 0; j < rpcount; j++)
                            {
                                contentJA.Add(ja[j]);
                            }
                        }
                        for (var index = 0; index < contentJA.Count; index++)
                        {
                            if (contentJA[index]["userid"].ToString() == "Tips")
                            {
                                contentJA.RemoveAt(index);
                                index--;
                            }
                        }
                        fpjson["replys"].Replace(contentJA);
                        var fjsonstr = JsonConvert.SerializeObject(fpjson, Formatting.Indented);
                        File.WriteAllText(path, fjsonstr);
                        ConvertToText(path);
                        ConvertToTextPoOnly(path);
                    }
                    catch
                    {
                        errCount++;
                    }
                }
                label4.Text = "已完成自动备份，有" + errCount + "个串的备份存在错误";
            }
        }
    }
}
