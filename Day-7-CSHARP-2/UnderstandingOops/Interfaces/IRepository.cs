using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnderstandingOops.Models;

namespace UnderstandingOops.Interfaces
{
    // use of generics for type independent code and forcing contract
    internal interface IRepository<K,T> where T : class
    {
        public T Create(T item);
        public T? GetAccount(K key);
        public List<T>? GetAccounts();

        public T? Update(K key,T item);
        public T? Delete(K key);

    }
}