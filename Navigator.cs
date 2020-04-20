using System;
using System.Diagnostics;
using System.Runtime.Remoting.Channels;
using System.Text;
using Panda.Examples.Move;
using UnityEditor;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;
using XNode.Examples.MathNodes;
using Debug = System.Diagnostics.Debug;

namespace Scrips
{
    public class Navigator
    {
        //Goalie stays at the goal for the entire game and does turn around 180 degrees, simply reverses and reverses controls.
        private GameObject car;
        public bool reverseMode;

        public bool crash;
        public Stopwatch stopwatch = new Stopwatch();
        
        public bool avoidBall;
        private float avoidBallAngle = 30f;

        public String getState()
        {
            StringBuilder state = new StringBuilder();
            state.Append("Navigator state -> ");
            state.AppendFormat("Crashing: {0}, StopWatch: {1}, ", crash, stopwatch.ElapsedMilliseconds);
            state.AppendFormat("AvoidBall: {0}, ", avoidBall);
            state.AppendFormat("Reversing: {0}, ", reverseMode);
            return state.ToString();
        }
        
        public Navigator(GameObject car)
        {
            this.car = car;
        }

        public Move moveToPosition(Vector3 guard_pos, Vector3 ballPos)
        {
            if (crash)
            {
                if (stopwatch.ElapsedMilliseconds < 2000)
                {
                    return crashRoutine();
                }
                crash = false;
                stopwatch.Stop();
                stopwatch.Reset();
            }
            
            Move move = followGoal(guard_pos);

            if (avoidBall)
            {
                move = checkAvoidBall(move, ballPos, car.transform.position, car.transform.forward);
            }

            return move;

        }

        private Move followGoal(Vector3 guard_pos)
        {
            Vector3 current_pos = car.transform.position;
            Vector3 dir = car.transform.forward;
            Vector3 right = car.transform.right;
            float magnitude = (current_pos - guard_pos).magnitude;
            check_Should_Reverse(current_pos, guard_pos, dir);
            float steer = steer_dir(current_pos, right, guard_pos);
            float accel = acceleration();
            float handbrake = 0;
            steer = variateSteering(dir, guard_pos, current_pos, steer);
            return new Move(steer, accel, accel, handbrake);
        }
        
        private Move checkAvoidBall(Move move, Vector3 ballPos, Vector3 myPos, Vector3 forward)
        {
            Vector3 between = ballPos - myPos;
            float angle = Vector3.Angle(forward, between);
            float steer = 0.4f;
            if (-avoidBallAngle < angle && angle < 0f)
            {
                //steer *= -1; //If should turn left
                move.steeringAngle = steer;
            }
            else if (avoidBallAngle > angle && angle >= 0f)
            {
                steer *= -1; //If should turn left
                move.steeringAngle = steer;
            }
            return move;
        }
        
        private float variateSteering(Vector3 dir, Vector3 guard_pos, Vector3 current_pos, float steer)
        {
            Vector3 between = guard_pos - current_pos;
            if (reverseMode)
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
            reverseMode = reverseScore < 0;
        }
        
        private float steer_dir(Vector3 pos, Vector3 right, Vector3 end_pos)
        {
            var dir = end_pos - pos;
            float dot = Vector3.Dot(right, dir);
            
            /*if (reverseMode)
            {
                return dot > 0f ? 1f : -1f;
            }*/

            return dot > 0f ? 1f : -1f;
        }

        private float acceleration()
        {
            if (reverseMode)
            {
                return -1;
            }

            return 1;
        }

        private Move crashRoutine()
        {
            if (reverseMode)
            {
                return new Move(0, 1, 0, 0);
            }
            
            return new Move(0, 0, -1, 0);
        }

        public void setToCrash()
        {
            stopwatch.Stop();
            stopwatch.Reset();
            stopwatch.Start();
            crash = true;
        }
        
        
        
        
    }
}