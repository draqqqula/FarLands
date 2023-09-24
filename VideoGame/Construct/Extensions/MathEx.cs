using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace VideoGame
{
    internal static class MathEx
    {
        public static float Lerp(float a, float b, float k)
        {
            return a + k * (b - a);
        }

        public static float Lerp(float a, float b, float k, double dt)
        {
            return Lerp(a, b, (float)(1 - Math.Pow(1-k, dt*60)));
        }

        public static float Lerp(float a, float b, float k, TimeSpan dt)
        {
            return Lerp(a, b, k, dt.TotalSeconds);
        }

        public static float Catch(float a, float b, float c)
        {
            if (Math.Abs(a - b) < c)
                return b;
            else
                return a;
        }

        public static float NormalizeAngle(this float num, float border)
        {
            bool isNegative = false;

            if (num < 0)
            {
                num = Math.Abs(num);
                isNegative = true;
            }

            int excessCount = (int)Math.Floor(num / border);

            num -= excessCount * border;

            if (isNegative) num = border - num;

            return num;
        }

        public static float GetShortestDifferenceTo(this float angle, float targetAngle, float border)
        {
            angle = (angle % border + border) % border;
            targetAngle = (targetAngle % border + border) % border;

            float absoluteDifference = Math.Abs(targetAngle - angle);

            if (absoluteDifference > border / 2)
            {
                if (angle < targetAngle)
                {
                    angle += border;
                }
                else
                {
                    targetAngle += border;
                }
            }

            return targetAngle - angle;
        }

        public static int GetDifferenceSign(this float angle, float targetAngle, float border)
        {
            float shortestDifference = angle.GetShortestDifferenceTo(targetAngle, border);

            return Math.Sign(shortestDifference);
        }

        public static float RotateTowards(this float angle, float targetAngle, float border, float speed)
        {
            float shortestDifference = angle.GetShortestDifferenceTo(targetAngle, border);

            // Calculate the step to rotate by, based on the speed
            float step = speed;

            // Ensure that step is within the absolute value of the shortest difference
            step = Math.Min(Math.Abs(shortestDifference), step);

            // Determine the direction of rotation
            int rotationDirection = Math.Sign(shortestDifference);

            // Calculate the new angle after rotation
            float newAngle = angle + step * rotationDirection;

            // Ensure that the new angle stays within the border
            newAngle = (newAngle % border + border) % border;

            return newAngle;
        }

        public static float RotateTowards(this float angle, float targetAngle, float border, float speed, double deltatime)
        {
            return angle.RotateTowards(targetAngle, border, speed * (float)deltatime);
        }

        public static float RotateTowards(this float angle, float targetAngle, float border, float speed, TimeSpan deltatime)
        {
            return angle.RotateTowards(targetAngle, border, speed, deltatime.TotalSeconds);
        }
    }
}
