using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Cargo/CargoTypeList", fileName = "CargoTypeList")]
	public class CargoTypeList : ScriptableObject
    {
        public List<CargoType> List;
    }
}