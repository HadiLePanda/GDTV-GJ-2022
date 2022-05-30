using UnityEngine;

namespace GameJam
{
    public partial class Utils
    {

        // hard mouse scrolling that is consistent between all platforms
        //   Input.GetAxis("Mouse ScrollWheel") and
        //   Input.GetAxisRaw("Mouse ScrollWheel")
        //   both return values like 0.01 on standalone and 0.5 on WebGL, which
        //   causes too fast zooming on WebGL etc.
        // normally GetAxisRaw should return -1,0,1, but it doesn't for scrolling
        public static float GetAxisRawScrollUniversal()
        {
            float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
            if (scroll < 0) { return -1; }
            if (scroll > 0) { return 1; }
            return 0;
        }
    }
}