using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace YanLib.YanException
{
    /// <summary>
    /// 事件ID错误
    /// </summary>
    [Serializable]
    [ComVisible(true)]
    public class InvalidEventIDException : InvalidCastException
    {
        /// <summary>
        /// 
        /// </summary>
        public InvalidEventIDException()
            : base("事件 ID 不符合要求")
        {
            HResult = -2147467260;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public InvalidEventIDException(string message)
            : base(message)
        {
            HResult = -2147467260;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public InvalidEventIDException(string message, System.Exception innerException)
            : base(message, innerException)
        {
            HResult = -2147467260;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected InvalidEventIDException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}