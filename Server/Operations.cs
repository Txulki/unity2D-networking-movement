using System;
using System.Collections;
using System.Collections.Generic;
using static vector_library.Operations;
//using UnityEngine;


namespace vector_library
{

    /*
        Some functions to make operating with float[,] faster.
        Perhaps I should just have used 2 different ints or something like that
        instead of float[,], but it is too late now and it works great.
    */
    public static class Operations
    {
        public static float[,] Substract(float[,] CompOne, float[,] CompTwo)
        {
            return new float[1, 2] { { CompTwo[0, 0] - CompOne[0, 0], CompTwo[0, 1] - CompOne[0, 1] } };
        }

        public static float[,] Sum(float[,] CompOne, float[,] CompTwo)
        {
            return new float[1, 2] { { CompTwo[0, 0] + CompOne[0, 0], CompTwo[0, 1] + CompOne[0, 1] } };
        }

        public static float[,] Divide(float[,] CompOne, float divisor)
        {
            return new float[1, 2] { { CompOne[0, 0] / divisor, CompOne[0, 1] / divisor } };
        }

        public static bool Equal(float[,] CompOne, float[,] CompTwo)
        {
            if(CompOne[0,0] == CompTwo[0,0] && CompOne[0,1] == CompTwo[0,1])
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static float Module(float[,] Comp)
        {
            return Convert.ToSingle(Math.Sqrt(Math.Pow(Comp[0, 0], 2) + Math.Pow(Comp[0, 1], 2)));
        }
    }
}
