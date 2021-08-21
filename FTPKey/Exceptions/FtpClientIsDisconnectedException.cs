using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTPKey.Exceptions
{
    /// <summary>
    /// Thrown when attempting any operation while the client is disconnected
    /// </summary>
    public class FtpClientIsDisconnectedException : Exception
    {
        #region Constructor
        public FtpClientIsDisconnectedException()
            :base()
        {
        }

        public FtpClientIsDisconnectedException(string message)
            :base(message)
        {
        }

        public FtpClientIsDisconnectedException(string message, Exception innerException)
            :base(message, innerException)
        {
        }
        #endregion
    }
}
