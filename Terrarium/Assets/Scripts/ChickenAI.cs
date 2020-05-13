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
    public class ChickenAI : CreatureAI
    {
        private Creature creature;

        float[,] exploarationMap;

        float worldMin = -200;
        float worldMax = 600;
        float worldSize;

        float resolution = 10;

        System.Random rand;

        public void Start()
        {
            Debug.Log($"Creature AI is ready");
            creature = GetComponent<Creature>();

            worldSize = worldMax - worldMin;
            rand = new System.Random();

            initExplorationMap();
            //Debug.DrawLine(new Vector3(worldMin, 0, worldMin), new Vector3(worldMax, 0, worldMax), Color.white, 100f);
            //Debug.DrawLine(cellPosition(0, 0), cellPosition((int)(worldSize / resolution) - 1, 0), Color.blue, 100f);
            //Debug.DrawLine( cellPosition((int)(worldSize / resolution) - 1, 0), cellPosition((int)(worldSize / resolution) - 1, (int)(worldSize / resolution) - 1), Color.blue, 100f);
        }

        public void Update()
        {
            updateExplorationMap();
            Vector3 dir = Vector3.zero;
            float speed = 1;

            List<GameObject> food= creature.Sensor.SensePlants(creature);
            if (food.Count > 0 && creature.Energy<creature.MaxEnergy*0.8)
            {
                // can see food, go and take it
                dir = (getClosest(food).transform.position - transform.position).normalized;
            }
            else
            {
                // cannot see any food, explore
                dir = unexploredDirection(creature.Sensor.SensingRadius * 2);
                speed = 0.5f;
            }

            creature.Move(dir, speed);

            Debug.Log("dir=" + dir + " speed=" +speed);
        }

        public override void OnAccessibleFood(GameObject food)
        {
            creature.Eat(food);
            if (creature.Energy > 0.2 * creature.MaxEnergy && UnityEngine.Random.Range(0,1)<0.1f) creature.Reproduce();
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
                    if(Vector3.Distance( cellPosition(i,j), transform.position) < creature.Sensor.SensingRadius)
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
                    if(exploarationMap[i,j] < 0.5)
                    {
                        Gizmos.color = Color.green;
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
            Vector3 dir = new Vector3(rand.Next(-1, 1), 0, rand.Next(-1, 1));
            dir = dir * 0.1f;
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
            drawExplorationMap();

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
    }
}