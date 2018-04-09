using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.IO;
using System.Net;

namespace test
{
    class k
    {
        public string kk;
        public IPEndPoint ep;
    }

    class S
    {
        static void fun(k kk)
        {
            kk = new k();
            kk.kk = "888";
        }

        static void Main()
        {
            k kk = new k();
            kk.kk = "111";

            k newk = kk;

            k fuk = new k();
            fuk.kk = "999";

            newk = fuk;

            List<k> listK = new List<k>();
            listK.Add(new k());
            listK[0] = kk;
            fun(listK[0]);
            
            Console.WriteLine(listK[0].kk);
        }
    }
}
