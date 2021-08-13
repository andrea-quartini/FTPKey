using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTPKey
{
    #region Public Enums
    /// <summary>
    /// Protocol used for connection
    /// </summary>
    public enum ConnectionProtocol
    {
        /// <summary>
        /// By default, the protocol is Ftp
        /// </summary>
        Default = 0,

        /// <summary>
        /// Ftp protocol
        /// </summary>
        Ftp = 1,

        /// <summary>
        /// Ftp with SSL
        /// </summary>
        Ftps = 2,

        /// <summary>
        ///  Sftp protocol
        /// </summary>
        Sftp = 3
    }

    /// <summary>
    /// Encryption method
    /// </summary>
    public enum EncryptionType
    {
        /// <summary>
        /// No encryption required
        /// </summary>
        None = 0,

        /// <summary>
        /// Implicit encryption (SSL)
        /// </summary>
        Implicit = 1,

        /// <summary>
        /// Explicit encryption (TLS)
        /// </summary>
        Explicit = 2
    }
    #endregion

    /// <summary>
    /// Provides the ftp client
    /// </summary>
    public static class ClientProvider
    {
        #region Methods
        /// <summary>
        /// Create a new instance of the client and connects to the Ftp
        /// </summary>
        /// <param name="host">Ftp Url</param>
        /// <param name="port">Ftp Port</param>
        /// <param name="userName">The authentication's user name</param>
        /// <param name="password">User password</param>
        /// <param name="connect">If true, the connection will start immediately without the need to call Connect method</param>
        public static IFtpClient GetFtpClient(string host, int port, string userName, string password, bool connect)
        {
            return GetFtpClient(host, port, userName, password, connect, string.Empty);
        }

        /// <summary>
        /// Create a new instance of the client and connects to the Ftp
        /// </summary>
        /// <param name="host">Ftp Url</param>
        /// <param name="port">Ftp Port</param>
        /// <param name="userName">The authentication's user name</param>
        /// <param name="password">User password</param>
        /// <param name="connect">If true, the connection will start immediately without the need to call Connect method</param>
        /// <param name="remoteFolder">The folder to whitch connect; if empty, the current folder will be the root path</param>
        public static IFtpClient GetFtpClient(string host, int port, string userName, string password, bool connect, string remoteFolder)
        {
            return GetFtpClient(host, port, userName, password, connect, remoteFolder, ConnectionProtocol.Default);
        }

        /// <summary>
        /// Create a new instance of the client and connects to the Ftp
        /// </summary>
        /// <param name="host">Ftp Url</param>
        /// <param name="port">Ftp Port</param>
        /// <param name="userName">The authentication's user name</param>
        /// <param name="password">User password</param>
        /// <param name="connect">If true, the connection will start immediately without the need to call Connect method</param>
        /// <param name="remoteFolder">The folder to whitch connect; if empty, the current folder will be the root path</param>
        /// <param name="protocol">The required protocol to establish connection</param>
        public static IFtpClient GetFtpClient(string host, int port, string userName, string password, bool connect, string remoteFolder, ConnectionProtocol protocol)
        {
            return GetFtpClient(host, port, userName, password, connect, remoteFolder, protocol, EncryptionType.None);
        }

        /// <summary>
        /// Create a new instance of the client and connects to the Ftp
        /// </summary>
        /// <param name="host">Ftp Url</param>
        /// <param name="port">Ftp Port</param>
        /// <param name="userName">The authentication's user name</param>
        /// <param name="password">User password</param>
        /// <param name="connect">If true, the connection will start immediately without the need to call Connect method</param>
        /// <param name="remoteFolder">The folder to whitch connect; if empty, the current folder will be the root path</param>
        /// <param name="protocol">The required protocol to establish connection</param>
        /// <param name="encryptionType">Specify what type of encryption is needed; Explicit equals to TLS, Implicit equals to SSL</param>
        public static IFtpClient GetFtpClient(string host, int port, string userName, string password, bool connect, string remoteFolder, ConnectionProtocol protocol, EncryptionType encryptionType)
        {
            return GetFtpClient(host, port, userName, password, connect, remoteFolder, protocol, encryptionType, string.Empty);
        }

        /// <summary>
        /// Create a new instance of the client and connects to the Ftp
        /// </summary>
        /// <param name="host">Ftp Url</param>
        /// <param name="port">Ftp Port</param>
        /// <param name="userName">The authentication's user name</param>
        /// <param name="password">User password</param>
        /// <param name="connect">If true, the connection will start immediately without the need to call Connect method</param>
        /// <param name="remoteFolder">The folder to whitch connect; if empty, the current folder will be the root path</param>
        /// <param name="protocol">The required protocol to establish connection</param>
        /// <param name="encryptionType">Specify what type of encryption is needed; Explicit equals to TLS, Implicit equals to SSL</param>
        /// <param name="fingerPrint">Fingerprint to validate connection (for SFTP only)</param>
        public static IFtpClient GetFtpClient(string host, int port, string userName, string password, bool connect, string remoteFolder, ConnectionProtocol protocol, EncryptionType encryptionType, string fingerPrint)
        {
            return new BaseClient.Client(host, port, userName, password, connect, remoteFolder, protocol, encryptionType, fingerPrint);
        }
        #endregion
    }
}
