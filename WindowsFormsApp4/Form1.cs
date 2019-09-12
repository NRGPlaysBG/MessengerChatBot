using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using xNet;

namespace WindowsFormsApp4
{



    public partial class Form1 : Form
    {

        #region Don't touch those!

        public List<Message> messages = new List<Message>();
        public bool isDoingWork = false;

        Thread CMThread;

        Form2 webForm = new Form2();
        WebBrowser wbRead;

        System.Timers.Timer timer = new System.Timers.Timer(60000);

        Clock c = new Clock();

        public bool isReadingMessages = false;
        public string lastMessageDate = "";
        public string lastMessage = "";

        public string LastSender = "NULL";
        public string absoluteSender = "NULL";

        public string picURL;
        public string replyUrl;
        public string readingUrl;

        public int lastMsgNum = 0;

        public TimeSpan startedTime;

        HttpRequest httpPls;

        string type = "application/x-www-form-urlencoded";
        string fb_dtsg = "";
        string jazoest = "";

        #endregion

        #region Variables To Change

        public long ChatID = -1;

        #region LoginDetails

        string email = "";
        string password = "";

        #endregion

        #endregion

        //Custom variables can be declared bellow this line





        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            webForm.Show();

            Test("works");
            InitializeFacebookLogin();
            SetupWebBrowser();
            wbRead.Navigate("www.facebook.com");

            timer.Elapsed += Timer_Elapsed;
            timer.Start();



        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ClearMemory();
        }

        private void WebBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        private void SetupWebBrowser()
        {

            if (wbRead != null)
                wbRead = null;



            webForm.Invoke(new Action(() => { webForm.Dispose(); }));


            webForm = new Form2();
            webForm.Show();


            wbRead = webForm.webBrowser1;


            webForm.Invoke(new Action(() => { wbRead = webForm.webBrowser1; }));



        }


        // This function is called everytime a new message is called
        public void RunCommand(Message btMessage)
        {

            /// <summary> Message class explained 
            /// btMessage.message returns the message content
            /// btMessage.senderID returns the message's author real username
            /// btMessage.senderName returns the message's author nickname
            /// btMessage.time returns the message's time stamp
            /// </summary>


            Console.WriteLine(btMessage.message);


            // You can use the function SendFile(link to the file) to send a file to the active chat
            // You can use the function SendTextMSG(message) to send a message to the active chat.
            


        }


        private void Button1_Click(object sender, EventArgs e)
        {

            wbRead.Visible = true;

            readingUrl = "https://www.facebook.com/messages/t/" + ChatID;
            wbRead.Navigate(readingUrl);

            CMThread = new Thread(CustomUpdate);
            CMThread.SetApartmentState(ApartmentState.STA);
            CMThread.Start();

            startedTime = new TimeSpan(DateTime.Now.TimeOfDay.Hours, DateTime.Now.TimeOfDay.Minutes, 0);

            httpPls = httpRetrieve();

            c.NewDay += C_NewDay;


        }


        #region Bots Logic

        // The entire bot's logic is here


        // This function clears the cached messages every day, this prevents the bot from glitching and picking up old messages
        private void C_NewDay(object sender, EventArgs e)
        {

            httpPls = httpRetrieve();
            startedTime = new TimeSpan(23, 59, 59);
            messages.Clear();



            CMThread.Abort();



            SetupWebBrowser();

            wbRead.Navigate(readingUrl);


            CMThread = new Thread(CustomUpdate);
            CMThread.Start();


            startedTime = new TimeSpan(DateTime.Now.TimeOfDay.Hours, DateTime.Now.TimeOfDay.Minutes, 0);

        }

        public void Test(string html)
        {

            StreamWriter r = new StreamWriter("t.html");
            r.Write(html);
            r.Close();
            //Process.Start("t.html");
        }


