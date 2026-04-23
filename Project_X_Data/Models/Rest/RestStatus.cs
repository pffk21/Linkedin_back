namespace Project_X_Data.Models.Rest
{
    public class RestStatus
    {
        public String Phrase { get; set; } = "Ok";
        public int Code { get; set; } = 200;
        public bool IsOK { get; set; } = true;

        public static readonly RestStatus Status200 = new() { IsOK = true, Code = 200, Phrase = "Ok" };
        public static readonly RestStatus Status400 = new() { IsOK = false, Code = 400, Phrase = "Bad request" };
        public static readonly RestStatus Status401 = new() { IsOK = false, Code = 401, Phrase = "Unauthorized" };
        public static readonly RestStatus Status403 = new() { IsOK = false, Code = 403, Phrase = "Forbidden" };
        public static readonly RestStatus Status404 = new() { IsOK = false, Code = 404, Phrase = "Not Found" };

    }
}
