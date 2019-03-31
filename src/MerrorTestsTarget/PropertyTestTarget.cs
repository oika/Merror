using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FooCompany.BarTeam.MerrorTestsTarget
{
    public class PropertyTestTarget
    {

        private static string DummyText1 { get; set; }
        private static string Text { get; set; }
        public static string DummyText2 { get; set; }
        private string DummyText3 { get; set; }

        private int DummyNum1 { get; set; }
        private int Num { get; set; }
        public int DummyNum2 { get; set; }
        private static int DummyNum3 { get; set; }

        public PropertyTestTarget(int objNum, string staticText)
        {
            Num = objNum;
            Text = staticText;
        }

    }
}
