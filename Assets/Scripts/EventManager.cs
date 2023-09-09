using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace KaizenApp
{
    public class EventManager:MonoBehaviour
    {

        private static EventManager _eventManager;
        private Dictionary<string, Action<Dictionary<string, object>>> _eventDictionary;

        public static EventManager instance
        {
            get
            {
                if (!_eventManager)
                {
                    _eventManager = FindObjectOfType(typeof(EventManager)) as EventManager;

                    if (!_eventManager)
                    {
                        Debug.LogError("There needs to be one active EventManager script on a GameObject in your scene.");
                    }
                    else
                    {
                        _eventManager.Init();

                        //  Sets this to not be destroyed when reloading scene
                        DontDestroyOnLoad(_eventManager);
                    }
                }
                return _eventManager;
            }
        }

        public void Init()
        {
            if (_eventDictionary == null)
            {
                _eventDictionary = new Dictionary<string, Action<Dictionary<string, object>>>();
            }
        }

        public static void StartListening(string eventName, Action<Dictionary<string, object>> listener)
        {
            Action<Dictionary<string, object>> thisEvent;
            if (instance._eventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent += listener;
                instance._eventDictionary[eventName] = thisEvent;
            }
            else
            {
                thisEvent += listener;
                instance._eventDictionary.Add(eventName, thisEvent);
            }
        }

        public static void StopListening(string eventName, Action<Dictionary<string, object>> listener)
        {
            Action<Dictionary<string, object>> thisEvent;
            if (instance._eventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent -= listener;
                instance._eventDictionary[eventName] = thisEvent;
            }
        }

        public static void TriggerEvent(string eventName, Dictionary<string, object> message)
        {
            Action<Dictionary<string, object>> thisEvent = null;
            if (instance._eventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent.Invoke(message);
            }
        }




    }

}
