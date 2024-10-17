using System.Threading;
using nanoFramework.Device.Bluetooth.Advertisement;
using NFAppAtomLite_Testing;

namespace S3Movement
{




    public class Car
    {
        private M5AtomicMotion _motor;


        public Car(M5AtomicMotion motor)
        {
            _motor = motor;
        }

        public void forward()
        {
            _motor.SetServoAngle(0, 0);
            _motor.SetServoAngle(2, 180);

            return;
        }

        public void stop()
        {
            _motor.SetServoAngle(0, 90);
            _motor.SetServoAngle(2, 90);

            return;
        }

        public void turnSquareRight()
        {
            _motor.SetServoAngle(2, 135);
            Thread.Sleep(1600);
            _motor.SetServoAngle(2, 90);
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
            _motor.SetServoAngle(2, 135);
            Thread.Sleep(turnTime);
            _motor.SetServoAngle(2, 90);
        }

        public void turnLeft(int angle)
        {
            int turnTime = (angle * 1520) / 90;
            _motor.SetServoAngle(0, 45);
            Thread.Sleep(turnTime);
            _motor.SetServoAngle(0,90);
        }




    }
}