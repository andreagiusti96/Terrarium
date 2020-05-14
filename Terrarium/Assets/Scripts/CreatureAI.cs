using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// This is the class where most of the work will happen,
    /// like in the previous assignments.
    /// </summary>
    public class CreatureAI : MonoBehaviour
    {
        public string specieName;
        public int specieID;

        /// <summary>
        /// This function is called when your creature is within an acceptable
        /// range to eat some food, adapted to your regime of course.
        /// You do not need to necessarily eat it of course.
        /// </summary>
        /// <param name="food"></param>
        public virtual void OnAccessibleFood(GameObject food)
        {
        }

        public virtual void updateStats()
        {
        }
    }
}