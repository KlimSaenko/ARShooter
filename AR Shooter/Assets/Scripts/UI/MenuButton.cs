namespace Game.UI
{
    public class MenuButton : UIButton
    {
        public override void OnValueChanged(bool value)
        {
            buttonAnimator.SetTrigger(isActiveId);

            if (buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Pressed 1") ||
                buttonAnimator.GetNextAnimatorStateInfo(0).IsName("Pressed 1"))
                OnButtonPressed();
        }
    }
}
