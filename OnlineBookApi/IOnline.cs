using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineBookApi
{
    interface IOnline
    {
        byte[] GetCover(string query);
    }
}
