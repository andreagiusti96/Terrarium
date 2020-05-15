using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class GameManager : MonoBehaviour
    {

        public float edibleRadius;
        public TerrainManager terrainManager;
        public CreatureAI[] species;
        public GameObject[] spawnAreas;
        public int nIndividualsPerSpecies;

        // DEBUG
        int nHerbioures;
        int nCarnivore;
        float avgSize;
        float avgSpeed;
        float avgSensing;
        float avgGeneration;
        float avgEnergy;

        public void Awake()
        {
            SpawnCreatures();
        }

        public void Update()
        {
            GameObject[] herbivores = GameObject.FindGameObjectsWithTag("herbivore");
            GameObject[] carnivores = GameObject.FindGameObjectsWithTag("carnivore");

            nCarnivore = carnivores.Length;
            nHerbioures = herbivores.Length;

            avgSensing = 0;
            avgSize = 0;
            avgSpeed = 0;
            avgGeneration = 0;
            avgEnergy = 0;

            //satisfy the herbivores
            foreach (var animal in herbivores)
            {
                Creature c = animal.GetComponent<Creature>();
                List<GameObject> animalFood;

                avgSensing += c.Sensor.SensingRadius;
                avgSize += c.Size;
                avgSpeed += c.MaxSpeed;
                avgGeneration += c.Generation;
                avgEnergy += c.Energy;

                // Herbivores eat plants
                animalFood = c.Sensor.SensePlants(c);

                //the closest within the edible radius can be eaten
                GameObject closestFood=null;
                float distance = float.MaxValue;
                foreach(var foodPiece in animalFood)
                {
                    float localDistance = Vector3.Distance(animal.transform.position, foodPiece.transform.position);
                    if (localDistance<distance && localDistance < edibleRadius)
                    {
                        distance = localDistance;
                        closestFood = foodPiece;
                    }
                }
                if (closestFood != null)
                {
                    animal.GetComponent<CreatureAI>().OnAccessibleFood(closestFood);
                    // Debug.Log($"Calling the eating method !");
                }
            }

            avgSensing = avgSensing / (float)nHerbioures;
            avgEnergy = avgEnergy / (float)nHerbioures;
            avgSize = avgSize / (float)nHerbioures;
            avgSpeed = avgSpeed / (float)nHerbioures;
            avgGeneration = ((float)avgGeneration) / (float)nHerbioures;

            //same with carnivores
            foreach (var animal in carnivores)
            {
                Creature c = animal.GetComponent<Creature>();
                List<GameObject> animalFood;

                //carnivores eat herbivores
                animalFood = c.Sensor.SensePreys(c);


                //the closest within the edible radius can be eaten
                GameObject closestFood = null;
                float distance = float.MaxValue;
                foreach (var foodPiece in animalFood)
                {
                    float localDistance = Vector3.Distance(animal.transform.position, foodPiece.transform.position);
                    if (localDistance < distance && localDistance < edibleRadius)
                    {
                        distance = localDistance;
                        closestFood = foodPiece;
                    }
                }
                if (closestFood != null)
                {
                    animal.GetComponent<CreatureAI>().OnAccessibleFood(closestFood);
                    Debug.Log($"Calling the eating method !");
                }
            }

            foreach(CreatureAI specie in species)
            {
                specie.updateStats();
            }
        }

        // Reset for ml agents
        public void ResetEnvironment()
        {
            EndEpisodes();
            DestroyCreatures();
            DestroyPlants();
            SpawnCreatures();
        }

        private void EndEpisodes()
        {
            GameObject[] herb = GameObject.FindGameObjectsWithTag("herbivore");
            GameObject[] carn = GameObject.FindGameObjectsWithTag("carnivore");
            for (var i = 0; i < herb.Length; i++)
            {
                CreatureAI c = herb[i].GetComponent<CreatureAI>();
               // c.EndMe();
            }
            for (var i = 0; i < carn.Length; i++)
            {
                CreatureAI c = carn[i].GetComponent<CreatureAI>();
                //c.EndMe();
            }
        }

        private void SpawnCreatures()
        {
            //int n = species.Length * nIndividualsPerSpecies;
            int n = nIndividualsPerSpecies;

            for (int k = 0; k < species.Length; k++)
            {
                species[k].specieID = k;
                if (k == 2) n = 8;  //less predators than preys
                for (int i = 0; i < n; i++)
                {
                    Debug.Log($"Creating species {k} - creature {i}");
                    //float angle = (k * nIndividualsPerSpecies + i) * 360f / n;
                    float angle = (nIndividualsPerSpecies + i) * 360f / n;
                    Vector3 position = spawnAreas[k].transform.position + Quaternion.Euler(0, angle, 0f) * Vector3.forward * 50;
                    position.y = 5f;
                    Instantiate(species[k], position, new Quaternion(0, angle, 0, 0));
                }
            }

        }

        private void DestroyPlants()
        {
            GameObject[] plants = GameObject.FindGameObjectsWithTag("plant");
            for (var i = 0; i < plants.Length; i++)
            {
                Destroy(plants[i]);
            }
        }

        private void DestroyCreatures()
        {
            GameObject[] herb = GameObject.FindGameObjectsWithTag("herbivore");
            GameObject[] carn = GameObject.FindGameObjectsWithTag("carnivore");
            for (var i = 0; i < herb.Length; i++)
            {
                Destroy(herb[i]);
            }
            for (var i = 0; i < carn.Length; i++)
            {
                Destroy(carn[i]);
            }
        }

    }
}
