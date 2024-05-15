# M5stack Atom Motion
## Simple servo control
### Hardware
* [Atom S3](https://docs.m5stack.com/en/core/AtomS3%20Lite)
* [Atom Motion Base](https://shop.m5stack.com/products/atomic-motion-base-stm32f030)
* [Catch Unit](https://shop.m5stack.com/products/catch-unit)

### Goal
Demo project of the Atom Motion Base with a servo catch unit closing itsef when the buildin button of the AtomS3 lite is pressed. When it is fully closed it opens itsef again.

### Ilustrations
![Atom Motion + Catcher unit + Atom S3lite](https://github.com/romankiss/R-IoT/assets/59760649/a9dba6e8-8435-4bfc-82e9-2f52974800a0)    
![VID20240515174642](https://github.com/romankiss/R-IoT/assets/59760649/cdc9e7df-c951-4ec0-84e2-30c65533bcf8)



### Software
[main program](https://github.com/romankiss/R-IoT/blob/main/Projects/2023/R-IoT-Fotopasca/Solutions/Basic/Code/Atom%20Motion%20Kit/program.cs)     
[Atom Motion Base lib](https://github.com/romankiss/R-IoT/blob/main/Projects/2023/teacher/Examples/Basic/AtomicMotionBase/M5AtomicMotion.cs)     
[I2C lib required for atom motion base](https://github.com/romankiss/R-IoT/blob/main/Projects/2023/teacher/Examples/Basic/AtomicMotionBase/I2cDeviceBase.cs)     
