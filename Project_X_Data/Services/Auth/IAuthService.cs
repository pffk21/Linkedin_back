namespace Project_X_Data.Services.Auth
{
    public interface IAuthService
    {
        void SetAuth(object payload);
        T? GetAuth<T>() where T : notnull;
        void RemoveAuth();

    }
}
