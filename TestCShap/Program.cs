using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UtilLibrary;

namespace TestCShap
{
    class Program
    {
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr hWnd1, IntPtr hWnd2, string lpsz1, string lpsz2);
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, IntPtr uMsg, IntPtr WParam, IntPtr LParam);
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindow(IntPtr hWnd, IntPtr uCmd);

        const int GW_HWNDFIRST = 0;
        const int GW_HWNDLAST = 1;
        const int GW_HWNDNEXT = 2;
        const int GW_HWNDPREV = 3;
        const int GW_OWNER = 4;
        const int GW_CHILD = 5;
        [STAThread]

        static void Main(string[] args)
        {
            //// 이건 나중에 C++ 로 짜는게 좋을듯
            //Console.WriteLine("메신저의 핸들값은?");
            //IntPtr loginWin = FindWindow(null, "영웅문W Login");
            //Console.WriteLine("캡션값만 입력한 핸들값 : {0:X}", loginWin);


            ////클래스명과 핸들값둘중 하나만 입력해도 값은 똑같이 리턴이 된다.
            ////그렇다면 이제 부모핸들값을들 저장시키면서 한번 그림 4에 있는 대화상대 추가 핸들값을 알아내보자.             int hw1=FindWindow(null,"MSN Messenger");
            //IntPtr idBox = FindWindowEx(loginWin, (IntPtr) 0, "Edit", null);
            //Console.WriteLine("idBox : {0:X}", idBox);
            ////     int hwnd_first = GetWindow(hwnd_main, GW_CHILD);
            //IntPtr idPw = GetWindow(idBox, (IntPtr) GW_HWNDNEXT);
            //Console.WriteLine("pwBox : {0:X}", idPw);
            //IntPtr textPtr = Marshal.AllocHGlobal(260);

            //string message = "wizard84";
            ////byte[] buffer = Encoding.Default.GetBytes(message);
            //textPtr = Marshal.StringToHGlobalAuto(message);
            ////String text1 = Marshal.PtrToStringUni(textPtr, 260);
            //SendMessage(idPw, (IntPtr) WIN_MESSAGE.WM_SETTEXT, IntPtr.Zero, textPtr);

            //Marshal.FreeHGlobal(textPtr);

            //IntPtr accountPw = GetWindow(idPw, (IntPtr) GW_HWNDNEXT);
            //Console.WriteLine("공인인증 : {0:X}", accountPw);

            //IntPtr loginBtn = GetWindow(accountPw, (IntPtr) GW_HWNDNEXT);
            //Console.WriteLine("로그인 버튼 : {0:X}", loginBtn);
            ////    SendMessage(loginBtn, (int)WIN_MESSAGE.BM_CLICK, 0, 1);
            //SendMessage(loginBtn, (IntPtr) WIN_MESSAGE.WM_LBUTTONDOWN, (IntPtr) 0, (IntPtr) 1);
            //SendMessage(loginBtn, (IntPtr) WIN_MESSAGE.WM_LBUTTONUP, (IntPtr) 0, (IntPtr) 1);
            //// SendMessage(idBox,(int) WIN_MESSAGE.WM_KEYDOWN, 0, 1);
            ////int hw3 = FindWindowEx(idBox, 0, "Edit", null);
            ////Console.WriteLine("핸들값3 : {0:X}", hw3);
            ////int hw4 = FindWindowEx(hw3, 0, "Edit", null);
            ////Console.WriteLine("핸들값4 : {0:X}", hw4);

         
            // Suspend the screen.  
            Console.ReadLine();
        }

        class ScriptEngine
        {
            Dictionary<string, double> varPool_ = new Dictionary<string, double>();
            void parseInit(string text)
            {
            
            }

            void parseEnter(string text)
            {

            }

            void parsePayoff(string text)
            {

            }

            void parseSafer(string text)
            {

            }

            public void loadScript(string path)
            {
                int counter = 0;
                string line;

                // Read the file and display it line by line.  
                StreamReader file = new StreamReader("script.txt");
                while ((line = file.ReadLine()) != null) {
                    Console.WriteLine(line);
                    //line 구문 분석
                    // 클래스 등록

                    counter++;
                }

                file.Close();
                Console.WriteLine("There were {0} lines.", counter);
            }
        }

        enum WIN_MESSAGE
        {
            WM_NULL = 0x0000,
            WM_CREATE = 0x0001,
            WM_DESTROY = 0x0002,
            WM_MOVE = 0x0003,
            WM_SIZE = 0x0005,
            WM_ACTIVATE = 0x0006,
            WM_SETFOCUS = 0x0007,
            WM_KILLFOCUS = 0x0008,
            WM_ENABLE = 0x000A,
            WM_SETREDRAW = 0x000B,
            WM_SETTEXT = 0x000C,
            WM_GETTEXT = 0x000D,
            WM_GETTEXTLENGTH = 0x000E,
            WM_PAINT = 0x000F,
            WM_CLOSE = 0x0010,
            WM_QUERYENDSESSION = 0x0011,
            WM_QUERYOPEN = 0x0013,
            WM_ENDSESSION = 0x0016,
            WM_QUIT = 0x0012,
            WM_ERASEBKGND = 0x0014,
            WM_SYSCOLORCHANGE = 0x0015,
            WM_SHOWWINDOW = 0x0018,
            WM_WININICHANGE = 0x001A,
            WM_SETTINGCHANGE = 0x001A,
            WM_DEVMODECHANGE = 0x001B,
            WM_ACTIVATEAPP = 0x001C,
            WM_FONTCHANGE = 0x001D,
            WM_TIMECHANGE = 0x001E,
            WM_CANCELMODE = 0x001F,
            WM_SETCURSOR = 0x0020,
            WM_MOUSEACTIVATE = 0x0021,
            WM_CHILDACTIVATE = 0x0022,
            WM_QUEUESYNC = 0x0023,
            WM_GETMINMAXINFO = 0x0024,
            WM_PAINTICON = 0x0026,
            WM_ICONERASEBKGND = 0x0027,
            WM_NEXTDLGCTL = 0x0028,
            WM_SPOOLERSTATUS = 0x002A,
            WM_DRAWITEM = 0x002B,
            WM_MEASUREITEM = 0x002C,
            WM_DELETEITEM = 0x002D,
            WM_VKEYTOITEM = 0x002E,
            WM_CHARTOITEM = 0x002F,
            WM_SETFONT = 0x0030,
            WM_GETFONT = 0x0031,
            WM_SETHOTKEY = 0x0032,
            WM_GETHOTKEY = 0x0033,
            WM_QUERYDRAGICON = 0x0037,
            WM_COMPAREITEM = 0x0039,
            WM_GETOBJECT = 0x003D,
            WM_COMPACTING = 0x0041,
            WM_COMMNOTIFY = 0x0044,
            WM_WINDOWPOSCHANGING = 0x0046,
            WM_WINDOWPOSCHANGED = 0x0047,
            WM_POWER = 0x0048,
            WM_COPYDATA = 0x004A,
            WM_CANCELJOURNAL = 0x004B,
            WM_NOTIFY = 0x004E,
            WM_INPUTLANGCHANGEREQUEST = 0x0050,
            WM_INPUTLANGCHANGE = 0x0051,
            WM_TCARD = 0x0052,
            WM_HELP = 0x0053,
            WM_USERCHANGED = 0x0054,
            WM_NOTIFYFORMAT = 0x0055,
            WM_CONTEXTMENU = 0x007B,
            WM_STYLECHANGING = 0x007C,
            WM_STYLECHANGED = 0x007D,
            WM_DISPLAYCHANGE = 0x007E,
            WM_GETICON = 0x007F,
            WM_SETICON = 0x0080,
            WM_NCCREATE = 0x0081,
            WM_NCDESTROY = 0x0082,
            WM_NCCALCSIZE = 0x0083,
            WM_NCHITTEST = 0x0084,
            WM_NCPAINT = 0x0085,
            WM_NCACTIVATE = 0x0086,
            WM_GETDLGCODE = 0x0087,
            WM_SYNCPAINT = 0x0088,
            WM_NCMOUSEMOVE = 0x00A0,
            WM_NCLBUTTONDOWN = 0x00A1,
            WM_NCLBUTTONUP = 0x00A2,
            WM_NCLBUTTONDBLCLK = 0x00A3,
            WM_NCRBUTTONDOWN = 0x00A4,
            WM_NCRBUTTONUP = 0x00A5,
            WM_NCRBUTTONDBLCLK = 0x00A6,
            WM_NCMBUTTONDOWN = 0x00A7,
            WM_NCMBUTTONUP = 0x00A8,
            WM_NCMBUTTONDBLCLK = 0x00A9,
            WM_NCXBUTTONDOWN = 0x00AB,
            WM_NCXBUTTONUP = 0x00AC,
            WM_NCXBUTTONDBLCLK = 0x00AD,
            WM_INPUT = 0x00FF,
            WM_KEYFIRST = 0x0100,
            WM_KEYDOWN = 0x0100,
            WM_KEYUP = 0x0101,
            WM_CHAR = 0x0102,
            WM_DEADCHAR = 0x0103,
            WM_SYSKEYDOWN = 0x0104,
            WM_SYSKEYUP = 0x0105,
            WM_SYSCHAR = 0x0106,
            WM_SYSDEADCHAR = 0x0107,
            WM_UNICHAR = 0x0109,
            WM_KEYLAST_NT501 = 0x0109,
            UNICODE_NOCHAR = 0xFFFF,
            WM_KEYLAST_PRE501 = 0x0108,
            WM_IME_STARTCOMPOSITION = 0x010D,
            WM_IME_ENDCOMPOSITION = 0x010E,
            WM_IME_COMPOSITION = 0x010F,
            WM_IME_KEYLAST = 0x010F,
            WM_INITDIALOG = 0x0110,
            WM_COMMAND = 0x0111,
            WM_SYSCOMMAND = 0x0112,
            WM_TIMER = 0x0113,
            WM_HSCROLL = 0x0114,
            WM_VSCROLL = 0x0115,
            WM_INITMENU = 0x0116,
            WM_INITMENUPOPUP = 0x0117,
            WM_MENUSELECT = 0x011F,
            WM_MENUCHAR = 0x0120,
            WM_ENTERIDLE = 0x0121,
            WM_MENURBUTTONUP = 0x0122,
            WM_MENUDRAG = 0x0123,
            WM_MENUGETOBJECT = 0x0124,
            WM_UNINITMENUPOPUP = 0x0125,
            WM_MENUCOMMAND = 0x0126,
            WM_CHANGEUISTATE = 0x0127,
            WM_UPDATEUISTATE = 0x0128,
            WM_QUERYUISTATE = 0x0129,
            WM_CTLCOLORMSGBOX = 0x0132,
            WM_CTLCOLOREDIT = 0x0133,
            WM_CTLCOLORLISTBOX = 0x0134,
            WM_CTLCOLORBTN = 0x0135,
            WM_CTLCOLORDLG = 0x0136,
            WM_CTLCOLORSCROLLBAR = 0x0137,
            WM_CTLCOLORSTATIC = 0x0138,
            WM_MOUSEFIRST = 0x0200,
            WM_MOUSEMOVE = 0x0200,
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_LBUTTONDBLCLK = 0x0203,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205,
            WM_RBUTTONDBLCLK = 0x0206,
            WM_MBUTTONDOWN = 0x0207,
            WM_MBUTTONUP = 0x0208,
            WM_MBUTTONDBLCLK = 0x0209,
            WM_MOUSEWHEEL = 0x020A,
            WM_XBUTTONDOWN = 0x020B,
            WM_XBUTTONUP = 0x020C,
            WM_XBUTTONDBLCLK = 0x020D,
            WM_MOUSELAST_5 = 0x020D,
            WM_MOUSELAST_4 = 0x020A,
            WM_MOUSELAST_PRE_4 = 0x0209,
            WM_PARENTNOTIFY = 0x0210,
            WM_ENTERMENULOOP = 0x0211,
            WM_EXITMENULOOP = 0x0212,
            WM_NEXTMENU = 0x0213,
            WM_SIZING = 0x0214,
            WM_CAPTURECHANGED = 0x0215,
            WM_MOVING = 0x0216,
            WM_POWERBROADCAST = 0x0218,
            WM_DEVICECHANGE = 0x0219,
            WM_MDICREATE = 0x0220,
            WM_MDIDESTROY = 0x0221,
            WM_MDIACTIVATE = 0x0222,
            WM_MDIRESTORE = 0x0223,
            WM_MDINEXT = 0x0224,
            WM_MDIMAXIMIZE = 0x0225,
            WM_MDITILE = 0x0226,
            WM_MDICASCADE = 0x0227,
            WM_MDIICONARRANGE = 0x0228,
            WM_MDIGETACTIVE = 0x0229,
            WM_MDISETMENU = 0x0230,
            WM_ENTERSIZEMOVE = 0x0231,
            WM_EXITSIZEMOVE = 0x0232,
            WM_DROPFILES = 0x0233,
            WM_MDIREFRESHMENU = 0x0234,
            WM_IME_SETCONTEXT = 0x0281,
            WM_IME_NOTIFY = 0x0282,
            WM_IME_CONTROL = 0x0283,
            WM_IME_COMPOSITIONFULL = 0x0284,
            WM_IME_SELECT = 0x0285,
            WM_IME_CHAR = 0x0286,
            WM_IME_REQUEST = 0x0288,
            WM_IME_KEYDOWN = 0x0290,
            WM_IME_KEYUP = 0x0291,
            WM_MOUSEHOVER = 0x02A1,
            WM_MOUSELEAVE = 0x02A3,
            WM_NCMOUSEHOVER = 0x02A0,
            WM_NCMOUSELEAVE = 0x02A2,
            WM_WTSSESSION_CHANGE = 0x02B1,
            WM_TABLET_FIRST = 0x02c0,
            WM_TABLET_LAST = 0x02df,
            WM_CUT = 0x0300,
            WM_COPY = 0x0301,
            WM_PASTE = 0x0302,
            WM_CLEAR = 0x0303,
            WM_UNDO = 0x0304,
            WM_RENDERFORMAT = 0x0305,
            WM_RENDERALLFORMATS = 0x0306,
            WM_DESTROYCLIPBOARD = 0x0307,
            WM_DRAWCLIPBOARD = 0x0308,
            WM_PAINTCLIPBOARD = 0x0309,
            WM_VSCROLLCLIPBOARD = 0x030A,
            WM_SIZECLIPBOARD = 0x030B,
            WM_ASKCBFORMATNAME = 0x030C,
            WM_CHANGECBCHAIN = 0x030D,
            WM_HSCROLLCLIPBOARD = 0x030E,
            WM_QUERYNEWPALETTE = 0x030F,
            WM_PALETTEISCHANGING = 0x0310,
            WM_PALETTECHANGED = 0x0311,
            WM_HOTKEY = 0x0312,
            WM_PRINT = 0x0317,
            WM_PRINTCLIENT = 0x0318,
            WM_APPCOMMAND = 0x0319,
            WM_THEMECHANGED = 0x031A,
            WM_HANDHELDFIRST = 0x0358,
            WM_HANDHELDLAST = 0x035F,
            WM_AFXFIRST = 0x0360,
            WM_AFXLAST = 0x037F,
            WM_PENWINFIRST = 0x0380,
            WM_PENWINLAST = 0x038F,
            WM_APP = 0x8000,
            WM_USER = 0x0400,
            EM_GETSEL = 0x00B0,
            EM_SETSEL = 0x00B1,
            EM_GETRECT = 0x00B2,
            EM_SETRECT = 0x00B3,
            EM_SETRECTNP = 0x00B4,
            EM_SCROLL = 0x00B5,
            EM_LINESCROLL = 0x00B6,
            EM_SCROLLCARET = 0x00B7,
            EM_GETMODIFY = 0x00B8,
            EM_SETMODIFY = 0x00B9,
            EM_GETLINECOUNT = 0x00BA,
            EM_LINEINDEX = 0x00BB,
            EM_SETHANDLE = 0x00BC,
            EM_GETHANDLE = 0x00BD,
            EM_GETTHUMB = 0x00BE,
            EM_LINELENGTH = 0x00C1,
            EM_REPLACESEL = 0x00C2,
            EM_GETLINE = 0x00C4,
            EM_LIMITTEXT = 0x00C5,
            EM_CANUNDO = 0x00C6,
            EM_UNDO = 0x00C7,
            EM_FMTLINES = 0x00C8,
            EM_LINEFROMCHAR = 0x00C9,
            EM_SETTABSTOPS = 0x00CB,
            EM_SETPASSWORDCHAR = 0x00CC,
            EM_EMPTYUNDOBUFFER = 0x00CD,
            EM_GETFIRSTVISIBLELINE = 0x00CE,
            EM_SETREADONLY = 0x00CF,
            EM_SETWORDBREAKPROC = 0x00D0,
            EM_GETWORDBREAKPROC = 0x00D1,
            EM_GETPASSWORDCHAR = 0x00D2,
            EM_SETMARGINS = 0x00D3,
            EM_GETMARGINS = 0x00D4,
            EM_SETLIMITTEXT = EM_LIMITTEXT,
            EM_GETLIMITTEXT = 0x00D5,
            EM_POSFROMCHAR = 0x00D6,
            EM_CHARFROMPOS = 0x00D7,
            EM_SETIMESTATUS = 0x00D8,
            EM_GETIMESTATUS = 0x00D9,
            BM_GETCHECK = 0x00F0,
            BM_SETCHECK = 0x00F1,
            BM_GETSTATE = 0x00F2,
            BM_SETSTATE = 0x00F3,
            BM_SETSTYLE = 0x00F4,
            BM_CLICK = 0x00F5,
            BM_GETIMAGE = 0x00F6,
            BM_SETIMAGE = 0x00F7,
            STM_SETICON = 0x0170,
            STM_GETICON = 0x0171,
            STM_SETIMAGE = 0x0172,
            STM_GETIMAGE = 0x0173,
            STM_MSGMAX = 0x0174,
            DM_GETDEFID = (WM_USER + 0),
            DM_SETDEFID = (WM_USER + 1),
            DM_REPOSITION = (WM_USER + 2),
            LB_ADDSTRING = 0x0180,
            LB_INSERTSTRING = 0x0181,
            LB_DELETESTRING = 0x0182,
            LB_SELITEMRANGEEX = 0x0183,
            LB_RESETCONTENT = 0x0184,
            LB_SETSEL = 0x0185,
            LB_SETCURSEL = 0x0186,
            LB_GETSEL = 0x0187,
            LB_GETCURSEL = 0x0188,
            LB_GETTEXT = 0x0189,
            LB_GETTEXTLEN = 0x018A,
            LB_GETCOUNT = 0x018B,
            LB_SELECTSTRING = 0x018C,
            LB_DIR = 0x018D,
            LB_GETTOPINDEX = 0x018E,
            LB_FINDSTRING = 0x018F,
            LB_GETSELCOUNT = 0x0190,
            LB_GETSELITEMS = 0x0191,
            LB_SETTABSTOPS = 0x0192,
            LB_GETHORIZONTALEXTENT = 0x0193,
            LB_SETHORIZONTALEXTENT = 0x0194,
            LB_SETCOLUMNWIDTH = 0x0195,
            LB_ADDFILE = 0x0196,
            LB_SETTOPINDEX = 0x0197,
            LB_GETITEMRECT = 0x0198,
            LB_GETITEMDATA = 0x0199,
            LB_SETITEMDATA = 0x019A,
            LB_SELITEMRANGE = 0x019B,
            LB_SETANCHORINDEX = 0x019C,
            LB_GETANCHORINDEX = 0x019D,
            LB_SETCARETINDEX = 0x019E,
            LB_GETCARETINDEX = 0x019F,
            LB_SETITEMHEIGHT = 0x01A0,
            LB_GETITEMHEIGHT = 0x01A1,
            LB_FINDSTRINGEXACT = 0x01A2,
            LB_SETLOCALE = 0x01A5,
            LB_GETLOCALE = 0x01A6,
            LB_SETCOUNT = 0x01A7,
        }


    }
}
