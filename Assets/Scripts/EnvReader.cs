using dotenv.net;
using System;
public class EnvReader
{
    public static string GetEnvVariable(string key)
    {
        DotEnv.Load();
        return Environment.GetEnvironmentVariable(key);
    }
}
