using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Currency/CurrencyTypeList", fileName = "CurrencyTypeList")]
	public class CurrencyTypeList : ScriptableObject
    {
        public List<CurrencyType> List;
    }
}