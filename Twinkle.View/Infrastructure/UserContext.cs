using System;
using System.Collections.Generic;

namespace Twinkle.View.Infrastructure
{
    public class UserContext
    {
        private Dictionary<string, Object> _userContext;

        public UserContext()
        {
            _userContext = new Dictionary<string, object>();
        }

        public void Set(string key, Object userObject)
        {
            _userContext.Add(key, userObject);
        }

        public T Get<T>(string key)
        {
            return (T)_userContext[key];
        }

        public void Delete(string key)
        {
            _userContext.Remove(key);
        }
        
        public void Clear()
        {
            _userContext.Clear();
        }
    }
}