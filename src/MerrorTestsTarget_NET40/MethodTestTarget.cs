using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FooCompany.BarTeam.MerrorTestsTarget
{
    public class MethodTestTarget
    {

        private static int StaticSum(int num1, int num2)
        {
            return num1 + num2;
        }

        int baseNum;

        public MethodTestTarget(int baseNum)
        {
            this.baseNum = baseNum;
        }

        protected int Sum(int addition)
        {
            return baseNum + addition;
        }

        internal void Sum(int addition, out int result)
        {
            result = baseNum + addition;
        }
    }
}
