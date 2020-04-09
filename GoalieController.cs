using Panda.Examples.Move;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;
using XNode.Examples.MathNodes;

namespace Scrips
{
    public class GoalieController
    {
        //Goalie stays at the goal for the entire game and does turn around 180 degrees, simply reverses and reverses controls.
        private GameObject goalie;
        private bool reversing;

        public GoalieController(GameObject goalie)
        {
            this.goalie = goalie;
        }

        public Move moveToPosition(Vector3 guard_pos)
        {
            CarController m_Car = goalie.GetComponent<CarController>();
            Vector3 current_pos = goalie.transform.position;
            Vector3 dir = goalie.transform.forward;
            Vector3 right = goalie.transform.forward;
            check_Should_Reverse(current_pos, guard_pos, dir);
            float steer = steer_dir(current_pos, right, guard_pos);
            float accel = acceleration();
            return new Move(steer, accel, accel, 0);
        }
        
        private void check_Should_Reverse(Vector3 currentPos, Vector3 goalPos, Vector3 carHeading)
        {
            float reverseScore = Vector3.Dot(goalPos-currentPos, carHeading);
            reversing = reverseScore < 0;
            
            /*Debug.DrawLine(currentPos, goalPos, Color.white);
            Debug.LogFormat("Reverse score: {0}", reverseScore);
            Debug.LogFormat("Carheading: {0} ", carHeading);*/
        }
        
        private float steer_dir(Vector3 pos, Vector3 right, Vector3 end_pos)
        {
            var dir = end_pos - pos;
            float dot = Vector3.Dot(right, dir);
            
            if (reversing)
            {
                return dot > 0f ? 1f : -1f;
            }

            return dot > 0f ? -1f : 1f;
        }

        private float acceleration()
        {
            if (reversing)
            {
                return -1;
            }

            return 1;
        }
        
        
        
        
    }
}