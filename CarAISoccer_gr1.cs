using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Panda;
using Scrips;
using UnityEditor;
using XNode.Examples.MathNodes;

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
        
        //Team Members
        private GameObject goalie;
        private GoalieController goalieController;
        
        //Team parameters
        private float max_distance_to_goal_goalie = 7f;
        private float min_distance_to_goal_goalie = 5f;
        private float def_radius = 6f; //Must be between above bounds
        private Vector3 optimal_def_pos;

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

            ball = GameObject.FindGameObjectWithTag("Ball");
            
            myName = gameObject.name;
            friends = GameObject.FindGameObjectsWithTag(friend_tag);
            enemies = GameObject.FindGameObjectsWithTag(enemy_tag);

            goalie = friends[0];
            goalieController = new GoalieController(goalie);

            // Plan your path here
            // ...
        }


        private void FixedUpdate()
        {
            
        }

        private void OnDrawGizmos()
        {
            //Assign team color
            if (friend_tag == "Blue")
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(transform.position, 2);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(transform.position, 2);
            }
            
            //Draw optimal defencive position

            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(optimal_def_pos, 3);
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
            Vector3 goal_pos = own_goal.transform.position;
            Vector3 goalie_pos = transform.position;
            float distance_to_goal = (goal_pos - goalie_pos).magnitude;

            return withinGoalMargin(distance_to_goal);
        }

        private bool withinGoalMargin(float distance_to_goal)
        {
            return max_distance_to_goal_goalie > distance_to_goal && min_distance_to_goal_goalie < distance_to_goal;
        }

        [Task]
        void TakeDefenciveStance()
        {
            optimal_def_pos = calculateOptimalDefPos();
            Move move = goalieController.moveToPosition(optimal_def_pos);
            m_Car.Move(move.steeringAngle, move.throttle, move.footBrake, move.handBrake);
        }

        private Vector3 calculateOptimalDefPos()
        {
            Vector3 ball_pos = ball.transform.position;
            Vector3 goal_pos = own_goal.transform.position;
            Vector3 attack_vector = ball_pos - goal_pos;
            Vector3 unit_attack_vector = attack_vector / attack_vector.magnitude;
            Vector3 final_attack_vector = unit_attack_vector * def_radius;
            return final_attack_vector + goal_pos;
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
