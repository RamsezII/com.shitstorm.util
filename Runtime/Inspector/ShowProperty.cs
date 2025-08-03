using UnityEngine;

namespace _UTIL_
{
    public class ShowPropertyAttribute : PropertyAttribute
    {
        public string propertyName;
        public ShowPropertyAttribute(string propertyName) => this.propertyName = propertyName;
    }
}