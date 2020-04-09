using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Remoting.Channels;
using Panda;
using Scrips;
using Unity.Collections;
using UnityEditor;
using UnityEngine.Analytics;
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

        public GameObject own_goal;
        public GameObject other_goal;
        public GameObject ball;

        private PandaBehaviour pb;
        
        //Team Members
        private GameObject goalie;
        private GoalieController goalieController;
        //private GameObject ballChaser;
        private GoalieController ballChaserController;
        private List<GameObject> chasers;
        //private List<GoalieController> chaserControllers;

        //Goalie parameters
        private readonly float _defRadius = 6f; //Must be between above bounds
        private float _maxDistanceToGoalGoalie;
        private float _minDistanceToGoalGoalie;
        //private float allowed_def_pos_err = 0.5f;
        private Vector3 _optimalDefPos;
        
        //Chaser parameters
        private float _dribblingDistance = 5f;

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

            friends = GameObject.FindGameObjectsWithTag(friend_tag);
            enemies = GameObject.FindGameObjectsWithTag(enemy_tag);

            goalie = friends[0];
            goalieController = new GoalieController(goalie);
            _maxDistanceToGoalGoalie = _defRadius + 2f;
            _minDistanceToGoalGoalie = _defRadius - 2f;

            foreach (var teamMate in friends)
            {
                if (isGoalie(teamMate))
                    continue;

                chasers.Add(teamMate);
                //chaserControllers.Add(new GoalieController(teamMate));
            }
            ballChaserController = new GoalieController(gameObject);
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
            Gizmos.DrawSphere(_optimalDefPos, 3);
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
            if (friends[0].name.Equals(name))
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
            return _maxDistanceToGoalGoalie > distance_to_goal && _minDistanceToGoalGoalie < distance_to_goal;
        }

        [Task]
        void TakeDefenciveStance()
        {
            _optimalDefPos = calculateOptimalDefPos();
            goToDefencePos();
            
            //Vector3 goalie_pos = transform.position;
            /*if (allowed_def_pos_err < (optimal_def_pos - transform.position).magnitude)
            {
                goToDefencePos();
            }*/
        }

        private Vector3 calculateOptimalDefPos()
        {
            Vector3 ball_pos = ball.transform.position;
            Vector3 goal_pos = own_goal.transform.position;
            Vector3 attack_vector = ball_pos - goal_pos;
            Vector3 unit_attack_vector = attack_vector / attack_vector.magnitude;
            Vector3 final_attack_vector = unit_attack_vector * _defRadius;
            return final_attack_vector + goal_pos;
        }

        [Task]
        void GoToOurGoal()
        {
            _optimalDefPos = calculateOptimalDefPos();
            goToDefencePos();
        }

        private void goToDefencePos()
        {
            Move move = goalieController.moveToPosition(_optimalDefPos);
            m_Car.Move(move.steeringAngle, move.throttle, move.footBrake, move.handBrake);
        }
        
        //***************************************************************************
        //CHASER
        
        [Task]
        bool ClosestToBall()
        {
            GameObject closestPlayer = findClosestCar(chasers);

            return closestPlayer.name == name;
        }
        
        private GameObject findClosestCar(List<GameObject> players)
        {
            GameObject closestPlayer = null;
            float shortestDistance = float.PositiveInfinity;
            foreach (var teamMate in players)
            {
                float distanceToBall = this.distanceToBall(teamMate);
                if (shortestDistance > distanceToBall)
                {
                    closestPlayer = teamMate;
                    shortestDistance = distanceToBall;
                }
            }

            return closestPlayer;
        }

        private bool isGoalie(GameObject teamMate)
        {
            return teamMate.name == friends[0].name;
        }

        [Task]
        bool HaveBall()
        {
            return _dribblingDistance > distanceToBall(gameObject);
        }

        private float distanceToBall(GameObject teamMate)
        {
            return (teamMate.transform.position - ball.transform.position).magnitude;
        }

        [Task]
        void Dribble()
        {
            Vector3 enemyGoalPos = other_goal.transform.position;
            Move move = ballChaserController.moveToPosition(enemyGoalPos);
            m_Car.Move(move.steeringAngle, move.throttle, move.footBrake, move.handBrake);
        }

        [Task]
        void InterceptBall()
        {
            Vector3 ballPos = ball.transform.position;
            Move move = ballChaserController.moveToPosition(ballPos);
            m_Car.Move(move.steeringAngle, move.throttle, move.footBrake, move.handBrake);
        }
        
        //***************************************************************************
        //DESTROYER
        
        [Task]
        void RamEnemyGoalie()
        {
            GameObject closestEnemy = null;
            float shortestDistance = float.PositiveInfinity;
            foreach (var enemy in enemies)
            {
                float distanceToEnemy = this.distanceToEnemy(enemy);
                if (shortestDistance > distanceToEnemy)
                {
                    closestEnemy = enemy;
                    shortestDistance = distanceToEnemy;
                }
            }

            Move move = ballChaserController.moveToPosition(closestEnemy.transform.position);
            m_Car.Move(move.steeringAngle, move.throttle, move.footBrake, move.handBrake);
        }
        
        private float distanceToEnemy(GameObject enemy)
        {
            return (transform.position - enemy.transform.position).magnitude;
        }
    }
}
