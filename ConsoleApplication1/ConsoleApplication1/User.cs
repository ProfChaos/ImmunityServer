using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication1
{
    class User
    {
        private string username;
        private int money = 0;
        private int kills = 0;
        private int towersBuilt = 0;
        private int ready = 0;

        public User(string username)
        {
            this.username = username;
        }
    }
}
