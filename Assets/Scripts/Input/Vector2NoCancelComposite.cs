using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Composites;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Utilities;

namespace VLDefenderArcade.Input
{
    // A custom composite for the Input System
    // https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/ActionBindings.html#writing-custom-composites

    /// <summary>
    /// Unlike the default <see cref="Vector2Composite"/> this one does not make the opposite buttons cancel each other.
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    [DisplayStringFormat("{up}/{left}/{down}/{right}")]
    [DisplayName("Up/Down/Left/Right Composite no cancel")]
    public class Vector2NoCancelComposite : InputBindingComposite<Vector2>
    {
        [InputControl(layout = "Axis")] public int up;
        [InputControl(layout = "Axis")] public int down;
        [InputControl(layout = "Axis")] public int left;
        [InputControl(layout = "Axis")] public int right;

        [Tooltip("How to synthesize a Vector2 from the inputs. Digital "
            + "treats part bindings as buttons (on/off) whereas Analog preserves "
            + "floating-point magnitudes as read from controls.")]
        public Vector2Composite.Mode mode;

        /// <inheritdoc />
        public override Vector2 ReadValue(ref InputBindingCompositeContext context)
        {
            var mode = this.mode;

            if (mode == Vector2Composite.Mode.Analog)
            {
                var upValue = context.ReadValue<float>(up);
                var downValue = context.ReadValue<float>(down);
                var leftValue = context.ReadValue<float>(left);
                var rightValue = context.ReadValue<float>(right);

                return DpadControl.MakeDpadVector(upValue, downValue, leftValue, rightValue);
            }

            var upIsPressed = context.ReadValueAsButton(up);
            var downIsPressed = context.ReadValueAsButton(down);
            var leftIsPressed = context.ReadValueAsButton(left);
            var rightIsPressed = context.ReadValueAsButton(right);

            // Opposite buttons won't cancel each other
            if (upIsPressed && downIsPressed)
            {
                if (context.GetPressTime(up) > context.GetPressTime(down))
                    downIsPressed = false;
                else
                    upIsPressed = false;
            }
            if (leftIsPressed && rightIsPressed)
            {
                if (context.GetPressTime(left) > context.GetPressTime(right))
                    rightIsPressed = false;
                else
                    leftIsPressed = false;
            }

            return DpadControl.MakeDpadVector(upIsPressed, downIsPressed, leftIsPressed, rightIsPressed, mode == Vector2Composite.Mode.DigitalNormalized);
        }

        /// <inheritdoc />
        public override float EvaluateMagnitude(ref InputBindingCompositeContext context)
        {
            var value = ReadValue(ref context);
            return value.magnitude;
        }

        static Vector2NoCancelComposite()
        {
            InputSystem.RegisterBindingComposite<Vector2NoCancelComposite>("2D Vector no cancel");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init() { }
    }
}
