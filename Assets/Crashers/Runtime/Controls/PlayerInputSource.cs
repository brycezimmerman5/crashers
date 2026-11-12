using UnityEngine;
using UnityEngine.InputSystem;

namespace Crashers.Controls
{
    public interface IInputSource
    {
        Vector2 Move { get; }
        bool JumpPressed { get; }
        bool LightPressed { get; }
        bool HeavyPressed { get; }
        bool Ability1Pressed { get; }
        bool Ability2Pressed { get; }
        bool Ability3Pressed { get; }
        bool InteractPressed { get; }
    }

    /// <summary>
    /// Keyboard and gamepad bindings created in code so the arena does not depend on a generated input wrapper.
    /// Move: WASD, left stick, d-pad. W and stick-up increase arena depth (up the screen).
    /// Jump: Space / South. Light: J / West. Heavy: K / North.
    /// Ability 1: U / right trigger. Ability 2: I / left trigger. Ability 3: O / right shoulder.
    /// Interact: E / East.
    /// </summary>
    public sealed class PlayerInputSource : MonoBehaviour, IInputSource
    {
        InputActionMap map;
        InputAction move;
        InputAction jump;
        InputAction lightAttack;
        InputAction heavyAttack;
        InputAction ability1;
        InputAction ability2;
        InputAction ability3;
        InputAction interact;

        public Vector2 Move => move != null ? move.ReadValue<Vector2>() : Vector2.zero;
        public bool JumpPressed => Pressed(jump);
        public bool LightPressed => Pressed(lightAttack);
        public bool HeavyPressed => Pressed(heavyAttack);
        public bool Ability1Pressed => Pressed(ability1);
        public bool Ability2Pressed => Pressed(ability2);
        public bool Ability3Pressed => Pressed(ability3);
        public bool InteractPressed => Pressed(interact);

        void Awake() => BuildMap();

        void OnEnable()
        {
            if (map == null) BuildMap();
            map.Enable();
        }

        void OnDisable()
        {
            if (map != null) map.Disable();
        }

        void OnDestroy()
        {
            if (map == null) return;
            map.Disable();
            map.Dispose();
            map = null;
        }

        void BuildMap()
        {
            if (map != null) return;
            map = new InputActionMap("Player");
            move = map.AddAction("Move", InputActionType.Value, expectedControlType: "Vector2");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            move.AddBinding("<Gamepad>/leftStick");
            move.AddBinding("<Gamepad>/dpad");
            jump = Button("Jump", "<Keyboard>/space", "<Gamepad>/buttonSouth");
            lightAttack = Button("Light", "<Keyboard>/j", "<Gamepad>/buttonWest");
            heavyAttack = Button("Heavy", "<Keyboard>/k", "<Gamepad>/buttonNorth");
            ability1 = Button("Ability1", "<Keyboard>/u", "<Gamepad>/rightTrigger");
            ability2 = Button("Ability2", "<Keyboard>/i", "<Gamepad>/leftTrigger");
            ability3 = Button("Ability3", "<Keyboard>/o", "<Gamepad>/rightShoulder");
            interact = Button("Interact", "<Keyboard>/e", "<Gamepad>/buttonEast");
        }

        InputAction Button(string actionName, string keyboard, string gamepad)
        {
            InputAction action = map.AddAction(actionName, InputActionType.Button);
            action.AddBinding(keyboard);
            action.AddBinding(gamepad);
            return action;
        }

        static bool Pressed(InputAction action) => action != null && action.WasPressedThisFrame();
    }
}
