namespace DemianzxBackend.Application.Common.Exceptions;

public class PasswordRecoveryUnavailableException : Exception
{
    public PasswordRecoveryUnavailableException()
        : base("La recuperación de contraseña no está disponible temporalmente. Inténtalo más tarde.")
    {
    }
}