        // Starts fetching messages
        private void CustomUpdate()
        {
            while (true)
            {


                // array is each line of the information the browser is going to pull out
                string[] array = null;

                try
                {
                    if (InvokeRequired)
                        webForm.Invoke(new Action(() => { try { array = wbRead.DocumentText.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries); } catch (Exception e) { Debug.WriteLine(e); }  }));
                    else
                        array = wbRead.DocumentText.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                }
                catch (InvalidCastException)
                {
                    Console.WriteLine("Something has happened!");
                }


                if (array == null)
                    return;


                lastMsgNum = 0;

                // starts going through each line, looking for messages and senders
                for (int i = 0; i < array.Length; i++)
                {

                    string currentLine = array[i];
                    //Console.WriteLine(currentLine);



                    // if the sender is not the Bot's profile
                    if (currentLine.StartsWith(@"<H5 class=""_ih3") && !currentLine.Contains("accessible_elem"))
                    {

                        // Gets the sender's nickname and clears it from HTML tags
                        LastSender = currentLine;

                        LastSender = Regex.Match(LastSender, @"aria-label="".+?"">").ToString();

                        LastSender = LastSender.Replace(@"aria-label=""", "");

                        if (LastSender.Length > 2)
                        {
                            LastSender = LastSender.Remove(LastSender.Length - 2);
                        }

                        // Gets the sender's username and clears it from HTML tags
                        absoluteSender = array[i - 2];

                        absoluteSender = Regex.Match(absoluteSender, @"alt="".+?""").ToString();
                        absoluteSender = absoluteSender.Replace(@"alt=""", "");
                        if (absoluteSender.Length > 1)
                        {
                            absoluteSender = absoluteSender.Remove(absoluteSender.Length - 1);
                        }

                    }

                    // If the line starts with this it's sent by the Bot's profile and shall not be read!
                    if (currentLine.StartsWith(@"<DIV class=""clearfix _o46 _3erg _3i_m _nd_ direction_ltr text_align_ltr"""))
                    {
                        continue;
                    }

                    // If the line starts with this it's sent by someone who isn't the bot
                    if (currentLine.StartsWith(@"<DIV class=""clearfix _o46"))
                    {



                        // fetches the message and time line and clears them from the HTML tags
                        string messageline = array[i + 2];
                        string timeLine = array[i + 1];
                        string idArray = array[i + 3];



                        if (array[i + 1].Contains("data-tooltip-content"))
                        {
                            timeLine = array[i + 1];
                        }
                        else
                        {
                            timeLine = array[i + 2];
                        }

                        if (array[i + 3].Contains("id"))
                        {
                            idArray = array[i + 3];
                        }
                        else
                        {
                            idArray = array[i + 4];
                        }


                        #region GetMessage

                        Match mc = Regex.Match(messageline, @"<DIV tabindex=""0"" class=""_aok"" aria-label="".+?"">");

                        string msg = mc.ToString();

                        msg = msg.Replace(@"<DIV tabindex=""0"" class=""_aok"" aria-label=""", "");



                        if (msg.Length > 2)
                        {
                            msg = msg.Remove(msg.Length - 2);
                        }

                        #endregion

                        #region GetTimeStamp

                        Match tm = Regex.Match(timeLine, @"data-tooltip-content="".+?""");
                        string TimeLineMSG = tm.ToString();
                        TimeLineMSG = TimeLineMSG.Replace(@"data-tooltip-content=""", "");
                        if (TimeLineMSG.Length > 1)
                        {
                            TimeLineMSG = TimeLineMSG.Remove(TimeLineMSG.Length - 1);
                        }

                        TimeSpan TimeMSG = new TimeSpan();
                        // if the message's time line contains any text it's believed that its older than a day and shall not be ran
                        if (!TimeSpan.TryParse(TimeLineMSG, out TimeMSG))
                        {
                            TimeLineMSG = "Invalid!";

                        }

                        // if the message's time line is before the time it started then the message shall not be ran
                        if (TimeMSG < startedTime)
                        {
                            TimeLineMSG = "Invalid!";

                        }

                        #region GetID

                        string msgID = i.ToString();

                        #endregion






                        #endregion

                        int tempNum = lastMsgNum;

                        if (TimeLineMSG != "Invalid!")
                        {
                            tempNum = ++lastMsgNum;
                        }
                        else
                        {
                            tempNum = -1;
                        }



                        // Check if the bot has just been started and set the last message's date and time
                        if (!isReadingMessages)
                        {
                            lastMessageDate = TimeLineMSG;
                            lastMessage = msg;
                            isReadingMessages = true;
                        }
                        else
                        {



                            // Check if the current message is contained in the cached messages
                            if (!cmContain(msg, tempNum) && msg.Length > 0)
                            {

                                Console.WriteLine(TimeLineMSG + "|" + TimeMSG + "|" + tempNum);

                                // Create a message variable
                                Message btMessage = new Message(msg, TimeLineMSG, tempNum, absoluteSender, LastSender);

                                // Add it to the cached messages variable
                                messages.Add(btMessage);

                                // If the message is valid then run it
                                if (TimeLineMSG != "Invalid!")
                                {

                                    RunCommand(btMessage);

                                }
                            }
                        }
                    }
                }


                Thread.Sleep(1000);
                ClearMemory();

            }
        }

        // The function goes through all the messages and compares date and timestamp, if they are the same than it returns true else it returns false
        public bool cmContain(string message, int id)
        {
            bool result = false;

            for (int i = 0; i < messages.Count; i++)
            {
                if (messages[i].message == message && messages[i].id == id)
                {
                    result = true;
                    return result;
                }
            }


            return result;

        }

        private HttpRequest httpRetrieve()
        {
            HttpRequest http = new HttpRequest();
            http.Cookies = new CookieDictionary();
            http.UserAgent = Http.IEUserAgent();
            string html = http.Get("m.facebook.com").ToString();
            string lsd = Regex.Match(html, "name=\"lsd\" value=\"(.*?)\"").Groups[1].Value;

            string m_ts = Regex.Match(html, "name=\"m_ts\" value=\"(.*?)\"").Groups[1].Value;
            string urLogin = "https://m.facebook.com/login/device-based/regular/login/?refsrc=https%3A%2F%2Fm.facebook.com%2F&lwv=101&refid=8";
            string data = "lsd=" + lsd + "&m_ts=" + m_ts + "&li=ksvKW7hIypNI-w3sLQ3efd2b&try_number=0&unrecognized_tries=0&email=" + email + "&pass=" + password + "&login=Вход";
            html = http.Post(urLogin, data, type).ToString();


            html = http.Post("https://m.facebook.com/home.php?refsrc=https%3A%2F%2Fm.facebook.com%2Flogin%2Fdevice-based%2Fedit-user%2F&_rdr").ToString();
            fb_dtsg = Regex.Match(html, "name=\"fb_dtsg\" value=\"(.*?)\"").Groups[1].Value;
            jazoest = Regex.Match(html, "name=\"jazoest\" value=\"(.*?)\"").Groups[1].Value;
            return http;
        }

        // This function should be the first function called, it logs into Facebook using email and password provided by the user
        private void InitializeFacebookLogin()
        {
            HttpRequest http = new HttpRequest();

            // Special Thanks to Ruslan Khuduev <x-rus@list.ru> without you this project would've taken more time! <3
            http.Cookies = new CookieDictionary();
            http.UserAgent = Http.IEUserAgent();
            string html = http.Get("m.facebook.com").ToString();
            string lsd = Regex.Match(html, "name=\"lsd\" value=\"(.*?)\"").Groups[1].Value;

            string m_ts = Regex.Match(html, "name=\"m_ts\" value=\"(.*?)\"").Groups[1].Value;
            string urLogin = "https://m.facebook.com/login/device-based/regular/login/?refsrc=https%3A%2F%2Fm.facebook.com%2F&lwv=101&refid=8";
            string data = "lsd=" + lsd + "&m_ts=" + m_ts + "&li=ksvKW7hIypNI-w3sLQ3efd2b&try_number=0&unrecognized_tries=0&email=" + email + "&pass=" + password + "&login=Вход";
            html = http.Post(urLogin, data, type).ToString();
            Test(html);


            html = http.Post("https://m.facebook.com/home.php?refsrc=https%3A%2F%2Fm.facebook.com%2Flogin%2Fdevice-based%2Fedit-user%2F&_rdr").ToString();
            fb_dtsg = Regex.Match(html, "name=\"fb_dtsg\" value=\"(.*?)\"").Groups[1].Value;
            jazoest = Regex.Match(html, "name=\"jazoest\" value=\"(.*?)\"").Groups[1].Value;
            Test(html);
        }

        // Send a text message to the current chat
        void SendTextMSG(string msg)
        {
            HttpRequest http = httpPls;

            string html = http.Get("https://m.facebook.com/messages/read/?tid=" + ChatID + "&request_type=send_success&_rdr").ToString();

            string tids = Regex.Match(html, "name=\"tids\" value=\"(.*?)\"").Groups[1].Value;
            Test(html);

            string msgContent = msg;
            string msgUrl = "https://m.facebook.com/messages/send/?icm=1&refid=12";
            string msgData = "fb_dtsg=" + fb_dtsg + "&jazoest=" + jazoest + "&body=" + msgContent + "&send=Изпращане&tids=" + tids + "&wwwupp=C3&referrer=&ctype=&cver=legacy&csid=19dcc9e5-0470-bb54-40c0-39c652a8f707";
            html = http.Post(msgUrl, msgData, type).ToString();
            Test(html);
            ClearMemory();

        }

        // Send a file to the current chat
        void SendFile(string filePath)
        {
            string uploadUrl = "https://upload.facebook.com/_mupload_/mbasic/messages/attachment/photo/";

            HttpRequest http = httpPls;

            string html = http.Get("https://m.facebook.com/messages/read/?tid=" + ChatID + "&request_type=send_success&_rdr").ToString();

            string tids = Regex.Match(html, "name=\"tids\" value=\"(.*?)\"").Groups[1].Value;

            var multipartContent = new xNet.MultipartContent()
            {
                {new xNet.StringContent(fb_dtsg), "fb_dtsg" },
                {new xNet.StringContent("cid.c."+ChatID+":100005091199018"), "tids"},
                {new xNet.StringContent("C3"), "wwwupp"},
                {new xNet.StringContent(tids), "tids"},
                {new xNet.StringContent(jazoest), "jazoest" },
                {new FileContent(filePath), "file1", "interesting.jpg"}

            };

            http.Post(uploadUrl, multipartContent);
            Test(html);
            ClearMemory();
        }

        #endregion




        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        public void ClearMemory()
        {
            System.Diagnostics.Process loProcess = System.Diagnostics.Process.GetCurrentProcess();
            try
            {
                loProcess.MaxWorkingSet = (IntPtr)((int)loProcess.MaxWorkingSet - 1);
                loProcess.MinWorkingSet = (IntPtr)((int)loProcess.MinWorkingSet - 1);
            }
            catch (System.Exception)
            {

                loProcess.MaxWorkingSet = (IntPtr)((int)1413120);
                loProcess.MinWorkingSet = (IntPtr)((int)204800);
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            C_NewDay(null, null);
        }
    }



}


public class CustomCommand
{
    public string name;
    public string response;
    public string addedby;

    public CustomCommand(string _name, string _response, string _addedby)
    {
        name = _name;
        response = _response;
        addedby = _addedby;
    }

}


public class AuthorizedMember
{
    public string name;
    public int ModerationLevel;

    public AuthorizedMember(string _name, int _modLevel)
    {
        name = _name;
        ModerationLevel = _modLevel;
    }
}

public class Message
{
    public string message;
    public string time;
    public int id;
    public string senderID;
    public string senderName;

    public Message(string _message, string _time, int _id, string _senderID, string _senderName)
    {
        message = _message;
        time = _time;
        id = _id;
        senderID = _senderID;
        senderName = _senderName;

    }

}