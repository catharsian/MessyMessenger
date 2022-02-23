using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Download_Manager_Server
{
    internal static class UserInfoFactory
    {
        public static UserInfo SingleTransform(UserInfoPattern x)
        {
            return new UserInfo(x.name, x.password);
        }
        public static UserInfoPattern SingleTransform(UserInfo x)
        {
            return new UserInfoPattern(x.username, x.password);
        }
        public static UserInfo[] ArrTransform(UserInfoPattern[] x)
        {
            var y = new UserInfo[x.Length];
            int i = 0;
            foreach (var p in x)
            {
                y[i] = SingleTransform(x[i]);
                i++;
            }
            return y;
        }
        public static UserInfoPattern[] ArrTransform(UserInfo[] x)
        {
            var y = new UserInfoPattern[x.Length];
            int i = 0;
            foreach (var p in x)
            {
                y[i] = SingleTransform(x[i]);
                i++;
            }
            return y;
        }
    }
    [Serializable]
    class UserInfoPattern
    {
        public string name { get; set; }
        public string password { get; set; }
        public UserInfoPattern (string name, string password)
        {
            this.name = name;
            this.password = password;
        }
    }
}
