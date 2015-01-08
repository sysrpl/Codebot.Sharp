using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Codebot.Xml;

namespace Codebot.Dota.Entities
{
    public class LookupCollection<T> : IEnumerable<T> where T : Lookup, new()
    {
        private List<T> list;
        private Func<string, T> locate;

        public LookupCollection()
        {
            list = new List<T>();
            list.Add(new T());
        }

        public LookupCollection(Func<string, T> locate)
            : this()
        {
            this.locate = locate;
        }

        public T Locate(string name)
        {
            name = name.ToLower();
            return locate == null ? list.Where(item => item.Name.ToLower() == name).FirstOrDefault() : locate(name);
        }

        internal void Add(T item)
        {
            list.Add(item);
        }

        internal void Add(Element element)
        {
            T item = new T();
            item.Parse(element);
            Add(item);
        }

        internal void AddRange(Elements elements)
        {
            foreach (Element element in elements)
                Add(element);
        }

        public T this [int id]
        {
            get
            {
                var query = from item in list
                                        where item.Equals(id)
                                        select item;
                return query.FirstOrDefault() ?? list[0];
            }
        }

        public T this [string handle]
        {
            get
            {
                var query = from item in list
                                        where item.Handle == handle
                                        select item;
                return query.FirstOrDefault() ?? list[0];
            }
        }

        public IList<T> ToList()
        {
            return list as IList<T>;
        }

        public T[] ToArray()
        {
            return list.ToArray();
        }

        #region IEnumerable implementation

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        #endregion

        #region IEnumerable implementation

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        #endregion
    }
}
