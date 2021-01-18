using System;
using System.Diagnostics.CodeAnalysis;
using Colore.Data;
using EliteChroma.Chroma;
using EliteFiles.Status;
using static EliteFiles.Journal.Events.StartJump;

namespace EliteChroma.Core.Layers
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by ChromaController.InitChromaEffect().")]
    internal sealed class VehicleModeLayer : LayerBase
    {
        private static readonly TimeSpan _fadeDuration = TimeSpan.FromSeconds(0.2);

        private (bool HardpointsDeployed, VehicleMode Mode) _lastState;
        private Color _animC1;
        private Color _animC2;

        private enum VehicleMode
        {
            Combat,
            Analysis,
            LandingGear,
        }

        public override int Order => 600;

        protected override void OnRender(ChromaCanvas canvas)
        {
            if (!Game.InCockpit || Game.FsdJumpType != FsdJumpType.None)
            {
                return;
            }

            var hardpointsDeployed = Game.Status.HasFlag(Flags.HardpointsDeployed) && !Game.Status.HasFlag(Flags.Supercruise);

            VehicleMode mode;
            if (Game.Status.HasFlag(Flags.LandingGearDeployed))
            {
                mode = VehicleMode.LandingGear;
            }
            else if (Game.Status.HasFlag(Flags.HudInAnalysisMode))
            {
                mode = VehicleMode.Analysis;
            }
            else
            {
                mode = VehicleMode.Combat;
            }

            var state = (hardpointsDeployed, mode);

            if (state != _lastState)
            {
                StartAnimation();
                _animC1 = GetBackgroundColor(_lastState.HardpointsDeployed, _lastState.Mode);
                _lastState = state;
            }

            if (Animated && AnimationElapsed >= _fadeDuration)
            {
                StopAnimation();
            }

            _animC2 = GetBackgroundColor(hardpointsDeployed, mode);

            var c = Animated
                ? PulseColor(_animC1, _animC2, _fadeDuration, PulseColorType.Sawtooth)
                : _animC2;

            canvas.Mouse.Set(c);
            canvas.Mousepad.Set(c);
            canvas.Headset.Set(c);
            canvas.ChromaLink.Set(c);
        }

        private Color GetBackgroundColor(bool hardpointsDeployed, VehicleMode mode)
        {
            Color c;
            switch (mode)
            {
                case VehicleMode.Combat:
                    c = Game.Colors.Hud;
                    break;
                case VehicleMode.Analysis:
                    c = Game.Colors.AnalysisMode;
                    break;
                case VehicleMode.LandingGear:
                default:
                    c = Color.Blue;
                    break;
            }

            if (!hardpointsDeployed)
            {
                c = c.Transform(Colors.DeviceDimBrightness);
            }

            return c;
        }
    }
}
