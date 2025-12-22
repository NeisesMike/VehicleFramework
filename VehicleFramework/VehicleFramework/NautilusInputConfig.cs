using Nautilus.Handlers;

namespace VehicleFramework
{
    public partial class MainPatcher
    {
        const string nextCameraButton = "VFNextCameraButton";
        const string previousCameraButton = "VFPrevCameraButton";
        const string exitCameraButton = "VFExitCameraButton";
        const string headlightsButton = "VFHeadlightsButton";
        const string magnetBootsButton = "VFMagBootsButton";

        internal GameInput.Button NextCameraKey = EnumHandler.AddEntry<GameInput.Button>(nextCameraButton)
            .CreateInput()
            .SetBindable()
            .WithKeyboardBinding(GameInputHandler.Paths.Keyboard.F)
            .AvoidConflicts(GameInput.Device.Keyboard)
            .WithCategory("Vehicle Framework");

        internal GameInput.Button PreviousCameraKey = EnumHandler.AddEntry<GameInput.Button>(previousCameraButton)
            .CreateInput()
            .SetBindable()
            .WithKeyboardBinding(GameInputHandler.Paths.Keyboard.T)
            .AvoidConflicts(GameInput.Device.Keyboard)
            .WithCategory("Vehicle Framework");

        internal GameInput.Button ExitCameraKey = EnumHandler.AddEntry<GameInput.Button>(exitCameraButton)
            .CreateInput()
            .SetBindable()
            .WithKeyboardBinding(GameInputHandler.Paths.Keyboard.V)
            .AvoidConflicts(GameInput.Device.Keyboard)
            .WithCategory("Vehicle Framework");

        internal GameInput.Button HeadlightsKey = EnumHandler.AddEntry<GameInput.Button>(headlightsButton)
            .CreateInput()
            .SetBindable()
            .WithKeyboardBinding(GameInputHandler.Paths.Mouse.LeftButton)
            .AvoidConflicts(GameInput.Device.Keyboard)
            .WithCategory("Vehicle Framework");

        internal GameInput.Button MagnetBootsKey = EnumHandler.AddEntry<GameInput.Button>(magnetBootsButton)
            .CreateInput()
            .SetBindable()
            .WithKeyboardBinding(GameInputHandler.Paths.Keyboard.G)
            .AvoidConflicts(GameInput.Device.Keyboard)
            .WithCategory("Vehicle Framework");
    }
}
