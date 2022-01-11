using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Game.Managers
{
    [RequireComponent(typeof(Animator))]
    public class NotificationManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI notificationText;
        private static TextMeshProUGUI _notificationText;
        private static RectTransform _notificationTextTransform;

        private static Animator _animator;

        [Space]
        [SerializeField] private Image[] images;
        private static Image[] _images;

        //private static bool _onScene;
        //internal static bool OnScene
        //{
        //    get
        //    {
        //        if (!_onScene) Instantiate();
        //        return _onScene;
        //    }
        //    set => _onScene = value;
        //}

        private void Awake()
        {
            _notificationText = notificationText;
            _notificationTextTransform = notificationText.transform.parent.GetComponent<RectTransform>();
            _animator = GetComponent<Animator>();
            _images = images;
        }

        private void Start()
        {
            Notification(NotificationTypes.Message, "You have no new messages");
        }

        private static readonly int activateId = Animator.StringToHash("Activate");
        private static readonly int activateUnclampedId = Animator.StringToHash("ActivateUnclamped");
        private static readonly int skipId = Animator.StringToHash("Skip");
        internal static void Notification(NotificationTypes notificationType, string message, bool withTimer = true)
        {
            //if (_onScene)

            if (_notificationText == null || _images == null) return;

            foreach (var image in _images) image.gameObject.SetActive(false);
            _images[(int)notificationType].gameObject.SetActive(true);
            
            _notificationText.text = message;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_notificationTextTransform);

            if (withTimer) _animator.SetTrigger(activateId);
            else _animator.SetTrigger(activateUnclampedId);
        }

        internal static void SkipNotification()
        {
            if (_animator != null)
                _animator.SetTrigger(skipId);
        }
    }

    internal enum NotificationTypes
    {
        Info,
        Hint,
        Message
    }
}
