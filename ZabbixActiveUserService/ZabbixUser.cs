using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZabbixActiveUserService
{
    public class ZabbixUser : IEquatable<ZabbixUser>
    {
        string _Username;
        string _ZabbixUsername;
        public ZabbixUser(string Username,string ZabbixUsername)
        {
            _Username = Username;
            _ZabbixUsername = ZabbixUsername;
        }

        public string Username
        {
            get { return _Username; }
            set { _Username = value; }
        }

        public string ZabbixUsername
        {
            get { return _ZabbixUsername; }
            set { _ZabbixUsername = value; }
        }

        #region Override функции
        public override int GetHashCode() { return 0; }
        public override string ToString()
        {
            return String.Format("{0}({1})", _Username, _ZabbixUsername);
        }
        public bool Equals(ZabbixUser other)
        {
            if (other == null) return false;
            if (
                (this._Username.Equals(other._Username)) &&
                (this._ZabbixUsername.Equals(other._ZabbixUsername))
                )
                return true;
            else return false;
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            ZabbixUser objAsGetЗапрос = obj as ZabbixUser;
            if (objAsGetЗапрос == null) return false;
            else return Equals(objAsGetЗапрос);
        }
        #endregion
    }
}
