using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace Codebot.Automation
{
    [Flags]
    public enum BrowserOption : int
    {
        None = 0,
        AddressBar = 1 << 0,
        FullScreen = 1 << 1,
        MenuBar = 1 << 2,
        Silent = 1 << 3,
        StatusBar = 1 << 4,
        ToolBar = 1 << 5,
        Visible = 1 << 6
    }

    public class Browser
    {
        public static readonly BrowserOption DefaultOptions = BrowserOption.AddressBar;

        [ComVisible(false)]
        private delegate void NavigateComplete2Handler([MarshalAs(UnmanagedType.IDispatch)] [In] object disp, [MarshalAs(UnmanagedType.Struct)] [In] ref object value);

        public delegate void NavigateCallback(dynamic browser, string url, object data);

        public static bool Navigate(string location, NavigateCallback callback, object data = null)
        {
            var type = Type.GetTypeFromProgID("InternetExplorer.Application");
            dynamic explorer = Activator.CreateInstance(type);
            var complete = false;
            dynamic browser = null;
            string url = String.Empty;
            NavigateComplete2Handler navigateComplete2 = (object disp, ref object value) =>
            {
                if (complete)
                    return;
                browser = disp;
                url = value.ToString();
                complete = true;
            };
            explorer.NavigateComplete2 += navigateComplete2;
            explorer.Navigate2(location);
            int i = 0;
            while (!complete)
            {
                Thread.Sleep(250);
                if (i++ == 150)
                {
                    complete = true;
                    explorer.Quit();
                    return false;
                }
            }
            callback(browser, url, data);
            explorer.Quit();
            Thread.Sleep(500);
            return true;
        }
    }
}

