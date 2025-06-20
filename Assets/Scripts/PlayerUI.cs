using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace VLDefenderArcade
{
    /// <summary>
    /// Display player lives, score, etc.
    /// </summary>
    public class PlayerUI : MonoBehaviour
    {
        [SerializeField]
        private Transform _livesBar;
        [SerializeField]
        private Text _scoreText;

        [SerializeField]
        private GameObject _gameOverScreen;

        [SerializeField]
        private PlayerShipController _player;

        private int _lastScore = -1;

        private InputAction _submitAction;
        private InputAction _clickAction;

        private void Start()
        {
            _submitAction = InputSystem.actions.FindAction("Submit");
            _clickAction = InputSystem.actions.FindAction("Click");
        }

        private void Update()
        {
            if (_lastScore != _player.Score)
            {
                _lastScore = _player.Score;
                _scoreText.text = _player.Score.ToString();
            }

            for (int i = 0; i < _livesBar.childCount; i++)
            {
                _livesBar.GetChild(i).gameObject.SetActive(i < _player.Lives);
            }

            _gameOverScreen.SetActive(_player.IsGameOver);

            if (_player.IsGameOver && (_submitAction.WasPressedThisFrame() || _clickAction.WasPressedThisFrame()))
            {
                RestartGame();
            }
        }

        private void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().path);
        }
    }
}
