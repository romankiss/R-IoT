


     #region Encoder
     Configuration.SetPinFunction(19, DeviceFunction.I2C1_DATA);     //AtomicMotionBase SD-G19, G33
     Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);    //AtomicMotionBase SC-G22, G23
     sensorEncoder = Encoder.Create(busId:1, pollingTimeInMilliseconds: 50 );
     if (sensorEncoder != null)
     {
         sensorEncoder.OnChangeValue += (sender, e) =>
         {
             Encoder encoder = sender as Encoder;
             if (e.OldValue != e.NewValue || e.ButtonStatus)
             {
                 Debug.WriteLine($"sensorEncoder: Val={e.NewValue}, button={e.ButtonStatus}");
                 if (e.NewValue < M5AtomicMotion.ServoAngleBack)
                     encoder.SetLEDColor(0, (byte)(e.NewValue), 0, 0);
                 else if (e.NewValue > M5AtomicMotion.ServoAngleAhead)
                     encoder.SetLEDColor(0, 0, 0, (byte)(e.NewValue));
                 else
                 {
                     encoder.SetLEDColor(0, 0, (byte)(e.NewValue), 0);
                     if (servo != null && e.ButtonStatus)
                     {
                         Debug.WriteLine($"ServoAngle motion = {e.NewValue}");
                         servo.SetServoAngle(3, (byte)(e.NewValue));
                         if (sensorToF != null)
                         {
                             Debug.WriteLine($"Distance: {sensorToF.Distance} mm");
                         }
                     }
                 }
             }
         };
     }
     #endregion
