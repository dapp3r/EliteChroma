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

        private (bool HardpointsDeployed, bool InAnalysisMode) _lastState;
        private Color _animC1;
        private Color _animC2;

        public override int Order => 600;

        protected override void OnRender(ChromaCanvas canvas)
        {
            if (!Game.InCockpit || Game.FsdJumpType != FsdJumpType.None)
            {
                return;
            }

            var hardpointsDeployed = Game.Status.HasFlag(Flags.HardpointsDeployed) && !Game.Status.HasFlag(Flags.Supercruise);
            var inAnalysisMode = Game.Status.HasFlag(Flags.HudInAnalysisMode);
            var state = (hardpointsDeployed, inAnalysisMode);

            if (state != _lastState)
            {
                StartAnimation();
                _animC1 = GetBackgroundColor(_lastState.HardpointsDeployed, _lastState.InAnalysisMode);
                _lastState = state;
            }

            if (Animated && AnimationElapsed >= _fadeDuration)
            {
                StopAnimation();
            }

            _animC2 = GetBackgroundColor(hardpointsDeployed, inAnalysisMode);

            var c = Animated
                ? PulseColor(_animC1, _animC2, _fadeDuration, PulseColorType.Sawtooth)
                : _animC2;

            canvas.Mouse.Set(c);
            canvas.Mousepad.Set(c);
            canvas.Headset.Set(c);
            canvas.ChromaLink.Set(c);
        }

        private Color GetBackgroundColor(bool hardpointsDeployed, bool inAnalysisMode)
        {
            var c = inAnalysisMode ? Game.Colors.AnalysisMode : Game.Colors.Hud;

            if (!hardpointsDeployed)
            {
                c = c.Transform(Colors.DeviceDimBrightness);
            }

            return c;
        }
    }
}
