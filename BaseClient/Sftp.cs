using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Renci.SshNet;

namespace FTPKey.BaseClient
{
    /// <summary>
    /// Gestione collegamento via SFTP
    /// </summary>
    internal class SftpClient : IFtpClient
    {
        #region Variables
        /// <summary>
        /// SSH Key for validation
        /// </summary>
        private string _fingerPrint;

        /// <summary>
        /// Sftp client
        /// </summary>
        private Renci.SshNet.SftpClient _client;
        #endregion

        #region Connection Methods
        /// <summary>
        /// Opens a connection with Ftp area
        /// </summary>
        public void Connect()
        {
            try
            {
                if (!this._client.IsConnected)
                    this._client.Connect();
            }
            catch (Renci.SshNet.Common.SshConnectionException ex)
            {
                throw new Exception(string.Format(Messages.Messages.ConnectionExceptionMessage, (ex.InnerException != null ? ex.InnerException.Message : ex.Message)), ex);
            }
            catch (Renci.SshNet.Common.SshAuthenticationException ex)
            {
                throw new Exception(Messages.Messages.AuthenticationExceptionMessage, ex);
            }
        }

        /// <summary>
        /// Closes the connection
        /// </summary>
        public void Disconnect()
        {
            if (this._client.IsConnected)
                this._client.Disconnect();
        }
        #endregion

