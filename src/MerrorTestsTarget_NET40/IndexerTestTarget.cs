using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FooCompany.BarTeam.MerrorTestsTarget
{
    public class IndexerTestTarget
    {

        readonly Dictionary<int, IndexerItem> dic;

        private IndexerItem this[int index]
        {
            get
            {
                return dic[index];
            }
            set
            {
                dic[index] = value;
            }
        }


        readonly Dictionary<int, Dictionary<string, IndexerItem>> dic2D;

        internal IndexerItem this[int index1, string index2]
        {
            get
            {
                return dic2D[index1][index2];
            }
            set
            {
                if (!dic2D.ContainsKey(index1)) dic2D[index1] = new Dictionary<string, IndexerItem>();
                dic2D[index1][index2] = value;
            }
        }


        public IndexerTestTarget()
        {
            //初期値
            dic = new Dictionary<int, IndexerItem> {
                { 3, new IndexerItem(30) },
                { 2, new IndexerItem(20) },
            };
            dic2D = new Dictionary<int, Dictionary<string, IndexerItem>> {
                { 2, new Dictionary<string, IndexerItem> {
                            { "one", new IndexerItem(1) },
                            { "two", new IndexerItem(2) },
                    }
                },
                { 4, new Dictionary<string, IndexerItem> {
                            { "three", new IndexerItem(3) },
                            { "four", new IndexerItem(4) },
                    }
                }
            };

        }

    }


    internal class IndexerItem
    {

        int num;

        internal IndexerItem(int num)
        {
            this.num = num;
        }
    }
}
