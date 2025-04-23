using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DolphinDBForExcel
{
    internal class Singleton<T> where T : class, new()
    {
        private readonly static T instance = new T();

        protected Singleton() { }

        public static T Instance
        {
            get { return instance; }
        }
    }
}
