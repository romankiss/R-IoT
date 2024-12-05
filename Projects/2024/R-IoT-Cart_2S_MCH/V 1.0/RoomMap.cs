using System;
using Iot.Device.Vl53L0X;
using S3Movement;

namespace S3
{
    public class RoomMap
    {
        private Vl53L0X _sensor;
        private Car _car;
        public int[] _roomMap = new int[360];
        
        public RoomMap(Vl53L0X sensor, Car car)
        {
            _sensor = sensor;
            _car = car;
        }

        public void map()
        {
            int position = 0; 
            
            while (position != 359){
                _car.turnRight(1);
                _roomMap[position] = _sensor.DistanceSingleMeasurement;
                position++;
            }

        }

        public int getFurthestValueIndex()
        {
            int max = 0;
            int maxIndex = 0;
            for (int i = 0; i < _roomMap.Length; i++)
            {
                if (_roomMap[i] > max)
                {
                    max = _roomMap[i];
                    maxIndex = i;

                ;
                }
            }

            return maxIndex;
        }

        public int getFurthestValueDistance()
        {
            int max = 0;
            int maxIndex = 0;
            for (int i = 0; i < _roomMap.Length; i++)
            {
                if (_roomMap[i] > max)
                {
                    max = _roomMap[i];
                    maxIndex = i;

                    ;
                }
            }

            return max;
        }

        public int[] getMap()
        {
            return _roomMap;
        }




    }
}