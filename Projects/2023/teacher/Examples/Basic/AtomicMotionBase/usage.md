
<h4>Create a driver class for Atomic Motion Base station</h4>

`var servo = M5AtomicMotion.Create(busId:2);`
<br></br>

<h4>Example of the controling Tilt servo SG90 connected on the servoChannel=1</h4> 
Note, that the each pressed button the Tilt is moved for 5 degrees in the range of 0 ~ 180 and then it is back to the 0. 
<br></br>

    private static void Button_Press(object sender, EventArgs e)
    {
      Debug.WriteLine($"\r\nButton has been pressed!");

      if(servo != null)
      {
          var angle = servo.GetServoAngle(1);
          Debug.WriteLine($"ServoAngle motion = {angle}");
          if (angle > 180)
              servo.SetServoAngle(1, 0);
          else
              servo.SetServoAngle(1, (byte)(angle + 5));
      }
    }
`
