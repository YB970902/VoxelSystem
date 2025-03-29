using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bean.Utils
{
    public class MonoSingleton<T> : MonoBehaviour where T : Component
    {
        private static T instance;

        private static bool isInit = false;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<T>();

                    if (instance == null)
                    {
                        instance = new GameObject(typeof(T).Name).AddComponent<T>();
                    }
                }

                return instance;
            }
        }

        private void Awake()
        {
            if (isInit)
            {
                Destroy(gameObject);
                return;
            }

            isInit = true;
            instance = GetComponent<T>();
            DontDestroyOnLoad(instance.gameObject);
        }

        protected virtual void Init()
        {
            
        }
    }
}