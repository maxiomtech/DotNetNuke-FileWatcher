using System.Linq;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.FileSystem;

namespace InspectorIT.FileWatcher.Components
{
    public class CustomFolderManager
    {
        public IFolderInfo MoveFolder(IFolderInfo folder, string newFolderPath)
        {
            Requires.NotNull("folder", folder);
            Requires.NotNullOrEmpty("newFolderPath", newFolderPath);
            
            newFolderPath = PathUtils.Instance.FormatFolderPath(newFolderPath);
            
            if (folder.FolderPath == newFolderPath) return folder;
            
            MoveFolders(folder, newFolderPath);
            
            return FolderManager.Instance.GetFolder(folder.FolderID);
        }


        private void MoveFolders(IFolderInfo folder, string newFolderPath)
        {
            var folderInfos = FolderManager.Instance.GetFolders(folder.PortalID).Where(f => f.FolderPath != string.Empty && f.FolderPath.StartsWith(folder.FolderPath)).ToList();
            foreach (var folderInfo in folderInfos)
            {
                var folderPath = newFolderPath + folderInfo.FolderPath.Substring(folder.FolderPath.Length);
                MoveFolder(folderPath, folderInfo);
            }
        }


        private void MoveFolder(string folderPath, IFolderInfo folderInfo)
        {
            RenameFiles(folderInfo, folderPath);
            folderInfo.FolderPath = folderPath;
            FolderManager.Instance.UpdateFolder(folderInfo);
        }

        private void RenameFiles(IFolderInfo folder, string newFolderPath)
        {
            var files = FolderManager.Instance.GetFiles(folder);
            foreach (var file in files)
            {
                file.Folder = newFolderPath;
                FileManager.Instance.UpdateFile(file);
            }
        }

    }
}