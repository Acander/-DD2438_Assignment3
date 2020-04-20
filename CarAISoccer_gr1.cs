using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Remoting.Channels;
using System.Security.Cryptography.X509Certificates;
using Panda;
using Scrips;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
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

        PandaBehaviour pb;
        
        //Team Members
        private GameObject goalie;
        private GameObject chaser;
        private GameObject destroyer;
        private Navigator navigator;

        //Goalie parameters
        private int defRadius = 15;
        private float maxDistanceToGoalGoalie;
        private float minDistanceToGoalGoalie;
        private Vector3 _optimalDefPos;
        
        //Chaser parameters
        private float kickDistance = 15f;
        private Vector3 optimalKickPos;
        private float kickRadius;

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

            kickRadius = kickDistance;

            goalie = friends[0];
            chaser = friends[1];
            destroyer = friends[2];
            
            maxDistanceToGoalGoalie = defRadius + 5f;
            minDistanceToGoalGoalie = defRadius - 5f;
            
            navigator = new Navigator(gameObject);
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
            if (goalie.name == name)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(_optimalDefPos, 3);
                Gizmos.DrawWireSphere(own_goal.transform.position, 2*defRadius);
                
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(transform.position, 2);
            }

            if (friend_tag == "Blue" && name != goalie.name)
            {
                if (ballOnOurSideOfTheField())
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawSphere(optimalKickPos, 3);
                    Gizmos.DrawWireSphere(optimalKickPos, kickRadius);
                }
                else
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(optimalKickPos, 3);
                    Gizmos.DrawWireSphere(optimalKickPos, kickRadius);
                }
            }
        }

        private void Update()
        {
            pb.Reset();
            pb.Tick();
            
            logCarState();
        }

        private void logCarState()
        {
            Debug.Log(name + " - " + navigator.getState());
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (shouldReverse(collision))
            {
                Debug.Log("I have crashed!");
                navigator.setToCrash();
            }
            
        }
        private bool shouldReverse(Collision collision)
        {
            if (collision.collider.attachedRigidbody == null)
            {
                return true;
            }
            
            return ball.name != collision.collider.attachedRigidbody.name;
        }
        
        //*********************************************************************
        //DEFENCE
        
        [Task]
        bool IsGoalie()
        {
            
            GameObject closestCar = findClosestCar(friends, own_goal);
            goalie = closestCar;
            return goalie.name == name;
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
            return maxDistanceToGoalGoalie > distance_to_goal && minDistanceToGoalGoalie < distance_to_goal;
        }

        [Task]
        void TakeDefenciveStance()
        {
            _optimalDefPos = calculateOptimalDefPos();
            goToDefencePos();
        }

        private Vector3 calculateOptimalDefPos()
        {
            Vector3 goal_pos = own_goal.transform.position;
            Vector3 final_attack_vector = attackVector();
            return final_attack_vector + goal_pos;
        }

        private Vector3 attackVector()
        {
            Vector3 ball_pos = ball.transform.position;
            Vector3 goal_pos = own_goal.transform.position;
            Vector3 attack_vector = ball_pos - goal_pos;
            Vector3 unit_attack_vector = attack_vector / attack_vector.magnitude;
            Vector3 final_attack_vector = unit_attack_vector * defRadius;
            return final_attack_vector;
        }

        [Task]
        bool ballWithinGoalArea()
        {
            float distanceBetweenGoalAndBall = (own_goal.transform.position - ball.transform.position).magnitude;
            return distanceBetweenGoalAndBall < 2 * defRadius;
        }

        private void goToDefencePos()
        {
            Move move = navigator.moveToPosition(_optimalDefPos, ball.transform.position);
            Debug.DrawLine(_optimalDefPos, transform.position);
            float distanceToGoalPos = (transform.position - _optimalDefPos).magnitude;
            float goalArea = 50f;
            float scaleFactor = 0.8f;
            if (distanceToGoalPos < 2f)
            {
                move.throttle = 0f;
                move.footBrake = 0f;
                move.handBrake = 1f;
            }
            if (distanceToGoalPos < goalArea)
            {
                move.throttle *= scaleFactor;
                move.footBrake *= scaleFactor;
            }

            m_Car.Move(move.steeringAngle, move.throttle, move.footBrake, move.handBrake);
        }
        
        //***************************************************************************
        //CHASER
        
        [Task]
        bool ClosestToBall()
        {
            List<GameObject> potentialChasers = new List<GameObject>();
            foreach(GameObject teamMate in friends)
            {
                if(teamMate.name != goalie.name)
                    potentialChasers.Add(teamMate);
            }
            GameObject closestPlayer = findClosestCar(potentialChasers.ToArray(), ball);
            chaser = closestPlayer;
            potentialChasers.Remove(closestPlayer);
            destroyer = potentialChasers[0];
            return chaser.name == gameObject.name;
        }
        
        private GameObject findClosestCar(GameObject[] players, GameObject something)
        {
            GameObject closestPlayer = gameObject;
            float shortestDistance = float.PositiveInfinity;
            foreach (var teamMate in players)
            {
                float distanceToBall = distanceToObject(teamMate, something);
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
        
        private float distanceToObject(GameObject teamMate, GameObject something)
        {
            return (teamMate.transform.position - something.transform.position).magnitude;
        }

        [Task]
        bool BehindBall()
        {
            SetOptimalKickPosition();
            float distanceToKickPos = (transform.position - optimalKickPos).magnitude;
            return distanceToKickPos < kickRadius;
        }

        [Task]
        void KickBall()
        {
            Move move = navigator.moveToPosition(ball.transform.position, ball.transform.position);
            Debug.DrawLine(ball.transform.position, transform.position);
            m_Car.Move(move.steeringAngle, move.throttle, move.footBrake, move.handBrake);
        }
        
        [Task]
        void InterceptBall()
        {
            navigator.avoidBall = true;
            Move move = navigator.moveToPosition(optimalKickPos, ball.transform.position);
            Debug.DrawLine(optimalKickPos, transform.position);
            m_Car.Move(move.steeringAngle, move.throttle, move.footBrake, move.handBrake);
            navigator.avoidBall = false;
        }
        
        private void SetOptimalKickPosition()
        {
            Vector3 ball_pos = ball.transform.position;
            Vector3 goal_pos = other_goal.transform.position;
            Vector3 attack_vector = ball_pos - goal_pos;
            if (ballOnOurSideOfTheField())
            {
                ball_pos = ball.transform.position;
                goal_pos = own_goal.transform.position;
                attack_vector = goal_pos - ball_pos;
            }

            Vector3 unit_attack_vector = attack_vector / attack_vector.magnitude;
            Vector3 final_attack_vector = unit_attack_vector * kickDistance;
            optimalKickPos = final_attack_vector + ball_pos;
            
        }

        private bool ballOnOurSideOfTheField()
        {
            return distanceToObject(other_goal, ball) > distanceToObject(own_goal, ball);
        }
        
        //***************************************************************************
        //DESTROYER
        
        [Task]
        void RamEnemyGoalie()
        {
            GameObject goalie = enemies[0];
            float shortestDistance = float.PositiveInfinity;
            foreach (var enemy in enemies)
            {
                float distanceToGoal = distanceToEnemyGoal(enemy);
                if (shortestDistance > distanceToGoal)
                {
                    goalie = enemy;
                    shortestDistance = distanceToGoal;
                }
            }

            Move move = navigator.moveToPosition(goalie.transform.position, ball.transform.position);
            Debug.DrawLine(goalie.transform.position, transform.position);
            m_Car.Move(move.steeringAngle, move.throttle, move.footBrake, move.handBrake);
        }
        
        private float distanceToEnemyGoal(GameObject enemy)
        {
            return (other_goal.transform.position - enemy.transform.position).magnitude;
        }
    }
}
