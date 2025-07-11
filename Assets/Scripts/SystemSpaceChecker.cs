using System.IO;

public static class SystemSpaceChecker
{
    /// <summary>
    /// Checks if the disk where the current application is running has enough free space.
    /// </summary>
    /// <param name="minBytesFree">
    /// Minimum number of free bytes required on the disk (default: 100MB).
    /// </param>
    /// <returns>
    /// Returns true if the available free space is greater than or equal to the required amount; otherwise, false.
    /// </returns>
    public static bool HasEnoughDiskSpace(long minBytesFree = 100 * 1024 * 1024) // Default: 100MB
    {
        // Get the current working directory of the application
        string path = System.Environment.CurrentDirectory;

        // Get the root drive of the current path (e.g., "C:\")
        DriveInfo drive = new DriveInfo(Path.GetPathRoot(path));

        // Check if the available free space on the drive meets the minimum requirement
        return drive.AvailableFreeSpace >= minBytesFree;
    }
}

