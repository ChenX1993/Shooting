using UnityEngine;

/// <summary>
/// Script created by Unity within their Standard Assets package.
/// </summary>
namespace Opsive.ThirdPersonController.Demos.Adventure
{
    [RequireComponent(typeof(WaterBase))]
    [ExecuteInEditMode]
    public class SpecularLighting : MonoBehaviour
    {
        public Transform specularLight;
        private WaterBase waterBase = null;

        public void Start()
        {
            waterBase = (WaterBase)gameObject.GetComponent(typeof(WaterBase));
        }

        public void Update()
        {
            if (!waterBase)
                waterBase = (WaterBase)gameObject.GetComponent(typeof(WaterBase));

            if (specularLight && waterBase.sharedMaterial)
                waterBase.sharedMaterial.SetVector("_WorldLightDir", specularLight.transform.forward);
        }

    }
}