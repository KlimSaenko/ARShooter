namespace Weapons
{
    public class Carbine : MainWeapon
    {
        private void Awake()
        {
            PlayerBehaviour.FiringAction += delegate(bool start) { Shoot(start); };
        }
    }
}