        #region Transfer Methods
        /// <summary>
        /// Deletes the desired remote file
        /// </summary>
        /// <param name="remoteFileName">The file to delete</param>
        public void DeleteFile(string remoteFileName)
        {
            try
            {
                this._client.DeleteFile(remoteFileName);
            }
            catch (Renci.SshNet.Common.SshConnectionException ex)
            {
                throw new Exception(Messages.Messages.ConnectionExceptionMessage, ex);
            }
            catch (Renci.SshNet.Common.SftpPathNotFoundException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PathNotFoundExceptionMessage, Messages.Messages.OperationDelete, remoteFileName), ex);
            }
            catch (Renci.SshNet.Common.SftpPermissionDeniedException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PermissionDeniedExceptionMessage, Messages.Messages.OperationDelete), ex);
            }
        }

        /// <summary>
        /// Downloads a file from Ftp area
        /// </summary>
        /// <param name="remoteFileName">The name of file to download</param>
        /// <param name="destinationFile">The destination local path</param>
        /// <param name="deleteFileAfterDownload">If true, it deletes the remote file after downloading it</param>
        public bool DownloadFile(string remoteFileName, string destinationFile, bool deleteFileAfterDownload)
        {
            Stream outputStream = null;
            if (DownloadFile(remoteFileName, outputStream, deleteFileAfterDownload))
            {
                using (FileStream destinationFileStream = new FileStream(destinationFile, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    outputStream.CopyTo(destinationFileStream);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Downloads a file from Ftp area
        /// </summary>
        /// <param name="remoteFileName">The name of file to download</param>
        /// <param name="outStream">The output stream of downloaded file</param>
        /// <param name="deleteFileAfterDownload">If true, it deletes the remote file after downloading it</param>
        public bool DownloadFile(string remoteFileName, Stream outStream, bool deleteFileAfterDownload)
        {
            long remoteFileSize = 0;

            try
            {
                this._client.DownloadFile(remoteFileName, outStream);
                remoteFileSize = this._client.GetAttributes(remoteFileName).Size;
            }
            catch (Renci.SshNet.Common.SshConnectionException ex)
            {
                throw new Exception(Messages.Messages.ConnectionExceptionMessage, ex);
            }
            catch (Renci.SshNet.Common.SftpPathNotFoundException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PathNotFoundExceptionMessage, Messages.Messages.OperationDownload, remoteFileName), ex);
            }
            catch (Renci.SshNet.Common.SftpPermissionDeniedException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PermissionDeniedExceptionMessage, Messages.Messages.OperationDownload), ex);
            }

            if (outStream.Length > 0)
            {
                if (remoteFileSize != outStream.Length)
                {
                    throw new Exception(string.Format(Messages.Messages.OperationNotCompletedException, Messages.Messages.OperationDownload, remoteFileName));
                }

                if (deleteFileAfterDownload)
                {
                    _client.DeleteFile(remoteFileName);
                }
                return true;
            }
            else
                throw new FileNotFoundException(string.Format(Messages.Messages.OperationNotCompletedException, Messages.Messages.OperationDownload, remoteFileName));
        }


        /// <summary>
        /// Uploads a file to the Ftp area
        /// </summary>
        /// <param name="localFile">Full local file path</param>
        /// <param name="destinationFileName">Destination file name</param>
        /// <param name="deleteFileAfterUpload">If true, it deletes the local file after uploading it</param>
        public void UploadFile(string localFile, string destinationFileName, bool deleteFileAfterUpload)
        {
            using (FileStream localFileStream = new FileStream(localFile, FileMode.Open, FileAccess.Read))
            {
                UploadFile(localFileStream, destinationFileName);
            }
        }
        public void UploadFile(string localFile, bool deleteFileAfterUpload)
        {
            this.UploadFile(localFile, Path.GetFileName(localFile), deleteFileAfterUpload);
        }

        /// <summary>
        /// Uploads a file to the Ftp area
        /// </summary>
        /// <param name="localFileStream">The local file stream</param>
        /// <param name="destinationFileName">Destination file name</param>
        public void UploadFile(Stream localFileStream, string destinationFileName)
        {
            long remoteFileLength = 0;

            try
            {
                this._client.UploadFile(localFileStream, destinationFileName, true);

                if (this._client.Exists(destinationFileName))
                    remoteFileLength = this._client.GetAttributes(destinationFileName).Size;
                else
                    throw new FileNotFoundException(string.Format(Messages.Messages.OperationNotCompletedException, Messages.Messages.OperationUpload, destinationFileName));

                if (remoteFileLength != localFileStream.Length)
                {
                    this._client.DeleteFile(destinationFileName);
                    throw new Exception(string.Format(Messages.Messages.OperationNotCompletedException, Messages.Messages.OperationUpload, destinationFileName));
                }
            }
            catch (Renci.SshNet.Common.SshConnectionException ex)
            {
                throw new Exception(Messages.Messages.ConnectionExceptionMessage, ex);
            }
            catch (Renci.SshNet.Common.SftpPathNotFoundException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PathNotFoundExceptionMessage, Messages.Messages.OperationUpload, destinationFileName), ex);
            }
            catch (Renci.SshNet.Common.SftpPermissionDeniedException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PermissionDeniedExceptionMessage, Messages.Messages.OperationUpload), ex);
            }
        }

        /// <summary>
        /// Gets a list of filenames from the current remote folder
        /// </summary>
        public string[] GetFilesList()
        {
            return this.GetFilesList(this.GetCurrentFolder());
        }

        /// <summary>
        /// Gets a list of files from the desired path
        /// </summary>
        /// <param name="path">The path from witch retrieve the files list</param>
        public string[] GetFilesList(string path)
        {
            string[] files = null;
            List<Renci.SshNet.Sftp.SftpFile> sftpFiles = null;

            try
            {
                sftpFiles = this._client.ListDirectory(path).ToList();
            }
            catch (Renci.SshNet.Common.SshConnectionException ex)
            {
                throw new Exception(Messages.Messages.ConnectionExceptionMessage, ex);
            }
            catch (Renci.SshNet.Common.SftpPathNotFoundException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PathNotFoundExceptionMessage, Messages.Messages.OperationGetFilesList, this._client.WorkingDirectory), ex);
            }
            catch (Renci.SshNet.Common.SftpPermissionDeniedException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PermissionDeniedExceptionMessage, Messages.Messages.OperationGetFilesList), ex);
            }

            if (sftpFiles.Count > 0)
                files = (from file in sftpFiles where !file.IsDirectory select file.Name).ToArray();

            return files;
        }

        /// <summary>
        /// Gets a list of sub-folders from the current remote folder
        /// </summary>
        public string[] GetFoldersList()
        {
            return this.GetFoldersList(this.GetCurrentFolder());
        }

        /// <summary>
        /// Gets a list of sub-folders from the desired path
        /// </summary>
        /// <param name="path">The main path to search in for the sub-folders list</param>
        public string[] GetFoldersList(string path)
        {
            string[] folders = null;
            List<Renci.SshNet.Sftp.SftpFile> sftpFolders = null;

            try
            {
                sftpFolders = this._client.ListDirectory(path).ToList();
            }
            catch (Renci.SshNet.Common.SshConnectionException ex)
            {
                throw new Exception(Messages.Messages.ConnectionExceptionMessage, ex);
            }
            catch (Renci.SshNet.Common.SftpPathNotFoundException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PathNotFoundExceptionMessage, Messages.Messages.OperationGetFoldersList, this._client.WorkingDirectory), ex);
            }
            catch (Renci.SshNet.Common.SftpPermissionDeniedException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PermissionDeniedExceptionMessage, Messages.Messages.OperationGetFoldersList), ex);
            }

            if (sftpFolders.Count > 0)
                folders = (from folder in sftpFolders where folder.IsDirectory select folder.Name).ToArray();

            return folders;
        }

        /// <summary>
        /// Renames a remote file
        /// </summary>
        /// <param name="currentName">the current remote file's name</param>
        /// <param name="newName">The new name</param>
        public void RenameFile(string currentName, string newName)
        {
            try
            {
                this._client.RenameFile(currentName, newName);
            }
            catch (Renci.SshNet.Common.SshConnectionException ex)
            {
                throw new Exception(Messages.Messages.ConnectionExceptionMessage, ex);
            }
            catch (Renci.SshNet.Common.SftpPathNotFoundException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PathNotFoundExceptionMessage, Messages.Messages.OperationRenameFile, currentName), ex);
            }
            catch (Renci.SshNet.Common.SftpPermissionDeniedException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PermissionDeniedExceptionMessage, Messages.Messages.OperationRenameFile), ex);
            }
        }

        /// <summary>
        /// Creates a new remote folder
        /// </summary>
        /// <param name="path">The partial or full path to create</param>
        public void CreateFolder(string path)
        {
            try
            {
                this._client.CreateDirectory(path);
            }
            catch (Renci.SshNet.Common.SshConnectionException ex)
            {
                throw new Exception(Messages.Messages.ConnectionExceptionMessage, ex);
            }
            catch (Renci.SshNet.Common.SftpPathNotFoundException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PathNotFoundExceptionMessage, Messages.Messages.OperationCreateFolder, path), ex);
            }
            catch (Renci.SshNet.Common.SftpPermissionDeniedException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PermissionDeniedExceptionMessage, Messages.Messages.OperationCreateFolder), ex);
            }
        }

        /// <summary>
        /// Deletes a remote folder, not recursively
        /// </summary>
        public void DeleteFolder(string path)
        {
            try
            {
                this._client.DeleteDirectory(path);
            }
            catch (Renci.SshNet.Common.SshConnectionException ex)
            {
                throw new Exception(Messages.Messages.ConnectionExceptionMessage, ex);
            }
            catch (Renci.SshNet.Common.SftpPathNotFoundException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PathNotFoundExceptionMessage, Messages.Messages.OperationDeleteFolder, path), ex);
            }
            catch (Renci.SshNet.Common.SftpPermissionDeniedException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PermissionDeniedExceptionMessage, Messages.Messages.OperationDeleteFolder), ex);
            }
            catch (Renci.SshNet.Common.SshException ex)
            {
                throw new Exception(string.Format(Messages.Messages.DeleteFolderNotEmpty, path), ex);
            }
        }

        /// <summary>
        /// Deletes a remote folder
        /// </summary>
        /// <param name="path">The folder to delete</param>
        /// <param name="deleteRecursively">If true, a recursive deletion will be performed</param>
        public void DeleteFolder(string path, bool deleteRecursively)
        {
            if (deleteRecursively)
            {
                List<Renci.SshNet.Sftp.SftpFile> files = null;

                try
                {
                    files = this._client.ListDirectory(path).ToList();
                }
                catch (Renci.SshNet.Common.SshConnectionException ex)
                {
                    throw new Exception(Messages.Messages.ConnectionExceptionMessage, ex);
                }
                catch (Renci.SshNet.Common.SftpPathNotFoundException ex)
                {
                    throw new Exception(string.Format(Messages.Messages.PathNotFoundExceptionMessage, Messages.Messages.OperationGetFilesList, path), ex);
                }
                catch (Renci.SshNet.Common.SftpPermissionDeniedException ex)
                {
                    throw new Exception(string.Format(Messages.Messages.PermissionDeniedExceptionMessage, Messages.Messages.OperationGetFilesList), ex);
                }

                foreach (Renci.SshNet.Sftp.SftpFile directory in files.Where(x => x.IsDirectory))
                {
                    if (directory.Name != "." && directory.Name != "..")
                    {
                        this.DeleteFolder(directory.FullName, deleteRecursively);
                    }
                }

                foreach (Renci.SshNet.Sftp.SftpFile file in files.Where(x => !x.IsDirectory))
                {
                    this.DeleteFile(file.FullName);
                }

                this.DeleteFolder(path);
            }
            else
            {
                this.DeleteFolder(path);
            }
        }

        /// <summary>
        /// Sets the current folder
        /// </summary>
        /// <param name="newFolder">The new relative or full path</param>
        public void SetCurrentFolder(string newFolder)
        {
            try
            {
                this._client.ChangeDirectory(newFolder);
            }
            catch (Renci.SshNet.Common.SshConnectionException ex)
            {
                throw new Exception(Messages.Messages.ConnectionExceptionMessage, ex);
            }
            catch (Renci.SshNet.Common.SftpPathNotFoundException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PathNotFoundExceptionMessage, Messages.Messages.OperationChangeDirectory, newFolder), ex);
            }
            catch (Renci.SshNet.Common.SftpPermissionDeniedException ex)
            {
                throw new Exception(string.Format(Messages.Messages.PermissionDeniedExceptionMessage, Messages.Messages.OperationChangeDirectory), ex);
            }
        }

        /// <summary>
        /// Gets the current folder's path
        /// </summary>
        public string GetCurrentFolder()
        {
            return this._client.WorkingDirectory;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Certificate's validation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _Client_HostKeyReceived(object sender, Renci.SshNet.Common.HostKeyEventArgs e)
        {
            if (!string.IsNullOrEmpty(this._fingerPrint))
            {
                byte[] fingerPrint = this._fingerPrint.Split(':').Select(s => Convert.ToByte(s, 16)).ToArray();
                e.CanTrust = e.FingerPrint.SequenceEqual(fingerPrint);
            }
            else
                e.CanTrust = true;
        }
        #endregion

        #region Constructor
        public SftpClient(string host, int port, string userName, string password, string fingerPrint)
        {
            // Save the fingerprint for validation
            this._fingerPrint = fingerPrint;

            this._client = new Renci.SshNet.SftpClient(host, port, userName, password);
            this._client.HostKeyReceived += _Client_HostKeyReceived;
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            if (this._client != null)
            {
                this._client.Dispose();
                this._client = null;
            }
        }
        #endregion
    }
}
