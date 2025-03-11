using System;
using System.Collections.Generic;
using UnityEngine;

namespace StaticData
{
    [CreateAssetMenu(menuName = "StaticData/PreloadConfig", fileName = "PreloadConfig", order = 0)]
    public class PreloadConfig : ScriptableObject
    {
        public List<PreloadGroup> LevelGroups;
    }
    
    [Serializable]
    public class PreloadGroup
    {
        public string AssetGroupName;
        public int LoadAfterUnlocked;
    }
}