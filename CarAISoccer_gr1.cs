using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Panda;
using UnityEditor;

namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof(CarController))]
    public class CarAISoccer_gr1 : MonoBehaviour
    {
        private CarController m_Car; // the car controller we want to use

        public GameObject terrain_manager_game_object;
        TerrainManager terrain_manager;

        public GameObject[] friends;
        public string friend_tag;
        public GameObject[] enemies;
        public string enemy_tag;
        private string myName;

        public GameObject own_goal;
        public GameObject other_goal;
        public GameObject ball;

        private PandaBehaviour pb;

        private void Start()
        {
            // get the car controller
            m_Car = GetComponent<CarController>();
            terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();

            pb = GetComponent<PandaBehaviour>();
            // note that both arrays will have holes when objects are destroyed
            // but for initial planning they should work
            friend_tag = gameObject.tag;
            if (friend_tag == "Blue")
                enemy_tag = "Red";
            else
                enemy_tag = "Blue";

            friends = GameObject.FindGameObjectsWithTag(friend_tag);
            enemies = GameObject.FindGameObjectsWithTag(enemy_tag);

            myName = gameObject.name;

            ball = GameObject.FindGameObjectWithTag("Ball");

            
            // Plan your path here
            // ...
        }


        private void FixedUpdate()
        {
            
        }

        private void OnDrawGizmos()
        {
            if (friend_tag == "Blue")
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(transform.position, 5);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(transform.position, 5);
            }
        }

        private void Update()
        {
            pb.Reset();
            pb.Tick();
        }

         
        //*********************************************************************
        //DEFENCE
        
        [Task]
        bool IsGoalie()
        {
            if (friends[0].name.Equals(gameObject.name))
            {
                return true;
            }

            return false;
        }

        [Task]
        bool AtGoal()
        {
            return false;
        }

        [Task]
        void TakeDefenciveStance()
        {
            
        }

        [Task]
        void GoToOurGoal()
        {
            
        }
        
        //***************************************************************************
        //CHASER
        
        [Task]
        bool ClosestToBall()
        {
            return true;
        }

        [Task]
        bool HaveBall(float distanceToBall)
        {
            return false;
        }

        [Task]
        void Dribble()
        {
            
        }

        [Task]
        void InterceptBall()
        {
            
        }
        
        //***************************************************************************
        //DESTROYER
        
        [Task]
        void RamEnemyGoalie()
        {
            
        }
    }
}
