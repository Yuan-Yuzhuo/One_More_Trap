using System;
using UnityEngine;

public enum PlayerInputAction
{
    MoveLeft,
    MoveRight,
    Jump,
    Dash
}

public static class PlayerInputConfig
{
    private const string KeyPrefix = "PlayerInput_";

    public static KeyCode MoveLeftKey
    {
        get { return GetKey(PlayerInputAction.MoveLeft); }
    }

    public static KeyCode MoveRightKey
    {
        get { return GetKey(PlayerInputAction.MoveRight); }
    }

    public static KeyCode JumpKey
    {
        get { return GetKey(PlayerInputAction.Jump); }
    }

    public static KeyCode DashKey
    {
        get { return GetKey(PlayerInputAction.Dash); }
    }

    public static KeyCode GetKey(PlayerInputAction action)
    {
        string saved = PlayerPrefs.GetString(GetPrefKey(action), "");
        KeyCode key;

        if (!string.IsNullOrEmpty(saved) && Enum.TryParse(saved, out key))
        {
            return key;
        }

        return GetDefaultKey(action);
    }

    public static void SetKey(PlayerInputAction action, KeyCode key)
    {
        PlayerPrefs.SetString(GetPrefKey(action), key.ToString());
        PlayerPrefs.Save();
    }

    public static void ResetDefaults()
    {
        PlayerPrefs.DeleteKey(GetPrefKey(PlayerInputAction.MoveLeft));
        PlayerPrefs.DeleteKey(GetPrefKey(PlayerInputAction.MoveRight));
        PlayerPrefs.DeleteKey(GetPrefKey(PlayerInputAction.Jump));
        PlayerPrefs.DeleteKey(GetPrefKey(PlayerInputAction.Dash));
        PlayerPrefs.Save();
    }

    public static float GetHorizontalMove()
    {
        float move = 0f;

        if (Input.GetKey(MoveLeftKey))
        {
            move -= 1f;
        }

        if (Input.GetKey(MoveRightKey))
        {
            move += 1f;
        }

        return Mathf.Clamp(move, -1f, 1f);
    }

    public static bool TryGetPressedKey(Event currentEvent, out KeyCode key)
    {
        key = KeyCode.None;

        if (currentEvent == null || currentEvent.type != EventType.KeyDown)
        {
            return false;
        }

        if (currentEvent.keyCode == KeyCode.None)
        {
            return false;
        }

        key = currentEvent.keyCode;
        return true;
    }

    public static string GetActionLabel(PlayerInputAction action)
    {
        if (action == PlayerInputAction.MoveLeft)
        {
            return "Move Left";
        }

        if (action == PlayerInputAction.MoveRight)
        {
            return "Move Right";
        }

        if (action == PlayerInputAction.Jump)
        {
            return "Jump";
        }

        return "Dash";
    }

    private static string GetPrefKey(PlayerInputAction action)
    {
        return KeyPrefix + action;
    }

    private static KeyCode GetDefaultKey(PlayerInputAction action)
    {
        if (action == PlayerInputAction.MoveLeft)
        {
            return KeyCode.A;
        }

        if (action == PlayerInputAction.MoveRight)
        {
            return KeyCode.D;
        }

        if (action == PlayerInputAction.Jump)
        {
            return KeyCode.W;
        }

        return KeyCode.Space;
    }
}
