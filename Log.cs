using System;
using System.Collections.Generic;
using System.Text;

namespace I_Robot
{
    static class Log
    {
        static public void LogMessage(string s)
        {
            System.Diagnostics.Debug.WriteLine(s);
            System.Windows.Forms.MessageBox.Show(s, "Message");
        }
        
    }
}
