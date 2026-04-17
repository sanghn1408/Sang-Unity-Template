using UnityEngine;
using System.Collections.Generic;
using System;
namespace GameUp.Core.Tutorial
{
    [CreateAssetMenu(fileName = "TutorialController", menuName = "Data/Tutorial/TutorialController")]
    public class SOTutorialController : Singleton<SOTutorialController>
    {
        public List<SOTutorialType> tutorialTypes;
        [NonSerialized] private Dictionary<TutorialType, SOTutorialType> _cacheTutorials;

        public bool IsCompleteAll
        {
            get
            {
                foreach (var tutorial in I.tutorialTypes)
                {
                    if (!tutorial.IsComplete) return false;
                }
                return true;
            }
        }
        public static Dictionary<TutorialType, SOTutorialType> Tutorials
        {
            get
            {
                if (I._cacheTutorials != null) return I._cacheTutorials;
                I._cacheTutorials = new Dictionary<TutorialType, SOTutorialType>();
                for (var i = 0; i < I.tutorialTypes.Count; i++)
                {
                    I._cacheTutorials.Add(I.tutorialTypes[i].tutorialType, I.tutorialTypes[i]);
                }
                return I._cacheTutorials;
            }
        }

        public void SetComplete(TutorialType tutorialType)
        {
            if (Tutorials.TryGetValue(tutorialType, out var tutorial))
            {
                tutorial.SetComplete();
            }
        }

        public void UnComplete(TutorialType tutorialType)
        {
            if (Tutorials.TryGetValue(tutorialType, out var tutorial))
            {
                tutorial.UnComplete();
            }
        }
    }
}