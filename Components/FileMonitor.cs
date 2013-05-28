using System;
using System.IO;
using System.Linq;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Web.Api;
using InspectorIT.FileWatcher.Components.Common;

namespace InspectorIT.FileWatcher.Components
{
    public class FileMonitor : IServiceRouteMapper
    {

        private readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileMonitor));

        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            foreach (PortalInfo portal in new PortalController().GetPortals().OfType<PortalInfo>())
            {
                Start(portal);
            }
        }

        public void Start(PortalInfo portalInfo)
        {
            var cache = DataCache.GetCache("InspectorIT.FileMonitor." + portalInfo.PortalID);

            if (cache == null)
            {

                FileSystemWatcher fileWatcher = new FileSystemWatcher();
                fileWatcher.Path = portalInfo.HomeDirectoryMapPath;
                fileWatcher.IncludeSubdirectories = true;
                fileWatcher.EnableRaisingEvents = true;
                fileWatcher.NotifyFilter = NotifyFilters.FileName;
                fileWatcher.Created += (s, e) => onFileChanged(s, e, portalInfo);
                fileWatcher.Deleted += (s, e) => onFileDeleted(s, e, portalInfo);
                fileWatcher.Renamed += (s, e) => onFileRenamed(s, e, portalInfo);

                FileSystemWatcher folderWatcher = new FileSystemWatcher();
                folderWatcher.Path = portalInfo.HomeDirectoryMapPath;
                folderWatcher.IncludeSubdirectories = true;
                folderWatcher.EnableRaisingEvents = true;
                folderWatcher.NotifyFilter = NotifyFilters.DirectoryName;
                folderWatcher.Created += (s, e) => onFolderChanged(s, e, portalInfo);
                folderWatcher.Deleted += (s, e) => onFolderDeleted(s, e, portalInfo);
                folderWatcher.Renamed += (s, e) => onFolderRenamed(s, e, portalInfo);


                DataCache.SetCache("InspectorIT.FileMonitor." + portalInfo.PortalID, true);
            }
        }


        #region Files

        private void onFileRenamed(object sender, RenamedEventArgs e, PortalInfo portalInfo)
        {
            try
            {
                var oldRelativeFilePath = Utils.GetRelativePath(portalInfo.HomeDirectoryMapPath, e.OldFullPath);
                var oldFileInfo = FileManager.Instance.GetFile(portalInfo.PortalID, oldRelativeFilePath);
                if (oldFileInfo != null)
                {
                    var newRelativeFilePath = Utils.GetRelativePath(portalInfo.HomeDirectoryMapPath, e.FullPath);
                    var newFileInfo = FileManager.Instance.GetFile(portalInfo.PortalID, newRelativeFilePath);
                    if (newFileInfo == null)
                    {
                        oldFileInfo.FileName = Path.GetFileName(e.Name);
                        FileManager.Instance.UpdateFile(oldFileInfo);
                    }
                }
            }
            catch (Exception ex)
            {

                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
            
        }

        private void onFileDeleted(object sender, FileSystemEventArgs e, PortalInfo portalInfo)
        {
            try
            {
                var relativeFilePath = Utils.GetRelativePath(portalInfo.HomeDirectoryMapPath, e.FullPath);
                var fileInfo = FileManager.Instance.GetFile(portalInfo.PortalID, relativeFilePath);
                if (fileInfo != null)
                {
                    FileManager.Instance.DeleteFile(fileInfo);
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        private void onFileChanged(object sender, FileSystemEventArgs e, PortalInfo portalInfo)
        {
            try
            {
                var relativeFilePath = Utils.GetRelativePath(portalInfo.HomeDirectoryMapPath, e.FullPath);
                string fileName = Path.GetFileName(e.Name);
                var fileInfo = FileManager.Instance.GetFile(0, relativeFilePath);
                if (fileInfo == null)
                {
                    //Get Folder
                    string folderPath = relativeFilePath.Replace(fileName, "");
                    var folderInfo = FolderManager.Instance.GetFolder(portalInfo.PortalID, folderPath);
                    if (folderInfo == null)
                    {
                        folderInfo = FolderManager.Instance.AddFolder(portalInfo.PortalID, folderPath);
                    }
                    FileManager.Instance.AddFile(folderInfo, fileName, null, false);
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        #endregion

        #region Folders

        private void onFolderRenamed(object sender, RenamedEventArgs e, PortalInfo portalInfo)
        {
            try
            {
                string relativeFolderPath = Utils.GetRelativePath(portalInfo.HomeDirectoryMapPath, e.OldFullPath);
                var oldFolderInfo = FolderManager.Instance.GetFolder(portalInfo.PortalID, relativeFolderPath);
                if (oldFolderInfo != null || e.OldFullPath.Contains("New folder"))
                {
                    var newFolderInfo = FolderManager.Instance.GetFolder(portalInfo.PortalID, e.FullPath);
                    if (newFolderInfo == null)
                    {
                        if (e.OldFullPath.Contains("New folder"))
                        {
                            FolderManager.Instance.AddFolder(portalInfo.PortalID, Utils.GetRelativePath(portalInfo.HomeDirectoryMapPath, e.FullPath));
                        }
                        else
                        {
                            new CustomFolderManager().MoveFolder(oldFolderInfo, Utils.GetRelativePath(portalInfo.HomeDirectoryMapPath, e.FullPath));
                        }
                        

                    }
                }
            }
            catch (Exception ex)
            {

                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        private void onFolderDeleted(object sender, FileSystemEventArgs e, PortalInfo portalInfo)
        {
            try
            {
                string relativeFolderPath = Utils.GetRelativePath(portalInfo.HomeDirectoryMapPath, e.FullPath);
                var folderInfo = FolderManager.Instance.GetFolder(portalInfo.PortalID, relativeFolderPath);
                if (folderInfo != null)
                {
                    var folderInfos = FolderManager.Instance.GetFolders(folderInfo.PortalID).Where(f => f.FolderPath != string.Empty && f.FolderPath.StartsWith(folderInfo.FolderPath)).ToList();
                    foreach (IFolderInfo folder in folderInfos)
                    {
                        FolderManager.Instance.DeleteFolder(folder);       
                    }
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        private void onFolderChanged(object sender, FileSystemEventArgs e, PortalInfo portalInfo)
        {
            try
            {
                string relativeFolderPath = Utils.GetRelativePath(portalInfo.HomeDirectoryMapPath, e.FullPath);
                var folderInfo = FolderManager.Instance.GetFolder(portalInfo.PortalID, relativeFolderPath);
                if (folderInfo == null && !e.Name.Contains("New folder"))
                {
                    FolderManager.Instance.AddFolder(portalInfo.PortalID, relativeFolderPath);
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        #endregion
    }
}