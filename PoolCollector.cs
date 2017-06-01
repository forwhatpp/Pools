using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Pool {
    public class PoolCollector<T> {
        private List<Collection<T>> _collectors;
        public delegate T CreateObject();
        private CreateObject _createObjectFunction;

        public PoolCollector() {
            this._collectors = new List<Collection<T>>();
        }

        public PoolCollector(CreateObject createObjectFcuntion) {
            this._collectors = new List<Collection<T>>();
            this._createObjectFunction = createObjectFcuntion;
        }

        public void AddObject(T obj, bool onUse = true) {
            Collection<T> collection = new Collection<T>(obj, !onUse);
            this._collectors.Add(collection);
        }

        public T GetFreeObject() {
            Collection<T> collection = _collectors.Find(o => o.IsFree());
            T obj = default(T);

            //Debug.Log("GetFreeObject > " + (collection != null));

            if(collection != null) {
                obj = collection.GetObject();
                collection.SetIsFree(false);
            } else {

                collection = createFreeObject();
                if(collection != null) {
                    obj = collection.GetObject();
                    collection.SetIsFree(false);
                }

            }

            return obj;
        }

        public T[] GetUseObject() {
            List<T> usedObject = new List<T>();
            foreach(var col in _collectors.FindAll(obj => !obj.IsFree())) {
                usedObject.Add(col.GetObject());
            }

            return usedObject.ToArray();
        }

        private Collection<T> createFreeObject() {
            if(_createObjectFunction != null) {
                T obj = _createObjectFunction();
                Collection<T> collection = new Collection<T>(obj);
                this._collectors.Add(collection);
                return collection;
            }
            return null;
        }

        //private static T DeepCopy<T>(T other) {
        //    using(MemoryStream ms = new MemoryStream()) {
        //        BinaryFormatter formatter = new BinaryFormatter();
        //        formatter.Serialize(ms, other);
        //        ms.Position = 0;
        //        return (T)formatter.Deserialize(ms);
        //    }
        //}


        public void DestroyAllObject() {
            //var usingCollections = this._collectors.FindAll(collection => !collection.IsFree());
            //foreach(var obj in usingCollections) {
            //    DestroyObject(obj);
            //}

            //Mayby find all still check all object :\
            foreach(var obj in _collectors) {
                if(!obj.IsFree()) DestroyObject(obj);
            }
        }

        /// <summary>
        /// Destroy Object from scence
        /// </summary>
        public void DestroyObject(T obj) {
            Collection<T> collection = _collectors.Find(o => !o.IsFree() && o.GetObject().Equals(obj));
            DestroyObject(collection);
        }

        public void DestroyObject(System.Func<T, bool> function) {
            foreach(var collection in _collectors) {
                if(function(collection.GetObject())) {
                    DestroyObject(collection);
                }
            }
        }

        private void DestroyObject(Collection<T> collection) {
            collection.SetIsFree(true);

            //Destroy object that implement onDestroy :3
            IPoolObject poolObject = collection.GetObject() as IPoolObject;
            if(poolObject != null) {
                poolObject.OnDestroy();
            }

        }

        public T Find(object obj) {
            return findCollection(obj).GetObject();
        }

        public T Find(System.Func<T, bool> function) {
            foreach(var obj in _collectors) {
                if(function(obj.GetObject())) {
                    return obj.GetObject();
                }
            }
            return default(T);
        }

        private Collection<T> findCollection(object obj) {
            Collection<T> c = _collectors.Find(o => !o.IsFree() && (object)o.GetObject() == obj);
            return c;
        }
    }

    class Collection<T> {
        private T _obj;
        private bool _isFree;

        public Collection(T obj, bool isFree = true) {
            this._obj = obj;
            this._isFree = isFree;
        }

        public T GetObject() {
            return _obj;
        }

        public bool IsFree() {
            return _isFree;
        }

        public void SetIsFree(bool isFree) {
            this._isFree = isFree;
        }
    }
}