﻿using System;
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
        private Creature creature;

        public void Start()
        {
            Debug.Log($"Creature AI is ready");
            creature = GetComponent<Creature>();
        }

        public void Update()
        {
            //here, you can call creature.Move
            // you can make it sense the surroundings and reproduce, mutation is encompassed in the IReproduction implementation

            /*creature.Move(...)
            *creature.Sensor.SensePreys()
            *creature.Reproduce()
            */

            //Current example :
            var food = creature.Sensor.SensePlants(creature);
            Vector3 closestFood = Vector3.zero;
            float bestDistance = Vector3.Distance(closestFood, transform.position);
            foreach (var foodPiece in food)
            {
                if (Vector3.Distance(foodPiece.transform.position, transform.position) < bestDistance)
                {
                    bestDistance = Vector3.Distance(foodPiece.transform.position, transform.position);
                    closestFood = foodPiece.transform.position;
                }
            }
            if (closestFood != Vector3.zero)
            {
                Debug.DrawLine(transform.position, closestFood, Color.red);
                creature.Move(closestFood - transform.position, 1f);
            }
            //Vector3 dir = new Vector3(0.1f, 0f, 0.2f);
            //creature.Move(dir, 1f);
        }

        private (GameObject, bool) FindFood()
        {
            List<GameObject> food = creature.Sensor.SensePlants(creature);
            Vector3 closestFood = Vector3.zero;
            float bestDistance = Vector3.Distance(closestFood, transform.position);
            GameObject closestFoodObj = new GameObject("empty");
            if (food.Count == 0)
                return ((closestFoodObj, false));
            foreach (var foodPiece in food)
            {
                if (Vector3.Distance(foodPiece.transform.position, transform.position) < bestDistance)
                {
                    bestDistance = Vector3.Distance(foodPiece.transform.position, transform.position);
                    closestFood = foodPiece.transform.position;
                    closestFoodObj = foodPiece;
                }
            }
            return (closestFoodObj, true);
        }

        /// <summary>
        /// This function is called when your creature is within an acceptable
        /// range to eat some food, adapted to your regime of course.
        /// You do not need to necessarily eat it of course.
        /// </summary>
        /// <param name="food"></param>
        public virtual void OnAccessibleFood(GameObject food)
        {
            creature.Eat(food);
        }


    }
}