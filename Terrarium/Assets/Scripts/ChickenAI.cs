using System;
using System.IO;
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
    public class ChickenAI : CreatureAI
    {
        private Creature creature;


        // navigation variables
        float[,] exploarationMap; // range [0,1] 0= just visited, 1= never visited
        float worldMin = -200;
        float worldMax = 600;
        float worldSize;
        float resolution = 20;

        System.Random rand;

        // Specie's stats (shared by all the speciemen)
        static int nOfSpeciemens;
        static float avgSize;
        static float avgSpeed;
        static float avgSensing;
        static float avgGeneration;
        static float avgEnergy;

        public void Start()
        {
            creature = GetComponent<Creature>();

            worldSize = worldMax - worldMin;
            rand = new System.Random();

            initExplorationMap();
        }

        public void Update()
        {
            updateExplorationMap();
            Vector3 dir = Vector3.zero;
            float speed = 1;

            List<GameObject> predators = creature.Sensor.SensePredators(creature);
            List<GameObject> food = creature.Sensor.SensePlants(creature);

            if (predators.Count > 0)
            {
                // run away from the predator
                dir = (transform.position - getClosest(predators).transform.position).normalized;
            }
            else if (food.Count > 0 && creature.Energy<creature.MaxEnergy*0.8)
            {
                // can see food, go and take it
                dir = (getClosest(food).transform.position - transform.position).normalized;
                speed = 0.6f;
            }
            else
            {
                // cannot see any food, explore
                dir = unexploredDirection(creature.Sensor.SensingRadius * 2);
                speed = 0.5f;
            }

            creature.Move(dir, speed);

            // Debug.Log("dir=" + dir + " speed=" +speed);
        }

        public override void OnAccessibleFood(GameObject food)
        {
            creature.Eat(food);
            if (UnityEngine.Random.Range(0, creature.MaxEnergy) < creature.Energy - creature.MaxEnergy * 0.1f) creature.Reproduce();
        }

        void initExplorationMap()
        {
            exploarationMap = new float[(int)(worldSize / resolution), (int)(worldSize / resolution)];
            int max = (int)(worldSize / resolution) - 1;

            for (int i = 0; i < max; i++)
            {
                for (int j = 0; j < max; j++)
                {
                        exploarationMap[i, j] = 1;
                }
            }
        }

        void updateExplorationMap()
        {
            int max = (int)(worldSize / resolution)-1;

            for(int i=0; i<max; i++)
            {
                for (int j = 0; j < max; j++)
                {
                    exploarationMap[i, j] = exploarationMap[i, j] + (1- exploarationMap[i, j]) * 0.004f;

                    if (Vector3.Distance( cellPosition(i,j), transform.position) < creature.Sensor.SensingRadius)
                    {
                        exploarationMap[i, j] = 0;
                    }
                }
            }
        }

        void drawExplorationMap()
        {
            int max = (int)(worldSize / resolution) - 1;

            for (int i = 0; i < max; i++)
            {
                for (int j = 0; j < max; j++)
                {
                    if(exploarationMap[i,j] < 0.2)
                    {
                        Gizmos.color = Color.green;
                    }

                    else if (exploarationMap[i, j] < 0.8)
                    {
                        Gizmos.color = Color.yellow;
                    }
                    else
                    {
                        Gizmos.color = Color.red;
                    }
                    Gizmos.DrawSphere(cellPosition(i, j), 1);
                }
            }
        }

        Vector3 unexploredDirection(float explorationRadius)
        {
            Vector3 dir = Vector3.zero;
            int max = (int)(worldSize / resolution) - 1;

            for (int i = 0; i < max; i++)
            {
                for (int j = 0; j < max; j++)
                {
                    Vector3 rel_pos = cellPosition(i, j) - transform.position;
                    if (rel_pos.magnitude < explorationRadius)
                    {
                        dir += exploarationMap[i, j] * rel_pos / rel_pos.sqrMagnitude;
                    }
                }
            }

            foreach(GameObject neighbour in creature.Sensor.SenseCreatures(creature))
            {
                Vector3 rel_pos = (neighbour.transform.position - transform.position);
                dir -= rel_pos / rel_pos.sqrMagnitude;
            }

            if (dir.magnitude == 0) dir = new Vector3(rand.Next(-1, 1), 0, rand.Next(-1, 1));

            return dir.normalized;
        }

        Vector3 cellPosition(int i, int j)
        { 
            float x = worldMin +  (float)j * resolution + resolution / 2f;
            float z = worldMin + (float)i * resolution + resolution / 2f;

            return new Vector3(x, 0f, z);
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, creature.Sensor.SensingRadius);
            //drawExplorationMap();
        }

        GameObject getClosest(List<GameObject> set)
        {
            GameObject closest = set[0];

            foreach(GameObject obj in set)
            {
                if (Vector3.Distance(obj.transform.position, transform.position) < Vector3.Distance(closest.transform.position, transform.position))
                    closest = obj;
            }

            return closest;
        }

        // inherit from CreatureAI
        // collects data from all the member of the specie and update specie's stats
        // stats are logged as csv in a txt file in Assets/Logs folder (you have to create the Logs folder)
        public override void updateStats()
        {
            List<GameObject> agents = GameObject.FindGameObjectsWithTag("carnivore").ToList();
            agents.AddRange(GameObject.FindGameObjectsWithTag("herbivore").ToList());

            agents = agents.FindAll(c => c.GetComponent<CreatureAI>().specieID == specieID);

            nOfSpeciemens = agents.Count;

            avgSensing = 0;
            avgSize = 0;
            avgSpeed = 0;
            avgGeneration = 0;
            avgEnergy = 0;

            foreach (GameObject agent in agents)
            {
                Creature c = agent.GetComponent<Creature>();

                avgSensing += c.Sensor.SensingRadius;
                avgSize += c.Size;
                avgSpeed += c.MaxSpeed;
                avgGeneration += c.Generation;
                avgEnergy += c.Energy;
            }

            avgSensing = avgSensing / (float)nOfSpeciemens;
            avgEnergy = avgEnergy / (float)nOfSpeciemens;
            avgSize = avgSize / (float)nOfSpeciemens;
            avgSpeed = avgSpeed / (float)nOfSpeciemens;
            avgGeneration = ((float)avgGeneration) / (float)nOfSpeciemens;

            Debug.Log(specieName + " " + nOfSpeciemens + " avgSize=" + avgSize + " avgSensing=" + avgSensing + " avgSpeed=" + avgSpeed + " avgGeneration=" + avgGeneration);

            string[] line = { Time.time.ToString() + "," + avgSensing.ToString() + "," + avgEnergy.ToString() + "," + avgSize.ToString() + "," + avgSpeed.ToString() + "," + avgGeneration.ToString() + "," + nOfSpeciemens.ToString() + "," };
            string docPath = Path.GetFullPath("Assets/Logs/");
            File.AppendAllLines(Path.Combine(docPath, "OutcomesChickens.txt"), line);
        }
    }
}