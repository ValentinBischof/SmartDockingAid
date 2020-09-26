using UnityEngine;

namespace SmartDockingAid.Flight
{
    public static class ExtensionFlight
    {
        /// <summary>
        /// Returns facing (or anti-facing) vector of a ITargetable
        /// </summary>
        /// <param name="target"></param>
        /// <param name="targetMode"></param>
        /// <returns>Vector3</returns>
        public static Vector3 getAttitude(this ITargetable target, SmartDockingAid.TargetMode targetMode)
        {
            if (targetMode == SmartDockingAid.TargetMode.PARALLEL_NEGATIVE)
            {
                if (target is ModuleDockingNode)
                {
                    return -target.GetTransform().forward;
                }
                else
                {
                    return -target.GetTransform().up;
                }
            }
            else
            {
                if (target is ModuleDockingNode)
                {
                    return target.GetTransform().forward;
                }
                else
                {
                    return target.GetTransform().up;
                }
            }
        }
    }
}
