using UnityEngine;

namespace FPS
{
    public class WeaponInitInfo
    {
        public AbstractEntity entity;
        public AmmoHandler ammoHandler;
        public Camera camera;
        public LayerMask aimMask;
        public float maxAimDistance;

        public WeaponInitInfo(AbstractEntity entity, AmmoHandler ammoHandler, Camera camera, LayerMask aimMask, float maxAimDistance)
        {
            this.entity = entity;
            this.ammoHandler = ammoHandler;
            this.camera = camera;
            this.aimMask = aimMask;
            this.maxAimDistance = maxAimDistance;
        }
    }
}
