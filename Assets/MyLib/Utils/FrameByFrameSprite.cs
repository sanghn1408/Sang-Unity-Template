using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FrameByFrameSprite : MonoBehaviour
{
    [SerializeField] protected SpriteRenderer m_spriteRenderer;
    [SerializeField] protected float          m_timeWait       = 0.1f;

    [SerializeField] [Header("Sprites")] protected List<Sprite> m_sprites = new List<Sprite>();

    public bool IsVisible { get; set; }
    public Vector3 Position { get { return transform.position; } }
    public SpriteRenderer SpriteRenderer { get { return m_spriteRenderer; } set { m_spriteRenderer = value; } }

    protected int          m_index = 0;
    protected float        m_time  = 0f;

    protected virtual void Awake()
    {
        if (m_spriteRenderer == null)
        {
            m_spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void UpdateSprite()
    {
        if (m_spriteRenderer)
        {
            int index = m_index % m_sprites.Count;
            m_spriteRenderer.sprite = m_sprites[index];
        }
    }

    private void FixedUpdate()
    {
        if (gameObject.activeSelf)
        {
            if (m_time > 0)
            {
                m_time -= Time.fixedDeltaTime;
            }

            if (m_time <= 0f)
            {
                m_time = m_timeWait;
                m_index += 1;
                if (m_index >= m_sprites.Count)
                {
                    m_index = 0;
                }
                UpdateSprite();
            }
        }
    }
}