namespace HypermediaClient.Authentication
{
    public class UsernamePasswordCredentials
    {
        public readonly string User;

        public readonly string Password;

        public UsernamePasswordCredentials(string user, string password)
        {
            this.User = user;
            this.Password = password;
        }
    }
}