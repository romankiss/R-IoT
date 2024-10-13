using System;
using ESP32_S3;
using NFAppAtomLite_Testing;

namespace ESP32_S33
{
    public class RotationHandler
    {
        private readonly M5AtomicMotion _motionController;
        private readonly Encoder _encoder;
        private int _previousEncoderValue;

        public RotationHandler(M5AtomicMotion motionController, Encoder encoder)
        {
            _motionController = motionController;
            _encoder = encoder;
            _previousEncoderValue = 0; 

        }

        private void EncoderValueChanged(object sender, int newValue, bool buttonStatus)
        {
            int difference = newValue - _previousEncoderValue;

            if (Math.Abs(difference) >= 10)  
            {
                RotateCart(difference);
                _previousEncoderValue = newValue;
            }
        }

        
        private void RotateCart(int change)
        {
            
            if (change > 0)
            {
                // Rotate right
                _motionController.SetServoAngle(0, M5AtomicMotion.ServoAngleAhead);
                _motionController.SetServoAngle(2, M5AtomicMotion.ServoAngleBack);
            }
            else if (change < 0)
            {
                // Rotate left
                _motionController.SetServoAngle(0, M5AtomicMotion.ServoAngleBack);
                _motionController.SetServoAngle(2, M5AtomicMotion.ServoAngleAhead);
            }

            System.Threading.Thread.Sleep(500);
            _motionController.SetServoAngle(0, M5AtomicMotion.ServoAngleStop);
            _motionController.SetServoAngle(2, M5AtomicMotion.ServoAngleStop);
        }
    }
}
