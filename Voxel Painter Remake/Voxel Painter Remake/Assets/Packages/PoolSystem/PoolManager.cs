using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region ### Overview ###
/*
    ### Summary ###
        PoolManager extends from Singleton class, so it can be globally accessed.
        If you do not add a PoolManager in the scene, it'll automatically create one when you call it.
        You can create a PoolInfoList before runtime. 
        If during runtime, you use NewGameObject(prefab) and prefab doesn't exist in the pool info list it will add the prefab to the pool info list and then create an instance of it.

    ### Important ###
        --------------------
        Namespace
        --------------------
        using Liminal;

        --------------------
        Instantiate(prefab)
        --------------------
            To Instantiate: PoolManager.Instance.NewGameObject(prefab);
            To Instantiate with visiblity: PoolManager.Instance.NewGameObject(prefab,true);
            To Instantiate with position: PoolManager.Instance.NewGameObject(prefab,pos,true);
        --------------------
        Destroy(GameObject):
        --------------------
            To Destroy: PoolManager.Instance.DestroyGameObject(instance);
            The instance is the instance created by the pool manager. 
            If the instance has not been listed (meaning it wasn't created by the pool) 
            it'll call Destroy(gameObject);
        --------------------
        Events:
        --------------------
            onInstantiated: 
                PoolManager.Instance.onInstantiated += OnPoolObjectInstantiated;
                void OnPoolObjectInstantiated(GameObject instance){}

            onInstantiated: 
                PoolManager.Instance.onDestroyed += OnInstanceDestroyed;
                void OnInstanceDestroyed(GameObject instance){}

        --------------------
        TODO
        --------------------
            - Capping so there is an absolute maximum
            - Amount of 'pooled' is not specified.

 */

#endregion

namespace Liminal
{
    public class PoolManager : Singleton<PoolManager>
    {
        //Human readable pool list
        public List<PoolInfo> poolInfoList = new List<PoolInfo>();

        void Awake()
        {
            Init();
        }

        #region ### Dictionaries ###
        private Dictionary<GameObject, PoolInfo> poolDictionary = new Dictionary<GameObject, PoolInfo>(); //Prefab/PoolInfo
        private Dictionary<GameObject, GameObject> IPDictionary = new Dictionary<GameObject, GameObject>(); //Instance/Prefab
        #endregion

        #region ### Events ###
        public delegate void PoolManagerDelegate(GameObject instance);
        public event PoolManagerDelegate onInstantiated;
        public event PoolManagerDelegate onDestroyed;
        #endregion

        #region ### Core ### 

        //Initilize the pool, basically filling up the dictionary
        private void Init()
        {
            foreach (PoolInfo pool in poolInfoList)
            {
                pool.InitPool(this);
                if (!poolDictionary.ContainsKey(pool.prefab))
                    poolDictionary.Add(pool.prefab, pool);
            }
        }

        //Listing a new prefab to the pool list.
        public void ListNewPrefab(GameObject prefab, int amount)
        {
            if (!poolDictionary.ContainsKey(prefab))
            {
                PoolInfo newPoolInfo = new PoolInfo(prefab.name, prefab, amount);
                poolDictionary.Add(prefab, newPoolInfo);
                poolInfoList.Add(newPoolInfo);
                newPoolInfo.InitPool(this);
            }
        }
        #endregion

        #region ### Destroy ###

        //Equalivant to Destroy(gameObject).
        public void DestroyGameObject(GameObject instance)
        {
            //Null check in case you are leisurely trying to destroy an object that has been removed from EXISTENCE, not sure how.. 
            if (instance != null)
            {
                if(onDestroyed != null)
                    onDestroyed.Invoke(instance);

                //If the instance is part of the pool
                if (IPDictionary.ContainsKey(instance))
                {
                    //Add it back into the pool as available
                    GameObject prefab = IPDictionary[instance];
                    PoolInfo poolInfo = poolDictionary[prefab];
                    poolInfo.AddToPool(instance);

                    //Hide gameobject
                    instance.SetActive(false);
                }
                else
                {
                    //Otherwise use Unity Destroy
                    Destroy(instance);
                }
            }
        }

