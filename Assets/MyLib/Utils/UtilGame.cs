using UnityEngine;

public static class UtilGame
{
    public static bool CheckTouchUI()
    {

#if UNITY_EDITOR
        //if (Input.GetMouseButtonDown (0)) {
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return true;
            //	}
        }

#elif UNITY_ANDROID || UNITY_IOS
		Touch[] lsttouch = Input.touches;
		if (lsttouch.Length > 0) {
		for (int i = 0; i < lsttouch.Length; i++) {
		if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject (lsttouch [i].fingerId)) {
		return true;
		}
		}
		}
#endif
        return false;
    }


    public static string GenerateName(int len = 4)
    {
        System.Random r = new System.Random();
        string[] consonants = { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "l", "n", "p", "q", "r", "s", "sh", "zh", "t", "v", "w", "x" };
        string[] vowels = { "a", "e", "i", "o", "u", "ae", "y" };
        string Name = "";
        Name += consonants[r.Next(consonants.Length)].ToUpper();
        Name += vowels[r.Next(vowels.Length)];
        int b = 2; //b tells how many times a new letter has been added. It's 2 right now because the first two letters are already in the name.
        while (b < len)
        {
            Name += consonants[r.Next(consonants.Length)];
            b++;
            Name += vowels[r.Next(vowels.Length)];
            b++;
        }
        return Name;
    }

    public static bool GetRandom(int valueCheck)
    {
        if (Random.Range(0, 100) < valueCheck)
            return true;
        else return false;
    }
}
