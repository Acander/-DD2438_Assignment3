namespace Scrips
{
    public class Move
    {
        public float steeringAngle;
        public float throttle;
        public float footBrake;
        public float handBrake;

        public Move(float steeringAngle, float throttle, float footBrake, float handBrake)
        {
            this.steeringAngle = steeringAngle;
            this.throttle = throttle;
            this.footBrake = footBrake;
            this.handBrake = handBrake;
        }

        
    }
}