using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FooCompany.BarTeam.MerrorTestsTarget
{
    internal class NewInstanceTarget
    {

        bool isDefaultConstructorCalled;

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        private NewInstanceTarget()
        {
            isDefaultConstructorCalled = true;
        }


        bool isNullTextReceived;

        /// <summary>
        /// Nullのstringだけ受け付けるコンストラクタ
        /// </summary>
        /// <param name="nullText"></param>
        public NewInstanceTarget(string nullText)
        {
            if (nullText != null) throw new ArgumentException();

            this.isNullTextReceived = true;
        }

        /// <summary>
        /// パラメータにrefを取るコンストラクタ
        /// </summary>
        /// <param name="num"></param>
        internal NewInstanceTarget(ref int num)
        {
            num = 123;
        }

    }
}
