namespace MPM_Betting.Services.Domains;

public partial class UserDomain
{
    public class InvalidBetParameter : Exception;

    private static readonly InvalidBetParameter s_InvalidBetParameter = new();

    public class NoUserException : Exception;

    private static readonly NoUserException s_NoUserException = new();

    public class GroupNotFoundException : Exception;

    private static readonly GroupNotFoundException s_GroupNotFoundException = new();
    
    public class UserNotFoundException : Exception;

    private static readonly UserNotFoundException s_UserNotFoundException = new();

    public class SeasonNotFoundException : Exception;

    private static readonly SeasonNotFoundException s_SeasonNotFoundException = new();

    public class AccessDeniedException : Exception;

    private static readonly AccessDeniedException s_AccessDeniedException = new();

    public class BadWordException : Exception;

    private static readonly BadWordException s_BadWordException = new();

    public class InvalidDateException : Exception;

    private static readonly InvalidDateException s_InvalidDateException = new();

    public class AlreadyExistsException : Exception;

    private static readonly AlreadyExistsException s_AlreadyExistsException = new();
}