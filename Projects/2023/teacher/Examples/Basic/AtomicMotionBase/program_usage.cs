#region Servo Atomic Motion Base           
servo = M5AtomicMotion.Create(busId:2);
Debug.WriteLine($"Servo busId = {(servo == null ? 0 : servo.I2cDevice.ConnectionSettings.BusId)}");
#endregion
