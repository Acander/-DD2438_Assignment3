using System.Diagnostics;
using System.Runtime.Remoting.Channels;
using Panda.Examples.Move;
using UnityEditor;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;
using XNode.Examples.MathNodes;

namespace Scrips
{
    public class Navigator
    {
        //Goalie stays at the goal for the entire game and does turn around 180 degrees, simply reverses and reverses controls.
        private GameObject car;
        private bool reversing;
        public bool crash;
        //private float goalRadius = 2f;
        public Stopwatch stopwatch = new Stopwatch();

        public Navigator(GameObject car)
        {
            this.car = car;
        }

        public Move moveToPosition(Vector3 guard_pos)
        {
            if (crash)
            {    
                /*Vector3 current_pos = car.transform.position;
                Vector3 dir = car.transform.forward;
                check_Should_Reverse(current_pos, guard_pos, dir);*/
                if (stopwatch.ElapsedMilliseconds < 2000)
                {
                    Move move = followGoal(guard_pos);
                    move.throttle *= -1;
                    move.footBrake *= -1;
                    return move;
                }
                crash = false;
                stopwatch.Stop();
                stopwatch.Reset();
            }

            return followGoal(guard_pos);

        }

        private Move followGoal(Vector3 guard_pos)
        {
            Vector3 current_pos = car.transform.position;
            Vector3 dir = car.transform.forward;
            Vector3 right = car.transform.right;
            check_Should_Reverse(current_pos, guard_pos, dir);
            float steer = steer_dir(current_pos, right, guard_pos);
            float accel = acceleration();
            float handbrake = 0;
            /*if ((guard_pos - current_pos).magnitude < goalRadius)
            {
                steer = 0;
                accel = 0;
                handbrake = 1;
            }*/
            steer = variateSteering(dir, guard_pos, current_pos, steer);
            return new Move(steer, accel, accel, handbrake);
        }

        private float variateSteering(Vector3 dir, Vector3 guard_pos, Vector3 current_pos, float steer)
        {
            Vector3 between = guard_pos - current_pos;
            if (reversing)
            {
                dir *= -1;
            }
            float angle = Vector3.Angle(dir, between);
            
            angle *= angle > 0f ? 1f : -1f;
            float maxAngle = 90f;
            float turningForce = angle / maxAngle;
            steer *= turningForce;
            return steer;

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

        private Move crashRoutine()
        {
            if (reversing)
            {
                return new Move(0, 1, 0, 0);
            }
            
            return new Move(0, 0, -1, 0);
        }

        public void setToCrash()
        {
            stopwatch.Reset();
            stopwatch.Start();
            crash = true;
        }
        
        
        
        
    }
}