using MhanoHarkness;
using System;

namespace Cake.Sample.Lib
{
    public class SuperClass
    {
        private readonly int i;

        public SuperClass(int i)
        {
            this.i = i;
        }

        public string DoSuperStuff()
        {
            return string.Format("Super {0} {1}!", this.i, Base64Url.ToBase64ForUrlString(BitConverter.GetBytes(DateTime.UtcNow.Ticks)));
        }
    }
}
