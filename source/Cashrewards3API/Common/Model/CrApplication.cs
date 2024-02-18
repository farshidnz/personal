using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Common.Model
{
    public class CrApplication
    {
        private readonly string m_key;
        public CrApplication(string key)
        {
            m_key = key;
        }

        public string Key => m_key;
    }
}
