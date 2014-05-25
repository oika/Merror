using NUnit.Gui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Oika.Libs.MerrorTests
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            AppEntry.Main(new[] { Application.ExecutablePath, "/run" });
        }
    }
}
