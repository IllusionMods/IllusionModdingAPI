using System;
using System.Collections.Generic;
using UnityEngine;

namespace KKAPI.Utilities
{
    /// <summary>
    /// Class that behaves like a Dictonary[TKey,TValue].
    /// Keys are held by weak reference; if a key is deleted by the GC, the Key-Value of that key is deleted.
    /// </summary>
    internal class WeakKeyDictionary<TKey,TValue> where TKey : class
    {
        // Practically the same as Dictionary<WeakKey<TKey>, TValue>()
        // Allows searches to both WeakKey/object by making the key an object.
        private readonly Dictionary<object, TValue> _dict = new Dictionary<object, TValue>( new WeakKeyComperer<object>() );
        private int _cleanFrame;

        public void Add( TKey key, TValue value )
        {
            SometimesSweepDeadValues();
            _dict.Add(new WeakKey<TKey>(key), value);
        }

        public bool Remove( TKey key )
        {
            SometimesSweepDeadValues();
            return _dict.Remove(key);
        }

        public bool TryGetValue( TKey key, out TValue value )
        {
            return _dict.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get 
            {
                return _dict[key];
            }

            set
            {
                SometimesSweepDeadValues();
                _dict[new WeakKey<TKey>(key)] = value;
            }
        }

        private void SometimesSweepDeadValues()
        {
            // The cost of a CPU that constantly performs perfect cleaning is high, so cleaning is performed only once per frame.
            // Memory may be sacrificed somewhat.

            if (_cleanFrame == Time.frameCount)
                return; // Run only once per frame.
            _cleanFrame = Time.frameCount;

            List<object> deads = new List<object>();

            foreach (var pair in _dict)
            {
                var key = (WeakKey<TKey>)pair.Key;
                if (!key.IsAlive)
                    deads.Add(key);
            }

            for (int i = 0; i < deads.Count; ++i)
                _dict.Remove(deads[i]);
        }

        public bool ContainsKey( TKey key )
        {
            return _dict.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            List<object> deads = new List<object>();
            _cleanFrame = Time.frameCount;

            foreach ( var pair in _dict )
            {
                var key = (WeakKey<TKey>)pair.Key;
                var target = key.Target;

                if (target != null)
                    yield return new KeyValuePair<TKey, TValue>(target, pair.Value);
                else
                    deads.Add(key);
            }

            for (int i = 0; i < deads.Count; ++i)
                _dict.Remove(deads[i]);
        }
    }

    /// <summary>
    /// A class that behaves as a dictionary key.
    /// Keys are held by weak reference.
    /// </summary>
    internal class WeakKey<TKey> where TKey : class
    {
        private System.WeakReference _ref;
        private int _hash;

        public WeakKey( TKey key )
        {
            if (key == null)
                throw new System.ArgumentNullException(nameof(key));

            _ref = new WeakReference(key);
            _hash = key.GetHashCode();
        }

        public bool IsAlive => _ref.IsAlive;

        public TKey Target => (TKey)_ref.Target;

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetHashCode() != _hash )
                return false;

            if( obj is TKey )
            {
                return _ref.Target == obj;
            }
            else if( obj is WeakKey<TKey> )
            {
                var other = (WeakKey<TKey>)obj;

                var selfKey = _ref.Target;
                var otherKey = other._ref.Target;

                var isAlive = selfKey != null;
                var isOtherAlive = otherKey != null;

                if (isAlive != isOtherAlive)
                    return false;          //Alive and dead values are not equal.

                if ( isAlive )
                {
                    //Both are alive and are determined by Target.
                    return selfKey == otherKey;
                }   

                //Both dead.
                return System.Object.ReferenceEquals(this, obj);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return _hash;
        }
    }

    internal class WeakKeyComperer<TKey> : IEqualityComparer<object> where TKey : class
    {
        /// <summary>
        /// Determine whether the objects are the same.
        /// Accepts TKey and WeakKey[TKey] as arguments.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public new bool Equals(object x, object y)
        {
            if (y is WeakKey<TKey>)
                return y.Equals(x);

            return x.Equals(y);
        }

        public int GetHashCode(object obj)
        {
            return obj.GetHashCode();
        }
    }
}
