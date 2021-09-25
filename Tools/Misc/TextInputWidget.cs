using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eitrix
{
    /// --------------------------------------------------------------
    /// <summary>
    /// A class for holding onto text that can be edited one character
    /// at a time by a controller
    /// </summary>
    /// --------------------------------------------------------------
    public class TextInputWidget
    {
        int maxSize = 20;
        char[] letters = new char[20];
 
        public int MaxSize 
        {
            get 
            { 
                return maxSize;
            }
        }

        public string MyValue
        {
            get
            {
                StringBuilder outString = new StringBuilder(maxSize);
                for (int i = 0; i < maxSize && letters[i] != 0; i++)
                {
                    outString.Append(letters[i]);   
                }
                return outString.ToString();
            }
            set
            {
                int i = 0;
                for (; i < maxSize && i < value.Length; i++)
                {
                    letters[i] = value[i];
                }
                if (i < maxSize) letters[i] = (char)0;
            }
        }


        static public implicit operator string(TextInputWidget widget)
        {
            return widget.MyValue;
        }
    }
}
