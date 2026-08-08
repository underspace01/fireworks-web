using UnityEngine;

namespace Fireworks
{
    public sealed class WebFireworkController : MonoBehaviour
    {
        [SerializeField]
        private LanWebServer _webServer;

        [SerializeField]
        private FireworkSettings _settings;

        [SerializeField]
        private FireworkLauncher _launcher;

        [SerializeField, Min(1)]
        private int _maximumCommandsPerFrame = 10;

        private void Update()
        {
            if (_webServer == null || _settings == null || _launcher == null)
            {
                return;
            }

            for (int count = 0; count < _maximumCommandsPerFrame; count++)
            {
                if (!_webServer.TryDequeueCommand(out WebFireworkCommand command))
                {
                    return;
                }

                ExecuteCommand(command);
            }
        }

        private void ExecuteCommand(WebFireworkCommand command)
        {
            if (!_settings.TryGetSizeIndex(command.sizeId, out int sizeIndex) ||
                !_settings.TryGetEffectIndex(command.structure, out int structureIndex) ||
                !_settings.TryGetColorIndex(command.colorId, out int colorIndex))
            {
                return;
            }

            if (_settings.GetEffect(structureIndex).IsTextFirework)
            {
                _launcher.LaunchTextFirework(command.textValue, sizeIndex, colorIndex);
                return;
            }

            if (!_settings.TryGetColorIndex(command.changeColorId, out int changeColorIndex)) return;

            _launcher.LaunchFirework(sizeIndex, structureIndex, colorIndex, changeColorIndex);
        }
    }
}
