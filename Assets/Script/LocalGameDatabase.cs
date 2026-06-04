using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

[Serializable]
public class ChallengeRecord
{
    public string challengerName;
    public int totalDeaths;
    public float clearTimeSeconds;
    public int doubleJumpUses;
    public string completedAtBeijing;
}

[Serializable]
public class PlayerAccount
{
    public string userName;
    public string passwordHash;
    public List<ChallengeRecord> challengeRecords = new List<ChallengeRecord>();
}

[Serializable]
public class LocalGameDatabaseState
{
    public List<PlayerAccount> accounts = new List<PlayerAccount>();
}

public enum RegisterResult
{
    Success,
    EmptyUserName,
    EmptyPassword,
    UserExists
}

public enum LoginResult
{
    Success,
    EmptyUserName,
    EmptyPassword,
    UserNotFound,
    WrongPassword
}

public static class LocalGameDatabase
{
    private const string DatabaseFileName = "one_more_trap_database.json";

    private static LocalGameDatabaseState state;
    private static string currentUserName = "";

    public static bool IsLoggedIn
    {
        get { return !string.IsNullOrEmpty(currentUserName); }
    }

    public static string CurrentUserName
    {
        get { return currentUserName; }
    }

    public static string DatabasePath
    {
        get { return Path.Combine(Application.persistentDataPath, DatabaseFileName); }
    }

    // Creates a local account and logs it in when validation succeeds.
    public static RegisterResult Register(string userName, string password)
    {
        Load();

        userName = NormalizeName(userName);

        if (string.IsNullOrEmpty(userName))
        {
            return RegisterResult.EmptyUserName;
        }

        if (string.IsNullOrEmpty(password))
        {
            return RegisterResult.EmptyPassword;
        }

        if (FindAccount(userName) != null)
        {
            return RegisterResult.UserExists;
        }

        PlayerAccount account = new PlayerAccount();
        account.userName = userName;
        account.passwordHash = HashPassword(password);
        state.accounts.Add(account);

        currentUserName = userName;
        Save();

        return RegisterResult.Success;
    }

    // Validates credentials against the local database and stores the active user.
    public static LoginResult Login(string userName, string password)
    {
        Load();

        userName = NormalizeName(userName);

        if (string.IsNullOrEmpty(userName))
        {
            return LoginResult.EmptyUserName;
        }

        if (string.IsNullOrEmpty(password))
        {
            return LoginResult.EmptyPassword;
        }

        PlayerAccount account = FindAccount(userName);
        if (account == null)
        {
            return LoginResult.UserNotFound;
        }

        if (account.passwordHash != HashPassword(password))
        {
            return LoginResult.WrongPassword;
        }

        currentUserName = account.userName;
        return LoginResult.Success;
    }

    // Clears the active local user.
    public static void Logout()
    {
        currentUserName = "";
    }

    // Adds a completed challenge record to the matching account.
    public static void AddChallengeRecord(ChallengeRecord record)
    {
        Load();

        if (record == null || string.IsNullOrEmpty(record.challengerName))
        {
            return;
        }

        PlayerAccount account = FindAccount(record.challengerName);
        if (account == null)
        {
            return;
        }

        if (account.challengeRecords == null)
        {
            account.challengeRecords = new List<ChallengeRecord>();
        }

        account.challengeRecords.Add(record);
        Save();
    }

    // Returns records sorted by lowest death count.
    public static List<ChallengeRecord> GetDeathRanking(int limit)
    {
        return GetAllRecords()
            .OrderBy(record => record.totalDeaths)
            .ThenBy(record => record.clearTimeSeconds)
            .Take(limit)
            .ToList();
    }

    // Returns records sorted by fastest clear time.
    public static List<ChallengeRecord> GetTimeRanking(int limit)
    {
        return GetAllRecords()
            .OrderBy(record => record.clearTimeSeconds)
            .ThenBy(record => record.totalDeaths)
            .Take(limit)
            .ToList();
    }

    // Returns records sorted by fewest double-jump uses.
    public static List<ChallengeRecord> GetDoubleJumpRanking(int limit)
    {
        return GetAllRecords()
            .OrderBy(record => record.doubleJumpUses)
            .ThenBy(record => record.clearTimeSeconds)
            .Take(limit)
            .ToList();
    }

    private static List<ChallengeRecord> GetAllRecords()
    {
        Load();

        List<ChallengeRecord> records = new List<ChallengeRecord>();

        for (int i = 0; i < state.accounts.Count; i++)
        {
            PlayerAccount account = state.accounts[i];

            if (account.challengeRecords == null)
            {
                continue;
            }

            records.AddRange(account.challengeRecords);
        }

        return records;
    }

    // Loads the JSON database from persistent storage once per session.
    private static void Load()
    {
        if (state != null)
        {
            return;
        }

        string path = DatabasePath;

        if (!File.Exists(path))
        {
            state = new LocalGameDatabaseState();
            return;
        }

        string json = File.ReadAllText(path);
        state = JsonUtility.FromJson<LocalGameDatabaseState>(json);

        if (state == null)
        {
            state = new LocalGameDatabaseState();
        }

        if (state.accounts == null)
        {
            state.accounts = new List<PlayerAccount>();
        }
    }

    // Saves the current database state to persistent storage.
    private static void Save()
    {
        string json = JsonUtility.ToJson(state, true);
        File.WriteAllText(DatabasePath, json);
    }

    // Finds an account by user name using case-insensitive comparison.
    private static PlayerAccount FindAccount(string userName)
    {
        Load();

        for (int i = 0; i < state.accounts.Count; i++)
        {
            PlayerAccount account = state.accounts[i];
            if (string.Equals(account.userName, userName, StringComparison.OrdinalIgnoreCase))
            {
                return account;
            }
        }

        return null;
    }

    private static string NormalizeName(string userName)
    {
        return string.IsNullOrEmpty(userName) ? "" : userName.Trim();
    }

    // Hashes a password before storing or comparing credentials.
    private static string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new StringBuilder(bytes.Length * 2);

            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }

            return builder.ToString();
        }
    }
}
