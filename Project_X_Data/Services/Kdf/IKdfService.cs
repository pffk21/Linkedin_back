namespace Project_X_Data.Services.Kdf
{
    // Key Derivation Functions Service by sec.5 RFC 2898
    public interface IKdfService
    {
        String Dk(String password, String salt);
        String GenerateSalt();
    }
}
