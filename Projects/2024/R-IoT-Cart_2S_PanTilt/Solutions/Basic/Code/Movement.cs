using System;
using System.Threading;

namespace ESP_Car
{
    public class Movement
    {
        private M5AtomicMotion _motor;

        //private M5AtomicMotion _motorSpeed;

        public Movement(M5AtomicMotion motor)
        {
            _motor = motor;
        }

        public void forward()
        {
            _motor.SetServoAngle(0, 0);
            _motor.SetServoAngle(1, 180);

            return;
        }

        public void stop()
        {
            _motor.SetServoAngle(0, 90);
            _motor.SetServoAngle(1, 90);

            return;
        }

        public void turnSquareRight()
        {
            _motor.SetServoAngle(1, 135);
            Thread.Sleep(1600);
            _motor.SetServoAngle(1, 90);
        }

        public void turnSquareLeft()
        {
            _motor.SetServoAngle(0, 45);
            Thread.Sleep(1600);
            _motor.SetServoAngle(0, 90);
        }

        public void turnAround()
        {
            _motor.SetServoAngle(0, 45);
            Thread.Sleep(6080);
            _motor.SetServoAngle(0, 90);
        }


        public void turnRight(int angle)
        {
            int turnTime = (angle * 1520) / 90;
            _motor.SetServoAngle(1, 135);
            Thread.Sleep(turnTime);
            _motor.SetServoAngle(1, 90);
        }

        public void turnLeft(int angle)
        {
            int turnTime = (angle * 1520) / 90;
            _motor.SetServoAngle(0, 45);
            Thread.Sleep(turnTime);
            _motor.SetServoAngle(0, 90);
        }
    }
}
