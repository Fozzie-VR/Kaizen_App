using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KaizenApp 
{

    public class IconFactory<T>
    {

        private readonly Queue<T> _objectPool = new();

        public System.Func<T> Factory;
        public System.Action<T> PreReturn;
        public System.Action<T> PreGet;

       public T GetIcon()
       {
            if(_objectPool.Count > 0)
            {
                return _objectPool.Dequeue();
            }
            T ret;
            if(Factory != null)
            {
                ret = Factory();
            }
            else
            {
                ret = default;
            }
            PreGet?.Invoke(ret);
            return ret;
        }

        public void ReturnIcon(T icon)
        {
            PreReturn?.Invoke(icon);
            _objectPool.Enqueue(icon);
        }

        public void AddToPool(int count)
        {
            for(int i = 0; i < count; i++)
            {
                _objectPool.Enqueue(Factory());
            }
        }
    }

}
