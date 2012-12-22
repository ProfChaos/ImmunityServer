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


        /// <summary>
        /// This may be used for storing all the data from a player.
        /// </summary>
        /// <param name="username"></param>
        public User(string username)
        {
            this.username = username;
        }
    }
}
