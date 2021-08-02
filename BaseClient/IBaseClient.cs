using System;

namespace FTPKey.BaseClient
{
    internal interface IBaseClient : IDisposable
    {
        #region Client Methods
        #region Connection
        /// <summary>
        /// Apre la connessione con il server ftp
        /// </summary>
        void Connect();
        /// <summary>
        /// Chiude la connessione con il server ftp
        /// </summary>
        void Disconnect();
        #endregion

        #region Transfer
        /// <summary>
        /// Cancella un file nella cartella di lavoro remota
        /// </summary>
        /// <param name="remoteFileName">Nome file da cancellare</param>
        /// <returns></returns>
        void DeleteFile(string remoteFileName);
        /// <summary>
        /// Download di un file dalla cartella di lavoro remota
        /// </summary>
        /// <param name="remoteFileName">Nome del file da scaricare</param>
        /// <param name="destinationFile">Percorso completo del file di destinazione</param>
        /// <param name="deleteFileAfterDownload">Indica se il file vada cancellato dalla cartella di lavoro remota dopo il download</param>
        /// <returns></returns>
        void DownloadFile(string remoteFileName, string destinationFile, bool deleteFileAfterDownload);
        /// <summary>
        /// Upload di un file nella cartella di lavoro remota
        /// </summary>
        /// <param name="localFile">Percorso completo del file da caricare</param>
        /// <param name="destinationFileName">Nome del file di destinazione</param>
        /// <param name="deleteFileAfterUpload">Indica se il file caricato vada cancellato dopo l'upload</param>
        /// <returns></returns>
        void UploadFile(string localFile, string destinationFileName, bool deleteFileAfterUpload);
        /// <summary>
        /// Recupera l'elenco dei file presenti nella cartella di lavoro remota
        /// </summary>
        /// <returns>Lista dei file presenti</returns>
        string[] GetFilesList();
        /// <summary>
        /// Recupera l'elenco delle sotto-directory presenti nella cartella di lavoro remota
        /// </summary>
        /// <returns>Lista delle sotto-directory</returns>
        string[] GetFoldersList();
        /// <summary>
        /// Recupera l'elenco dei file presenti nel percorso indicato
        /// </summary>
        /// <param name="path">Percorso dal quale ricavare la lista dei file</param>
        /// <returns>Lista dei file presenti</returns>
        string[] GetFilesList(string path);
        /// <summary>
        /// Recupera l'elenco delle sotto-directory presenti nel percorso indicato
        /// </summary>
        /// <param name="path">Percorso dal quale ricavare la lista delle sotto-directory</param>
        /// <returns>Lista delle sotto-directory</returns>
        string[] GetFoldersList(string path);
        /// <summary>
        /// Rinomina un file nella cartella di lavoro remota
        /// </summary>
        /// <param name="currentName">Nome del file remoto da rinominare</param>
        /// <param name="newName">Nuovo nome da assegnare al file remoto</param>
        void RenameFile(string currentName, string newName);
        /// <summary>
        /// Crea una nuova cartella
        /// </summary>
        /// <param name="path">Percorso completo o relativo alla cartella di lavoro</param>
        void CreateFolder(string path);
        /// <summary>
        /// Cancella una cartella
        /// </summary>
        /// <param name="path">Percorso completo o relativo alla cartella di lavoro</param>
        void DeleteFolder(string path);
        /// <summary>
        /// Cancella una cartella
        /// </summary>
        /// <param name="path">Percorso completo o relativo alla cartella di lavoro</param>
        /// <param name="deleteRecursively">Indica se eseguire la cancellazione ricorsivamente nel caso la cartella non sia vuota</param>
        void DeleteFolder(string path, bool deleteRecursively);
        /// <summary>
        /// Imposta la cartella di lavoro corrente
        /// </summary>
        /// <param name="newFolder">Percorso completo della nuova cartella di lavoro</param>
        void SetCurrentFolder(string newFolder);
        /// <summary>
        /// Restituisce l'attuale cartella di lavoro
        /// </summary>
        /// <returns></returns>
        string GetCurrentFolder();
        #endregion
        #endregion
    }
}
