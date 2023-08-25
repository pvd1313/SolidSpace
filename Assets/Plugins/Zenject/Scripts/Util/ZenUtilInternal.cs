using UnityEngine;
using UnityEngine.SceneManagement;
#if !NOT_UNITY3D

#endif

namespace Zenject
{
    public static class ZenUtilInternal
    {
#if UNITY_EDITOR
        static GameObject _disabledIndestructibleGameObject;
#endif

        // Due to the way that Unity overrides the Equals operator,
        // normal null checks such as (x == null) do not always work as
        // expected
        // In those cases you can use this function which will also
        // work with non-unity objects
        public static bool IsNull(System.Object obj)
        {
            return obj == null || obj.Equals(null);
        }

#if !NOT_UNITY3D
        // Call this before calling GetInjectableMonoBehavioursUnderGameObject to ensure that the StateMachineBehaviour's
        // also get injected properly
        // The StateMachineBehaviour's cannot be retrieved until after the Start() method so we
        // need to use ZenjectStateMachineBehaviourAutoInjecter to do the injection at that
        // time for us
        public static void AddStateMachineBehaviourAutoInjectersUnderGameObject(GameObject root)
        {
#if ZEN_INTERNAL_PROFILING
            using (ProfileTimers.CreateTimedBlock("Searching Hierarchy"))
#endif
            {
                var animators = root.GetComponentsInChildren<Animator>(true);

                foreach (var animator in animators)
                {
                    if (animator.gameObject.GetComponent<ZenjectStateMachineBehaviourAutoInjecter>() == null)
                    {
                        animator.gameObject.AddComponent<ZenjectStateMachineBehaviourAutoInjecter>();
                    }
                }
            }
        }

#if UNITY_EDITOR
        // Returns a Transform in the DontDestroyOnLoad scene (or, if we're not in play mode, within the current active scene)
        // whose GameObject is inactive, and whose hide flags are set to HideAndDontSave. We can instantiate prefabs in here
        // without any of their Awake() methods firing.
        public static Transform GetOrCreateInactivePrefabParent()
        {
            if (_disabledIndestructibleGameObject == null || (!Application.isPlaying &&
                                                              _disabledIndestructibleGameObject.scene !=
                                                              SceneManager.GetActiveScene()))
            {
                var go = new GameObject("ZenUtilInternal_PrefabParent");
                go.hideFlags = HideFlags.HideAndDontSave;
                go.SetActive(false);

                if (Application.isPlaying)
                {
                    UnityEngine.Object.DontDestroyOnLoad(go);
                }

                _disabledIndestructibleGameObject = go;
            }

            return _disabledIndestructibleGameObject.transform;
        }
#endif

#endif
    }

}
