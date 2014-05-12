using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fizzi.Applications.ChallongeVisualization.Common
{
    public static class ExceptionEx
    {
        public static string[] TraverseMessages(this Exception ex)
        {
            var errorMsgList = new List<string>();
            Exception currentEx = ex;

            while (currentEx != null)
            {
                errorMsgList.Add(currentEx.Message);

                currentEx = currentEx.InnerException;
            }

            return errorMsgList.ToArray();
        }

        public static string NewLineDelimitedMessages(this Exception ex)
        {
            return TraverseMessages(ex).Aggregate((one, two) => one + "\n" + two);
        }
    }
}
