using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZabbixActiveUserService
{
    public class User : IEquatable<User>
    {
        string _Username;
        int _Condition;
        public User(string lUsername, int lCondition)
        {
            _Username = lUsername;
            _Condition = lCondition;
        }

        public string Username
        {
            get { return _Username; }
            set { _Username = value; }
        }

        public int Condition
        {
            get { return _Condition; }
            set { _Condition = value; }
        }

        #region Override функции
        public override int GetHashCode() { return 0; }
        public override string ToString()
        {
            return String.Format("{0}({1})", _Username, _Condition);
        }
        public bool Equals(User other)
        {
            if (other == null) return false;
            if (
                (this._Username.Equals(other._Username)) &&
                (this._Condition.Equals(other._Condition))
                )
                return true;
            else return false;
        }
        public bool Equals(string lUsername)
        {
            if (lUsername == "") return false;
            if (this.Username== lUsername)
                return true;
            else return false;
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            User objAsGetЗапрос = obj as User;
            if (objAsGetЗапрос == null) return false;
            else return Equals(objAsGetЗапрос);
        }
        #endregion
    }
}