        #endregion

        #region ### Instantiation ###

        public GameObject NewGameObject(GameObject prefab, bool sendEvent, bool isActive)
        {
            ListNewPrefab(prefab, 1);
            PoolInfo poolInfo = poolDictionary[prefab];

            #region ### Validating list ###

            if (poolInfo.availablePool.Count == 0)
                poolInfo.InitPoolObjects(this);

            //Check if object 0 is not null
            while (poolInfo.availablePool[0] == null)
            {
                poolInfo.availablePool.RemoveAt(0);

                if (poolInfo.availablePool.Count == 0)
                    poolInfo.InitPoolObjects(this);
            }

            #endregion

            #region ### Assign New GameObject ###
            GameObject newGo = poolInfo.availablePool[0];
            poolInfo.RemoveFromPool(newGo);

            newGo.SetActive(isActive);

            #endregion

            if (sendEvent) {
                if(onInstantiated != null)
                    onInstantiated.Invoke(newGo);
            }
                

            return newGo;
        }
        public GameObject NewGameObject(GameObject prefab,bool isActive)
        {
            return NewGameObject(prefab, true, isActive);
        }
        public GameObject NewGameObject(GameObject prefab, Vector3 newPos, bool isActive)
        {
            GameObject newGo = NewGameObject(prefab, isActive);
            newGo.transform.position = newPos;

            if(onInstantiated != null)
                onInstantiated.Invoke(newGo); //Send my event here instead.
            return newGo;
        }

        #endregion

        #region ### PoolInfo ###

        [System.Serializable]
        public class PoolInfo
        {
            public string m_name = "";
            public GameObject prefab;
            public int amount = 10;
            private int spawnCount = 0;
            public List<GameObject> availablePool = new List<GameObject>();
            public List<GameObject> unavailablePool = new List<GameObject>();

            private Transform parent;
            public int curAmount = 0;

            public PoolInfo(string name)
            {
                m_name = name;
            }
            public PoolInfo(string name, GameObject _prefab, int _amount)
            {
                m_name = name;
                prefab = _prefab;
                amount = _amount;
            }

            public void InitPool(PoolManager poolManager)
            {
                //Create a parent holder for it in inside this script
                GameObject parentObject = new GameObject();
                parentObject.name = prefab.name + " pool group";
                parentObject.transform.SetParent(this.parent);

                this.parent = parentObject.transform;
                InitPoolObjects(poolManager);
            }

            public void InitPoolObjects(PoolManager poolManager)
            {
                for (int i = 0; i < amount; i++)
                {
                    bool prefabState = prefab.activeInHierarchy;

                    #region Creation

                    //Turn off the prefab so that "OnEnable" doesn't get called.
                    prefab.SetActive(false);
                    //Create the actual instance
                    GameObject newGo = Instantiate(prefab);
                    //Turn the prefab back on
                    prefab.SetActive(true);

                    #endregion

                    #region Organizing Instance, Naming Etc

                    newGo.name = prefab.name + "Instance" + spawnCount;
                    newGo.transform.SetParent(parent);

                    #endregion

                    #region Organizing Pool

                    spawnCount++;
                    poolManager.IPDictionary.Add(newGo, prefab);
                    availablePool.Add(newGo);

                    #endregion
                }

                curAmount = availablePool.Count + unavailablePool.Count;

            }

            public void AddToPool(GameObject instance)
            {
                availablePool.Add(instance); //Add from available list
                unavailablePool.Remove(instance); //Remove to waiting list.
                instance.transform.SetParent(parent); //Return to list
            }

            public void RemoveFromPool(GameObject instance)
            {
                availablePool.Remove(instance); //Remove from available list
                unavailablePool.Add(instance); //Add to waiting list.
            }
        }

        #endregion

    }
}
