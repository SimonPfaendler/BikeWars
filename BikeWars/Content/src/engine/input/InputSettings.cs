namespace BikeWars.Content.engine.input;

public enum ControlType
{
    Keyboard,
    Controller
}

public static class InputSettings
{
    public static ControlType Player1Control { get; set; } = ControlType.Keyboard;
    public static ControlType Player2Control { get; set; } = ControlType.Controller;

    public static void SetControlType(bool isPlayer1, ControlType type)
    {
        if (isPlayer1)
        {
            Player1Control = type;
            // only one player is allowed to use the keyboard, but both could use a controller
            // if one player switches to the keyboard, the other must automatically switch to a controller
            if (type == ControlType.Keyboard)
                Player2Control = ControlType.Controller;
        }
        else
        {
            Player2Control = type;
            if (type == ControlType.Keyboard)
                Player1Control = ControlType.Controller;
        }
    }
}