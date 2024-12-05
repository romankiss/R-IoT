using System;
using System.Diagnostics;


namespace LoRa
{
    public static class ConvertExtencion
    {

        public static string Reverse(this string zdroj)
        {
            var o = string.Empty;
            for (int i = zdroj.Length - 1; i >= 0; i--)
                o += zdroj[i];

            return o;
        }

        public static string ConvertBits(this string number)
        {
            var num = Convert.ToInt32(number);
            var databits = string.Empty;

            while (num > 0)
            {
                if (num % 2 == 0)
                {
                    databits += "0";
                }
                else
                {
                    databits += "1";
                    --num;
                }
                num = num / 2;
            }

            return databits.Reverse();
        }
        public static int ConvertBitsToInt(this string number)
        {
            char[] bytes = number.Reverse().ToCharArray();

            double num = 0;


            for (int x = bytes.Length - 1; x >= 0; --x)
            {
                num += Math.Pow(((Int32.Parse(bytes[x].ToString())) * 2), x);
            }


            return (int)num;
        }
        public static int ConvertBitsToInt(this char[] bytes)
        {

            double num = 0;

            for (int x = bytes.Length - 1; x >= 0; --x)
            {
                num += Math.Pow(((Int32.Parse(bytes[x].ToString())) * 2), bytes.Length-1 - x);
            }


            return (int)num;
        }

    }
}
