using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FooCompany.BarTeam.MerrorTestsTarget
{
    public class FieldTestTarget
    {

        static string dummyText1;
        static string text;
        static string dummyText2;
        string dummyText3;

        int dummyNum1;
        int num;
        int dummyNum2;
        static int dummyNum3;

        public FieldTestTarget(int objNum, string staticText)
        {
            num = objNum;
            text = staticText;
        }

    }
}
