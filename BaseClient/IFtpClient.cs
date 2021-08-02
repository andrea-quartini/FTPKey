using System;
using System.IO;

namespace FTPKey.BaseClient
{
    internal interface IFtpClient : IDisposable
    {
        #region Connection
        /// <summary>
        /// Opens a connection with Ftp area
        /// </summary>
        void Connect();

        /// <summary>
        /// Closes the connection
        /// </summary>
        void Disconnect();
        #endregion

        #region Transfer
        /// <summary>
        /// Deletes the desired remote file
        /// </summary>
        /// <param name="remoteFileName">The file to delete</param>
        void DeleteFile(string remoteFileName);

        /// <summary>
        /// Downloads a file from Ftp area
        /// </summary>
        /// <param name="remoteFileName">The name of file to download</param>
        /// <param name="destinationFile">The destination local path</param>
        /// <param name="deleteFileAfterDownload">If true, it deletes the remote file after downloading it</param>
        bool DownloadFile(string remoteFileName, string destinationFile, bool deleteFileAfterDownload);

        /// <summary>
        /// Downloads a file from Ftp area
        /// </summary>
        /// <param name="remoteFileName">The name of file to download</param>
        /// <param name="outStream">The output stream of downloaded file</param>
        /// <param name="deleteFileAfterDownload">If true, it deletes the remote file after downloading it</param>
        bool DownloadFile(string remoteFileName, Stream outStream, bool deleteFileAfterDownload);

        /// <summary>
        /// Uploads a file to the Ftp area
        /// </summary>
        /// <param name="localFile">Full local file path</param>
        /// <param name="destinationFileName">Destination file name</param>
        /// <param name="deleteFileAfterUpload">If true, it deletes the local file after uploading it</param>
        void UploadFile(string localFile, string destinationFileName, bool deleteFileAfterUpload);

        /// <summary>
        /// Uploads a file to the Ftp area
        /// </summary>
        /// <param name="localFileStream">The local file stream</param>
        /// <param name="destinationFileName">Destination file name</param>
        void UploadFile(Stream localFileStream, string destinationFileName);
        
        /// <summary>
        /// Gets a list of filenames from the current remote folder
        /// </summary>
        string[] GetFilesList();
        
        /// <summary>
        /// Gets a list of sub-folders from the current remote folder
        /// </summary>
        string[] GetFoldersList();
        
        /// <summary>
        /// Gets a list of files from the desired path
        /// </summary>
        /// <param name="path">The path from witch retrieve the files list</param>
        string[] GetFilesList(string path);
        
        /// <summary>
        /// Gets a list of sub-folders from the desired path
        /// </summary>
        /// <param name="path">The main path to search in for the sub-folders list</param>
        string[] GetFoldersList(string path);
        
        /// <summary>
        /// Renames a remote file
        /// </summary>
        /// <param name="currentName">the current remote file's name</param>
        /// <param name="newName">The new name</param>
        void RenameFile(string currentName, string newName);
        
        /// <summary>
        /// Creates a new remote folder
        /// </summary>
        /// <param name="path">The partial or full path to create</param>
        void CreateFolder(string path);
        
        /// <summary>
        /// Deletes a remote folder, not recursively
        /// </summary>
        void DeleteFolder(string path);

        /// <summary>
        /// Deletes a remote folder
        /// </summary>
        /// <param name="path">The folder to delete</param>
        /// <param name="deleteRecursively">If true, a recursive deletion will be performed</param>
        void DeleteFolder(string path, bool deleteRecursively);
        
        /// <summary>
        /// Sets the current folder
        /// </summary>
        /// <param name="newFolder">The new relative or full path</param>
        void SetCurrentFolder(string newFolder);
        
        /// <summary>
        /// Gets the current folder's path
        /// </summary>
        string GetCurrentFolder();
        #endregion
    }
}
