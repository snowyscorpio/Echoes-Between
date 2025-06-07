using System.IO;

public static class SystemSpaceChecker
{
    public static bool HasEnoughDiskSpace(long minBytesFree = 100 * 1024 * 1024) // 100MB
    {
        string path = System.Environment.CurrentDirectory;
        DriveInfo drive = new DriveInfo(Path.GetPathRoot(path));
        return drive.AvailableFreeSpace >= minBytesFree;
    }
}
