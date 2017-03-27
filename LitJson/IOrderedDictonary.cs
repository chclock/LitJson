using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace LitJson.LitJson
{
    public interface IOrderedDictionary : IDictionary, ICollection, IEnumerable
    {
        // Methods
        new IDictionaryEnumerator GetEnumerator();
        void Insert(int index, object key, object value);
        void RemoveAt(int index);

        // Properties
        object this[int index] { get; set; }

    }
}
