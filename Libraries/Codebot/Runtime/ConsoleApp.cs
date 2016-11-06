using System;
using System.IO;

namespace Codebot.Runtime
{
    public static class ConsoleApp
    {
        private static string running = System.AppDomain.CurrentDomain.FriendlyName + ".running";
        public static bool WaitOnExit = Settings.ReadBool("waitOnExit");

        private static void Cancel(object sender, ConsoleCancelEventArgs e)
        {
            File.Delete(running);
            e.Cancel = false;
        }

        public static void Startup()
        {
            File.Create(running).Dispose();
            Console.CancelKeyPress += Cancel;
            
        }

        public static void Shutdown()
        {
            File.Delete(running);
            Console.WriteLine("Done");
            if (WaitOnExit)
            {
                Console.WriteLine("Press any key to continue");
                Console.ReadKey();
            }
        }

        public static bool Terminated
        {
            get
            {
                return !File.Exists(running);
            }
            set
            {
                if (value)
                    Terminate();
            }
        }

        public static void Terminate()
        {
            File.Delete(running);
        }

    }
}

