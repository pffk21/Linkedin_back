namespace Project_X_Data.Services.Storage
{
    public interface IStorageService
    {
        String Save(IFormFile file);
        byte[]? Load(String filename);
    }
}
