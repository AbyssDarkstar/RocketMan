using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Rockets/RocketTypeList", fileName = "RocketTypeList")]
	public class RocketTypeList : ScriptableObject
    {
        public List<RocketType> List;
    }
}