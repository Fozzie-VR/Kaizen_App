using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace KaizenApp
{
    [CreateAssetMenu(fileName = "KaizenEvents", menuName = "KaizenApp/KaizenEvents", order = 1)]
    public class KaizenEvents:ScriptableObject
    {
        public event Action<FloorIcon> FloorIconSpawned;
        public event Action<FloorIcon> FloorIconRemoved;

        public void OnFloorIconSpawned(FloorIcon icon)
        {
            FloorIconSpawned?.Invoke(icon);
        }

        public void OnFloorIconRemoved(FloorIcon icon)
        {
            FloorIconRemoved?.Invoke(icon);
        }

    }

}
