using UnityEngine;

namespace Opsive.ThirdPersonController.Wrappers
{
    /// <summary>
    /// Wrapper component to prevent the references from being lost when switching from the Third Person Controller assembly to the Third Person Controller source.
    /// See this page for information on importing the source code: http://opsive.com/assets/ThirdPersonController/documentation.php?id=50.
    /// </summary>
    [RequireComponent(typeof(Opsive.ThirdPersonController.Wrappers.CameraHandler))]
    [RequireComponent(typeof(Opsive.ThirdPersonController.Wrappers.CameraMonitor))]
    public class CameraController : Opsive.ThirdPersonController.CameraController
    {
        // Intentionally left blank. The parent class has all of the implementation.
    }
}