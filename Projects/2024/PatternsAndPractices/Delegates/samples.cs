// Mastering Delegates and Events In C# .NetFramework
// https://www.c-sharpcorner.com/UploadFile/84c85b/delegates-and-events-C-Sharp-net/
//
// Events

// declaration of delegate
public delegate void OnTouchEvent(object source, OnTouchEventArgs e);

// interesting event
public class OnTouchEventArgs : EventArgs
{
    public ushort X { get; set; }
    public ushort Y { get; set; } 
    // ... add more based on the needs
}

// my driver
public class MyDriver 
{
    public event OnTouchEvent OnChangeValue = null;

    public MyDriver() 
    {
      //...
      var task = new Thread(() =>
      {
        while(true) 
        {
          // polling my event, compare old and new values, store new valuse, set flag IsThereChange
          ...
          if(OnChangeValue != null && IsThereChange)
          {
            var args = new OnTouchEventArgs() {X = newX, Y=newY};
            OnChangeValue.Invoke(this, args);
          }
          Thread.Sleep(1000);
        }
            
      })
      { Priority = ThreadPriority.BelowNormal };
      task.Start();      
    }

}



// usage of events in the application
MyDriver mydriver = new MyDriver();
if(mydriver != null)
{
  mydriver.OnChangeValue += (sender, e) =>
  {
    // to do
  }

  
}




