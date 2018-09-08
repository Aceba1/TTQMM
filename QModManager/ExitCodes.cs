namespace QModManager
{
    public struct ExitCodes
    {
        public const int
            UnknownError = -1, // A generic exception was caught
            TaskCompleted = 0, // Task completed successfully with no exceptions OR task already done
            RequiredFileMissing = 1, // The assembly file is missing
            RequiredFileInUse = 2, // The assembly file is in use (maybe the game is running?)
            ArgumentParsingError = 3; // There was a problem parsing arguments
    }
}
