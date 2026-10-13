namespace DemianzxBackend.Infrastructure.Identity;

public class PasswordRecoveryOptions
{
    public const string SectionName = "PasswordRecovery";
    public string ResetUrl { get; set; } = string.Empty;
}
