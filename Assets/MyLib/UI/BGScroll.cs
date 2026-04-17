using UnityEngine;
using UnityEngine.UI;

public class BGScroller : MonoBehaviour
{
    [SerializeField] private float scrollSpeedX = 0.1f;
    [SerializeField] private float scrollSpeedY = 0f;
    [SerializeField] private RawImage myRawImg;

    private Vector2 uvRectPos;

    private void Awake()
    {
        myRawImg = GetComponent<RawImage>();
        uvRectPos = myRawImg.uvRect.position;
    }

    private void Update()
    {
        uvRectPos.x = (uvRectPos.x + scrollSpeedX * Time.deltaTime) % 1f;
        uvRectPos.y = (uvRectPos.y + scrollSpeedY * Time.deltaTime) % 1f;

        Rect uv = myRawImg.uvRect;
        uv.position = uvRectPos;
        myRawImg.uvRect = uv;
    }
}
